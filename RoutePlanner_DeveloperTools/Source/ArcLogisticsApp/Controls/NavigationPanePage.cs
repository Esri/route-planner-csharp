using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ESRI.ArcLogistics.App.Controls.BitmapEx;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    ///  Class NavigationPanePage - represents navigation pane page button.
    /// </summary>
    [TemplatePart(Name = "PART_HeaderButton", Type = typeof(ToggleButton))]
    internal class NavigationPanePage : ContentControl
    {
        #region Constructors

        /// <summary>
        /// Constructor of type: initializes NavigationPanePage type.
        /// </summary>
        static NavigationPanePage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NavigationPanePage),
                new FrameworkPropertyMetadata(typeof(NavigationPanePage))
                );
        }

        /// <summary>
        /// Initializes a new instance of <c>NavigationPanePage</c>.
        /// </summary>
        public NavigationPanePage()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of <c>NavigationPanePage</c>.
        /// </summary>
        /// <param name="pageContent">Page content.</param>
        public NavigationPanePage(UIElement pageContent)
        {
            this.Content = pageContent;
        }

        #endregion Constructors

        #region Overridden methods of the base class

        /// <summary>
        /// Invoked whenever application code or internal processes call ApplyTemplate.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _InitControlParts();
            _InitCommandBindings();
        }

        #endregion Overridden methods of the base class

        #region Public static properties

        /// <summary>
        /// Dependency property IsPageRequired.
        /// </summary>
        public static readonly DependencyProperty IsPageRequiredProperty =
                DependencyProperty.Register("IsPageRequired", typeof(bool), typeof(NavigationPanePage));

        /// <summary>
        /// Dependency property IsPageComplete.
        /// </summary>
        public static readonly DependencyProperty IsPageCompleteProperty =
               DependencyProperty.Register("IsPageComplete", typeof(bool), typeof(NavigationPanePage));

        /// <summary>
        /// Dependency property DoesSupportCompleteStatus.
        /// </summary>
        public static readonly DependencyProperty DoesSupportCompleteStatusProperty =
              DependencyProperty.Register("DoesSupportCompleteStatus", typeof(bool), typeof(NavigationPanePage));

        /// <summary>
        /// Dependency property IsPageSelected.
        /// </summary>
        public static readonly DependencyProperty IsPageSelectedProperty =
                DependencyProperty.Register("IsPageSelected", typeof(bool), typeof(NavigationPanePage));

        /// <summary>
        /// Dependency property IsPageButtonMinimized.
        /// </summary>
        public static readonly DependencyProperty IsPageButtonMinimizedProperty =
                DependencyProperty.Register("IsPageButtonMinimized", typeof(bool), typeof(NavigationPanePage));

        /// <summary>
        /// Dependency property Image.
        /// </summary>
        public static readonly DependencyProperty ImageProperty =
                DependencyProperty.Register("Image", typeof(TileBrush), typeof(NavigationPanePage));

        /// <summary>
        /// Dependency property PageHeader.
        /// </summary>
        public static readonly DependencyProperty PageHeaderProperty =
               DependencyProperty.Register("PageHeader", typeof(string), typeof(NavigationPanePage));

        #endregion Public static properties

        #region Public static events

        /// <summary>
        /// Event raised when current page clicked
        /// </summary>
        public static readonly RoutedEvent ClickPageEvent = EventManager.RegisterRoutedEvent("ClickPage",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavigationPanePage));

        #endregion Public static events

        #region Public events

        /// <summary>
        /// Adds / removes routed event handler for Click event.
        /// </summary>
        public event RoutedEventHandler Click
        {
            add { AddHandler(NavigationPanePage.ClickPageEvent, value); }

            remove { RemoveHandler(NavigationPanePage.ClickPageEvent, value); }
        }

        #endregion Public events

        #region Public properties

        /// <summary>
        /// Gets / sets page support complete status.
        /// </summary>
        public bool DoesSupportCompleteStatus
        {
            get { return (bool)GetValue(DoesSupportCompleteStatusProperty); }

            set { SetValue(DoesSupportCompleteStatusProperty, value); }
        }

        /// <summary>
        /// Gets / sets page complete property of current page.
        /// </summary>
        public bool IsPageComplete
        {
            get { return (bool)GetValue(IsPageCompleteProperty); }

            set { SetValue(IsPageCompleteProperty, value); }
        }
       
        /// <summary>
        /// Gets / sets page required property of current page.
        /// </summary>
        public bool IsPageRequired
        {
            get { return (bool)GetValue(IsPageRequiredProperty); }

            set { SetValue(IsPageRequiredProperty, value); }
        }

        /// <summary>
        /// Gets / sets page button minimized property property of current page.
        /// </summary>
        public bool IsPageButtonMinimized
        {
            get
            {
                return (bool)GetValue(IsPageButtonMinimizedProperty); 
            }

            set
            {
                SetValue(IsPageButtonMinimizedProperty, value);
                ToolTipService.SetIsEnabled(this, value);
            }
        }

        /// <summary>
        /// Gets / sets page selected property of current page.
        /// </summary>
        public bool IsPageSelected
        {
            get { return (bool)GetValue(IsPageSelectedProperty); }

            set { SetValue(IsPageSelectedProperty, value); }
        }

        /// <summary>
        /// Gets / sets page content property of current page.
        /// </summary>
        public Object PageContent
        {
            get { return _pageContent; }

            set { _pageContent = value; }
        }

        /// <summary>
        /// Gets / sets TileBrush which is used to draw button's Icon.
        /// </summary>
        public TileBrush Image
        {
            get { return (TileBrush)GetValue(ImageProperty); }

            set 
            {
                SetValue(ImageProperty, value); 
            }
        }

        /// <summary>
        /// Gets/sets page's header
        /// </summary>
        public string PageHeader
        {
            get { return (string)GetValue(PageHeaderProperty); }
            internal set { SetValue(PageHeaderProperty, value); }
        }

        #endregion Public properties

        #region Internal static properties

        /// <summary>
        /// Gets defined click command.
        /// </summary>
        internal static RoutedUICommand ClickCommand
        {
            get { return _clickCommand; }
        }

        #endregion Internal static properties

        #region Private methods

        /// <summary>
        /// Initializes commands binding.
        /// </summary>
        private void _InitCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(ClickCommand, _ClickPageHeaderCommandExecuted));
        }

        /// <summary>
        /// Initializes parts of control.
        /// </summary>
        private void _InitControlParts()
        {
            _toggleButton = this.GetTemplateChild("PART_HeaderButton") as ToggleButton;
            _toggleButton.Command = ClickCommand;

            _iconBitmap = this.GetTemplateChild("iconBitmap") as SmartBitmap;

            _iconBorder = this.GetTemplateChild("iconBorder") as Border;

            _bitmapStackPanel = this.GetTemplateChild("bitmapStackPanel") as StackPanel;

            _InitIcon();
        }

        /// <summary>
        /// Handler for the Click command.
        /// </summary>
        /// <param name="sender">Source of an event (Ignored).</param>
        /// <param name="eventArgs">Event data (Ignored).</param>
        private void _ClickPageHeaderCommandExecuted(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            this.RaiseEvent(new RoutedEventArgs(NavigationPanePage.ClickPageEvent));
        }

        /// <summary>
        /// Gets bitmap source of a button icon.
        /// </summary>
        /// <returns>Bitmap source if Image property contains ImageBrush with bitmap source, or null - otherwise.</returns>
        private BitmapSource _GetIconBitmapSource()
        {
            // Type cast Image property to ImageBrush type.
            ImageBrush imageBrush = Image as ImageBrush;

            BitmapSource bitmapSource = null;

            // If Image property contains image brush.
            if (imageBrush != null)
            {
                ImageSource imageSource = imageBrush.ImageSource;

                bitmapSource = (BitmapSource)imageSource;
            }
            // Bitmap source can't be obtained from Image property.
            else
            {
                bitmapSource = null;
            }

            return bitmapSource;
        }

        /// <summary>
        /// Initializes button's icon: sets bitmap source and visibility property of icon.
        /// </summary>
        private void _InitIcon()
        {
            if (_iconBitmap != null)
            {
                _iconBitmap.Source = _GetIconBitmapSource();

                if (_iconBitmap.Source != null)
                {
                    _bitmapStackPanel.Visibility = System.Windows.Visibility.Visible;
                    _iconBorder.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    _bitmapStackPanel.Visibility = System.Windows.Visibility.Hidden;
                    _iconBorder.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        #endregion Private methods

        #region Private static fields

        /// <summary>
        /// Click command.
        /// </summary>
        private static RoutedUICommand _clickCommand = new RoutedUICommand("Click", "ClickCommand", typeof(NavigationPanePage));

        #endregion Private static fields

        #region Private fields

        /// <summary>
        /// Content of the current page.
        /// </summary>
        private Object _pageContent;

        /// <summary>
        /// Header toggle button.
        /// </summary>
        private ToggleButton _toggleButton;

        /// <summary>
        /// Bitmap which is button's icon.
        /// </summary>
        private SmartBitmap _iconBitmap;

        /// <summary>
        /// Border of button icon (visible when Image property is set to the other than ImageBrush type).
        /// </summary>
        private Border _iconBorder;

        /// <summary>
        /// Bitmap stack panel (contains button's icon bitmap, visible when icon bitmap exists).
        /// </summary>
        private StackPanel _bitmapStackPanel;

        #endregion Private fields
    }
}

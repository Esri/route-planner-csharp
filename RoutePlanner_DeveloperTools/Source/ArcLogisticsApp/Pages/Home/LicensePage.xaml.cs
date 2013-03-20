using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.Services;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for LicensePage.xaml
    /// </summary>
    internal partial class LicensePage : PageBase
    {
        #region Constructors
        /// <summary>
        /// Initializes static members of the LicensePage class.
        /// </summary>
        static LicensePage()
        {
            _MustBeShownPropertyKey = DependencyProperty.RegisterReadOnly(
                "MustBeShown",
                typeof(bool),
                typeof(LicensePage),
                new PropertyMetadata(false));
            MustBeShownProperty = _MustBeShownPropertyKey.DependencyProperty;
        }

        public LicensePage()
        {
            InitializeComponent();
            IsRequired = true;
            IsAllowed = true;
            DoesSupportCompleteStatus = true;
            IsComplete = true;
            this.Loaded += new RoutedEventHandler(LicensePage_Loaded);

            // add handler to check page's complete status after services initialized
            App.Current.ApplicationInitialized += new EventHandler(OnApplicationInitialized);
        }

        #endregion

        #region public dependency properties
        /// <summary>
        /// Identifies the "MustBeShown" dependency property.
        /// </summary>
        public static readonly DependencyProperty MustBeShownProperty;
        #endregion

        #region public properties
        /// <summary>
        /// Gets a value indicating if the page must be shown on application
        /// startup.
        /// </summary>
        public bool MustBeShown
        {
            get
            {
                return (bool)this.GetValue(MustBeShownProperty);
            }

            private set
            {
                this.SetValue(_MustBeShownPropertyKey, value);
            }

        }
        #endregion

        #region Page Overrided Members

        public override string Name
        {
            get { return PAGE_NAME; }
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("LicensePageCaption"); }
        }

        public override System.Windows.Media.TileBrush Icon
        {
            get
            {
                ImageBrush brush = (ImageBrush)App.Current.FindResource("LicenseBrush");
                return brush;
            }
        }

        #endregion

        #region PageBase overrided members

        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.LicensePagePath); }
        }

        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method checks all services and license and define is page complete
        /// </summary>
        private void _OnPageCompleteChanged()
        {
            if (_viewModel == null)
            {
                return;
            }

            this.IsComplete = _viewModel.IsComplete;

            this.MustBeShown =
                _viewModel != null &&
                (!_viewModel.LoggedIn ||
                _viewModel.RequiresExpirationWarning);
        }
        #endregion

        #region Event Handlers

        private void OnApplicationInitialized(object sender, EventArgs e)
        {
            var serversWithAuthentication =
                from server in App.Current.Servers
                where server.AuthenticationType == AgsServerAuthenticationType.Yes
                select server;
            var workingStatusController = new ApplicationWorkingStatusController();
            var uriNavigator = new DefaultBrowserUriNavigator();
            _viewModel = new LicensePageModel(
                serversWithAuthentication,
                App.Current.Messenger,
                workingStatusController,
                uriNavigator,
                App.Current.LicenseManager);
            _viewModel.PropertyChanged += _ViewModelPropertyChanged;
            this.DataContext = _viewModel;

            _OnPageCompleteChanged();
        }

        /// <summary>
        /// Handles changes of view model properties.
        /// </summary>
        /// <param name="sender">The reference to the event sender object.</param>
        /// <param name="e">The event arguments object.</param>
        private void _ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _viewModel.PropertyNameIsComplete ||
                e.PropertyName == _viewModel.PropertyNameLoggedIn ||
                e.PropertyName == _viewModel.PropertyNameRequiresExpirationWarning)
            {
                _OnPageCompleteChanged();
            }
        }

        private void LicensePage_Loaded(object sender, RoutedEventArgs e)
        {
            // set void status bar content
            ((MainWindow)App.Current.MainWindow).StatusBar.SetStatus(this, "");
        }
        #endregion

        #region private constants
        private const string PAGE_NAME = "License";

        /// <summary>
        /// The key identifying the "MustBeShown" dependency property.
        /// </summary>
        private static readonly DependencyPropertyKey _MustBeShownPropertyKey;
        #endregion

        #region private fields
        /// <summary>
        /// A reference to the license page view model object.
        /// </summary>
        private LicensePageModel _viewModel;
        #endregion
    }
}

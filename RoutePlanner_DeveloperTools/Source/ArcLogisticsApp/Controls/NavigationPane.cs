/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Windows.Controls.Primitives;
using System.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// links to parts of control defined in template
    /// </summary>
    [TemplatePart(Name = "PART_PaneContentPresenter", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_PopupContentPresenter", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_HorisontalSplitter", Type = typeof(GridSplitter))]
    [TemplatePart(Name = "PART_VerticalSplitter", Type = typeof(GridSplitter))]
    [TemplatePart(Name = "PART_CommonGrid", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_ButtonsStack", Type = typeof(StackPanel))]
    [TemplatePart(Name = "PART_MinimizedButtonsStack", Type = typeof(StackPanel))]
    [TemplatePart(Name = "PART_Menu", Type = typeof(ContextMenu))]
    [TemplatePart(Name = "PART_Expander", Type = typeof(Expander))]
    [TemplatePart(Name = "PART_MainCollapsedButton", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_PopupContentPresenter", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_ButtonsStackRow", Type = typeof(RowDefinition))]
    [TemplatePart(Name = "PART_ButtonsStackRow", Type = typeof(RowDefinition))]
    [TemplatePart(Name = "PART_CollapsedButtonLabel", Type = typeof(Label))]
    [TemplatePart(Name = "PART_MenuButton", Type = typeof(ToggleButton))]
    /// <summary>
    /// class defines navigation pane control includes navigation pages with dynamically defined content
    /// control can be collapsed, expanded or resized
    /// contents resizible panels with minimized & maximized page buttons, header & popup menu
    /// in collapsed mode content of control shows on popup panel
    /// </summary>
    internal class NavigationPane : ComboBox
    {
        #region Constructors & override methods

        static NavigationPane()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigationPane), new FrameworkPropertyMetadata(typeof(NavigationPane)));
        }

        public NavigationPane()
            : base()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitControlParts();
            _InitEventHandlers();
        }

        #endregion

        #region Public Routed Events

        public static readonly RoutedEvent ExpandEvent = EventManager.RegisterRoutedEvent("ExpandPane",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavigationPane));

        public static readonly RoutedEvent CollapseEvent = EventManager.RegisterRoutedEvent("CollapsePane",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavigationPane));

        public static readonly RoutedEvent MaximizeEvent = EventManager.RegisterRoutedEvent("MaximizePane",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavigationPane));

        public static readonly RoutedEvent ChangeSelectionEvent = EventManager.RegisterRoutedEvent("CurrentPageSelectionChanged",
           RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavigationPane));

        #endregion

        #region Public Methods

        /// <summary>
        /// adds new page to collection
        /// </summary>
        /// <param name="page"></param>
        public void AddPage(NavigationPanePage page)
        {
            _collPages.Add(page);

            _collMaximizedPages.Add(page);

            _UpdateButtonsStackLayout();

            _UpdateButtonPanelsContent();
            _UpdateMenu();
        }

        /// <summary>
        /// removes page from all collection
        /// </summary>
        /// <param name="page"></param>
        public void RemovePage(NavigationPanePage page)
        {
            _collPages.Remove(page);

            int i = _collMinimizedPages.BinarySearch(page);

            if (i >= 0)
                _collMinimizedPages.Remove(page);
            else
                _collMaximizedPages.Remove(page);

            _UpdateButtonsStackLayout();

            _UpdateButtonPanelsContent();
            _UpdateMenu();
        }

        /// <summary>
        /// removes all pages from all collections
        /// </summary>
        public void RemoveAllPages()
        {
            if (_collPages.Count > 0)
                _collPages.Clear();

            if (_collMaximizedPages.Count > 0)
                _collMaximizedPages.Clear();

            if (_collMinimizedPages.Count > 0)
                _collMinimizedPages.Clear();

            _UpdateButtonPanelsContent();
            _UpdateMenu();
        }

        /// <summary>
        /// Set selected certain page
        /// </summary>
        public void SelectPage(NavigationPanePage currentPage)
        {
            _SetPaneContent(currentPage);
        }

        /// <summary>
        /// select previous page.
        /// </summary>
        public void SelectPreviousPage()
        {
            if (_selectedPageIndex >= 1)
            {
                NavigationPanePage currentPage = _collPages[_selectedPageIndex - 1] as NavigationPanePage;
                _SetPaneContent(currentPage);
            }
        }

        /// <summary>
        /// Select next page.
        /// </summary>
        public void SelectNextPage()
        {
            if (_selectedPageIndex < (_collPages.Count - 1))
            {
                NavigationPanePage currentPage = _collPages[_selectedPageIndex + 1] as NavigationPanePage;
                _SetPaneContent(currentPage);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// dependency property defines enable/disable state for all control parts
        /// </summary>
        public static readonly DependencyProperty EnabledProperty =
               DependencyProperty.Register("Enabled", typeof(bool), typeof(NavigationPane));

        /// <summary>
        /// gets/sets is enabled property
        /// </summary>
        public bool Enabled
        {
            get
            {
                return (bool)GetValue(IsEnabledProperty);
            }
            set
            {
                SetValue(IsEnabledProperty, value);
                _popupContentPresenter.IsEnabled = value;
            }
        }

        /// <summary>
        /// dependency property defines checked/unchecked state for main collapsed toggle button
        /// </summary>
        public static readonly DependencyProperty IsMainCollapsedButtonCheckedProperty =
                DependencyProperty.Register("IsCollapsedButtonChecked", typeof(bool), typeof(NavigationPane));

        /// <summary>
        /// gets/sets is collapsed button checked or not
        /// </summary>
        public bool IsCollapsedButtonChecked
        {
            get { return (bool)GetValue(IsMainCollapsedButtonCheckedProperty); }
            set { SetValue(IsMainCollapsedButtonCheckedProperty, value); }
        }

        /// <summary>
        /// gets/sets control min width
        /// </summary>
        public double MinPaneWidth
        {
            get { return _expander.MinWidth; }
            set { _expander.MinWidth = value; }
        }

        /// <summary>
        /// gets/sets control max width
        /// </summary>
        public double MaxPaneWidth
        {
            get { return _expander.MaxWidth; }
            set { _expander.MaxWidth = value; }
        }

        /// <summary>
        /// gets/sets last width of expanded control
        /// </summary>
        public double LastExpandedWidth
        {
            get
            {
                if (_dLastExpandedWidth <= this.MaxWidth)
                    return this.MaxWidth;
                return _dLastExpandedWidth;
            }
            set { _dLastExpandedWidth = value; }
        }

        /// <summary>
        /// gets current selected page
        /// </summary>
        public NavigationPanePage SelectedPage
        {
            get
            {
                foreach (NavigationPanePage page in _collPages)
                    if (page.IsPageSelected)
                        return page;
                return null;
            }
        }

        public IList Pages
        {
            get
            {
                return _collPages;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// moves one element from minimized collection to maximized collection
        /// </summary>
        private void _IncrementMaximizedCollection()
        {
            if (_collMinimizedPages.Count > 0)
            {
                NavigationPanePage currentPage = (NavigationPanePage)_collMinimizedPages[0];
                _collMaximizedPages.Insert(_collMaximizedPages.Count, currentPage);
                _collMinimizedPages.Remove(currentPage);
                currentPage.IsPageButtonMinimized = false;
            }
        }

        /// <summary>
        /// moves one element from maximized collection to minimized collection
        /// </summary>
        private void _DecrementMaximizedCollection()
        {
            if (_collMaximizedPages.Count > 0)
            {
                // gets first page in maximized collection
                NavigationPanePage currentPage = (NavigationPanePage)_collMaximizedPages[_collMaximizedPages.Count - 1];
                _collMinimizedPages.Insert(0, currentPage);
                _collMaximizedPages.Remove(currentPage);
                currentPage.IsPageButtonMinimized = true;
            }
        }

        /// <summary>
        /// defines menu items
        /// </summary>
        private void _UpdateMenuItems()
        {
            if (_menu.Items.Count > 0)
                _menu.Items.Clear();

            foreach (NavigationPanePage page in _collMinimizedPages)
            {
                MenuItem item = new MenuItem();
                item.Header = page.PageHeader;
                item.Tag = page;
                item.Click += new RoutedEventHandler(MenuItemClickEventHandler);
                _menu.Items.Add(item);
            }
        }

        /// <summary>
        /// updates menu content after resizing maximized buttons panel
        /// </summary>
        private void _UpdateMenu()
        {
            _UpdateMenuItems();

            // update enabled/disabled menu property
            if (_collMinimizedPages.Count == 0)
                _menuButton.IsEnabled = false;

            else
                _menuButton.IsEnabled = true;
        }

        /// <summary>
        /// updates height of buttons stack
        /// </summary>
        private void _UpdateButtonsStackLayout()
        {
            double changedSize = _collMaximizedPages.Count * _expander.MinWidth + _horisontalSplitter.Height;

            GridLengthConverter gridLenghtConverter = new GridLengthConverter();

            double contentPresenterHeight = this.ActualHeight - changedSize - _expander.MinWidth * 2;

            _ResizeMainCollapsedButton();

            _buttonsStack.Background = new SolidColorBrush(Colors.Black);

            _buttonsRow.Height = new GridLength(changedSize);
            _buttonsStack.Height = changedSize;
        }

        /// <summary>
        /// updates minimized & maximized buttons content after resizing maximized buttons panel
        /// </summary>
        private void _UpdateButtonPanelsContent()
        {
            if (_buttonsStack.Children.Count > 0)
                _buttonsStack.Children.Clear();

            if (_minimizedButtonsStack.Children.Count > 0)
                _minimizedButtonsStack.Children.Clear();

            foreach (NavigationPanePage page in _collMinimizedPages)
            {
                page.Width = _expander.MinWidth;
                _minimizedButtonsStack.Children.Add(page);
                page.IsPageButtonMinimized = true;
            }

            foreach (NavigationPanePage page in _collMaximizedPages)
            {
                page.Width = _buttonsStack.ActualWidth;
                _buttonsStack.Children.Add(page);
                page.IsPageButtonMinimized = false;
            }

            // define top limit of buttons stack height
            _buttonsRow.MaxHeight = _collPages.Count * _expander.MinWidth + _horisontalSplitter.Height;
            _previousButtonsStackHeight = _buttonsRow.ActualHeight;
        }

        /// <summary>
        /// define control content depending on selected page
        /// </summary>
        private void _SetPaneContent(NavigationPanePage page)
        {
            _DeselectAllPages();
            page.IsPageSelected = true;
            _strHeader = page.PageHeader.ToString();

            if (_expander.IsExpanded)
                _expander.Header = _strHeader;

            _CollapsedButtonLabel.Content = _strHeader;

            this._contentPresenter.Content = (UIElement)page.PageContent;

            _selectedPageIndex = _collPages.IndexOf(page);

            if (IsDropDownOpen)
                IsDropDownOpen = false;

            this.RaiseEvent(new RoutedEventArgs(NavigationPane.ChangeSelectionEvent));
        }

        /// <summary>
        /// sets all pages deselected
        /// </summary>
        private void _DeselectAllPages()
        {
            foreach (NavigationPanePage page in _collPages)
            {
                page.IsPageSelected = false;
            }
        }

        /// <summary>
        /// initialization of all control parts
        /// </summary>
        private void _InitControlParts()
        {
            // buttons stack
            _buttonsStack = this.GetTemplateChild("PART_ButtonsStack") as StackPanel;

            // pane content presenter
            _contentPresenter = this.GetTemplateChild("PART_PaneContentPresenter") as ContentPresenter;

            // popup content presenter
            _popupContentPresenter = this.GetTemplateChild("PART_PopupContentPresenter") as ContentPresenter;

            // horisontal splitter
            _horisontalSplitter = this.GetTemplateChild("PART_HorisontalSplitter") as GridSplitter;

            // vertical splitter
            _verticalSplitter = this.GetTemplateChild("PART_VerticalSplitter") as GridSplitter;

            // resizible grid
            _commonGrid = this.GetTemplateChild("PART_CommonGrid") as Grid;

            // grid row with maximized buttons
            _buttonsRow = this.GetTemplateChild("PART_ButtonsStackRow") as RowDefinition;

            // panel with minimized buttons
            _minimizedButtonsStack = this.GetTemplateChild("PART_MinimizedButtonsStack") as StackPanel;

            // menu button
            _menuButton = this.GetTemplateChild("PART_MenuButton") as ToggleButton;

            // menu 
            _menu = this.GetTemplateChild("PART_Menu") as ContextMenu;
            _menu.PlacementTarget = _menuButton;
            _menu.Placement = PlacementMode.Top;

            // header with expander
            _expander = this.GetTemplateChild("PART_Expander") as Expander;

            // collapsed header button
            _btnMainCollapsedButton = this.GetTemplateChild("PART_MainCollapsedButton") as ToggleButton;

            // popup panel
            _popupPanel = this.GetTemplateChild("PART_Popup") as Popup;

            //collapsed button header
            _CollapsedButtonLabel = this.GetTemplateChild("PART_CollapsedButtonLabel") as Label;

            // init start value for maximized buttons stack
            _dPreviousButtonsStackSizeWidth = _commonGrid.ActualWidth;

            // init start value of expanded width
            _dLastExpandedWidth = _expander.MaxWidth;

            _selectedPageIndex = 0;
        }

        /// <summary>
        /// inits handlers for all routed events
        /// </summary>
        private void _InitEventHandlers()
        {
            // define handler for click on page header event
            this.AddHandler(NavigationPanePage.ClickPageEvent, new RoutedEventHandler(PageClickEventHandler));

            // define handler for collapse expander
            _expander.Collapsed += new RoutedEventHandler(CollapseExpander);

            // define handler for expanded expander
            _expander.Expanded += new RoutedEventHandler(ExpandedExpander);

            // define handler for mouse move
            this.PreviewMouseMove += new MouseEventHandler(NavigationPane_PreviewMouseMove);

            // define handler for window size change
            Application.Current.MainWindow.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);

            // define handler for resizing grid with maximized buttons
            _commonGrid.SizeChanged += new SizeChangedEventHandler(_commonGrid_SizeChanged);

            // define handler for resizing control
            _expander.SizeChanged += new SizeChangedEventHandler(_expander_SizeChanged);

            // define handler for popup panel close event
            _popupPanel.Closed += new EventHandler(_popupPanel_Closed);

            // define handler for popup panel open event
            _popupPanel.Opened += new EventHandler(_popupPanel_Opened);

            _menuButton.Checked += new RoutedEventHandler(_menuButton_Checked);
            _menu.Closed += new RoutedEventHandler(_menu_Closed);
        }

        /// <summary>
        /// method checks if mouse leave bounds of maximized buttons stack in drag mode
        /// and updates collections of minimized & maximized buttons 
        /// </summary>
        /// <param name="currentMousePosition"></param>
        private void _ButtonsStackMouseMoved(Point currentMousePosition)
        {
            _buttonsRow.MaxHeight = _collPages.Count * _expander.MinWidth + _horisontalSplitter.Height;

            // bottom bound of maximized buttons stack
            double maxPosition = this.ActualHeight - _buttonsRow.MaxHeight - _expander.MinWidth - _horisontalSplitter.Height;

            // top bound of maximized buttons stack
            double minPosition = this.ActualHeight - _expander.MinWidth;// -_horisontalSplitter.Height;

            if (_horisontalSplitter.IsDragging)
            {
                if (currentMousePosition.Y < (maxPosition))
                {
                    int count = _collMinimizedPages.Count;
                    for (int i = 0; i < count; i++)
                        _IncrementMaximizedCollection();
                }
                else if (currentMousePosition.Y > minPosition)
                {
                    int count = _collMaximizedPages.Count;
                    for (int i = 0; i < count; i++)
                        _DecrementMaximizedCollection();
                }
            }
        }

        /// <summary>
        /// actions during horisontal splitter moved
        /// </summary>
        /// <param name="shift"></param>
        private void _HorisontalSplitterMoved()
        {
            // define top limit of buttons stack height
            _buttonsRow.MaxHeight = _collPages.Count * _expander.MinWidth + _horisontalSplitter.Height;

            int previousNumberOfMaximizedButtons = _buttonsStack.Children.Count;

            int currentNumberOfMaximizedButtons = (int)(_buttonsRow.ActualHeight / _expander.MinWidth);

            if (previousNumberOfMaximizedButtons < currentNumberOfMaximizedButtons)
            {
                _IncrementMaximizedCollection();
            }

            else if (previousNumberOfMaximizedButtons > currentNumberOfMaximizedButtons)
            {
                _DecrementMaximizedCollection();
            }

            _UpdateButtonPanelsContent();

            double changedSize = _collMaximizedPages.Count * _expander.MinWidth + _horisontalSplitter.Height;

            _buttonsRow.Height = new GridLength(changedSize);

            _ResizePageButtons(_commonGrid.ActualWidth);
            _UpdateMenu();

            _previousButtonsStackHeight = _buttonsRow.ActualHeight;
        }

        /// <summary>
        /// actions when vertical splitter moved
        /// </summary>
        /// <param name="shift"></param>
        private void _VerticalSplitterMoved()
        {
            _expander.Expanded -= ExpandedExpander;
            _expander.Collapsed -= CollapseExpander;

            double delta = _dPreviousButtonsStackSizeWidth - _commonGrid.ActualWidth;

            double currentWidth = _buttonsStack.ActualWidth;

            // dragged to left
            if (_commonGrid.ActualWidth <= _expander.MinWidth)
            {
                _CollapseControl();
            }

            // dragged to right
            if ((!_expander.IsExpanded) && (delta < 0))
            {
                _ExpandControl();
            }

            _dPreviousButtonsStackSizeWidth = _commonGrid.ActualWidth;

            _ResizePageButtons(currentWidth);

            _expander.Expanded += new RoutedEventHandler(ExpandedExpander);
            _expander.Collapsed += new RoutedEventHandler(CollapseExpander);
        }

        /// <summary>
        /// recount size of page header buttons
        /// </summary>
        /// <param name="currentWidth"></param>
        private void _ResizePageButtons(double currentWidth)
        {
            foreach (Control button in this._buttonsStack.Children)
            {
                button.Width = currentWidth;
            }
            _ResizeMainCollapsedButton();
        }

        /// <summary>
        /// recount size of main collapsed button
        /// </summary>
        private void _ResizeMainCollapsedButton()
        {
            double changedSize = _collMaximizedPages.Count * _expander.MinWidth + _horisontalSplitter.Height;
            double contentPresenterHeight = this.ActualHeight - changedSize - _expander.MinWidth * 2;

            if (contentPresenterHeight < 0)
                contentPresenterHeight = 0;

            _contentPresenter.Height = contentPresenterHeight;

            if (!_expander.IsExpanded)
                _btnMainCollapsedButton.Height = contentPresenterHeight;
        }


        /// <summary>
        /// define chandes in visual presentation of control when it's collapsed
        /// </summary>
        private void _CollapseControl()
        {
            _btnMainCollapsedButton.Width = _expander.MinWidth;

            _btnMainCollapsedButton.Visibility = Visibility.Visible;

            _contentPresenter.Visibility = Visibility.Collapsed;
            _minimizedButtonsStack.Visibility = Visibility.Collapsed;

            _expander.Header = string.Empty; // hide page header in collapsed mode

            double defaultNavigationPaneWidth = (double)App.Current.FindResource("DefaultNavigationPaneWidth");

            if (_dPreviousButtonsStackSizeWidth <= defaultNavigationPaneWidth)
                _dLastExpandedWidth = defaultNavigationPaneWidth;

            else
                _dLastExpandedWidth = _dPreviousButtonsStackSizeWidth;

            foreach (NavigationPanePage page in _collMaximizedPages)
                ToolTipService.SetIsEnabled(page, true); // enable tooltips

            _expander.IsExpanded = false;
        }

        /// <summary>
        /// define chandes in visual presentation of control when it's collapsed
        /// </summary>
        private void _ExpandControl()
        {
            _expander.IsExpanded = true;
            _btnMainCollapsedButton.Visibility = Visibility.Hidden;
            _contentPresenter.Visibility = Visibility.Visible;
            _minimizedButtonsStack.Visibility = Visibility.Visible;
            _expander.Header = _strHeader; // show page header in expand mode   

            foreach (NavigationPanePage page in _collMaximizedPages)
                ToolTipService.SetIsEnabled(page, false); // disable tooltips
        }

        /// <summary>
        /// moves content from first content presenter to second
        /// /// </summary>
        /// <param name="cp1"></param>
        /// <param name="cp2"></param>
        private void _ExchangeContent(ContentPresenter cpFrom, ContentPresenter cpTto)
        {
            cpTto.Content = cpFrom.Content;
            cpFrom.Content = null;
        }

        #endregion

        #region Private event handlers

        private void _menu_Closed(object sender, RoutedEventArgs e)
        {
            _menuButton.IsChecked = false;
        }

        private void _menuButton_Checked(object sender, RoutedEventArgs e)
        {
            _menu.IsOpen = true;
        }

        private void _menuButton_Click(object sender, RoutedEventArgs e)
        {
            _menu.IsOpen = true;
        }

        private void NavigationPane_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = e.GetPosition(this);
            _ButtonsStackMouseMoved(currentMousePosition);
        }

        /// <summary>
        /// actions when mouse moved over control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigationPane_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = e.GetPosition(this);
            _ButtonsStackMouseMoved(currentMousePosition);
        }

        /// <summary>
        /// actions when control size changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _expander_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _VerticalSplitterMoved();
        }

        /// <summary>
        /// actions when main window size changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _ResizePageButtons(this.ActualWidth);
            _UpdateButtonPanelsContent();
        }

        /// <summary>
        /// actions when popup panel opened
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _popupPanel_Opened(object sender, EventArgs e)
        {
            if ((StackPanel)_contentPresenter.Content != null && ((StackPanel)_contentPresenter.Content).Children.Count > 0)
            {
                _ExchangeContent(_contentPresenter, _popupContentPresenter);

                int visibleCount = 0;

                if (_popupContentPresenter.Content is StackPanel)
                    foreach (FrameworkElement fe in ((StackPanel)_popupContentPresenter.Content).Children)
                        if (fe.Visibility == Visibility.Visible)
                            visibleCount++;

                _popupPanel.IsOpen = (visibleCount > 0);
            }
            else
                _popupPanel.IsOpen = false;

            IsCollapsedButtonChecked = true;
        }

        /// <summary>
        /// actions when popup panel closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _popupPanel_Closed(object sender, EventArgs e)
        {
            if (_contentPresenter.Content == null)
                _ExchangeContent(_popupContentPresenter, _contentPresenter);

            IsCollapsedButtonChecked = false;
        }

        /// <summary>
        /// actions when maximized buttons stack size changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _commonGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_horisontalSplitter.IsDragging)
            {
                _HorisontalSplitterMoved();
            }
        }

        /// <summary>
        /// handle page button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageClickEventHandler(object sender, RoutedEventArgs e)
        {
            NavigationPanePage currentPage = (NavigationPanePage)e.OriginalSource;
            _SetPaneContent(currentPage);
        }

        /// <summary>
        /// define actions after collapsed expander
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollapseExpander(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(NavigationPane.CollapseEvent));
        }

        /// <summary>
        /// define actions after epanded expander
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExpandedExpander(object sender, RoutedEventArgs e)
        {
            _ExpandControl();
            _ResizePageButtons(_dLastExpandedWidth);
            _btnMainCollapsedButton.IsChecked = false;
            this.RaiseEvent(new RoutedEventArgs(NavigationPane.ExpandEvent));
        }


        /// <summary>
        /// handle menu item click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemClickEventHandler(object sender, RoutedEventArgs e)
        {
            MenuItem item = e.OriginalSource as MenuItem;
            NavigationPanePage currentPage = (NavigationPanePage)item.Tag; // gets item's dependent page
            _SetPaneContent(currentPage);
        }

        #endregion

        #region Private Variables

        /// <summary>
        /// collection of all pages
        /// </summary>
        private ArrayList _collPages = new ArrayList();

        /// <summary>
        /// collection pages with maximized headers
        /// </summary>
        private ArrayList _collMaximizedPages = new ArrayList();

        /// <summary>
        /// collection pages with minimized headers
        /// </summary>
        private ArrayList _collMinimizedPages = new ArrayList();

        /// <summary>
        /// collection of menu items
        /// </summary>
        private ArrayList _collMenuItems = new ArrayList();

        /// <summary>
        /// section with button headers of pages
        /// </summary>
        private StackPanel _buttonsStack;

        /// <summary>
        /// section with minimized buttons
        /// </summary>
        private StackPanel _minimizedButtonsStack;

        /// <summary>
        /// content container for displaying panel content inside current pane
        /// </summary>
        private ContentPresenter _contentPresenter;

        /// <summary>
        /// content container for displaying panel content inside popup window
        /// </summary>
        private ContentPresenter _popupContentPresenter;

        /// <summary>
        /// horisontal splitter
        /// </summary>
        private GridSplitter _horisontalSplitter;

        /// <summary>
        /// vertical splitter
        /// </summary>
        private GridSplitter _verticalSplitter;

        /// <summary>
        /// menu
        /// </summary>
        private ContextMenu _menu;

        /// <summary>
        /// menu button
        /// </summary>
        private ToggleButton _menuButton;

        /// <summary>
        /// grid with maximized buttons
        /// </summary>
        private Grid _commonGrid;

        /// <summary>
        /// expander
        /// </summary>
        private Expander _expander;

        /// <summary>
        /// button showed on pane when it's collapsed
        /// </summary>
        private ToggleButton _btnMainCollapsedButton;

        /// <summary>
        /// panel opened near main collapsed button when it's clicked
        /// </summary>
        private Popup _popupPanel;

        /// <summary>
        /// grid row with maximized buttons
        /// </summary>
        private RowDefinition _buttonsRow;

        /// <summary>
        /// header of opened panel
        /// </summary>
        private String _strHeader = "";

        private Label _CollapsedButtonLabel;

        /// <summary>
        /// default value of buttons stack width
        /// </summary>
        private double _dPreviousButtonsStackSizeWidth = 0;

        /// <summary>
        /// default value of component width
        /// </summary>
        private double _dLastExpandedWidth;

        /// <summary>
        /// index of selected page
        /// </summary>
        private int _selectedPageIndex;

        private double _previousButtonsStackHeight;

        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Dialogs;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Properties;
using AppPages = ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IUIManager
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new instance of the <c>MainWindow</c> class.
        /// </summary>
        public MainWindow()
        {
            // Splash screen is created at first and automatically becomes Main Window. 
            // So when real main window is created it should override Application.Current.MainWindow property.
            Application.Current.MainWindow = this;

            InitializeComponent();

            _LoadSettings();
            _HideMessageWindow(false);
            _InitEventHandlers();
            PageFrame.CommandBindings.Clear(); // NOTE: remove automatic navigation in Frame

            this.help.Visibility = (_IsTopicPresent()) ? Visibility.Visible : Visibility.Hidden;
        }

        #endregion

        #region Public Properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the status bar.
        /// </summary>
        public ESRI.ArcLogistics.App.Controls.StatusBar StatusBar
        {
            get { return statusBar; }
        }

        // APIREV: this event should be removed.
        /// <summary>
        /// Fires when application date changed in any calendar control.
        /// </summary>
        public event EventHandler NavigationCalled;

        /// <summary>
        /// Returns the currently open page.
        /// </summary>
        public AppPages.Page CurrentPage
        {
            get { return _currentPage; }
        }

        #endregion

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Navigate to page using page path.
        /// </summary>
        /// <param name="pagePath">string in format CategoryName\\PageName </param>
        public void Navigate(string pagePath)
        {
            Debug.Assert(pagePath != null);

            INavigationItem item = null;
            if (_navigationTree.FindItem(pagePath, out item))
                _Navigate(item as PageItem);
        }

        /// <summary>
        /// Show Main Prefernces page and lock UI.
        /// </summary>
        public void ShowMainPreferences()
        {
            // Find general preferences page item.
            INavigationItem generalPreferencesPageItem = null;
            _navigationTree.FindItem(typeof(GeneralPreferencesPage),
                out generalPreferencesPageItem);

            // Init and show preference page.
            _InitNavigationItem(generalPreferencesPageItem);
            PageFrame.Navigate(((PageItem)generalPreferencesPageItem).Page);

            // Lock visible UI.
            CategoriesButtons.IsEnabled = false;
            CategoriesButtons.Focusable = false;
            preferences.IsEnabled = false;
            preferences.Focusable = false;
            lockedNavigationPane.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Method returns page by path.
        /// </summary>
        /// <param name="PagePath">String in format CategoryName\PageName.</param>
        /// <returns>Found page.</returns>
        public AppPages.Page GetPage(string pagePath)
        {
            Debug.Assert(pagePath != null);

            INavigationItem foundPageItem;
            return (_navigationTree.FindItem(pagePath, out foundPageItem) ? ((PageItem)foundPageItem).Page : null);
        }

        #endregion

        #region IUIManager interface
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Fires when MainWindow UI locked.
        /// </summary>
        public event EventHandler Locked;

        /// <summary>
        /// Fires when MainWindow UI unlocked.
        /// </summary>
        public event EventHandler UnLocked;

        /// <summary>
        /// Locks MainWindow UI.
        /// </summary>
        /// <param name="lockPageFrame">Lock page frame flag.</param>
        public void Lock(bool lockPageFrame)
        {
            _isLocked = true;

            CategoriesButtons.IsEnabled = false;
            CategoriesButtons.Focusable = false;

            if (Visibility.Hidden != preferences.Visibility)
            {
                preferences.IsEnabled = false;
                preferences.Focusable = false;
            }

            if (navigationPane.IsDropDownOpen)
                navigationPane.IsDropDownOpen = false;

            if (lockPageFrame)
                lockedGridFrame.Visibility = Visibility.Visible;

            lockedNavigationPane.Visibility = Visibility.Visible;

            AppPages.OptimizeAndEditPage schedulePage = (AppPages.OptimizeAndEditPage)App.Current.MainWindow.GetPage(AppPages.PagePaths.SchedulePagePath);
            schedulePage.IsLocked = true;

            if (Locked != null)
                Locked(this, EventArgs.Empty);
        }

        /// <summary>
        /// Unlocks MainWindow UI.
        /// </summary>
        public void Unlock()
        {
            CategoriesButtons.Focusable = true;
            CategoriesButtons.IsEnabled = true;

            if (Visibility.Hidden != preferences.Visibility)
            {
                preferences.Focusable = true;
                preferences.IsEnabled = true;
            }

            lockedGridFrame.Visibility = Visibility.Hidden;
            lockedNavigationPane.Visibility = Visibility.Hidden;

            AppPages.OptimizeAndEditPage schedulePage = (AppPages.OptimizeAndEditPage)App.Current.MainWindow.GetPage(AppPages.PagePaths.SchedulePagePath);
            schedulePage.IsLocked = false;

            _isLocked = false;

            if (UnLocked != null)
                UnLocked(this, EventArgs.Empty);
        }

        /// <summary>
        /// Returns a boolean value based on whether or not MainWindow UI is locked.
        /// </summary>
        public bool IsLocked
        {
            get { return _isLocked; }
        }

        /// <summary>
        /// Lock message window UI.
        /// </summary>
        public void LockMessageWindow()
        {
            _HideMessageWindow(true);

            messageWindow.IsEnabled =
                StatusBar.ButtonMessages.IsEnabled = false;
        }

        /// <summary>
        /// Unlock message window UI.
        /// </summary>
        public void UnlockMessageWindow()
        {
            messageWindow.IsEnabled =
                StatusBar.ButtonMessages.IsEnabled = true;
        }

        #endregion // IUIManager interface

        #region Internal properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets message window
        /// </summary>
        internal ESRI.ArcLogistics.App.Controls.MessageWindow MessageWindow
        {
            get { return messageWindow; }
        }

        /// <summary>
        /// Gets Navigation Tree
        /// </summary>
        internal NavigationTree NavigationTree
        {
            get { return _navigationTree; }
        }

        /// <summary>
        /// Gets Collapsed Widgets
        /// </summary>
        internal List<Type> CollapsedWidgets
        {
            get { return _collapsedWidgets; }
        }

        internal bool IsHelpVisible
        {
            get { return _isHelpVisible; }
            set
            {
                _isHelpVisible = value;
                _taskPanelContentBuilder.UpdateTaskPanelWidgetsState((StackPanel)_widgetsHash[((PageItem)_currentPageItem).PageType.FullName]);
            }
        }

        // APIREV: Consider removing static modifier.
        /// <summary>
        /// Skin loader 
        /// </summary>
        internal static AppSkinLoader SkinLoader = new AppSkinLoader();

        #endregion Internal properties

        #region Internal methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Message window show/hide function
        /// </summary>
        internal void ToggleMessageWindowState()
        {
            if (Visibility.Visible == messageWindow.Visibility)
                _HideMessageWindow(true);
            else
                _ShowMessageWindow();
        }

        /// <summary>
        /// Toggle task panel widgets state.
        /// </summary>
        /// <param name="page">Page for selecting widgets.</param>
        /// <param name="ignoredWidgets">Collection of widgets that ignored this toggle.</param>
        /// <param name="isEnable">Enable state flag.</param>
        internal void ToggleWidgetsState(AppPages.Page page, ICollection<Type> ignoredWidgets, bool isEnable)
        {
            Debug.Assert(null != page);
            Debug.Assert(null != ignoredWidgets);

            StackPanel stackPanel = (StackPanel)_widgetsHash[page.GetType().FullName];
            _taskPanelContentBuilder.ToggleTaskPanelWidgetsState(stackPanel, ignoredWidgets, isEnable);
        }

        /// <summary>
        /// Navigates to the initial application page.
        /// </summary>
        /// <remarks>Should be called only after application was initialized.</remarks>
        internal void Start()
        {
            Debug.Assert(App.Current.IsInitialized);

            _startFleetSetupWizardOnContinue = true;

            var licensePage = (LicensePage)this.GetPage(PagePaths.LicensePagePath);
            var descriptor = DependencyPropertyDescriptor.FromProperty(
                LicensePage.MustBeShownProperty,
                typeof(LicensePage));
            descriptor.AddValueChanged(licensePage, delegate
            {
                if (!licensePage.MustBeShown)
                {
                    _ContinueWorking(PagePaths.GettingStartedPagePath);
                }
            });

            if (licensePage.MustBeShown)
            {
                this.Navigate(PagePaths.LicensePagePath);

                return;
            }

            _ContinueWorking();
        }
        #endregion // Internal methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init all items in navigation tree.
        /// </summary>
        private void _InitNavigationTree()
        {
            foreach (INavigationItem navigationItem in _navigationTree.NaviationTreeRoot.Children)
            {
                _InitNavigationItem(navigationItem);
            }
        }

        /// <summary>
        /// Init navigation item
        /// </summary>
        /// <param name="navigationItem">Navigation item to init.</param>
        private void _InitNavigationItem(INavigationItem navigationItem)
        {
            if (navigationItem.Type == NavigationItemType.Category)
            {
                CategoryItem category = (CategoryItem)navigationItem;
                foreach (PageItem pageItem in navigationItem.Children)
                {
                    if (pageItem.Page == null)
                    {
                        AppPages.Page newPage = (AppPages.Page)Activator.CreateInstance(pageItem.PageType);
                        pageItem.Page = newPage;
                    }

                    pageItem.Page.Initialize(App.Current);
                }
            }
            else
            {
                AppPages.Page newPage = (AppPages.Page)Activator.CreateInstance(((PageItem)navigationItem).PageType);
                newPage.Initialize(App.Current);
                ((PageItem)navigationItem).Page = newPage;
            }
        }

        #endregion // Internal methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Inits all event handlers.
        /// </summary>
        private void _InitEventHandlers()
        {
            App.Current.ApplicationInitialized += new EventHandler(App_ApplicationInitialized);

            this.Closed += new EventHandler(MainWindow_Closed);

            this.AddHandler(ExpanderControl.ClickButtonEvent, new RoutedEventHandler(ExpanderClicked));

            this.Loaded += new RoutedEventHandler(OnLoaded);
            this.AddHandler(NavigationPane.CollapseEvent, new RoutedEventHandler(OnCollapseNavigationPane));
            this.AddHandler(NavigationPane.ExpandEvent, new RoutedEventHandler(OnExpandNavigationPane));
            this.AddHandler(NavigationPane.ChangeSelectionEvent, new RoutedEventHandler(OnCurrentPageChanged));

            // Skin changed event handlers
            SkinLoader.SkinChange += new RoutedEventHandler(SkinLoader_SkinChange);

            this.preferences.MouseLeftButtonUp += new MouseButtonEventHandler(preferences_MouseLeftButtonUp);
            this.help.MouseLeftButtonUp += new MouseButtonEventHandler(help_MouseLeftButtonUp);
            this.about.MouseLeftButtonUp += new MouseButtonEventHandler(about_MouseLeftButtonUp);
        }

        /// <summary>
        /// Loads saved project settings.
        /// </summary>
        private void _LoadSettings()
        {
            Settings settings = Settings.Default;

            SkinLoader.SkinName = settings.Skin;
            SkinLoader.ApplySkin();

            _lastPageName = settings.LastPageName;

            _isHelpVisible = settings.IsHelpVisible;

            if (settings.CollapsedWidgets != null)
            {
                foreach (Type type in settings.CollapsedWidgets)
                    _collapsedWidgets.Add(type);
            }
        }

        /// <summary>
        /// Saves current state of application to settings file.
        /// </summary>
        private void _SaveSettings()
        {
            if (App.Current.IsInitialized)
            {
                Settings settings = Settings.Default;
                settings.Skin = SkinLoader.SkinName;

                string categoryName = (_currentCategory != null) ? _currentCategory.Name : HOME_CATEGORY_NAME;
                settings.LastPageName = string.Format(FULL_PAGE_NAME_FORMAT, categoryName, _currentPage.Name);

                settings.LastSavedDate = App.Current.CurrentDate;

                settings.IsHelpVisible = _isHelpVisible;

                settings.CollapsedWidgets = new ArrayList();

                foreach (Type type in _collapsedWidgets)
                    settings.CollapsedWidgets.Add(type);

                _SavePagesSettings();

                settings.Save();
            }
        }

        /// <summary>
        /// Navigates to the specified page.
        /// </summary>
        /// <param name="item"></param>
        private void _Navigate(PageItem item)
        {
            // If this page cannot be left - do not navigate from current page.
            if ((_currentPage != item.Page) && !_currentPage.CanBeLeft)
                item = _currentPageItem;

            _NotifyNavigationCalled();

            this.RemoveHandler(NavigationPane.ChangeSelectionEvent, (RoutedEventHandler)OnCurrentPageChanged);

            CategoryItem currentCategory = (CategoryItem)item.Parent;

            if (item.Page.IsAllowed && 
                ((null == currentCategory.PageCategory) || // NOTE: category can be not belong to categories ("Preferences")
                ((null != currentCategory.PageCategory) && currentCategory.PageCategory.IsEnabled)))
            {
                PageFrame.Navigate(item.Page);

                _ChangeCurrentCategory(item.Parent as INavigationItem);
                _UpdateCurrentCategoryButton();

                // Add widgets panel to hash table
                if (!_widgetsHash.Contains(item.PageType.FullName))
                {
                    StackPanel widgetsStack = _CreateWidgetsPanel(item.Page);
                    _widgetsHash.Add(item.PageType.FullName, widgetsStack);
                }

                foreach (NavigationPanePage panePage in navigationPane.Pages)
                {
                    if (panePage.Tag == item)
                    {   // set navigation pane content
                        panePage.PageContent = (StackPanel)_widgetsHash[item.PageType.FullName];
                        navigationPane.SelectPage(panePage);
                        break;
                    }
                }
                _taskPanelContentBuilder.UpdateTaskPanelWidgetsState((StackPanel)_widgetsHash[item.PageType.FullName]);
                _currentPageItem = item;
                _currentPage = item.Page;
            }

            this.AddHandler(NavigationPane.ChangeSelectionEvent, new RoutedEventHandler(OnCurrentPageChanged));
        }

        /// <summary>
        /// Method builds widgets panel.
        /// </summary>
        private StackPanel _CreateWidgetsPanel(AppPages.Page page)
        {
            StackPanel widgetsPanel = new StackPanel();
            if (page.Widgets.Count != 0)
                widgetsPanel = _taskPanelContentBuilder.BuildTaskPanelContent(page);
            return widgetsPanel;
        }

        /// <summary>
        /// Method navigate to last opened page in previous session.
        /// </summary>
        private void _NavigateToLastSavedPage()
        {
            INavigationItem item = null;
            var canNavigateToLastSavedPage =
                Settings.Default.StartApplicationAtLastSavedPage &&
                !string.IsNullOrEmpty(_lastPageName) &&
                _navigationTree.FindItem(_lastPageName, out item);
            if (canNavigateToLastSavedPage)
            {
                _Navigate(item as PageItem);
            }
            else
            {
                _NavigateHome();
            }
        }

        /// <summary>
        /// Loads main window layout.
        /// </summary>
        private void _LoadLayout()
        {
            // create navigation tree
            _navigationTree = new NavigationTree();

            // add categories buttons
            foreach (INavigationItem item in _navigationTree.NaviationTreeRoot.Children)
            {
                if (item.Type == NavigationItemType.Page)
                    continue; // NOTE: skip simple pages

                if (!item.IsVisible)
                {
                    // NOTE: support only "Preferences"
                    Debug.Assert(item.Name.Equals("Preferences", StringComparison.OrdinalIgnoreCase));
                    _preferencesCategory = item;

                    continue; // NOTE: skip items if it not visible
                }

                // create new button that will be entry point to the category pages
                ToggleButton button = new ToggleButton();
                button.Tag = item;
                button.Content = item.Caption;

                button.AddHandler(ToggleButton.ClickEvent, new RoutedEventHandler(OnCategoryButtonClick));
                button.Style = Application.Current.Resources["CategoriesToggleButtonStyle"] as Style;

                CategoryItem category = (CategoryItem)item;
                if (category.PageCategory == null)
                    category.PageCategory = (PageCategoryItem)Activator.CreateInstance(category.CategoryType);

                // set binding to "IsEnabled" property
                _SetBinding(PageCategoryItem.IS_ENABLED_PROPERTY_NAME, button, category.PageCategory, ToggleButton.IsEnabledProperty);

                // set binding to "ToooltipText"
                _SetBinding(PageCategoryItem.TOOLTIP_PROPERTY_NAME, button, category.PageCategory, ToggleButton.ToolTipProperty);

                CategoriesButtons.AddContentItem(button);
            }

            _InitSavedPagesRepository();
        }

        /// <summary>
        /// Method creates property binding with stated parameters
        /// </summary>
        private void _SetBinding(string propertyName, DependencyObject bindingTarget, object bindingSource, DependencyProperty bindingProperty)
        {
            Binding binding = new Binding();
            binding.Mode = BindingMode.OneWay;
            binding.Source = bindingSource;
            binding.Path = new PropertyPath(propertyName);
            binding.NotifyOnSourceUpdated = false;
            BindingOperations.SetBinding(bindingTarget, bindingProperty, binding);
        }

        /// <summary>
        /// Changes current category.
        /// </summary>
        private void _ChangeCurrentCategory(INavigationItem newCategory)
        {
            if (_currentCategory != null)
                _SaveSelectedPageName();

            if (newCategory.Name != "Root")
            {
                if (newCategory != _currentCategory)
                {
                    _currentCategory = newCategory;
                    _UpdateNavigationPaneButtonsStack();
                }
            }
            else
            {
                _currentCategory = null;
                _UpdateNavigationPaneButtonsStack();
            }
        }

        /// <summary>
        /// Updates buttons stack and only current category button stays checked.
        /// </summary>
        private void _UpdateCurrentCategoryButton()
        {
            foreach (ToggleButton button in CategoriesButtons.ContentItems)
                button.IsChecked = ((button.Tag as INavigationItem) == _currentCategory);
        }

        /// <summary>
        /// Refill navigation pane with pan pages.
        /// </summary>
        private void _UpdateNavigationPaneButtonsStack()
        {
            if (navigationPane.Pages.Count != 0)
                navigationPane.RemoveAllPages();

            if (_currentCategory != null)
            {
                // fill navigation pane with pages
                foreach (PageItem item in _currentCategory.Children)
                {
                    NavigationPanePage navPage = new NavigationPanePage();
                    navPage.PageHeader = item.Caption;

                    ImageSourceConverter converter = new ImageSourceConverter();

                    navPage.Image = (null == item.Page.Icon) ? (ImageBrush)App.Current.FindResource("DefaultBrush") : item.Page.Icon;

                    navPage.Tag = item;

                    _SetPropertiesBinding(navPage, item);

                    navigationPane.AddPage(navPage);
                }
            }
            else
            {
                NavigationPanePage navPage = new NavigationPanePage();
                navPage.PageHeader = _currentPageItem.Caption;

                ImageSourceConverter converter = new ImageSourceConverter();

                navPage.Image = ((PageItem)_currentPageItem).Page.Icon;

                navPage.Tag = _currentPageItem;
                _SetPropertiesBinding(navPage, (PageItem)_currentPageItem);
                navigationPane.AddPage(navPage);
            }
        }

        /// <summary>
        /// Sets binding to all necessary properties.
        /// </summary>
        /// <param name="navPage"></param>
        /// <param name="item"></param>
        private void _SetPropertiesBinding(NavigationPanePage navPage, PageItem item)
        {
            Binding enablingBinding = new Binding("IsAllowed");
            enablingBinding.Mode = BindingMode.OneWay;
            enablingBinding.Source = item.Page;
            enablingBinding.NotifyOnSourceUpdated = true;
            BindingOperations.SetBinding(navPage, NavigationPanePage.IsEnabledProperty, enablingBinding);

            Binding reuquiredBinding = new Binding("IsRequired");
            reuquiredBinding.Mode = BindingMode.OneWay;
            reuquiredBinding.Source = item.Page;
            reuquiredBinding.NotifyOnSourceUpdated = true;
            BindingOperations.SetBinding(navPage, NavigationPanePage.IsPageRequiredProperty, reuquiredBinding);

            Binding canCompleteBinding = new Binding("DoesSupportCompleteStatus");
            canCompleteBinding.Mode = BindingMode.OneWay;
            canCompleteBinding.Source = item.Page;
            canCompleteBinding.NotifyOnSourceUpdated = true;
            BindingOperations.SetBinding(navPage, NavigationPanePage.DoesSupportCompleteStatusProperty, canCompleteBinding);

            Binding completeBinding = new Binding("IsComplete");
            completeBinding.Mode = BindingMode.OneWay;
            completeBinding.Source = item.Page;
            completeBinding.NotifyOnSourceUpdated = true;
            BindingOperations.SetBinding(navPage, NavigationPanePage.IsPageCompleteProperty, completeBinding);
        }

        /// <summary>
        /// Saves last selected page in each category.
        /// </summary>
        private void _SaveSelectedPageName()
        {
            int i = _navigationTree.NaviationTreeRoot.Children.IndexOf(_currentCategory);
            if (i >= 0)
                _selectedPagesRepository[i] = _currentPageItem;
        }

        /// <summary>
        /// Inits saved pages.
        /// </summary>
        private void _InitSavedPagesRepository()
        {
            for (int i = 0; i < _navigationTree.NaviationTreeRoot.Children.Count; i++)
                _selectedPagesRepository.Add(null);
        }

        /// <summary>
        /// Method starts column animation
        /// </summary>
        /// <param name="column"></param>
        /// <param name="startSize"></param>
        /// <param name="endSize"></param>
        /// <param name="duration"></param>
        private void _StartColumnAnimation(ColumnDefinition column, double startSize, double endSize, int duration)
        {
            GridLengthAnimation sizeAnimation = new GridLengthAnimation();
            sizeAnimation.From = new GridLength(startSize, GridUnitType.Pixel);
            sizeAnimation.To = new GridLength(endSize, GridUnitType.Pixel);
            sizeAnimation.Duration = new TimeSpan(0, 0, 0, 0, duration);
            column.BeginAnimation(ColumnDefinition.WidthProperty, sizeAnimation);
        }

        /// <summary>
        /// Method starts row animation
        /// </summary>
        /// <param name="row"></param>
        /// <param name="startSize"></param>
        /// <param name="endSize"></param>
        /// <param name="duration"></param>
        private void _StartRowAnimation(RowDefinition row, double startSize, double endSize, int duration)
        {
            GridLengthAnimation sizeAnimation = new GridLengthAnimation();
            sizeAnimation.From = new GridLength(startSize, GridUnitType.Pixel);
            sizeAnimation.To = new GridLength(endSize, GridUnitType.Pixel);
            sizeAnimation.Duration = new TimeSpan(0, 0, 0, 0, duration);
            row.BeginAnimation(RowDefinition.HeightProperty, sizeAnimation);
        }

        #endregion

        #region Protected event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when called Frame.Navigate()
        /// and the content that is being navigated to has been found,
        /// and is available from the Content property
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageFrame_Navigated(object sender, NavigationEventArgs e)
        {
            PageFrame.Navigated -= PageFrame_Navigated;

            AppPages.PageBase currentPage = (AppPages.PageBase)e.Content;

            foreach (INavigationItem rootItem in _navigationTree.NaviationTreeRoot.Children)
            {
                if (rootItem.GetType().Equals(typeof(PageItem)))
                {
                    if (string.Format("{0}Page", rootItem.Name).Equals(currentPage.GetType().Name))
                    {
                        Navigate(rootItem.Name);
                        break;
                    }
                }
                else
                {
                    foreach (PageItem item in rootItem.Children)
                    {
                        if (item.PageType.Equals(currentPage.GetType()))
                        {
                            _Navigate(item);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raises when _Navigate(pageitem) method called
        /// </summary>
        private void _NotifyNavigationCalled()
        {
            if (NavigationCalled != null)
                NavigationCalled(null, EventArgs.Empty);
        }

        private void help_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_IsTopicPresent())
            {
                HelpTopics topics = App.Current.HelpTopics;
                HelpTopic topic = topics.GetTopic(AppPages.PagePaths.MainPage);
                Debug.Assert(null != topic);

                HelpLinkCommand cmd = new HelpLinkCommand(topic.Path, topic.Key);
                cmd.Execute(null);
            }
        }

        private void preferences_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_preferencesCategory != _currentCategory)
                _NavigateToCategory(_preferencesCategory);
        }

        private void about_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.Owner = App.Current.MainWindow;
            aboutDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            aboutDialog.ShowDialog();
        }

        /// <summary>
        /// Called when application initualized.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_ApplicationInitialized(object sender, EventArgs e)
        {
            _InitNavigationTree();

            // add extensional pages to apllication structure
            _InsertExtensionalPages();

            _LoadPagesSettings();
        }

        /// <summary>
        /// Called when application closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _SaveSettings();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Called when skin is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkinLoader_SkinChange(object sender, RoutedEventArgs e)
        {
            foreach (ToggleButton button in CategoriesButtons.ContentItems)
                button.Style = Application.Current.Resources["CategoriesToggleButtonStyle"] as Style;
        }

        /// <summary>
        /// Called when main window is loaded.
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // load layout
                _LoadLayout();

                foreach (PageItem pageItem in _navigationTree.NaviationTreeRoot.Children[0].Children)
                {
                    if (pageItem.Page == null)
                    {
                        AppPages.Page newPage = (AppPages.Page)Activator.CreateInstance(pageItem.PageType);
                        newPage.Initialize(App.Current);
                        pageItem.Page = newPage;
                    }
                }
                _currentPage = ((PageItem)_navigationTree.NaviationTreeRoot.Children[0].Children[0]).Page;
                _NavigateHome();
            }
            catch (Exception ex)
            {
                // unable to load window layout
                App.Current.Messenger.AddError(ex.Message);
                // close the application
                Application.Current.Shutdown();
            }

            NavigationPaneContainer.Width = new GridLength((double)App.Current.FindResource("DefaultNavigationPaneWidth"));
        }

        /// <summary>
        /// Called when current navigation pane page is changed.
        /// </summary>
        private void OnCurrentPageChanged(object sender, RoutedEventArgs e)
        {
            PageItem selectedPageItem = navigationPane.SelectedPage.Tag as PageItem;

            Debug.Assert(selectedPageItem != null);
            _Navigate(selectedPageItem);
        }

        /// <summary>
        /// Called when user clicks on category button.
        /// </summary>
        private void OnCategoryButtonClick(object sender, RoutedEventArgs e)
        {
            ToggleButton clickedButton = sender as ToggleButton;
            if ((CategoryItem)clickedButton.Tag == _currentCategory)
                clickedButton.IsChecked = true;
            else
                _NavigateToCategory(clickedButton.Tag as INavigationItem);
        }

        /// <summary>
        /// Called when task section clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExpanderClicked(object sender, RoutedEventArgs e)
        {
            ExpanderControl control = e.OriginalSource as ExpanderControl;
        }

        /// <summary>
        /// Called when navigation pane is collapsed.
        /// </summary>
        private void OnCollapseNavigationPane(object sender, RoutedEventArgs e)
        {
            _StartColumnAnimation(NavigationPaneContainer, navigationPane.ActualWidth, (double)App.Current.FindResource("MinNavigationPaneWidth"), (int)App.Current.FindResource("NavigationPaneAnimationDuration"));
        }

        /// <summary>
        /// Called when navigation pane is expanded.
        /// </summary>
        private void OnExpandNavigationPane(object sender, RoutedEventArgs e)
        {
            _StartColumnAnimation(NavigationPaneContainer, (double)App.Current.FindResource("MinNavigationPaneWidth"), navigationPane.MaxWidth, (int)App.Current.FindResource("NavigationPaneAnimationDuration"));
        }

        private void mainVerticalSplitter_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // resize navigation pane when user drags vertical splitter
            double newWidth = navigationPane.ActualWidth + e.HorizontalChange;
            double minWidth = (double)App.Current.FindResource("MinNavigationPaneWidth");
            
            if (newWidth < minWidth)
            {
                newWidth = minWidth;
                navigationPane.MaxWidth = (double)App.Current.FindResource("DefaultNavigationPaneWidth");
            }
            else
                navigationPane.MaxWidth = newWidth;

            _StartColumnAnimation(NavigationPaneContainer, navigationPane.ActualWidth, newWidth, 0);
        }

        private void mainHorizontalSplitter_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // resize message window when user drags horisontal splitter
            double newHeight = MessageWindow.ActualHeight - e.VerticalChange;
            double minHeight = (double)App.Current.FindResource("MinMessageWindowHeight");

            if (newHeight < minHeight)
                newHeight = minHeight;

            _StartRowAnimation(messageWindowGrid, MessageWindow.ActualHeight, newHeight, 0);
        }

        /// <summary>
        /// Selects stated category (tab)
        /// </summary>
        /// <param name="navigationItem"></param>
        private void _NavigateToCategory(INavigationItem navigationItem)
        {
            Debug.Assert(navigationItem.Type == NavigationItemType.Category);

            int index = _navigationTree.NaviationTreeRoot.Children.IndexOf(navigationItem);
            if (_selectedPagesRepository[index] != null && _selectedPagesRepository[index].Page.IsAllowed)
                _Navigate(_selectedPagesRepository[index]);
            else
            {
                foreach (PageItem page in _navigationTree.NaviationTreeRoot.Children[index].Children)
                {
                    if (page.Page.IsAllowed)
                    {
                        _Navigate(page);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Hides message window
        /// </summary>
        /// <param name="delayCollapsing">If 'true' then collapsing will be invoked, otherwise no.</param>
        private void _HideMessageWindow(bool delayCollapsing)
        {
            messageWindowGrid.MinHeight = 0;
            _messageWindowHeight = messageWindowGrid.Height.Value;

            _StartRowAnimation(messageWindowGrid, messageWindow.ActualHeight, 0, (int)App.Current.FindResource("MessageWindowAnimationDuration"));

            // Check collapsing flag.
            if (delayCollapsing)
            {
                // postpone changing message window visibility
                this.Dispatcher.BeginInvoke(new HideMessageWindowDelegate(_CollapseMessageWindow),
                    System.Windows.Threading.DispatcherPriority.SystemIdle);
            }
            else
                _CollapseMessageWindow();
        }

        /// <summary>
        /// Shows message window
        /// </summary>
        private void _ShowMessageWindow()
        {
            double messageWindowMinHeight = (double)App.Current.FindResource("MinMessageWindowHeight");

            _messageWindowHeight = Math.Max(_messageWindowHeight, messageWindowMinHeight);
            _StartRowAnimation(messageWindowGrid, 0, _messageWindowHeight, (int)App.Current.FindResource("MessageWindowAnimationDuration"));
            messageWindow.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Delegate for hide message window
        /// </summary>
        private delegate void HideMessageWindowDelegate();

        /// <summary>
        /// Uses to postpopne message window visibility change to correct showing animation.
        /// </summary>
        private void _CollapseMessageWindow()
        {
            MessageWindow.Visibility = Visibility.Collapsed;
        }


        private bool _IsTopicPresent()
        {
            bool isPresent = false;
            HelpTopics topics = App.Current.HelpTopics;
            if (null != topics)
            {
                HelpTopic topic = topics.GetTopic(AppPages.PagePaths.MainPage);
                isPresent = ((null != topic) &&
                             ((!string.IsNullOrEmpty(topic.Path)) || (!string.IsNullOrEmpty(topic.Key))));
            }

            return isPresent;
        }

        /// <summary>
        /// Add custom page to category
        /// </summary>
        private void _AddCustomPage(Assembly pluginAssembly, Type pluginType, INavigationItem category)
        {
            try
            {
                PageItem newPage = new PageItem();
                newPage.PageType = pluginAssembly.GetType(pluginType.ToString());
                newPage.Page = (AppPages.Page)Activator.CreateInstance(newPage.PageType);
                newPage.Page.Initialize(App.Current);

                INavigationItem foundItem = null;
                if (category.FindItem(newPage.Page.Name, out foundItem))
                {   // not unique page - add warning
                    string messageFormat = (string)App.Current.FindResource("UnableLoadNotUniqueCustomPage");
                    string path = string.Format(FULL_PAGE_NAME_FORMAT, category.Name, newPage.Page.Name);
                    string message = string.Format(messageFormat, path, Path.GetFileName(pluginAssembly.Location));
                    App.Current.Messenger.AddWarning(message);
                }
                else
                {   // add new page to the parent
                    newPage.Parent = category;
                    category.AddChild(newPage);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        /// <summary>
        /// Get all custom pages from loaded extensions
        /// </summary>
        private void _InsertExtensionalPages()
        {
            ICollection<string> assemblyFiles = CommonHelpers.GetAssembliesFiles();
            foreach (string assemblyPath in assemblyFiles)
            {
                try
                {
                    Assembly pluginAssembly = Assembly.LoadFrom(assemblyPath);
                    foreach (Type pluginType in pluginAssembly.GetTypes())
                    {
                        if (!pluginType.IsPublic || pluginType.IsAbstract ||
                            ((typeof(ESRI.ArcLogistics.App.Pages.Page) != pluginType.BaseType) &&
                             (typeof(ESRI.ArcLogistics.App.Pages.PageBase) != pluginType.BaseType)))
                            continue; // NOTE: skip this type

                        if (Attribute.IsDefined(pluginType, typeof(AppPages.PagePlugInAttribute)))
                        { // NOTE: specifying this attribute is obligatory 
                            AppPages.PagePlugInAttribute attribute = (AppPages.PagePlugInAttribute)Attribute.GetCustomAttribute(pluginType, typeof(AppPages.PagePlugInAttribute));

                            // find category
                            // NOTE: not supported nested categories
                            foreach (INavigationItem item in NavigationTree.NaviationTreeRoot.Children)
                            {
                                if (item.Type != NavigationItemType.Category)
                                    continue; // NOTE: ignore

                                if (item.Name.Equals(attribute.Category, StringComparison.OrdinalIgnoreCase))
                                {
                                    _AddCustomPage(pluginAssembly, pluginType, item);
                                    break; // NOTE: exit page added
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        private void _InitPageSettings(IDictionary<string, string> settingsMap, string categoryName,
                                       AppPages.Page page)
        {
            Debug.Assert(null != page);

            try
            {
                AppPages.ISupportSettings settings = page as AppPages.ISupportSettings;
                if (null != settings)
                {
                    string key = string.IsNullOrEmpty(categoryName) ? page.Name : string.Format(FULL_PAGE_NAME_FORMAT, categoryName, page.Name);
                    if (settingsMap.ContainsKey(key))
                        settings.LoadUserSettings(settingsMap[key]);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void _LoadPagesSettings()
        {
            string loadedSettings = Settings.Default.ActionPanelSettings;
            if (!string.IsNullOrEmpty(loadedSettings))
            {
                // parse settings
                Dictionary<string, string> map = new Dictionary<string, string>();
                // split by groups
                string[] groupSplitters = new string[] { SPLITTER_GROUP };
                string[] groups = loadedSettings.Split(new string[] { SPLITTER_GROUPS }, StringSplitOptions.RemoveEmptyEntries);
                for (int groupNum = 0; groupNum < groups.Length; ++groupNum)
                {
                    // split by group info
                    string[] group = groups[groupNum].Split(groupSplitters, StringSplitOptions.None);
                    Debug.Assert(2 == group.Length);

                    if (!map.ContainsKey(group[0])) // NOTE: store only for first unique name
                        map.Add(group[0], group[1]);
                }

                // set stored setting to pages
                foreach (INavigationItem navigationItem in _navigationTree.NaviationTreeRoot.Children)
                {
                    if (navigationItem.Type == NavigationItemType.Category)
                    {
                        CategoryItem category = (CategoryItem)navigationItem;
                        foreach (PageItem pageItem in navigationItem.Children)
                            _InitPageSettings(map, category.Name, pageItem.Page);
                    }
                    else
                    {
                        AppPages.Page page = ((PageItem)navigationItem).Page;
                        _InitPageSettings(map, null, page);
                    }
                }
            }
        }

        private void _SavePageSetting(string categoryName, AppPages.Page page, ref StringCollection names,
                                      ref StringBuilder settingsStorage)
        {
            // NOTE: store only unique named pages
            Debug.Assert(null != page);
            Debug.Assert(null != names);
            Debug.Assert(null != settingsStorage);

            AppPages.ISupportSettings settings = page as AppPages.ISupportSettings;
            if (null != settings)
            {
                string name = string.IsNullOrEmpty(categoryName) ? page.Name : string.Format(FULL_PAGE_NAME_FORMAT, categoryName, page.Name);
                if (!string.IsNullOrEmpty(name))
                {
                    string storeSettings = null;
                    try
                    {
                        storeSettings = settings.SaveUserSettings();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }

                    if (!string.IsNullOrEmpty(storeSettings))
                    {
                        if (!names.Contains(name))
                        {
                            if (!string.IsNullOrEmpty(settingsStorage.ToString()))
                                settingsStorage.Append(SPLITTER_GROUPS);

                            settingsStorage.AppendFormat("{0}{1}{2}", name, SPLITTER_GROUP, storeSettings);
                            names.Add(name);
                        }
                    }
                }
            }
        }

        private void _SavePagesSettings()
        {
            StringBuilder sb = new StringBuilder();
            StringCollection names = new StringCollection();

            foreach (INavigationItem navigationItem in _navigationTree.NaviationTreeRoot.Children)
            {
                if (navigationItem.Type == NavigationItemType.Category)
                {
                    CategoryItem category = (CategoryItem)navigationItem;
                    foreach (PageItem pageItem in navigationItem.Children)
                        _SavePageSetting(category.Name, pageItem.Page, ref names, ref sb);
                }
                else
                {
                    AppPages.Page page = ((PageItem)navigationItem).Page;
                    _SavePageSetting(null, page, ref names, ref sb);
                }
            }

            string saveSettings = sb.ToString().Trim();
            if (!string.IsNullOrEmpty(saveSettings))
                Settings.Default.ActionPanelSettings = saveSettings;
        }

        /// <summary>
        /// React on key down.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Key down event args.</param>
        private void _KeyDown(object sender, KeyEventArgs e)
        {
            // If command manager isn't inited - do not try to execute commands.
            if (App.Current.CommandManager == null)
                return;

            App.Current.CommandManager.ExecuteCommand(CurrentPage, e);
        }

        /// <summary>
        /// Navigates to the application home page.
        /// </summary>
        private void _NavigateHome()
        {
            var homePage = (PageItem)_navigationTree.NaviationTreeRoot.Children[0].Children[0];
            _Navigate(homePage);
        }

        /// <summary>
        /// Handles "CanExecute" event for the browse home command.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments of the event.</param>
        private void _ContinueWorkingCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute =
                _navigationTree != null &&
                (this.CurrentPage == null ||
                !this.CurrentPage.DoesSupportCompleteStatus ||
                this.CurrentPage.IsComplete);
            e.Handled = true;
        }

        /// <summary>
        /// Handles browse home command execution.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments of the event.</param>
        private void _ContinueWorkingExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _ContinueWorking();
            e.Handled = true;
        }

        /// <summary>
        /// Continues working with the application by navigation to the specified
        /// page.
        /// </summary>
        private void _ContinueWorking(string pagePath)
        {
            if (string.IsNullOrEmpty(pagePath))
            {
                _NavigateToLastSavedPage();
            }
            else
            {
                this.Navigate(pagePath);
            }

            if (_startFleetSetupWizardOnContinue)
            {
                CommonHelpers.StartFleetSetupWizard();
                _startFleetSetupWizardOnContinue = false;
            }
        }

        /// <summary>
        /// Continues working with the application by navigation to an appropriate
        /// page.
        /// </summary>
        private void _ContinueWorking()
        {
            _ContinueWorking(null);
        }
        #endregion

        #region Private Variables
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // const for pages settings store
        private const string SPLITTER_GROUPS = "#$ALSettingGrpsSplit$#";
        private const string SPLITTER_GROUP = "#$ALSettingGrpSplit$#";
        private const string FULL_PAGE_NAME_FORMAT = @"{0}\{1}";

        private const string HOME_CATEGORY_NAME = "Home";

        /// <summary>
        /// Tasl panel content builder.
        /// </summary>
        private TaskPanelContentBuilder _taskPanelContentBuilder = new TaskPanelContentBuilder();

        /// <summary>
        /// Collection of types of widgets which should be collapsed.
        /// </summary>
        private List<Type> _collapsedWidgets = new List<Type>();

        /// <summary>
        /// Current selected page
        /// </summary>
        private AppPages.Page _currentPage = null;

        /// <summary>
        /// Name of last selected page.
        /// </summary>
        private string _lastPageName = null;

        /// <summary>
        /// List of all selected pages.
        /// </summary>
        private List<PageItem> _selectedPagesRepository = new List<PageItem>();

        /// <summary>
        /// Full navigation tree.
        /// </summary>
        private NavigationTree _navigationTree = null;

        /// <summary>
        /// Current navigation category.
        /// </summary>
        private INavigationItem _currentCategory = null;

        /// <summary>
        /// Preferences navigation category.
        /// </summary>
        /// <remarks>For special navigate routine</remarks>
        private INavigationItem _preferencesCategory = null;

        /// <summary>
        /// Is help visible flag.
        /// </summary>
        private bool _isHelpVisible = true;

        /// <summary>
        /// Is GUI locked flag.
        /// </summary>
        private bool _isLocked = false;

        /// <summary>
        /// Current navigation page.
        /// </summary>
        private PageItem _currentPageItem = null;

        private Hashtable _widgetsHash = new Hashtable();

        /// <summary>
        /// Message window users store height
        /// </summary>
        private double _messageWindowHeight;

        /// <summary>
        /// A value indicating if the fleet setup wizard should be started when
        /// continue working was requested.
        /// </summary>
        private bool _startFleetSetupWizardOnContinue;
        #endregion
    }
}

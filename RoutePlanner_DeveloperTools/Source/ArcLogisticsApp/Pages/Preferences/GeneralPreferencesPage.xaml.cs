using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.OrderSymbology;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.Archiving;
using ESRI.ArcLogistics.Services;

using Xceed.Wpf.DataGrid;

using WinInput = System.Windows.Input;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for GeneralPreferencesPage.xaml
    /// </summary>
    internal partial class GeneralPreferencesPage : PageBase
    {
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string PAGE_NAME = "General";

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>GeneralPreferencesPage</c>.
        /// </summary>
        public GeneralPreferencesPage()
        {
            InitializeComponent();
            _InitPageContent();

            IsRequired = true;
            IsAllowed = true;
            CanBeLeft = true;
            DoesSupportCompleteStatus = false;
        }

        #endregion Constructors

        #region Page overridden members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets name of the page.
        /// </summary>
        public override string Name
        {
            get { return PAGE_NAME; }
        }

        /// <summary>
        /// Gets title of the page.
        /// </summary>
        public override string Title
        {
            get { return App.Current.FindString("GeneralPreferencesPageCaption"); }
        }

        /// <summary>
        /// Gets icon of the page.
        /// </summary>
        public override TileBrush Icon
        {
            get { return (ImageBrush)App.Current.FindResource("PreferencesBrush"); }
        }

        #endregion Page overridden members

        #region Public PageBase overridden members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets help topic of the page.
        /// </summary>
        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.GeneralPreferencesPagePath); }
        }

        /// <summary>
        /// Gets PageCommandsCategoryName (null).
        /// </summary>
        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets flag which defines where we can left this page or not.
        /// Page can't be left if it contains errors.
        /// </summary>
        public override bool CanBeLeft
        {
            get
            {
                return base.CanBeLeft &&
                       _customOrderPropertiesControl.DataIsValid();
            }

            protected internal set
            {
                base.CanBeLeft = value;
            }
        }

        #endregion PageBase overridden members

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Inits startup state of radio buttons.
        /// </summary>
        protected void _InitPageContent()
        {
            App appCurrent = App.Current;
            appCurrent.Exit += new ExitEventHandler(_CurrentExit);
            appCurrent.ProjectClosed += new EventHandler(_AppProjectClosed);
            this.Loaded += new RoutedEventHandler(_PageLoaded);
            this.Unloaded += new RoutedEventHandler(_PageUnloaded);

            textBoxPeriod.KeyDown += new WinInput.KeyEventHandler(_NumericTextBoxKeyDown);
            textBoxTimeDomain.KeyDown += new WinInput.KeyEventHandler(_NumericTextBoxKeyDown);

            Settings settings = Settings.Default;

            ShowQuickHelpButton.IsChecked = appCurrent.MainWindow.IsHelpVisible;
            ShowQuickHelpButton.Click += new RoutedEventHandler(_ShowQuickHelpButtonClick);

            AllwaysAskBeforeDeletingButton.Click +=
                new RoutedEventHandler(_AllwaysAskBeforeDeletingButtonClick);
            FleetSetupWizardButton.Click +=
                new RoutedEventHandler(_FleetSetupWizardButtonClick);
            RouteLocationsVirtualWarningButton.Click +=
                new RoutedEventHandler(_RouteLocationsVirtualWarningButtonClick);

            // Initialize projects path text box.
            ProjectsPathEdit.Text = _GetProjectsPath(settings);

            // Initialize plug-ins path text box.
            PlugInsPathEdit.Text = _GetPlugInsPath(settings);

            ProjectsPathEdit.TextChanged += _ProjectsPathEditTextChanged;
            PlugInsPathEdit.TextChanged += _PlugInsPathEditTextChanged;

            _proxyConfigurationService = appCurrent.Container.Resolve<IProxyConfigurationService>();
            this.ProxySettings.DataContext = _proxyConfigurationService.Settings;

            _InitUpdateValues();
        }        

        /// <summary>
        /// Method gets projects path selected by user,
        /// or default path, if nothing selected.
        /// </summary>
        /// <param name="settings">Settings.</param>
        private string _GetProjectsPath(Settings settings)
        {
            Debug.Assert(settings != null);

            string resultPath = string.Empty;

            if (!string.IsNullOrEmpty(settings.UsersProjectsFolder))
            {
                resultPath = settings.UsersProjectsFolder;
            }
            else
            {
                resultPath = App.Current.ProjectCatalog.FolderPath;
            }

            return resultPath;
        }

        /// <summary>
        /// Method gets plug-ins path selected by user,
        /// or default path, if nothing selected.
        /// </summary>
        /// <param name="settings">Settings.</param>
        private string _GetPlugInsPath(Settings settings)
        {
            Debug.Assert(settings != null);

            string resultPath = string.Empty;

            if (!string.IsNullOrEmpty(settings.UsersPlugInsFolder))
                resultPath = settings.UsersPlugInsFolder;
            else
                resultPath = AppDomain.CurrentDomain.BaseDirectory;

            return resultPath;
        }

        /// <summary>
        /// Method creates instance of UpdateSettingsHelper and sets binding between user settings and UI elements
        /// </summary>
        private void _InitUpdateValues()
        {
            _updateHelper = new UpdateSettingsHelper();

            // init check boxes state
            automaticUpdate.IsChecked = _updateHelper.CheckForUpdate;
            silentUpdate.IsChecked = _updateHelper.SilenceUpdate;

            // enable/disable check boxes
            automaticUpdate.IsEnabled = (_updateHelper.UserEditable);
            silentUpdate.IsEnabled = (_updateHelper.UserEditable && (bool)automaticUpdate.IsChecked);

            // add handlers to automaticUpdate check box event for disable silentUpdate when necessary
            automaticUpdate.Click += new RoutedEventHandler(_AutomaticUpdateClick);
            silentUpdate.Click += new RoutedEventHandler(_SilentUpdateClick);
        }

        /// <summary>
        /// Checks if given folder's path is exact path.
        /// </summary>
        /// <param name="folder">Folder path.</param>
        /// <returns>True - if path is exact, otherwise - false.</returns>
        private bool _IsExactPath(string folder)
        {
            bool isExactPath = false;
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                isExactPath = directoryInfo.FullName == folder;
            }
            catch
            {
            }

            return isExactPath;
        }

        /// <summary>
        /// Updates archive settings.
        /// </summary>
        private void _UpdateArchiveSettings()
        {
            checkBoxRunAuto.Click -= _CheckBoxRunAutoClick;
            textBoxPeriod.TextChanged -= _PeriodTextChanged;
            textBoxTimeDomain.TextChanged -= _TimeDomainTextChanged;

            IProject project = App.Current.Project;

            bool setAsDefault = true;
            if (null != project)
            {
                if ((null != project.ProjectArchivingSettings) && !project.ProjectArchivingSettings.IsArchive)
                    setAsDefault = false;
            }

            // update archiving settings
            if (setAsDefault)
            {   // to default state
                textArchivingSettings.Text = App.Current.FindString("ArchivingSettingsString");

                foreach (UIElement child in stackPanelArchivingSettings.Children)
                    child.IsEnabled = false;

                checkBoxRunAuto.IsChecked = false;
                textBoxPeriod.Text = string.Empty;
                textBoxTimeDomain.Text = string.Empty;
            }
            else
            {
                textArchivingSettings.Text = App.Current.GetString("ArchivingSettingsFormatString",
                                                                   project.Name);

                foreach (UIElement child in stackPanelArchivingSettings.Children)
                    child.IsEnabled = true;

                System.Diagnostics.Debug.Assert(null != project.ProjectArchivingSettings);

                ProjectArchivingSettings settings = project.ProjectArchivingSettings;
                checkBoxRunAuto.IsChecked = settings.IsAutoArchivingEnabled;
                textBoxPeriod.Text = settings.AutoArchivingPeriod.ToString();
                textBoxTimeDomain.Text = settings.TimeDomain.ToString();

                bool isChecked = (true == checkBoxRunAuto.IsChecked);
                labelMonths.IsEnabled = isChecked;
                textBoxPeriod.IsEnabled = isChecked;
            }

            checkBoxRunAuto.Click += new RoutedEventHandler(_CheckBoxRunAutoClick);
            textBoxPeriod.TextChanged += new TextChangedEventHandler(_PeriodTextChanged);
            textBoxTimeDomain.TextChanged += new TextChangedEventHandler(_TimeDomainTextChanged);
        }

        /// <summary>
        /// Converts string to integer.
        /// </summary>
        /// <param name="text">String to convert.</param>
        /// <returns>Converted integer value if string was converted successfully, otherwise - null.</returns>
        private int? _ConvertStringToInt(string text)
        {
            int? value = null;
            try
            {
                int val = int.Parse(text);
                if (0 < val) // NOTE: 0 ignore value
                    value = val;
            }
            catch
            {
            }

            return value;
        }

        /// <summary>
        /// Saves changes made on the preferences page to persistent storages.
        /// </summary>
        private void _SaveChanges()
        {
            _updateHelper.StoreSettings();

            Settings.Default.Save();

            _proxyConfigurationService.Update();
        }

        /// <summary>
        /// Initializes custom order properties control.
        /// </summary>
        private void _InitializeCustomOrderPropertiesControl()
        {
            // Get current project.
            Project currentProject = App.Current.Project;

            if (currentProject != null)
            {
                // Get order custom properties info.
                OrderCustomPropertiesInfo customPropertiesInfo = currentProject.OrderCustomPropertiesInfo;
                Debug.Assert(customPropertiesInfo != null);

                // Load custom order properties to the control's collection.
                _customOrderPropertiesControl.LoadCustomOrderProperties(customPropertiesInfo);

                // Enable custom order properties control.
                _customOrderPropertiesControl.IsEnabled = true;
            }
            else
            {
                // Disable custom order properties control.
                _customOrderPropertiesControl.IsEnabled = false;
            }
        }

        /// <summary>
        /// Saves custom order properties to persistent storage and updates default settings.
        /// </summary>
        private void _SaveCustomOrderProperties()
        {
            Debug.Assert(_customOrderPropertiesControl.DataIsValid());

            // If list of custom order properties was modified.
            if (_customOrderPropertiesControl.CustomOrderPropertiesModified())
            {
                // Update custom order properties info for current project and
                // update defaults settings for new projects.
                App.Current.UpdateProjectCustomOrderPropertiesInfo(
                    _customOrderPropertiesControl.GetOrderCustomPropertiesInfo(), _currentProjectConfiguration);
            }
        }

        #endregion Private methods

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handler for the NavigationCalled event which occurs when navigation called.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _GeneralPreferencesPageNavigationCalled(object sender, EventArgs e)
        {
            // Check validity of custom order properties.
            if (!CanBeLeft)
            {
                // If there are validation errors - show them.
                _customOrderPropertiesControl.ShowErrorMessagesInMessageWindow();
            }
        }

        /// <summary>
        /// Handler for the page's Loaded event.
        /// </summary>
        /// <param name="sender">The object where the event handler is attached.</param>
        /// <param name="e">Event data.</param>
        private void _PageLoaded(object sender, RoutedEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).NavigationCalled +=
                new EventHandler(_GeneralPreferencesPageNavigationCalled);

            App.Current.MainWindow.StatusBar.SetStatus(this, null);

            Settings settings = Settings.Default;
            AllwaysAskBeforeDeletingButton.IsChecked =
                settings.IsAllwaysAskBeforeDeletingEnabled;
            FleetSetupWizardButton.IsChecked =
                settings.IsAutomaticallyShowFleetWizardEnabled;
            RouteLocationsVirtualWarningButton.IsChecked =
                settings.WarnUserIfVirtualLocationDetected;

            // Init breaks apply radio button group
            if (settings.IsAlwaysAskAboutApplyingBreaksToDefaultRoutes)
                IsAlwaysAksAboutApplyingBreaks.IsChecked = true;
            else if (settings.ApplyBreaksChangesToDefaultRoutes)
                ApplyBreaks.IsChecked = true;
            else
                DontApplyBreaks.IsChecked = true;

            // Init routes apply radio button group
            if (settings.IsAlwaysAskAboutApplyingDefaultRoutesToSheduledRoutes)
                AlwaysAskAboutApplyingRoutes.IsChecked = true;
            else if (settings.ApplyDefaultRoutesToSheduledRoutes)
                ApplyDefaultRoutesToScheduledRoutes.IsChecked = true;
            else
                DontApplyDefaultRoutesToScheduledRoutes.IsChecked = true;

            _UpdateArchiveSettings();

            // Initialize custom order properties control.
            _InitializeCustomOrderPropertiesControl();

            // Get configuration of the current project.
            _currentProjectConfiguration = App.Current.GetCurrentProjectConfiguration();
        }

        /// <summary>
        /// Create error log on desktop.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _LogButtonClick(object sender, RoutedEventArgs e)
        {
            // Array of exceptions wich user can handle.
            var copyingExceptions = new[]
            {
                typeof(IOException),
                typeof(UnauthorizedAccessException),
                typeof(DirectoryNotFoundException),
                typeof(PathTooLongException),
                typeof(SecurityException)
            };

            try
            {
                // Get path to log file.
                string logPath = Path.Combine(DataFolder.Path, App.LOGS_FOLDER);
                string logFilePath = Path.Combine(logPath, Settings.Default.LogFileName);
                FileInfo logInfo = new FileInfo(logFilePath);

                // Get desktop folder name.
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                // Compute full log file name. 
                // It must be like ...\Desktop\ArcLogistics Log 1_12_2011 3_50_22 PM.txt
                var dateTimeFormat =
                    CultureInfo.InstalledUICulture.DateTimeFormat.Clone() as DateTimeFormatInfo;
                dateTimeFormat.DateSeparator = LOG_DATE_TIME_SEPARATOR;
                dateTimeFormat.TimeSeparator = LOG_DATE_TIME_SEPARATOR;
                string time = DateTime.Now.ToString(dateTimeFormat);
                string fileName =
                    Path.Combine(desktopPath, LOG_FILE_NAME + time + logInfo.Extension);

                // Copy log file to desktop.
                logInfo.CopyTo(fileName, true);

                // Show message about succefull coping in message window and status bar.
                var strFmt = (string)App.Current.FindResource("LogCopied");
                var message = string.Format(strFmt, Path.GetFileName(fileName));
                _app.Messenger.AddInfo(message);
                _app.MainWindow.StatusBar.SetStatus(this, message);
            }

            // If there are some exceptions wich user can handle, show it to user.
            catch (Exception ex)
            {
                if (!copyingExceptions.Any(type => type.IsInstanceOfType(ex)))
                {
                    throw;
                }
                App.Current.Messenger.AddError(ex.Message);
            }
        }

        /// <summary>
        /// Handler for the Exit event of the current application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _CurrentExit(object sender, ExitEventArgs e)
        {
            _SaveChanges();

            // Save collection of custom order properties.
            if (_currentProjectConfiguration != null && _customOrderPropertiesControl.DataIsValid())
                _SaveCustomOrderProperties();
        }

        /// <summary>
        /// Handler for the page's Unloaded event.
        /// </summary>
        /// <param name="sender">The object where the event handler is attached.</param>
        /// <param name="e">Event data.</param>
        private void _PageUnloaded(object sender, RoutedEventArgs e)
        {
            // Unsubscribe from events.
            ((MainWindow)App.Current.MainWindow).NavigationCalled -= _GeneralPreferencesPageNavigationCalled;

            _SaveChanges();

            // Save collection of custom order properties.
            if (_currentProjectConfiguration != null)
            {
                // If this event occured collection of custom order properties should be valid.
                Debug.Assert(_customOrderPropertiesControl.DataIsValid());

                _SaveCustomOrderProperties();
            }

            _currentProjectConfiguration = null;
        }

        /// <summary>
        /// Handler for the ProjectClosed event of the current application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _AppProjectClosed(object sender, EventArgs e)
        {
            _SaveChanges();
        }

        /// <summary>
        /// Handler for the TextChanged event of the ProjectsPathEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _ProjectsPathEditTextChanged(object sender, TextChangedEventArgs e)
        {
            var source = sender as System.Windows.Controls.TextBox;
            if (string.IsNullOrEmpty(source.Text))
                Settings.Default.UsersProjectsFolder = string.Empty;
            else if (_IsExactPath(source.Text) && Directory.Exists(source.Text))
                Settings.Default.UsersProjectsFolder = source.Text;
        }

        /// <summary>
        /// Handler for the Click event of the ProjectsFolderButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _ProjectsFolderButtonClick(object sender, RoutedEventArgs e)
        {
            using (var folderDlg = new FolderBrowserDialog())
            {
                string selectedPath = Settings.Default.UsersProjectsFolder;
                if (string.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
                    folderDlg.RootFolder = Environment.SpecialFolder.MyComputer;
                else
                    folderDlg.SelectedPath = selectedPath;

                if (DialogResult.OK == folderDlg.ShowDialog())
                {
                    if (Directory.Exists(folderDlg.SelectedPath))
                        ProjectsPathEdit.Text = folderDlg.SelectedPath;
                }
            }
        }

        /// <summary>
        /// Handler for the TextChanged event of the PlugInsPathEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _PlugInsPathEditTextChanged(object sender, TextChangedEventArgs e)
        {
            var source = sender as System.Windows.Controls.TextBox;
            if (string.IsNullOrEmpty(source.Text))
                Settings.Default.UsersPlugInsFolder = string.Empty;
            else if (_IsExactPath(source.Text) && Directory.Exists(source.Text))
                Settings.Default.UsersPlugInsFolder = source.Text;
        }

        /// <summary>
        /// Handler for the Click event of the PlugInsFolderButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _PlugInsFolderButtonClick(object sender, RoutedEventArgs e)
        {
            using (var folderDlg = new FolderBrowserDialog())
            {
                string selectedPath = Settings.Default.UsersPlugInsFolder;
                if (string.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
                    folderDlg.RootFolder = Environment.SpecialFolder.MyComputer;
                else
                    folderDlg.SelectedPath = selectedPath;

                if (DialogResult.OK == folderDlg.ShowDialog())
                {
                    if (Directory.Exists(folderDlg.SelectedPath))
                        PlugInsPathEdit.Text = folderDlg.SelectedPath;
                }
            }
        }

        /// <summary>
        /// Handler for the Click event of the ShowQuickHelpButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _ShowQuickHelpButtonClick(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.IsHelpVisible = (true == ShowQuickHelpButton.IsChecked);
        }

        /// <summary>
        /// Handler for the Click event of the automaticUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _AutomaticUpdateClick(object sender, RoutedEventArgs e)
        {
            bool isAutomaticUpdate = (true == (sender as System.Windows.Controls.CheckBox).IsChecked);
            silentUpdate.IsEnabled = isAutomaticUpdate;
            _updateHelper.CheckForUpdate = isAutomaticUpdate;
        }

        /// <summary>
        /// Handler for the Click event of the silentUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _SilentUpdateClick(object sender, RoutedEventArgs e)
        {
            _updateHelper.SilenceUpdate = (true == (sender as System.Windows.Controls.CheckBox).IsChecked);
        }

        /// <summary>
        /// Handler for the Checked event of the automaticUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _AutomaticUpdateChecked(object sender, RoutedEventArgs e)
        {
            silentUpdate.IsEnabled = true;
        }

        /// <summary>
        /// Handler for the Unchecked event of the automaticUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _AutomaticUpdateUnchecked(object sender, RoutedEventArgs e)
        {
            silentUpdate.IsEnabled = false;
        }

        /// <summary>
        /// Handler for the KeyDown event of the textBoxPeriod, textBoxTimeDomain controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _NumericTextBoxKeyDown(object sender, WinInput.KeyEventArgs e)
        {
            // determine whether the keystroke is a number from the keypad.
            bool isNumPadNumeric = ((WinInput.Key.NumPad0 <= e.Key) &&
                                    (e.Key <= WinInput.Key.NumPad9));
            // determine whether the keystroke is a number from the top of the keyboard.
            bool isNumeric = ((WinInput.Key.D0 <= e.Key) && (e.Key <= WinInput.Key.D9));
            // ignore all not numeric keys or Tab
            e.Handled = (!isNumeric && !isNumPadNumeric && (e.Key != WinInput.Key.Tab));
        }

        /// <summary>
        /// Handler for the TextChanged event of the textBoxPeriod control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _PeriodTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!textBoxPeriod.HasParsingError && !textBoxPeriod.HasValidationError &&
                !string.IsNullOrEmpty(textBoxPeriod.Text))
            {
                int? value = _ConvertStringToInt(textBoxPeriod.Text);
                if (value.HasValue)
                {
                    ProjectArchivingSettings settings = App.Current.Project.ProjectArchivingSettings;
                    if (null != settings)
                    {
                        settings.AutoArchivingPeriod = value.Value;
                        App.Current.Project.Save();
                    }
                }
            }
        }

        /// <summary>
        /// Handler for the TextChanged event of the textBoxTimeDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _TimeDomainTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!textBoxTimeDomain.HasParsingError && !textBoxTimeDomain.HasValidationError &&
                !string.IsNullOrEmpty(textBoxTimeDomain.Text))
            {
                int? value = _ConvertStringToInt(textBoxTimeDomain.Text);
                if (value.HasValue)
                {
                    ProjectArchivingSettings settings = App.Current.Project.ProjectArchivingSettings;
                    if (null != settings)
                    {
                        settings.TimeDomain = value.Value;
                        App.Current.Project.Save();
                    }
                }
            }
        }

        /// <summary>
        /// Handler for the Click event of the checkBoxRunAuto control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _CheckBoxRunAutoClick(object sender, RoutedEventArgs e)
        {
            bool isChecked = (true == checkBoxRunAuto.IsChecked);
            labelMonths.IsEnabled = isChecked;
            textBoxPeriod.IsEnabled = isChecked;

            ProjectArchivingSettings settings = App.Current.Project.ProjectArchivingSettings;
            if (null != settings)
            {
                settings.IsAutoArchivingEnabled = isChecked;
                App.Current.Project.Save();
            }
        }

        /// <summary>
        /// Handler for the Click event of the AllwaysAskBeforeDeletingButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _AllwaysAskBeforeDeletingButtonClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsAllwaysAskBeforeDeletingEnabled =
                (true == AllwaysAskBeforeDeletingButton.IsChecked);
        }

        /// <summary>
        /// If user select radio button - we need to update corresponding setting.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _AlwaysAskAboutApplyingBreaksClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsAlwaysAskAboutApplyingBreaksToDefaultRoutes = true;
        }

        /// <summary>
        /// If user select radio button - we need to update corresponding setting.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ApplyBreaksToRoutesClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsAlwaysAskAboutApplyingBreaksToDefaultRoutes = false;

            Settings.Default.ApplyBreaksChangesToDefaultRoutes = 
                ApplyBreaks.IsChecked == true;
        }

        /// <summary>
        ///If user select radio button - we need to update corresponding setting.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _AlwaysAskAboutApplyingRoutesClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsAlwaysAskAboutApplyingDefaultRoutesToSheduledRoutes = true;
        }

        /// <summary>
        /// If user select radio button - we need to update corresponding setting.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ApplyDefaultRoutesToScheduledRoutesClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsAlwaysAskAboutApplyingDefaultRoutesToSheduledRoutes = false;

            Settings.Default.ApplyDefaultRoutesToSheduledRoutes =
                ApplyDefaultRoutesToScheduledRoutes.IsChecked == true;
        }

        /// <summary>
        /// Handler for the Click event of the FleetSetupWizardButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _FleetSetupWizardButtonClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsAutomaticallyShowFleetWizardEnabled =
                (true == FleetSetupWizardButton.IsChecked);
        }

        /// <summary>
        /// Handler for the Click event of the RouteLocationsVirtualWarningButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _RouteLocationsVirtualWarningButtonClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.WarnUserIfVirtualLocationDetected =
                (true == RouteLocationsVirtualWarningButton.IsChecked);
        }

        #endregion Event handlers

        #region Private constants

        /// <summary>
        /// Part of log file name, which is copied to desktop.
        /// </summary>
        private const string LOG_FILE_NAME = "Route Planner Log ";

        /// <summary>
        /// Separator for date and time in copied to desktop log file name.
        /// </summary>
        private const string LOG_DATE_TIME_SEPARATOR = "_";

        #endregion Private constants

        #region Private fields

        /// <summary>
        /// The reference to the proxy configuration service.
        /// </summary>
        private IProxyConfigurationService _proxyConfigurationService;

        /// <summary>
        /// User's update setting helper
        /// </summary>
        private UpdateSettingsHelper _updateHelper = null;

        /// <summary>
        /// Configuration of the current project.
        /// </summary>
        private ProjectConfiguration _currentProjectConfiguration;

        #endregion Private fields
    }
}

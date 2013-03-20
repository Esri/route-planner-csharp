using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Pages;

namespace ArcLogisticsPlugIns.SendRoutesToNavigatorPage
{
    /// <summary>
    /// Interaction logic for ExportToNavigatorPreferencesPage.xaml
    /// </summary>
    [PagePlugInAttribute("Preferences")]
    public partial class ExportToNavigatorPreferencesPage : PageBase, ISupportSettings
    {
        #region constants

        public const string PAGE_NAME = "ExportToNavigatorSettings";

        #endregion

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ExportToNavigatorPreferencesPage()
        {
            InitializeComponent();

            IsRequired = true;
            IsAllowed = true;
            DoesSupportCompleteStatus = false;
            
            Password.PasswordChanged += new RoutedEventHandler(Password_PasswordChanged);

            this.Loaded += new RoutedEventHandler(OnLoaded);

            _defaultBackground = (Brush)UserName.Background;
            _defaultForeground = (Brush)UserName.Foreground;
        }

        #endregion // Constructors

        #region Page Overrided Members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string Name
        {
            get { return PAGE_NAME; }
        }

        public override string Title
        {
            get { return Properties.Resources.SendRoutesToNavigatorTitle; }
        }

        public override System.Windows.Media.TileBrush Icon
        {
            get
            {
                return Resources["SendRoutesSettingsBrush"] as ImageBrush; 
            }
        }

        #endregion

        #region Public PageBase overrided members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override HelpTopic HelpTopic
        {
            get { return null; }
        }

        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        #endregion // PageBase overrided members

        #region Public ISupportSettings members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load settings from string
        /// </summary>
        /// <param name="settingsString">String with serialized settings</param>
        public void LoadUserSettings(string settingsString)
        {
            GrfExporterSettingsConfig.Instance.Deserialize(settingsString);
        }

        /// <summary>
        /// Save settings config to serialized string
        /// </summary>
        /// <returns>Serialized string</returns>
        public string SaveUserSettings()
        {
            string serialized = GrfExporterSettingsConfig.Instance.Serialize();
            return serialized;
        }

        #endregion

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init grid content by grf exporter settings config
        /// </summary>
        /// <param name="grfExporterConfig">Grf exporter settings config to use</param>
        private void _SetContent(GrfExporterSettingsConfig grfExporterConfig)
        {
            ServerAddress.Text = grfExporterConfig.ServerAddress;
            ServerPort.Text = grfExporterConfig.ServerPort.ToString();
            ServerRequiresAutentication.IsChecked = grfExporterConfig.AutenticationRequired;
            UserName.Text = grfExporterConfig.UserName;
            RememberPassword.IsChecked = grfExporterConfig.RememberPassword;
            Password.Password = grfExporterConfig.Password;
            EncryptedConnectionRequires.IsChecked = grfExporterConfig.EncryptedConnectionRequires;
        }

        /// <summary>
        /// Create binding from page content to grf exporter settings config 
        /// </summary>
        /// <param name="grfExporterConfig">Grf exporter settings config to use</param>
        private void _BindConfig(GrfExporterSettingsConfig grfExporterConfig)
        {
            _CreateBinding("ServerAddress", grfExporterConfig, ServerAddress,
                TextBox.TextProperty);
            _CreateBinding("ServerPort", grfExporterConfig, ServerPort,
                TextBox.TextProperty);
            _CreateBinding("AutenticationRequired", grfExporterConfig, ServerRequiresAutentication,
                ToggleButton.IsCheckedProperty);
            _CreateBinding("UserName", grfExporterConfig, UserName,
                TextBox.TextProperty);
            _CreateBinding("RememberPassword", grfExporterConfig, RememberPassword,
                ToggleButton.IsCheckedProperty);
            _CreateBinding("EncryptedConnectionRequires", grfExporterConfig, EncryptedConnectionRequires,
                ToggleButton.IsCheckedProperty);

            RouteGRFcompression.IsChecked = grfExporterConfig.RouteGrfCompression;
            _CreateBinding("RouteGrfCompression", grfExporterConfig, RouteGRFcompression,
                CheckBox.IsCheckedProperty);

            _isLoaded = true;
        }

        #endregion // Event handlers

        #region event handlers

        /// <summary>
        /// React on load
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // set cleared password back
            Password.Password = GrfExporterSettingsConfig.Instance.Password;

            if (!_isLoaded)
                _BindConfig(GrfExporterSettingsConfig.Instance);

            _SetContent(GrfExporterSettingsConfig.Instance);

            _SetEnabled();

            App.Current.MainWindow.StatusBar.SetStatus(this, null);
        }

        /// <summary>
        /// React on server address text changed
        /// </summary>
        private void ServerAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded)
                return;
            _SetEnabled();
        }

        /// <summary>
        /// React on server port text changed
        /// </summary>
        private void ServerPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            int? serverPort = null;
            try
            {
                serverPort = int.Parse(ServerPort.Text);
            }
            catch
            { }

            if (serverPort.HasValue)
                _previousServerPort = serverPort.Value;
            else
                ServerPort.Text = _previousServerPort.ToString();
        }

        /// <summary>
        /// React on user name text changed
        /// </summary>
        private void UserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded)
                return;

            _SetEnabled();
        }

        /// <summary>
        /// React on settings chanded by user
        /// </summary>
        private void On_Click(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
                return;
            _SetEnabled();
        }

        /// <summary>
        /// React on password changed
        /// </summary>
        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // workaround for clearing password on navigating
            if (Password.IsFocused)
                GrfExporterSettingsConfig.Instance.Password = Password.Password;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Set enabled property to fields
        /// </summary>
        private void _SetEnabled()
        {
            UserName.Background = _defaultBackground;
            UserName.Foreground = _defaultForeground;
            Password.Background = _defaultBackground;
            Password.Foreground = _defaultForeground;

            if (ServerAddress.Text.Length == 0)
            {
                ServerPort.IsEnabled = false;
                ServerRequiresAutentication.IsEnabled = false;
                UserName.IsEnabled = false;
                Password.IsEnabled = false;
                RememberPassword.IsEnabled = false;
                EncryptedConnectionRequires.IsEnabled = false;
            }
            else
            {
                // if server address is not empty
                ServerPort.IsEnabled = true;
                ServerRequiresAutentication.IsEnabled = true;
                if (ServerRequiresAutentication.IsChecked.Value)
                {
                    _SetEnabledAutenticationFields();
                }
                else
                {
                    // if server not requires authentication
                    UserName.IsEnabled = false;
                    Password.IsEnabled = false;
                    RememberPassword.IsEnabled = false;
                    EncryptedConnectionRequires.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// Set enabled property to autentication fields
        /// </summary>
        private void _SetEnabledAutenticationFields()
        {
            if (UserName.Text.Length == 0)
            {
                UserName.Background = _errorBackground;
                UserName.Foreground = _errorForeground;
            }

            UserName.IsEnabled = true;
            RememberPassword.IsEnabled = true;

            if (!RememberPassword.IsChecked.Value)
            {
                Password.Password = "";
            }
            else
            {
                if (Password.Password.Length == 0)
                {
                    Password.Background = _errorBackground;
                    Password.Foreground = _errorForeground;
                }
            }

            Password.IsEnabled = RememberPassword.IsChecked.Value;
            EncryptedConnectionRequires.IsEnabled = true;
        }

        /// <summary>
        /// Create binding for framework element
        /// </summary>
        /// <param name="propertyPath">Binding property path</param>
        /// <param name="source">Source object for binding</param>
        /// <param name="element">Element to bund</param>
        /// <param name="property">Dependency property to bind</param>
        private void _CreateBinding(string propertyPath, object source,
            FrameworkElement element, DependencyProperty property)
        {
            Binding binding = new Binding();

            binding.Path = new PropertyPath(propertyPath);
            binding.Source = source;
            binding.Mode = BindingMode.TwoWay;

            element.SetBinding(property, binding);
        }

        #endregion

        #region private members

        private bool _isLoaded;

        private int _previousServerPort;

        private Brush _defaultBackground;
        private Brush _defaultForeground;
        private Brush _errorBackground = new SolidColorBrush(Colors.Red);
        private Brush _errorForeground = new SolidColorBrush(Colors.White);

        #endregion
    }
}

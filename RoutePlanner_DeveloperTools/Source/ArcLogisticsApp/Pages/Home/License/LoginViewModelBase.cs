using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Utility.ComponentModel;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Provides base implementation for the loging view models.
    /// </summary>
    internal abstract class LoginViewModelBase : NotifyPropertyChangedBase
    {
        #region public properties
        /// <summary>
        /// Gets or sets value of the license state.
        /// </summary>
        public AgsServerState LicenseState
        {
            get
            {
                return _state;
            }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_LICENSE_STATE);
                }
            }
        }

        /// <summary>
        /// Gets header text value.
        /// </summary>
        [PropertyDependsOn(PROPERTY_NAME_LICENSE_STATE)]
        public string Header
        {
            get
            {
                string header;
                if (_headers.TryGetValue(this.LicenseState, out header))
                {
                    return header;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets a view model for the login state.
        /// </summary>
        public LoginStateViewModel LoginState
        {
            get
            {
                return _loginState;
            }
            set
            {
                if (_loginState != value)
                {
                    _loginState = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_LOGIN_STATE);
                }
            }
        }

        /// <summary>
        /// Gets or sets a view model for the connected state.
        /// </summary>
        public ConnectedStateViewModel ConnectedState
        {
            get
            {
                return _connectedState;
            }
            set
            {
                if (_connectedState != value)
                {
                    _connectedState = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_CONNECTED_STATE);
                }
            }
        }

        /// <summary>
        /// Gets or sets a view model for the not connected state.
        /// </summary>
        public NotConnectedStateViewModel NotConnectedState
        {
            get
            {
                return _notConnectedState;
            }
            set
            {
                if (_notConnectedState != value)
                {
                    _notConnectedState = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_NOT_CONNECTED_STATE);
                }
            }
        }
        #endregion

        #region protected static methods
        /// <summary>
        /// Applies license view style for the specified flow document instance.
        /// </summary>
        /// <param name="document">The flow document to apply style for.</param>
        /// <returns>The <paramref name="document"/> with the style applied.</returns>
        protected static FlowDocument ApplyStyle(FlowDocument document)
        {
            Debug.Assert(document != null);

            document.Style = (Style)App.Current.FindResource(LICENSE_VIEW_FLOW_DOCUMENT_STYLE_KEY);

            return document;
        }
        #endregion

        #region protected methods
        /// <summary>
        /// Registers the specified header for the specified state.
        /// </summary>
        /// <param name="state">The state to register header for.</param>
        /// <param name="header">The header to be used for the specified state.</param>
        protected void RegisterHeader(AgsServerState state, string header)
        {
            _headers[state] = header;
        }
        #endregion

        #region private constants
        /// <summary>
        /// Resource key for the style of license view flow documents.
        /// </summary>
        private const string LICENSE_VIEW_FLOW_DOCUMENT_STYLE_KEY = "LicenseViewFlowDocumentStyle";

        /// <summary>
        /// Name of the LicenseState property.
        /// </summary>
        private const string PROPERTY_NAME_LICENSE_STATE = "LicenseState";
        
        /// <summary>
        /// Name of the LoginState property.
        /// </summary>
        private const string PROPERTY_NAME_LOGIN_STATE = "LoginState";
        
        /// <summary>
        /// Name of the ConnectedState property.
        /// </summary>
        private const string PROPERTY_NAME_CONNECTED_STATE = "ConnectedState";

        /// <summary>
        /// Name of the NotConnectedState property.
        /// </summary>
        private const string PROPERTY_NAME_NOT_CONNECTED_STATE = "NotConnectedState";
        #endregion

        #region private fields
        /// <summary>
        /// Stores value of the LicenseState property.
        /// </summary>
        private AgsServerState _state;

        /// <summary>
        /// Stores values of the Header property.
        /// </summary>
        private Dictionary<AgsServerState, string> _headers = new Dictionary<AgsServerState, string>();

        /// <summary>
        /// Stores value of the LoginState property.
        /// </summary>
        private LoginStateViewModel _loginState;

        /// <summary>
        /// Stores value of the ConnectedState property.
        /// </summary>
        private ConnectedStateViewModel _connectedState;

        /// <summary>
        /// Stores value of the NotConnectedState property.
        /// </summary>
        private NotConnectedStateViewModel _notConnectedState;
        #endregion
    }
}

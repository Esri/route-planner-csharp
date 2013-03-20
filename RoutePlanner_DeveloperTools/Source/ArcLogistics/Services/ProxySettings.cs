using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using ESRI.ArcLogistics.Utility;
using ESRI.ArcLogistics.Utility.ComponentModel;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Provides access to proxy server settings to be used by application.
    /// </summary>
    [Serializable]
    internal sealed class ProxySettings : NotifyPropertyChangedBase
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the ProxySettingsViewModel class.
        /// </summary>
        public ProxySettings()
        {
            this.HttpSettings = new ProxyProtocolSettings();
            _Initialize();
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets or sets a value indicating if manual proxy configuration should be used.
        /// </summary>
        public bool UseManualConfiguration
        {
            get
            {
                return _useManualConfiguration;
            }

            set
            {
                if (_useManualConfiguration != value)
                {
                    _useManualConfiguration = value;
                    this.NotifyPropertyChanged("UseManualConfiguration");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if all protocols should use the same proxy settings.
        /// </summary>
        public bool UseSameSettings
        {
            get
            {
                return _useSameSettings;
            }

            set
            {
                if (_useSameSettings != value)
                {
                    _useSameSettings = value;
                    this.NotifyPropertyChanged("UseSameSettings");
                }
            }
        }

        /// <summary>
        /// Gets a reference to the HTTP protocol proxy settings object.
        /// </summary>
        public ProxyProtocolSettings HttpSettings
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a reference to the HTTPS protocol proxy settings object.
        /// </summary>
        [PropertyDependsOn("UseSameSettings")]
        public ProxyProtocolSettings HttpsSettings
        {
            get
            {
                return this.UseSameSettings ? this.HttpSettings : _httpsSettings;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if authentication should be used for proxy servers.
        /// </summary>
        public bool UseAuthentication
        {
            get
            {
                return _useAuthentication;
            }

            set
            {
                if (_useAuthentication != value)
                {
                    _useAuthentication = value;
                    this.NotifyPropertyChanged("UseAuthentication");
                }
            }
        }

        /// <summary>
        /// Gets or sets a username for proxy authentication.
        /// </summary>
        public string Username
        {
            get
            {
                return _username;
            }

            set
            {
                if (_username != value)
                {
                    _username = value;
                    this.NotifyPropertyChanged("Username");
                }
            }
        }

        /// <summary>
        /// Gets or sets a password for proxy authentication.
        /// </summary>
        public string Password
        {
            get
            {
                return _password;
            }

            set
            {
                if (_password != value)
                {
                    _password = value;
                    this.NotifyPropertyChanged("Password");
                }
            }
        }
        #endregion

        #region internal methods
        /// <summary>
        /// Prepares proxy settings state for serialization.
        /// </summary>
        /// <param name="context">The reference to the serialization stream description
        /// object.</param>
        [OnSerializing]
        internal void OnSerializing(StreamingContext context)
        {
            _serializationPassword = _password != null ?
                StringProcessor.TransformData(_password) : null;
        }

        /// <summary>
        /// Finalizes proxy settings deserialization.
        /// </summary>
        /// <param name="context">The reference to the serialization stream description
        /// object.</param>
        [OnDeserialized]
        internal new void OnDeserialized(StreamingContext context)
        {
            _password = null;
            if (_serializationPassword != null)
            {
                StringProcessor.TryTransformDataBack(_serializationPassword, out _password);
            }

            _Initialize();
        }
        #endregion

        #region private methods
        /// <summary>
        /// Performs common initialization for construction and deserialization.
        /// </summary>
        private void _Initialize()
        {
            Debug.Assert(this.HttpsSettings != null);

            this.HttpSettings.PropertyChanged +=
                (_s, _e) => this.NotifyPropertyChanged("HttpSettings");
            _httpsSettings.PropertyChanged +=
                (_s, _e) => this.NotifyPropertyChanged("HttpsSettings");
        }
        #endregion

        #region private fields
        /// <summary>
        /// Stores value of the <see cref="UseManualConfiguration"/> property.
        /// </summary>
        private bool _useManualConfiguration;

        /// <summary>
        /// Stores value of the <see cref="UseSameSettings"/> property.
        /// </summary>
        private bool _useSameSettings;

        /// <summary>
        /// Stores value of the <see cref="UseAuthentication"/> property.
        /// </summary>
        private bool _useAuthentication;

        /// <summary>
        /// Stores value of the <see cref="Username"/> property.
        /// </summary>
        private string _username;

        /// <summary>
        /// Stores value of the <see cref="Password"/> property.
        /// </summary>
        [NonSerialized]
        private string _password;

        /// <summary>
        /// Stores value of the <see cref="Password"/> property converted to a form suitable for
        /// serialization.
        /// </summary>
        private string _serializationPassword;

        /// <summary>
        /// The reference to the HTTPS protocol settings object.
        /// </summary>
        private ProxyProtocolSettings _httpsSettings = new ProxyProtocolSettings();
        #endregion
    }
}

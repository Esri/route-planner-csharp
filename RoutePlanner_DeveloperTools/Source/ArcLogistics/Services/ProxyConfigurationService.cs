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
using System.Diagnostics;
using System.Net;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Implements <see cref="IProxyConfigurationService"/> providing access to application-wide
    /// proxy settings.
    /// </summary>
    internal sealed class ProxyConfigurationService : IProxyConfigurationService
    {
        /// <summary>
        /// Initializes a new instance of the ProxyConfigurationService class.
        /// </summary>
        /// <param name="storage">The reference to the proxy settings storage.</param>
        /// <exception cref="ArgumentNullException"><paramref name="storage"/> is a null
        /// reference.</exception>
        public ProxyConfigurationService(
            IGenericStorage<ProxySettings> storage,
            IHostNameValidator validator)
        {
            if (storage == null)
            {
                throw new ArgumentNullException("storage");
            }

            if (validator == null)
            {
                throw new ArgumentNullException("validator");
            }

            _proxySettingsStorage = storage;
            _validator = validator;
            this.Settings = _proxySettingsStorage.Load();
        }

        /// <summary>
        /// Gets reference to the current proxy settings object.
        /// </summary>
        public ProxySettings Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// Saves current proxy settings to the appropriate storage and updates application-wide
        /// proxy settings.
        /// </summary>
        public void Update()
        {
            var settings = this.Settings;

            _proxySettingsStorage.Save(settings);

            var proxy = _Create(settings);
            if (settings.UseAuthentication)
            {
                proxy.Credentials = new NetworkCredential(settings.Username, settings.Password);
            }

            WebRequest.DefaultWebProxy = proxy;
        }

        #region private static methods
        /// <summary>
        /// Creates a new <see cref="IWebProxy"/> object from the specified protocol settings.
        /// </summary>
        /// <param name="settings">The reference to the particular protocol proxy settings
        /// to create proxy object from.</param>
        /// <returns>A new <see cref="IWebProxy"/> object based on the specified settings.</returns>
        private static IWebProxy _Create(ProxyProtocolSettings settings)
        {
            Debug.Assert(settings != null);

            if (settings.Port != null)
            {
                return new WebProxy(settings.Host, settings.Port.Value);
            }

            return new WebProxy(settings.Host);
        }
        #endregion

        #region private methods
        /// <summary>
        /// Creates a new <see cref="IWebProxy"/> object from the specified settings without
        /// applying authentication credentials.
        /// </summary>
        /// <param name="settings">The reference to the proxy settings to create proxy object
        /// from.</param>
        /// <returns>A new <see cref="IWebProxy"/> object based on the specified settings.</returns>
        private IWebProxy _Create(ProxySettings settings)
        {
            Debug.Assert(settings != null);

            if (!_HasValidManualSettings(settings))
            {
                return WebRequest.GetSystemWebProxy();
            }

            var httpProxy = _Create(settings.HttpSettings);
            if (settings.UseSameSettings)
            {
                return httpProxy;
            }

            var httpsProxy = _Create(settings.HttpsSettings);
            var proxy = new AggregateWebProxy();
            proxy.AddProxy("http", httpProxy);
            proxy.AddProxy("https", httpsProxy);

            return proxy;
        }

        /// <summary>
        /// Checks if the specified proxy settings contains valid manual settings.
        /// </summary>
        /// <param name="settings">The reference to settings object to be checked.</param>
        /// <returns>true if and only if the specified settings object contains valid
        /// settings.</returns>
        private bool _HasValidManualSettings(ProxySettings settings)
        {
            if (!settings.UseManualConfiguration)
            {
                return false;
            }

            if (!_validator.Validate(settings.HttpSettings.Host).IsValid)
            {
                return false;
            }

            if (settings.UseSameSettings)
            {
                return true;
            }

            if (!_validator.Validate(settings.HttpsSettings.Host).IsValid)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region private fields
        /// <summary>
        /// The reference to the proxy settings storage object.
        /// </summary>
        private IGenericStorage<ProxySettings> _proxySettingsStorage;

        /// <summary>
        /// The reference to the host names validator object.
        /// </summary>
        private IHostNameValidator _validator;
        #endregion
    }
}

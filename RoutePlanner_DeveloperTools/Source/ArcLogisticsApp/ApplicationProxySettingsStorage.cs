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
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Implements proxy settings storage using application settings and ensuring backwards
    /// compatibility.
    /// </summary>
    internal sealed class ApplicationProxySettingsStorage : IGenericStorage<ProxySettings>
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationProxySettingsStorage class.
        /// </summary>
        /// <param name="storage">The reference to the base proxy storage settings.</param>
        public ApplicationProxySettingsStorage(IGenericStorage<ProxySettings> storage)
        {
            Debug.Assert(storage != null);

            _baseStorage = storage;
        }

        /// <summary>
        /// Loads <see cref="ProxySettings"/> objects from the application settings storage.
        /// </summary>
        /// <returns>Reference to the loaded object.</returns>
        public ProxySettings Load()
        {
            var settings = _baseStorage.Load();
            if (settings.UseManualConfiguration)
            {
                return settings;
            }

            var settingsStorage = Properties.Settings.Default;
            if (!string.IsNullOrEmpty(settingsStorage.ProxyServerUsername) &&
                !string.IsNullOrEmpty(settingsStorage.ProxyServerPassword) &&
                !settings.UseAuthentication)
            {
                var password = default(string);
                var hasValidPassword = StringProcessor.TryTransformDataBack(
                    settingsStorage.ProxyServerPassword,
                    out password);
                if (hasValidPassword)
                {
                    settings.UseAuthentication = true;
                    settings.Username = settingsStorage.ProxyServerUsername;
                }
            }

            return settings;
        }

        /// <summary>
        /// Saves <see cref="ProxySettings"/> objects to the application settings storage.
        /// </summary>
        /// <param name="obj">The reference to the object to be saved.</param>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is a null
        /// reference.</exception>
        public void Save(ProxySettings obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            _baseStorage.Save(obj);

            var settingsStorage = Properties.Settings.Default;
            settingsStorage.ProxyServerUsername = null;
            settingsStorage.ProxyServerPassword = null;
            settingsStorage.Save();
        }

        #region private fields
        /// <summary>
        /// The reference to the proxy settings storage object.
        /// </summary>
        private IGenericStorage<ProxySettings> _baseStorage;
        #endregion
    }
}

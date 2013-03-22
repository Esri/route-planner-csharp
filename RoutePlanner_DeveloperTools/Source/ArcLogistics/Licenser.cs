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
using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Licenser class.
    /// </summary>
    internal sealed class Licenser : ILicenser
    {
        #region events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raises when license is activated.
        /// </summary>
        public static event EventHandler LicenseActivated;

        #endregion events

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Licenser()
        {
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static readonly Licenser instance = new Licenser();

        public static Licenser Instance
        {
            get { return instance; }
        }

        public static License ActivatedLicense
        {
            get { return _license; }
        }

        /// <summary>
        /// Gets a date/time of the <see cref="P:ActivatedLicense"/> validation.
        /// </summary>
        public static DateTime? ActivatedLicenseValidationDate
        {
            get
            {
                return _licenseValidationDate;
            }
        }

        /// <summary>
        /// Gets a reference to the license when it is expired.
        /// </summary>
        public static License ExpiredLicense
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a reference to the license expiration checker object.
        /// </summary>
        public static ILicenseExpirationChecker LicenseExpirationChecker
        {
            get;
            private set;
        }

        internal static NetworkCredential LicenseAccount
        {
            get { return _licAccount; }
        }

        #endregion public properties

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes licenser instance.
        /// </summary>
        /// <param name="licenseCacheStorage">The reference to the license cache
        /// storage object.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="licenseCacheStorage"/> is a null reference.</exception>
        public static void Initialize(ILicenseCacheStorage licenseCacheStorage)
        {
            if (licenseCacheStorage == null)
            {
                throw new ArgumentNullException("licenseCacheStorage");
            }

            // load license component
            Assembly licAssembly = null;
            ILicenser licenser = _LoadLicenseComponent(out licAssembly);
            if (licenser == null)
            {
                throw new LicenseException(LicenseError.LicenseComponentNotFound,
                    Properties.Messages.Error_LicenseComponentNotFound);
            }

            // verify if assembly correctly signed
            _VerifySignature(licAssembly);

            _innerLicenser = licenser;
            _licenseCacheStorage = licenseCacheStorage;

            Licenser.LicenseExpirationChecker = new LicenseExpirationChecker(
                _licenseCacheStorage);
        }

        #endregion public methods

        #region ILicenser interface
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This text property is used to show login prompt on license page.
        /// </summary>
        public string LoginPrompt
        {
            get { return _InnerLicenser.LoginPrompt; }
        }

        /// <summary>
        /// This property contains URL where user will be redirected in case he
        /// wants to upgrade his license to get more vehicles available for routing.
        /// </summary>
        public string UpgradeLicenseURL
        {
            get { return _InnerLicenser.UpgradeLicenseURL; }
        }

        /// <summary>
        /// This property contains URL where user will be redirected in case he
        /// wants to create an account.
        /// </summary>
        public string CreateAccountURL
        {
            get { return _InnerLicenser.CreateAccountURL; }
        }

        /// <summary>
        /// Gets a URL to redirect user to when he forgot his username and/or password.
        /// </summary>
        public string RecoverCredentialsURL
        {
            get
            {
                return _InnerLicenser.RecoverCredentialsURL;
            }
        }

        /// <summary>
        /// Licensing notes.
        /// </summary>
        public string LicensingNotes
        {
            get { return _InnerLicenser.LicensingNotes; }
        }

        /// <summary>
        /// Troubleshooting notes.
        /// </summary>
        public string TroubleshootingNotes
        {
            get { return _InnerLicenser.TroubleshootingNotes; }
        }

        /// <summary>
        /// This boolean property indicates either user has to provide
        /// credentials to get the license.
        /// </summary>
        public bool RequireAuthentication
        {
            get { return _InnerLicenser.RequireAuthentication; }
        }

        /// <summary>
        /// This method gets username and password and returns license. If some
        /// error occurs (connectivity error or invalid credentials) exception
        /// will be thrown.
        /// </summary>
        public LicenseInfo GetLicense(string userName, string password)
        {
            var license = _GetLicense(
                _InnerLicenser.GetLicense,
                userName,
                password);

            return license;
        }

        /// <summary>
        /// Gets free license for the specified credentials.
        /// </summary>
        /// <param name="userName">The user name to be used to authenticate within
        /// license service.</param>
        /// <param name="password">The password to be used to authenticate within
        /// license service.</param>
        /// <returns>License info object with a license for the free single vehicle role.</returns>
        /// <exception cref="T:ESRI.ArcLogistics.LicenseException">
        /// <list type="bullet">
        /// <item>
        /// <description>License service failed processing request.</description>
        /// </item>
        /// <item>
        /// <description>Specified credentials are invalid.</description>
        /// </item>
        /// <item>
        /// <description>There is no license for the user with the specified
        /// credentials or all licenses have expired.</description>
        /// </item>
        /// </list></exception>
        /// <exception cref="T:ESRI.ArcLogistics.CommunicationError">Failed to
        /// communicate with the License service.</exception>
        public LicenseInfo GetFreeLicense(string userName, string password)
        {
            var license = _GetLicense(
                _InnerLicenser.GetFreeLicense,
                userName,
                password);

            return license;
        }

        /// <summary>
        /// This method returns license in case Require Authentication is false.
        /// If some error occurs (connectivity error or invalid credentials)
        /// exception will be thrown.
        /// </summary>
        public LicenseInfo GetLicense()
        {
            // get license
            var licenseInfo = _InnerLicenser.GetLicense();
            _license = licenseInfo.License;
            _licenseValidationDate = licenseInfo.LicenseValidationDate;

            // reset account
            _licAccount = null;

            // notify clients
            _NotifyLicenseActivated();

            return licenseInfo;
        }

        #endregion ILicenser interface

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static ILicenser _InnerLicenser
        {
            get
            {
                if (_innerLicenser == null)
                    throw new InvalidOperationException(Properties.Messages.Error_LicenseComponentNotSet);

                return _innerLicenser;
            }
        }

        private static void _ValidateCredentials(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName) ||
                String.IsNullOrEmpty(password))
            {
                throw new LicenseException(LicenseError.InvalidCredentials,
                    Properties.Messages.Error_LoginFailed);
            }
        }

        private static LicenseInfo _GetLicense(string userName, string password)
        {
            LicenseInfo license = null;
            if (_InnerLicenser.RequireAuthentication)
            {
                _ValidateCredentials(userName, password);
                license = _InnerLicenser.GetLicense(userName, password);
            }
            else
                license = _InnerLicenser.GetLicense();

            return license;
        }

        private static void _NotifyLicenseActivated()
        {
            if (LicenseActivated != null)
                LicenseActivated(null, EventArgs.Empty);
        }

        private static ILicenser _LoadLicenseComponent(out Assembly licAssembly)
        {
            licAssembly = null;

            ICollection<string> assemblyFiles = CommonHelpers.GetAssembliesFiles(
                AppDomain.CurrentDomain.BaseDirectory);

            string interfaceName = typeof(ILicenser).ToString();

            ILicenser licenser = null;
            foreach (string path in assemblyFiles)
            {
                Assembly assembly = Assembly.LoadFrom(path);

                licenser = _LoadLicenserFromAssembly(assembly, interfaceName);
                if (licenser != null)
                {
                    licAssembly = assembly;
                    break;
                }
            }

            return licenser;
        }

        private static ILicenser _LoadLicenserFromAssembly(Assembly assembly,
            string interfaceName)
        {
            ILicenser licenser = null;
            try
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsPublic && !type.IsAbstract)
                    {
                        Type interfaceType = type.GetInterface(interfaceName);
                        if (interfaceType != null)
                        {
                            try
                            {
                                licenser = (ILicenser)Activator.CreateInstance(type);
                                break;
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return licenser;
        }

        private static void _VerifySignature(Assembly licAssembly)
        {
            Debug.Assert(licAssembly != null);

            byte[] licKey = licAssembly.GetName().GetPublicKey();
            if (licKey.Length == 0)
            {
                // license component must be signed
                throw new LicenseException(LicenseError.InvalidComponentSignature,
                    Properties.Messages.Error_InvalidLicenserSignature);
            }

            // check if component's key is correct
            byte[] key = Assembly.GetExecutingAssembly().GetName().GetPublicKey();
            if (!_AreArraysEqual(key, licKey))
            {
                // license component is signed with some other key
                throw new LicenseException(LicenseError.InvalidComponentSignature,
                    Properties.Messages.Error_InvalidLicenserSignature);
            }
        }

        private static bool _AreArraysEqual(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;

            return true;
        }

        /// <summary>
        /// Implements license retrieval.
        /// </summary>
        /// <param name="getLicense">The function to be used for license retrival.</param>
        /// <param name="userName">The user name to be used to authenticate within
        /// license service.</param>
        /// <param name="password">The password to be used to authenticate within
        /// license service.</param>
        /// <returns>License object for the free single vehicle role.</returns>
        /// <exception cref="T:ESRI.ArcLogistics.LicenseException">
        /// <list type="bullet">
        /// <item>
        /// <description>License service failed processing request.</description>
        /// </item>
        /// <item>
        /// <description>Specified credentials are invalid.</description>
        /// </item>
        /// <item>
        /// <description>There is no license for the user with the specified
        /// credentials or all licenses have expired.</description>
        /// </item>
        /// </list></exception>
        /// <exception cref="T:ESRI.ArcLogistics.CommunicationError">Failed to
        /// communicate with the License service.</exception>
        private LicenseInfo _GetLicense(
            Func<string, string, LicenseInfo> getLicense,
            string userName,
            string password)
        {
            var licenseCache = _licenseCacheStorage.Load();

            LicenseCacheEntry cacheEntry = null;
            licenseCache.Entries.TryGetValue(userName, out cacheEntry);

            LicenseInfo licenseInfo = null;

            // get license
            try
            {
                licenseInfo = getLicense(userName, password);
                _license = licenseInfo.License;
                _licenseValidationDate = licenseInfo.LicenseValidationDate;
            }
            catch (LicenseException ex)
            {
                if (ex.ErrorCode == LicenseError.LicenseExpired && cacheEntry != null)
                {
                    Licenser.ExpiredLicense = cacheEntry.License;
                }

                throw;
            }

            // set new account
            if (RequireAuthentication)
                _licAccount = new NetworkCredential(userName, password);

            // notify clients
            _NotifyLicenseActivated();

            // We need to store license in cache in order to display it's routes
            // number (and probably other info) when it expires. However, if
            // the license is a free single vehicle one we should not store it
            // in the cache to not overwrite a "real" license info.
            if (_license.PermittedRouteNumber > 1)
            {
                var licenseExpirationWarningWasShown = false;
                if (cacheEntry != null && cacheEntry.License == _license)
                {
                    licenseExpirationWarningWasShown = cacheEntry.LicenseExpirationWarningWasShown;
                }

                cacheEntry = new LicenseCacheEntry()
                {
                    LicenseExpirationWarningWasShown = licenseExpirationWarningWasShown,
                    License = _license,
                };

                licenseCache.Entries[userName] = cacheEntry;
                _licenseCacheStorage.Save(licenseCache);
            }

            return licenseInfo;
        }
        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static License _license;

        /// <summary>
        /// Stores <see cref="P:ActivatedLicenseValidationDate"/> property value.
        /// </summary>
        private static DateTime? _licenseValidationDate;

        private static NetworkCredential _licAccount;
        private static ILicenser _innerLicenser;

        /// <summary>
        /// The reference to the current license cache storage object.
        /// </summary>
        private static ILicenseCacheStorage _licenseCacheStorage;
        
        #endregion private fields
    }
}

using System;
using EnterpriseLicenser.Properties;
using ESRI.ArcLogistics;

namespace ESRI.ArcLogistics.Licensing
{
    public class EnterpriseLicenser : ILicenser
    {
        #region ILicenser interface
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This text property is used to show login prompt on license page.
        /// </summary>
        public string LoginPrompt
        {
            get { return ""; }
        }

        /// <summary>
        /// This property contains URL where user will be redirected in case he
        /// wants to upgrade his license to get more vehicles available for routing.
        /// </summary>
        public string UpgradeLicenseURL
        {
            get { return ""; }
        }

        /// <summary>
        /// This property contains URL where user will be redirected in case he
        /// wants to create an account.
        /// </summary>
        public string CreateAccountURL
        {
            get { return ""; }
        }

        /// <summary>
        /// Gets a URL to redirect user to when he forgot his username and/or password.
        /// </summary>
        public string RecoverCredentialsURL
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Licensing notes.
        /// </summary>
        public string LicensingNotes
        {
            get { return ""; }
        }

        /// <summary>
        /// Troubleshooting notes.
        /// </summary>
        public string TroubleshootingNotes
        {
            get { return ""; }
        }

        /// <summary>
        /// This boolean property indicates either user has to provide
        /// credentials to get the license.
        /// </summary>
        public bool RequireAuthentication
        {
            get { return false; }
        }

        /// <summary>
        /// This method gets username and password and returns license. If some
        /// error occurs (connectivity error or invalid credentials) exception
        /// will be thrown.
        /// </summary>
        public LicenseInfo GetLicense(string userName, string password)
        {
            return GetLicense();
        }

        /// <summary>
        /// Gets free license for the specified credentials.
        /// </summary>
        /// <param name="username">The user name to be used to authenticate within
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
            return GetLicense();
        }

        /// <summary>
        /// This method returns license in case Require Authentication is false.
        /// If some error occurs (connectivity error or invalid credentials)
        /// exception will be thrown.
        /// </summary>
        public LicenseInfo GetLicense()
        {
            var license = new License(null, null, null, null, false,
                Resources.LicenseDescription);
            return new LicenseInfo(license, null);
        }

        #endregion ILicenser interface
    }
}

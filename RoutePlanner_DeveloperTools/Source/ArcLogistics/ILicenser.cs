using System;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// ILicenser interface.
    /// </summary>
    public interface ILicenser
    {
        #region properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This text property is used to show login prompt on license page.
        /// </summary>
        string LoginPrompt { get; }

        /// <summary>
        /// This property contains URL where user will be redirected in case he
        /// wants to upgrade his license to get more vehicles available for routing.
        /// </summary>
        string UpgradeLicenseURL { get; }

        /// <summary>
        /// This property contains URL where user will be redirected in case he
        /// wants to create an account.
        /// </summary>
        string CreateAccountURL { get; }

        /// <summary>
        /// Gets a URL to redirect user to when he forgot his username and/or password.
        /// </summary>
        string RecoverCredentialsURL { get; }

        /// <summary>
        /// Licensing notes.
        /// </summary>
        string LicensingNotes { get; }

        /// <summary>
        /// Troubleshooting notes.
        /// </summary>
        string TroubleshootingNotes { get; }

        /// <summary>
        /// This boolean property indicates either user has to provide
        /// credentials to get the license.
        /// </summary>
        bool RequireAuthentication { get; }

        #endregion properties

        #region methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This method gets username and password and returns license. If some
        /// error occurs (connectivity error or invalid credentials) exception
        /// will be thrown.
        /// </summary>
        LicenseInfo GetLicense(string userName, string password);

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
        LicenseInfo GetFreeLicense(string userName, string password);

        /// <summary>
        /// This method returns license in case Require Authentication is false.
        /// If some error occurs (connectivity error or invalid credentials)
        /// exception will be thrown.
        /// </summary>
        LicenseInfo GetLicense();

        #endregion methods
    }
}

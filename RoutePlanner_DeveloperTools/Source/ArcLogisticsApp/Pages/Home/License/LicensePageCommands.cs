using System.Windows.Input;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Implements <see cref="T:ESRI.ArcLogistics.App.Pages.ILicensePageCommands"/>
    /// allowing setting references to license page commands.
    /// </summary>
    internal sealed class LicensePageCommands : ILicensePageCommands
    {
        #region ILicensePageCommands Members
        /// <summary>
        /// Gets a reference to the Create Account command.
        /// </summary>
        public ICommand CreateAccount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a reference to the Upgrade License command.
        /// </summary>
        public ICommand UpgradeLicense
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a reference to the Recover Credentials command.
        /// </summary>
        public ICommand RecoverCredentials
        {
            get;
            set;
        }
        #endregion
    }
}

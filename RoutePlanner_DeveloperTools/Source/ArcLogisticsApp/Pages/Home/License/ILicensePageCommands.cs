using System.Windows.Input;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Provides access to command license page commands.
    /// </summary>
    internal interface ILicensePageCommands
    {
        /// <summary>
        /// Gets a reference to the Create Account command.
        /// </summary>
        ICommand CreateAccount
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the Upgrade License command.
        /// </summary>
        ICommand UpgradeLicense
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the Recover Credentials command.
        /// </summary>
        ICommand RecoverCredentials
        {
            get;
        }
    }
}

using System;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Encapsulates license expiration date checking.
    /// </summary>
    internal interface ILicenseExpirationChecker
    {
        /// <summary>
        /// Checks if the license is about to expire.
        /// </summary>
        /// <param name="license">The license to be checked.</param>
        /// <param name="currentDate">The current date to check license for.</param>
        /// <returns>True if and only if the license is about to expire.</returns>
        bool LicenseIsExpiring(License license, DateTime? currentDate);

        /// <summary>
        /// Checks if the license is about to expire and corresponding warning
        /// should be shown to the user.
        /// </summary>
        /// <param name="username">The name of the user the license was issued for.</param>
        /// <param name="license">The license to be checked.</param>
        /// <param name="currentDate">The current date to check license for.</param>
        /// <returns>True if and only if the license is about to expire and
        /// expiration warning should be shown to the user.</returns>
        bool RequiresExpirationWarning(
            string username,
            License license,
            DateTime? currentDate);
    }
}

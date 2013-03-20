using System;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Stores expiration information for a license.
    /// </summary>
    [Serializable]
    internal class LicenseCacheEntry
    {
        /// <summary>
        /// Gets or sets a value indicating if the message about license expiration
        /// was already shown.
        /// </summary>
        public bool LicenseExpirationWarningWasShown
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a reference to the license object.
        /// </summary>
        public License License
        {
            get;
            set;
        }
    }
}

using System;
using System.Collections.Generic;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Provides access to a collection of license cache entries.
    /// </summary>
    [Serializable]
    internal class LicenseCache
    {
        /// <summary>
        /// Gets a reference to the dictionary mapping usernames to license cache entries.
        /// </summary>
        public IDictionary<string, LicenseCacheEntry> Entries
        {
            get
            {
                return _entries;
            }
        }

        /// <summary>
        /// Stores license cache entries.
        /// </summary>
        private Dictionary<string, LicenseCacheEntry> _entries =
            new Dictionary<string, LicenseCacheEntry>();
    }
}

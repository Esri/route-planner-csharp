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

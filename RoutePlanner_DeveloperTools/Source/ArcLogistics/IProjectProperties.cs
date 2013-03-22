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
    /// Project properties interface.
    /// </summary>
    public interface IProjectProperties
    {
        // REV: rename to GetPropertyNames
        /// <summary>
        /// Collection of presented properties name
        /// </summary>
        /// <remarks>Read only collection</remarks>
        ICollection<string> GetPropertiesName();

        /// <summary>
        /// Properties indexer
        /// </summary>
        string this[string name] { get; }

        /// <summary>
        /// Get property by name
        /// </summary>
        string GetPropertyByName(string name);

        /// <summary>
        /// Update property value
        /// </summary>
        /// <remarks>If not present, automatical adding to properties</remarks>
        void UpdateProperty(string name, string value);

        /// <summary>
        /// Add property to properties
        /// </summary>
        /// <remarks>Name must be unique</remarks>
        void AddProperty(string name, string value);

        /// <summary>
        /// Remove property from properties
        /// </summary>
        void RemoveProperty(string name);
    }
}

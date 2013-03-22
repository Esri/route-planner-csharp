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
using System.Diagnostics;
using System.Collections.Generic;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Project properties class.
    /// </summary>
    internal class ProjectProperties : IProjectProperties
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ProjectProperties()
        {
        }

        public ProjectProperties(Dictionary<string, string> propertiesMap)
        {
            if (null == propertiesMap)
                _propertiesMap.Clear();
            else
                _propertiesMap = propertiesMap;
        }

        #endregion // Constructors

        #region IProperties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Collection of presented properties name
        /// </summary>
        /// <remarks>Read only collection</remarks>
        public ICollection<string> GetPropertiesName()
        {
            return _propertiesMap.Keys;
        }

        /// <summary>
        /// Properties indexer
        /// </summary>
        public string this[string name]
        {
            get { return GetPropertyByName(name); }
        }

        /// <summary>
        /// Get property by name
        /// </summary>
        public string GetPropertyByName(string name)
        {
            return (_propertiesMap.ContainsKey(name))? _propertiesMap[name] : null;
        }

        /// <summary>
        /// Update property value
        /// </summary>
        /// <remarks>If not present, automatical adding to properties</remarks>
        public void UpdateProperty(string name, string value)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            if (_propertiesMap.ContainsKey(name))
                _propertiesMap.Remove(name);

            _propertiesMap.Add(name, value);
        }

        /// <summary>
        /// Add property to properties
        /// </summary>
        /// <remarks>Name must be unique</remarks>
        public void AddProperty(string name, string value)
        {
            UpdateProperty(name, value);
        }

        /// <summary>
        /// Remove property from properties
        /// </summary>
        public void RemoveProperty(string name)
        {
            _propertiesMap.Remove(name);
        }

        #endregion // IProperties

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        Dictionary<string, string> _propertiesMap = new Dictionary<string, string>();
        #endregion // Private members
    }
}

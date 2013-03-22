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

namespace ESRI.ArcLogistics.DomainObjects.Attributes
{
    /// <summary>
    /// This attribute is applied to any domain object property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class DomainPropertyAttribute : Attribute
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public DomainPropertyAttribute() : this(null) { }
        public DomainPropertyAttribute(string resourceName) : this(resourceName, false) { }
        public DomainPropertyAttribute(string resourceName, bool isMandatory)
        {
            Title = (string.IsNullOrEmpty(resourceName)) ? string.Empty : Properties.Resources.ResourceManager.GetString(resourceName);
            IsMandatory = isMandatory;
        }
        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Property title for showing in UI.
        /// </summary>
        public readonly string Title = null;

        /// <summary>
        /// Indicates either property is mandatory.
        /// </summary>
        public readonly bool IsMandatory = false;
        
        #endregion // Public properties

        #region Override functions
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            return string.Format("[DomainPropertyAttribute] Title: {0} IsMandatory: {1}", Title, IsMandatory.ToString());
        }
        #endregion // Override functions
    }
}

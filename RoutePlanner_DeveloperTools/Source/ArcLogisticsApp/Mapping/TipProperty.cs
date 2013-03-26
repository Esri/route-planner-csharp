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
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// TipProperty class
    /// </summary>
    internal class TipProperty
    {
        #region constructors

        public TipProperty(string name, string title, Unit? valueUnits, Unit? displayUnits)
        {
            Name = name;
            Title = title;
            ValueUnits = valueUnits;
            DisplayUnits = displayUnits;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Tip property string
        /// </summary>
        /// <returns>Property title</returns>
        public override string ToString()
        {
            return Title;
        }

        #endregion

        #region Public members

        /// <summary>
        /// Property title
        /// </summary>
        public string Title
        {
            get;
            private set;
        }

        /// <summary>
        /// Property name
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Prefix path to bind the property.
        /// </summary>
        public string PrefixPath
        {
            get;
            set;
        }

        /// <summary>
        /// Property value units
        /// </summary>
        public Unit? ValueUnits
        {
            get;
            private set;
        }

        /// <summary>
        /// Property Display units
        /// </summary>
        public Unit? DisplayUnits
        {
            get;
            private set;
        }

        #endregion
    }
}

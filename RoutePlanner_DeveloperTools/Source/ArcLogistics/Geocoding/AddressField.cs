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

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Class that represents address field.
    /// </summary>
    public class AddressField
    {
        #region constructors

        internal AddressField(string title, AddressPart type, bool visible, string decsription)
        {
            _title = title;
            _type = type;
            _visible = visible;
            _decsription = decsription;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Address field title.
        /// </summary>
        public string Title
        {
            get { return _title; }
        }

        /// <summary>
        /// Address field type.
        /// </summary>
        public AddressPart Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Indicates whether this address field should be shown in the UI.
        /// </summary>
        public bool Visible
        {
            get { return _visible; }
        }

        /// <summary>
        /// Address field description.
        /// </summary>
        public string Description
        {
            get { return _decsription; }
        }

        #endregion

        #region private members

        private string _title;
        private string _decsription;
        private AddressPart _type;
        private bool _visible;

        #endregion
    }
}

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
using System.Windows.Media;

namespace ESRI.ArcLogistics.App.OrderSymbology
{
    /// <summary>
    /// Symbology datacontext class
    /// </summary>
    class SymbologyContext
    {
        #region constants

        public const string FILL_ATTRIBUTE_NAME = "Fill";
        public const string SIZE_ATTRIBUTE_NAME = "Size";
        public const string FULLSIZE_ATTRIBUTE_NAME = "FullSize";
        public const string IS_VIOLATED_ATTRIBUTE_NAME = "IsViolated";
        public const string IS_LOCKED_ATTRIBUTE_NAME = "IsLocked";
        public const string OFFSETX_ATTRIBUTE_NAME = "OffsetX";
        public const string OFFSETY_ATTRIBUTE_NAME = "OffsetY";
        public const string SEQUENCE_NUMBER_ATTRIBUTE_NAME = "SequenceNumber";

        #endregion

        #region constructors

        public SymbologyContext()
        {
            _attributes = new Dictionary<string, object>();
            Attributes.Add(FILL_ATTRIBUTE_NAME, null);
            Attributes.Add(SIZE_ATTRIBUTE_NAME, null);
            Attributes.Add(FULLSIZE_ATTRIBUTE_NAME, null);
            Attributes.Add(IS_VIOLATED_ATTRIBUTE_NAME, false);
            Attributes.Add(IS_LOCKED_ATTRIBUTE_NAME, false);
            Attributes.Add(OFFSETX_ATTRIBUTE_NAME, 0);
            Attributes.Add(OFFSETY_ATTRIBUTE_NAME, 0);
        }

        #endregion

        #region public members

        /// <summary>
        /// Attributes for binding
        /// </summary>
        public IDictionary<string, object> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        #endregion

        #region private members

        private Dictionary<string, object> _attributes;

        #endregion
    }
}

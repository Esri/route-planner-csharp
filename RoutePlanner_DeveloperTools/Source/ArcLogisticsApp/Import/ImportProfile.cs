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

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Import profile class.
    /// </summary>
    internal class ImportProfile : ICloneable
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of ImportProfile class.
        /// </summary>
        public ImportProfile()
        {
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of import profile
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Import type
        /// </summary>
        public ImportType Type
        {
            get { return _type; }
            set { _type = value; }
        }
        
        /// <summary>
        /// Is profile default
        /// </summary>
        public bool IsDefault
        {
            get { return _isDefault; }
            set { _isDefault = value; }
        }

        /// <summary>
        /// Description of import profile
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Settings of import
        /// </summary>
        public ImportSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }

        public bool IsOnTime
        {
            get { return _isOnTime; }
            set { _isOnTime = value; }
        }

        #endregion // Public properties

        #region ICloneable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public object Clone()
        {
            ImportProfile obj = new ImportProfile();
            obj._name = this._name;
            obj._type = this._type;
            obj._isDefault = this._isDefault;
            obj._isOnTime = this._isOnTime;
            obj._description = this._description;
            obj._settings = this._settings.Clone() as ImportSettings;

            return obj;
        }

        #endregion // ICloneable members

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _name = string.Empty;
        private ImportType _type = ImportType.Orders;
        private bool _isDefault = false;
        private string _description = string.Empty;
        private ImportSettings _settings = new ImportSettings();
        private bool _isOnTime = false;

        #endregion // Private members
    }
}

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

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Import settings class.
    /// </summary>
    internal class ImportSettings : ICloneable
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of ImportSettings class.
        /// </summary>
        public ImportSettings()
        {
        }
        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Data source
        /// </summary>
        /// <remarks>File path or DataLink connection string</remarks>
        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }

        /// <summary>
        /// Name of the Table
        /// </summary>
        /// <remarks>Can be Empty</remarks>
        public string TableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }

        /// <summary>
        /// Field map
        /// </summary>
        public List<FieldMap> FieldsMap
        {
            get { return _fieldsMap; }
            set { _fieldsMap = value; }
        }
        #endregion // Public properties

        #region ICloneable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public object Clone()
        {
            ImportSettings obj = new ImportSettings();
            obj._source = this._source;
            obj._tableName = this._tableName;

            foreach (FieldMap map in this._fieldsMap)
                obj._fieldsMap.Add(map);

            return obj;
        }
        #endregion // ICloneable members

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _source = string.Empty;
        private string _tableName = string.Empty;
        private List<FieldMap> _fieldsMap = new List<FieldMap>();
        #endregion // Private members
    }
}

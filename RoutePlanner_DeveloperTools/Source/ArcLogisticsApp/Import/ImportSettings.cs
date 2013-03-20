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

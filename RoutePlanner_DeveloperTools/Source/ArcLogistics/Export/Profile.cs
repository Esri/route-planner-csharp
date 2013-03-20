using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.Export
{
    /// <summary>
    /// Export profile type.
    /// </summary>
    public enum ExportType
    {
        /// <summary>
        /// Export all data to access file (.mdb).
        /// </summary>
        Access,
        /// <summary>
        /// Export routes data to text file (.txt or .csv).
        /// </summary>
        TextRoutes,
        /// <summary>
        /// Export stops data to text file (.txt or .csv).
        /// </summary>
        TextStops,
        /// <summary>
        /// Export orders date to text file (.txt or .csv).
        /// </summary>
        TextOrders,
        /// <summary>
        /// Export routes data to shape file (.shp).
        /// </summary>
        ShapeRoutes,
        /// <summary>
        /// Export stops data to shape file (.shp).
        /// </summary>
        ShapeStops
    }

    /// <summary>
    /// Class that represents export profile.
    /// </summary>
    public class Profile : ICloneable
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates instance.
        /// </summary>
        /// <param name="structureKeeper">Export structure keeper.</param>
        /// <param name="type">Export type.</param>
        /// <param name="filePath">Export file path.</param>
        /// <param name="isDefault">Is profile default.</param>
        internal Profile(ExportStructureReader structureKeeper,
                         ExportType type, string filePath, bool isDefault)
        {
            Debug.Assert(null != structureKeeper);

            _structureKeeper = structureKeeper;
            _InitTables(type);

            _name = System.IO.Path.GetFileNameWithoutExtension(filePath);
            _type = type;
            _file = filePath;
            _isDefault = isDefault;
        }

        #endregion // Constructors

        #region Public interface
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Profile name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Profile export type.
        /// </summary>
        public ExportType Type { get { return _type; } }

        /// <summary>
        /// Export profile description.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Full path to the file where data will be exported.
        /// </summary>
        public string FilePath
        {
            get { return _file; }
            set { _file = value; }
        }

        /// <summary>
        /// Gets collection of export table definitions.
        /// </summary> 
        public ICollection<ITableDefinition> TableDefinitions
        {
            get { return _tables.AsReadOnly(); }
        }

        /// <summary>
        /// Is default profile flag.
        /// </summary>
        public bool IsDefault
        {
            get { return _isDefault; }
        }

        #endregion // Public interface

        #region ICloneable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public object Clone()
        {
            Profile obj = new Profile(this._structureKeeper, this._type, this._file, this._isDefault);
            obj._name = this._name;
            obj._type = this._type;
            obj._description = this._description;

            foreach (ITableDefinition table in this._tables)
            {
                foreach (ITableDefinition tableObj in obj._tables)
                {
                    if (table.Type != tableObj.Type)
                        continue; // NOKE: skip this

                    foreach (string field in table.Fields)
                        tableObj.AddField(field);
                }
            }

            return obj;
        }
        #endregion // ICloneable members

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Inits tables for selected export type.
        /// </summary>
        /// <param name="type">Export type.</param>
        private void _InitTables(ExportType type)
        {
            _tables.Clear();

            bool isShortNameMode = _IsShortNameMode(type);
            foreach (TableInfo table in _structureKeeper.GetPattern(type))
            {
                TableDescription description = _structureKeeper.GetTableDescription(table.Type);
                _tables.Add(new TableDefinition(description, table.IgnoredFields, isShortNameMode));
            }
        }

        /// <summary>
        /// Checks for this export type use short name mode.
        /// </summary>
        /// <param name="type">Export type.</param>
        /// <returns>TRUE if need use short name mode.</returns>
        private bool _IsShortNameMode(ExportType type)
        {
            return ((ExportType.ShapeRoutes == type) || (ExportType.ShapeStops == type));
        }

        #endregion // Private methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Profile name.
        /// </summary>
        private string _name;
        /// <summary>
        /// Profile description.
        /// </summary>
        private string _description;
        /// <summary>
        /// Is profile default.
        /// </summary>
        private bool _isDefault;

        /// <summary>
        /// Export type.
        /// </summary>
        private ExportType _type = ExportType.Access;
        /// <summary>
        /// Export file path.
        /// </summary>
        private string _file;
        /// <summary>
        /// Export table definitions.
        /// </summary>
        List<ITableDefinition> _tables = new List<ITableDefinition>();
        /// <summary>
        /// Export structure keeper.
        /// </summary>
        ExportStructureReader _structureKeeper;

        #endregion // Private members
    }
}

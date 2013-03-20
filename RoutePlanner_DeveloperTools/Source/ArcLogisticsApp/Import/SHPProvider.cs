using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using ESRI.ArcLogistics.ShapefileReader;
using System.IO;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Shape file data provider class
    /// </summary>
    internal sealed class SHPProvider : IDataProvider
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public SHPProvider()
        { }
        #endregion // Constructors

        #region static helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets shape type of file.
        /// </summary>
        /// <param name="fileName">Shape file full name.</param>
        /// <param name="messageFailure">Message failure.</param>
        /// <returns>Readed shape type.</returns>
        public static ShapeType GetShapeType(string fileName, out string messageFailure)
        {
            Debug.Assert(!string.IsNullOrEmpty(fileName));

            ShapeType type = ShapeType.Null;
            messageFailure = null;
            try
            {
                Shapefile file = new Shapefile(fileName, false);
                if (file.RecordCount < 1)
                    throw new NotSupportedException();

                Shape shape = file.ReadRecord(0);
                if (null == shape)
                    throw new NotSupportedException();

                type = shape.ShapeType;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                messageFailure = e.Message;
            }

            return type;
        }

        /// <summary>
        /// Check that projection file is present and projection is right.
        /// </summary>
        /// <param name="shapeFilePath">Path to shape file.</param>
        /// <param name="messageFailure">Message failure.</param>
        public static bool ProjectionIsRight(string shapeFilePath, out string messageFailure)
        {
            Debug.Assert(!string.IsNullOrEmpty(shapeFilePath));

            // Check that projection file is present.
            var prjFilePath = Path.ChangeExtension(shapeFilePath, PROJECTION_FILE_EXTENSION);
            try
            {
                // Check that projection is right.
                var projectionFileStart = File.ReadAllText(prjFilePath);
                if (!projectionFileStart.StartsWith(WGS1984_FILESTART))
                {
                    messageFailure = Properties.Resources.Error_WrongOrUnknownProjection;
                    return false;
                }
            }
            // Catch exception in case if projection file isn't present.
            catch(FileNotFoundException ex)
            {
                messageFailure = Properties.Resources.Error_WrongOrUnknownProjection;
                return false;
            }

            // If we came here - projection is right.
            messageFailure = null;
            return true;
        }

        #endregion // static helpers

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Inits procedure.
        /// </summary>
        /// <param name="fileName">Shape file full name.</param>
        /// <param name="messageFailure">Message failure.</param>
        /// <returns>TRUE if initialize successed.</returns>
        public bool Init(string fileName, out string messageFailure)
        {
            Debug.Assert(!string.IsNullOrEmpty(fileName));

            messageFailure = null;
            try
            {
                _file = new Shapefile(fileName, false);

                DataTable table = _file.SchemaTable;
                foreach (DataRow row in table.Rows)
                {
                    string name = (string)row["ColumnName"];
                    if (!string.IsNullOrEmpty(name.Trim()))
                        _fieldsInfo.Add(new DataFieldInfo(name, (Type)row["DataType"]));
                }
            }
            catch (Exception e)
            {
                _file = null;

                Logger.Error(e);

                messageFailure = e.Message;

                // WORKAROUND: hardcoded replace info message
                if (messageFailure.Contains(INVALID_FILE_SHAPELIB_ERROR_TEXT))
                {
                    messageFailure = App.Current.FindString("ImportSHPFileInvalidMessage");
                }
            }

            return _IsInited;
        }
        #endregion // public methods

        #region IDataProvider
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Records count
        /// </summary>
        public int RecordsCount
        {
            get
            {
                Debug.Assert(_IsInited);
                return _file.RecordCount;
            }
        }

        /// <summary>
        /// Set cursor to first row
        /// </summary>
        public void MoveFirst()
        {
            Debug.Assert(_IsInited);

            _currentRowNum = 0;
            _currentRow = _file.ReadAttributes(_currentRowNum);
        }

        /// <summary>
        /// Move cursor to next row
        /// </summary>
        public void MoveNext()
        {
            Debug.Assert(_IsInited);
            Debug.Assert(!IsEnd());

            ++_currentRowNum;
            _currentRow = (IsEnd()) ? null : _file.ReadAttributes(_currentRowNum);
        }

        /// <summary>
        /// All records iterated
        /// </summary>
        public bool IsEnd()
        {
            Debug.Assert(_IsInited);

            return (RecordsCount <= _currentRowNum);
        }

        /// <summary>
        /// Field count
        /// </summary>
        public int FieldCount
        {
            get
            {
                Debug.Assert(_IsInited);
                return _fieldsInfo.Count;
            }
        }

        /// <summary>
        /// Obtain fields info list
        /// </summary>
        public ICollection<DataFieldInfo> FieldsInfo
        {
            get
            {
                Debug.Assert(_IsInited);
                return _fieldsInfo;
            }
        }

        /// <summary>
        /// Obtain field value
        /// </summary>
        public object FieldValue(int index)
        {
            Debug.Assert(null != _currentRow);
            return _currentRow[_fieldsInfo[index].Name];
        }
        
        /// <summary>
        /// Flag is current record empty
        /// </summary>
        public bool IsRecordEmpty
        {
            get { return false; }
        }

        /// <summary>
        /// Flag is format support geometry
        /// </summary>
        public bool IsGeometrySupport
        {
            get { return true; }
        }

        /// <summary>
        /// Geometry
        /// </summary>
        /// <remarks>if format not support geometry - return null</remarks>
        public object Geometry
        {
            get
            {
                Debug.Assert(_IsInited);

                Shape shape = _file.ReadRecord(_currentRowNum);

                return (object)shape.Geometry;
            }
        }
        #endregion // IDataProvider

        #region Private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is inited flag.
        /// </summary>
        public bool _IsInited
        {
            get
            {
                return (_file != null);
            }
        }

        #endregion // Private properties

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string INVALID_FILE_SHAPELIB_ERROR_TEXT = "Unable to find the specified file";

        /// <summary>
        /// File start for a projection file for WGS 1984 projection.
        /// </summary>
        private const string WGS1984_FILESTART = "GEOGCS[\"GCS_WGS_1984\"";

        /// <summary>
        /// Projection file extension.
        /// </summary>
        private const string PROJECTION_FILE_EXTENSION = ".prj";

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Shapefile _file = null;
        private readonly List<DataFieldInfo> _fieldsInfo = new List<DataFieldInfo>();

        private DataRow _currentRow = null;
        private int _currentRowNum = 0;
        #endregion // Private members
    }
}

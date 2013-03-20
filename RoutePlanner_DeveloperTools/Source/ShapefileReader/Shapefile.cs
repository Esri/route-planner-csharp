using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Esri.ShapefileReader;

namespace ESRI.ArcLogistics.ShapefileReader
{
    /// <summary>
    /// Shapefile class perform reading of ESRI Shape Files.
    /// </summary>
    public class Shapefile
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="loadIntoMemory">Load into memory.</param>
        public Shapefile(string path, bool loadIntoMemory)
        {
            _file = new Esri.ShapefileReader.Shapefile(path, loadIntoMemory);
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Records count.
        /// </summary>
        public int RecordCount
        {
            get { return _file.RecordCount; }
        }

        /// <summary>
        /// Schema table.
        /// </summary>
        public DataTable SchemaTable
        {
            get { return _file.SchemaTable; }
        }

        /// <summary>
        /// SRID.
        /// </summary>
        public int SRID
        {
            get { return _file.SRID; }
        }

        /// <summary>
        /// SRWKT.
        /// </summary>
        public string SRWKT
        {
            get { return _file.SRWKT; }
        }

        /// <summary>
        /// Method read attributes from file for current index.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <returns>DataRow associated to index.</returns>
        public DataRow ReadAttributes(int index)
        {
            return _file.ReadAttributes(index);
        }

        /// <summary>
        /// Method read Shape record from file by given record number.
        /// </summary>
        /// <param name="recNum">Record number.</param>
        /// <returns>Shape.</returns>
        public Shape ReadRecord(int recNum)
        {
            Esri.ShapefileReader.Shape shape = _file.ReadRecord(recNum);

            Shape newShape = new Shape(shape);

            return newShape;
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Shape file.
        /// </summary>
        private Esri.ShapefileReader.Shapefile _file;

        #endregion
    }
}

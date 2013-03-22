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
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Data provider class
    /// </summary>
    internal class DBProvider : IDataProvider
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public DBProvider(DataTable table)
        {
            Debug.Assert(null != table);

            _table = table;

            DataColumnCollection drc = table.Columns;
            foreach (DataColumn dc in drc)
            {
              DataFieldInfo info = new DataFieldInfo(dc.ColumnName, dc.DataType);
              _fieldsInfo.Add(info);
            }
        }
        #endregion // Constructors

        #region IDataProvider
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Records count
        /// </summary>
        public int RecordsCount
        {
            get { return _table.Rows.Count; }
        }

        /// <summary>
        /// Set cursor to first row
        /// </summary>
        public void MoveFirst()
        {
            _currentRowNum = 0;
            _currentRow = _table.Rows[_currentRowNum];
        }

        /// <summary>
        /// Move cursor to next row
        /// </summary>
        public void MoveNext()
        {
            Debug.Assert(!IsEnd());

            ++_currentRowNum;
            _currentRow =(IsEnd())? null : _table.Rows[_currentRowNum];
        }

        /// <summary>
        /// All records iterated
        /// </summary>
        public bool IsEnd()
        {
            return (RecordsCount <= _currentRowNum);
        }

        /// <summary>
        /// Field count
        /// </summary>
        public int FieldCount
        {
            get { return _table.Columns.Count; }
        }

        /// <summary>
        /// Obtain fields info list
        /// </summary>
        public ICollection<DataFieldInfo> FieldsInfo
        {
            get { return _fieldsInfo; }
        }

        /// <summary>
        /// Obtain field value
        /// </summary>
        public object FieldValue(int index)
        {
            Debug.Assert(null != _currentRow);
            return _currentRow[index];
        }

        /// <summary>
        /// Flag is current record empty
        /// </summary>
        public bool IsRecordEmpty
        {
            get
            {
                Debug.Assert(null != _currentRow);
                bool isEmpty = true;
                foreach (object item in _currentRow.ItemArray)
                {
                    if (!string.IsNullOrEmpty(item.ToString()))
                    {
                        isEmpty = false;
                        break; // NOTE: result founded. Exit
                    }
                }

                return isEmpty;
            }
        }

        /// <summary>
        /// Flag is format support geometry
        /// </summary>
        public bool IsGeometrySupport
        {
            get { return false; }
        }

        /// <summary>
        /// Geometry
        /// </summary>
        /// <remarks>if format not support geometry - return null</remarks>
        public object Geometry
        {
            get
            {
                Debug.Assert(IsGeometrySupport);
                return null;
            }
        }
        #endregion // IDataProvider

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DataTable _table = null;
        private readonly List<DataFieldInfo> _fieldsInfo = new List<DataFieldInfo>();

        private DataRow _currentRow = null;
        private int _currentRowNum = 0;
        #endregion // Private members
    }
}

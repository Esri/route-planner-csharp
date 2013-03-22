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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ESRI.ArcLogistics.Export
{
    /// <summary>
    /// Table definition base class.
    /// </summary>
    internal sealed class TableDefinition : ITableDefinition
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>TableDefinition</c> class.
        /// </summary>
        /// <param name="description">Description of table.</param>
        /// <param name="ignorableFields">Ignorable fields.</param>
        /// <param name="isShortNamesMode">Is short names mode indicator.</param>
        public TableDefinition(TableDescription description,
                               StringCollection ignorableFields,
                               bool isShortNamesMode)
        {
            Debug.Assert(null != description);
            Debug.Assert(null != ignorableFields);

            _type = description.Type;
            _name = description.Name;

            foreach (string fieldName in description.GetFieldNames())
            {
                if (ignorableFields.Contains(fieldName))
                    continue; // skip ignorable fields

                // set default selected
                Debug.Assert(null != description.GetFieldInfo(fieldName));
                FieldInfo fieldInfo = description.GetFieldInfo(fieldName);
                if (fieldInfo.IsDefault)
                    _fields.Add(fieldName);

                if (fieldInfo.IsHidden)
                    _hiddenFields.Add(fieldName);
                else
                    _supportedFields.Add(fieldName);

                string name = (isShortNamesMode) ? fieldInfo.ShortName : fieldInfo.LongName;
                _mapFaceNameByName.Add(fieldName, name);
                _mapDescriptionByName.Add(fieldName, fieldInfo.Description);
            }
        }

        #endregion // Constructors

        #region TableDefinitionI interface
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Table type.
        /// </summary>
        public TableType Type { get { return _type; } }

        /// <summary>
        /// Table name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets collection of supported field names.
        /// </summary>
        public ICollection<string> SupportedFields
        {
            get { return _supportedFields.AsReadOnly(); }
        }

        /// <summary>
        /// Adds field to export field collection.
        /// </summary>
        /// <remarks>Name from <c>SupportedFields</c> collection.</remarks>
        public void AddField(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            if (!_supportedFields.Contains(name) && !_hiddenFields.Contains(name))
                throw new ArgumentException(Properties.Resources.InvalidTableFieldName, name); // exception

            if (!_fields.Contains(name))
                _fields.Add(name);
        }

        /// <summary>
        /// Removes field from export field collection.
        /// </summary>
        /// <remarks>Name from <c>SupportedFields</c> collection.</remarks>
        public void RemoveField(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            if (_fields.Contains(name))
                _fields.Remove(name);
        }

        /// <summary>
        /// Removes all fields from export fields collection.
        /// </summary>
        public void ClearFields()
        {
            _fields.Clear();
        }

        /// <summary>
        /// Gets field collection that must be present in the export table.
        /// </summary>
        public ICollection<string> Fields
        {
            get { return _fields.AsReadOnly(); }
        }

        /// <summary>
        /// Gets field title by name.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <returns>Returns localizable field name that corresponds to the <c>name</c> field.</returns>
        public string GetFieldTitleByName(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            string result = string.Empty;
            if (_mapFaceNameByName.ContainsKey(name))
                result = _mapFaceNameByName[name];

            return result;
        }

        /// <summary>
        /// Gets field name by title.
        /// </summary>
        /// <param name="faceName">Field title.</param>
        /// <returns>Returns field name that corresponds to the <c>faceName</c> title.</returns>
        public string GetFieldNameByTitle(string faceName)
        {
            Debug.Assert(!string.IsNullOrEmpty(faceName));

            string result = string.Empty;
            if (_mapFaceNameByName.ContainsValue(faceName))
            {
                Dictionary<string, string>.Enumerator enumerator =
                    _mapFaceNameByName.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<string, string> pair = enumerator.Current;
                    if (faceName.Equals(pair.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        result = pair.Key;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets field description by name.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <returns>Returns localizable field description.</returns>
        public string GetDescriptionByName(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            string result = string.Empty;
            if (_mapDescriptionByName.ContainsKey(name))
                result = _mapDescriptionByName[name];

            return result;
        }

        #endregion // TableDefinitionI interface

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of table.
        /// </summary>
        private string _name;
        /// <summary>
        /// Type of table.
        /// </summary>
        private TableType _type = TableType.Schedules;

        /// <summary>
        /// Collection of tables selected field names.
        /// </summary>
        private List<string> _fields = new List<string>();
        /// <summary>
        /// Collection of supported field names.
        /// </summary>
        private List<string> _supportedFields = new List<string>();
        /// <summary>
        /// Collection of hidden field names.
        /// </summary>
        private List<string> _hiddenFields = new List<string>();
        /// <summary>
        /// Face name by internal name dictionary.
        /// </summary>
        private Dictionary<string, string> _mapFaceNameByName = new Dictionary<string,string>();
        /// <summary>
        /// Face name by internal name dictionary.
        /// </summary>
        private Dictionary<string, string> _mapDescriptionByName = new Dictionary<string, string>();

        #endregion // Private members
    }
}

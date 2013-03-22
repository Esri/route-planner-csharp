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
using System.Collections.Generic;

using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.Export
{
    /// <summary>
    /// Class that reperesents a validator of exporter data.
    /// </summary>
    internal sealed class ExportValidator
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of ExportValidator.
        /// </summary>
        /// <param name="capacitiesInfo">Current capacities info.</param>
        /// <param name="addressFields">Current address fields.</param>
        public ExportValidator(CapacitiesInfo capacitiesInfo,
                               AddressField[] addressFields)
        {
            if (null == capacitiesInfo)
                throw new ArgumentNullException("capacitiesInfo"); // exception

            if (null == addressFields)
                throw new ArgumentNullException("addressFields"); // exception

            // load export structure
            ExportStructureReader reader = Exporter.GetReader(capacitiesInfo,
                                                              new OrderCustomPropertiesInfo(),
                                                              addressFields);

            _readedNames = new List<string>();
            _GetFieldNames(reader, ExportType.Access, _readedNames);
            _GetFieldNames(reader, ExportType.TextOrders, _readedNames);
            _GetFieldNames(reader, ExportType.TextStops, _readedNames);

            _description = reader.GetTableDescription(TableType.Stops);
        }

        #endregion // Constructors

        #region Publics methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Checks if custom order property with given name is unique for other export field name.
        /// </summary>
        /// <param name="name">Name of a custom order property.</param>
        /// <param name="orderCustomPropertyNames">Current custom order property names.</param>
        /// <returns>True - fields with given name not found, otherwise - false.</returns>
        public bool IsCustomOrderFieldNameUnique(string name, ICollection<string> orderCustomPropertyNames)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(); // exception

            // find name in export field names
            bool result = true;
            string validateName = _description.ValidateRelativeName(name);
            if (!string.IsNullOrEmpty(validateName))
            {
                for (int index = 0; index < _readedNames.Count; ++index)
                {
                    string fieldName = _readedNames[index];
                    if (fieldName.Equals(validateName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result = false;
                        break; // NOTE: result found
                    }
                }
            }

            // check order custom properties
            if (result)
            {
                // counter of properties with given name.
                int equalNamesCount = 0;

                // look for custom order property with the same name in collection.
                foreach (string customPropertyName in orderCustomPropertyNames)
                {
                    string validateCustomPropertyName = _description.ValidateRelativeName(customPropertyName);
                    if (validateCustomPropertyName.Equals(validateName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ++equalNamesCount;
                        // name must present only once
                        if (1 < equalNamesCount)
                        {
                            result = false;
                            break; // NOTE: result found
                        }
                    }
                }
            }

            return result;
        }

        #endregion // Publics methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets field names from export table description.
        /// </summary>
        /// <param name="reader">Export structure reader to decsription access.</param>
        /// <param name="type">Export type.</param>
        /// <param name="readedNames">Readed unique names (in\out).</param>
        private void _GetFieldNames(ExportStructureReader reader, ExportType type, IList<string> readedNames)
        {
            Debug.Assert(null != reader);
            Debug.Assert(null != readedNames);

            ICollection<TableInfo> tableInfos = reader.GetPattern(type);
            foreach (TableInfo tableInfo in tableInfos)
            {
                if ((TableType.Stops != tableInfo.Type) && (TableType.Orders != tableInfo.Type))
                    continue; // NOTE: skip other tables

                TableDescription descr = reader.GetTableDescription(tableInfo.Type);
                foreach (string name in descr.GetFieldNames())
                {
                    if (!readedNames.Contains(name))
                    {
                        readedNames.Add(name);
                    }
                }
            }
        }

        #endregion // Private methods

        #region Private Fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Readed export field names (without Custom Order Properties fields).
        /// </summary>
        private IList<string> _readedNames;
        /// <summary>
        /// Table description - for get access to name conversion.
        /// </summary>
        private TableDescription _description;

        #endregion // Private Fields
    }
}

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
using System.Xml;
using System.Data.OleDb;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;

using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.Export
{
    /// <summary>
    /// Field info class.
    /// </summary>
    internal sealed class FieldInfo : ICloneable
    {
        /// <summary>
        /// Language-independent name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Language-dependent long name.
        /// </summary>
        public string LongName { get; set; }

        /// <summary>
        /// Language-dependent short name.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Field description.
        /// </summary>
        /// <remarks>can be empty</remarks>
        public string Description { get; set; }

        /// <summary>
        /// Relation type name.
        /// </summary>
        /// <remarks>can be empty, "Address", "Capacities" or "CustomOrderProperties"</remarks>
        public string RelationType { get; set; }

        /// <summary>
        /// Format string for name creation.
        /// </summary>
        /// <remarks> if RelationType not empty - containt format string, otherwise empty</remarks>
        public string NameFormat { get; set; }

        // Data settings.

        /// <summary>
        /// Data type.
        /// </summary>
        public OleDbType Type { get; set; }
        /// <summary>
        /// Data size.
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// Data precizion.
        /// </summary>
        public int Precision { get; set; }
        /// <summary>
        /// Data scale.
        /// </summary>
        public int Scale { get; set; }

        /// <summary>
        /// Is field select as default.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Is field store as image.
        /// </summary>
        public bool IsImage { get; set; }

        /// <summary>
        /// Is field hidden from GUI.
        /// </summary>
        public bool IsHidden { get; set; }

        #region ICloneable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clone procedure.
        /// </summary>
        /// <returns>Deep copy of object.</returns>
        public object Clone()
        {
            FieldInfo obj = new FieldInfo();
            obj.RelationType = this.RelationType;
            obj.NameFormat = this.NameFormat;
            obj.Type = this.Type;
            obj.Size = this.Size;
            obj.Precision = this.Precision;
            obj.Scale = this.Scale;
            obj.IsDefault = this.IsDefault;
            obj.IsHidden = this.IsHidden;
            obj.Name = this.Name;
            obj.LongName = this.LongName;
            obj.ShortName = this.ShortName;
            obj.Description = this.Description;
            obj.IsImage = this.IsImage;

            return obj;
        }

        #endregion // ICloneable members
    }

    /// <summary>
    /// Possible table index types.
    /// </summary>
    internal enum TableIndexType
    {
        Primary,
        Simple,
        Multiple
    }

    /// <summary>
    /// Table index class.
    /// </summary>
    internal sealed class TableIndex
    {
        /// <summary>
        /// Index's field names.
        /// </summary>
        public StringCollection FieldNames { get; set; }
        /// <summary>
        /// Index type.
        /// </summary>
        public TableIndexType Type { get; set; }
    }

    /// <summary>
    /// Table info structure.
    /// </summary>
    internal sealed class TableInfo
    {
        /// <summary>
        /// Table type.
        /// </summary>
        public TableType Type { get; set; }
        /// <summary>
        /// Ignored fields.
        /// </summary>
        public StringCollection IgnoredFields { get; set; }
        /// <summary>
        /// Table indexes.
        /// </summary>
        public List<TableIndex> Indexes { get; set; }
    }

    /// <summary>
    /// Class that represents a table full description.
    /// </summary>
    internal sealed class TableDescription
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>TableDescription</c> class.
        /// </summary>
        /// <param name="name">Table name.</param>
        /// <param name="type">Table type.</param>
        /// <param name="fields">Table fields.</param>
        /// <param name="capacityInfos">Capacity infos.</param>
        /// <param name="orderCustomPropertyInfos">Order custom properties infos.</param>
        /// <param name="addressFields">Address fields.</param>
        public TableDescription(string name,
                                TableType type,
                                List<FieldInfo> fields,
                                CapacitiesInfo capacityInfos,
                                OrderCustomPropertiesInfo orderCustomPropertyInfos,
                                AddressField[] addressFields)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            Debug.Assert(0 < fields.Count);
            Debug.Assert(null != capacityInfos);
            Debug.Assert(null != orderCustomPropertyInfos);
            Debug.Assert(null != addressFields);

            _capacityInfos = capacityInfos;
            _orderCustomPropertyInfos = orderCustomPropertyInfos;
            _addressFields = addressFields;

            _name = name;
            _type = type;

            foreach (FieldInfo info in fields)
            {
                Debug.Assert(!_fieldsMap.ContainsKey(info.Name));

                if (string.IsNullOrEmpty(info.RelationType))
                    _fieldsMap.Add(info.Name, info);
                else
                {   // special routine to relative fields
                    switch (info.RelationType)
                    {
                        case "Capacities":
                            _AddCapacityRelativeFields(capacityInfos, info);
                            break;

                        case "CustomOrderProperties":
                            _AddCustomOrderPropertyRelativeFields(orderCustomPropertyInfos, info);
                            break;

                        case "Address":
                            _AddAddressRelativeFields(addressFields, info);
                            break;

                        default:
                            Debug.Assert(false); // NOTE: not supported
                            break;
                    }
                }
            }
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Table name.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Table property.
        /// </summary>
        public TableType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets address fields.
        /// </summary>
        public AddressField[] AddressFields
        {
            get { return _addressFields; }
        }

        #endregion  // Public properties

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets all field names.
        /// </summary>
        /// <returns>Presented field names.</returns>
        public ICollection<string> GetFieldNames()
        {
            return _fieldsMap.Keys;
        }

        /// <summary>
        /// Gets field info by field name.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <returns>Field information.</returns>
        public FieldInfo GetFieldInfo(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            if (null == _fieldsMap)
                return null;

            return _fieldsMap[name];
        }

        /// <summary>
        /// Gets capacity index by name.
        /// </summary>
        /// <param name="name">Capacity name.</param>
        /// <param name="nameFormat">Capacity name format.</param>
        /// <returns>Capacity index, or 0 if not founded.</returns>
        public int GetCapacityIndex(string name, string nameFormat)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            Debug.Assert(!string.IsNullOrEmpty(nameFormat));

            for (int index = 0; index < _capacityInfos.Count; ++index)
            {
                string nameCapacity =
                    string.Format(nameFormat, ValidateRelativeName(_capacityInfos[index].Name));
                if (name == nameCapacity)
                    return index;
            }

            Debug.Assert(false);
            return 0;
        }

        /// <summary>
        /// Gets custom order property index by name.
        /// </summary>
        /// <param name="name">Custom order property name.</param>
        /// <param name="nameFormat">Custom order property name format.</param>
        /// <returns>Custom order property index, or 0 if not founded.</returns>
        public int GetOrderCustomPropertyIndex(string name, string nameFormat)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            Debug.Assert(!string.IsNullOrEmpty(nameFormat));

            for (int index = 0; index < _orderCustomPropertyInfos.Count; ++index)
            {
                string relativeName = ValidateRelativeName(_orderCustomPropertyInfos[index].Name);
                string nameProperty =
                    string.Format(nameFormat, relativeName);
                if (name == nameProperty)
                    return index;
            }

            Debug.Assert(false);
            return 0;
        }

        /// <summary>
        /// Validates relative field name.
        /// </summary>
        /// <param name="infoName">Readed field name.</param>
        /// <returns>Validated field name.</returns>
        public string ValidateRelativeName(string infoName)
        {
            Debug.Assert(!string.IsNullOrEmpty(infoName));

            string name = infoName.Trim();
            name = name.Replace(" ", ""); // Remove spaces symbols

            Debug.Assert(!string.IsNullOrEmpty(name));
            return name;
        }

        #endregion  // Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Updates info with info name.
        /// </summary>
        /// <param name="infoName">Property name.</param>
        /// <param name="infoRead">Readed info.</param>
        /// <param name="info">Validated info.</param>
        private void _UpdateInfoWithInfoName(string infoName,
                                             FieldInfo infoRead,
                                             ref FieldInfo info)
        {
            Debug.Assert(!string.IsNullOrEmpty(infoRead.Name));
            Debug.Assert(!string.IsNullOrEmpty(infoRead.LongName));
            Debug.Assert(!string.IsNullOrEmpty(infoRead.ShortName));

            string name = ValidateRelativeName(infoName);

            info.Name = string.Format(infoRead.Name, name);
            info.LongName = string.Format(infoRead.LongName, name);

            string shortName = string.Format(infoRead.ShortName, name);
            info.ShortName = (SHORT_NAME_LENGTH < shortName.Length)?
                                        shortName.Substring(0, SHORT_NAME_LENGTH) : shortName;
        }

        /// <summary>
        /// Formats capacity description.
        /// </summary>
        /// <param name="capacityInfo">Capacity information.</param>
        /// <param name="descriptionFormat">Description format.</param>
        /// <returns>Formated description string.</returns>
        private string _FormatCapacityDescription(CapacityInfo capacityInfo,
                                                  string descriptionFormat)
        {
            string result = null;
            string capacityName = capacityInfo.Name.ToLower();
            if (capacityInfo.DisplayUnitMetric == capacityInfo.DisplayUnitUS)
            {   // one effective Unit
                if (capacityInfo.DisplayUnitMetric == Unit.Unknown)
                {   // The maximum {0} for the route.
                    int startRemoveIndex = descriptionFormat.IndexOf('(') - 1;
                    Debug.Assert(0 < startRemoveIndex);

                    string format =
                        descriptionFormat.Remove(startRemoveIndex,
                                                 descriptionFormat.Length - 1 - startRemoveIndex);
                                                 // -1 last point must present.
                    result = string.Format(format, capacityName);
                }
                else
                {   // The maximum {0} for the route (in predefined units; {1}).
                    int startRemoveIndex = descriptionFormat.IndexOf(";") + 2; // show space
                    Debug.Assert(0 < startRemoveIndex);
                    int sropRemoveIndex = descriptionFormat.IndexOf(FORMAT_FIRST_ELEMENT);
                    Debug.Assert(0 < sropRemoveIndex);
                    string format =
                        descriptionFormat.Remove(startRemoveIndex,
                                                 sropRemoveIndex - startRemoveIndex);
                    int startRemoveIndex2 =
                        format.IndexOf(FORMAT_FIRST_ELEMENT) + FORMAT_FIRST_ELEMENT.Length;
                    Debug.Assert(0 < startRemoveIndex2);
                    format =
                        format.Remove(startRemoveIndex2, format.Length - 2 - startRemoveIndex2);
                        // -1 last point must present.
                    result = string.Format(format, capacityName,
                                           UnitFormatter.GetUnitTitle(capacityInfo.DisplayUnitUS));
                }
            }
            else
            {   // The maximum {0} for the route (in predefined units; may be {1} or {2} depending
                // on your location).
                string format = descriptionFormat;
                result = string.Format(format, capacityName,
                                       UnitFormatter.GetUnitTitle(capacityInfo.DisplayUnitUS),
                                       UnitFormatter.GetUnitTitle(capacityInfo.DisplayUnitMetric));
            }

            return result;
        }

        /// <summary>
        /// Formats capacity total description.
        /// </summary>
        /// <param name="capacityInfo">Capacity information.</param>
        /// <param name="descriptionFormat">Description format.</param>
        /// <returns>Formated description string.</returns>
        private string _FormatCapacityDescriptionTotal(CapacityInfo capacityInfo,
                                                       string descriptionFormat)
        {
            string result = null;
            string capacityName = capacityInfo.Name.ToLower();
            if (capacityInfo.DisplayUnitMetric == capacityInfo.DisplayUnitUS)
            {   // one effective Unit
                string unit = (Unit.Unknown == capacityInfo.DisplayUnitUS)?
                                    Unit.Unknown.ToString().ToLower() :
                                    UnitFormatter.GetUnitTitle(capacityInfo.DisplayUnitUS);
                // Total {0} of all goods in {1}.
                int startRemoveIndex =
                    descriptionFormat.IndexOf(FORMAT_FIRST_ELEMENT) + FORMAT_FIRST_ELEMENT.Length;
                Debug.Assert(0 < startRemoveIndex);
                string format =
                    descriptionFormat.Remove(startRemoveIndex,
                                             descriptionFormat.Length - 1 - startRemoveIndex);
                                             // -1 last point must present.
                result = string.Format(format, capacityName, unit);
            }
            else
            {   // Total {0} of all goods in {1} or {2} for the route.
                string format = descriptionFormat;
                result = string.Format(format, capacityName,
                                       UnitFormatter.GetUnitTitle(capacityInfo.DisplayUnitUS),
                                       UnitFormatter.GetUnitTitle(capacityInfo.DisplayUnitMetric));
            }

            return result;
        }

        /// <summary>
        /// Formats capacity utilization description.
        /// </summary>
        /// <param name="capacityInfo">Capacity information.</param>
        /// <param name="descriptionFormat">Description format.</param>
        /// <returns>Formated description string.</returns>
        private string _FormatCapacityDescriptionUtilization(CapacityInfo capacityInfo,
                                                             string descriptionFormat)
        {
            // Percent (%) of available capacity ({0}) used on the route (100 * ({1}/{2})).
            string capacityName = capacityInfo.Name;
            string capacityNameLower = capacityName.ToLower();
            string capacity =
                string.Format(Properties.Resources.ExportFieldNameLongCapacity, capacityName);
            string capacityTotal =
                string.Format(Properties.Resources.ExportFieldNameLongTotal, capacityName);
            return string.Format(descriptionFormat, capacityNameLower, capacity, capacityTotal);
        }

        /// <summary>
        /// Adds capacity relative fields.
        /// </summary>
        /// <param name="capacityInfos">Capacities informations.</param>
        /// <param name="info">Field information with data settings.</param>
        private void _AddCapacityRelativeFields(CapacitiesInfo capacityInfos, FieldInfo info)
        {
            for (int index = 0; index < capacityInfos.Count; ++index)
            {
                FieldInfo infoRealtion = (FieldInfo)info.Clone();

                CapacityInfo capacityInfo = capacityInfos[index];
                _UpdateInfoWithInfoName(capacityInfo.Name, info, ref infoRealtion);

                // format description
                if (!string.IsNullOrEmpty(infoRealtion.Description))
                {
                    if ((infoRealtion.Description == Properties.Resources.ExportFieldDescriptionCapacity) ||
                        (infoRealtion.Description == Properties.Resources.ExportFieldDescriptionRelativeCapacity))
                    {
                        infoRealtion.Description =
                            _FormatCapacityDescription(capacityInfo, infoRealtion.Description);
                    }

                    else if (infoRealtion.Description == Properties.Resources.ExportFieldDescriptionTotal)
                    {
                        infoRealtion.Description =
                            _FormatCapacityDescriptionTotal(capacityInfo, infoRealtion.Description);
                    }

                    else if (infoRealtion.Description == Properties.Resources.ExportFieldDescriptionUtilization)
                    {
                        infoRealtion.Description =
                            _FormatCapacityDescriptionUtilization(capacityInfo, infoRealtion.Description);
                    }

                    // else Do nothing - use without modification
                }

                Debug.Assert(!string.IsNullOrEmpty(infoRealtion.Name));
                _fieldsMap.Add(infoRealtion.Name, infoRealtion);
            }
        }

        /// <summary>
        /// Adds custom order property relative fields.
        /// </summary>
        /// <param name="orderCustomPropertyInfos">Order custom order properties info information.</param>
        /// <param name="info">Field information with data settings.</param>
        private void _AddCustomOrderPropertyRelativeFields(
            OrderCustomPropertiesInfo orderCustomPropertyInfos,
            FieldInfo info)
        {
            for (int index = 0; index < orderCustomPropertyInfos.Count; ++index)
            {
                FieldInfo infoRealtion = (FieldInfo)info.Clone();

                OrderCustomProperty prorety = orderCustomPropertyInfos[index];
                _UpdateInfoWithInfoName(prorety.Name, info, ref infoRealtion);
                infoRealtion.Description = prorety.Description;

                Debug.Assert(!string.IsNullOrEmpty(infoRealtion.Name));

                // NOTE: special issue
                //  support numeric custom order propertiy - need change type description
                if (orderCustomPropertyInfos[index].Type == OrderCustomPropertyType.Numeric)
                {
                    infoRealtion.Type = OleDbType.Double;
                    infoRealtion.Size = 0;
                    infoRealtion.Scale = 2;
                    infoRealtion.Precision = 14;
                }

                _fieldsMap.Add(infoRealtion.Name, infoRealtion);
            }
        }

        /// <summary>
        /// Add address relative fields.
        /// </summary>
        /// <param name="addressFields">Address fields.</param>
        /// <param name="info">Field information with data settings.</param>
        private void _AddAddressRelativeFields(AddressField[] addressFields, FieldInfo info)
        {
            for (int index = 0; index < addressFields.Length; ++index)
            {
                AddressField adress = addressFields[index];

                FieldInfo infoRealtion = (FieldInfo)info.Clone();
                infoRealtion.Name = infoRealtion.LongName = adress.Title;
                infoRealtion.ShortName = (SHORT_NAME_LENGTH < infoRealtion.LongName.Length) ?
                        infoRealtion.LongName.Substring(0, SHORT_NAME_LENGTH) :
                        infoRealtion.LongName;
                infoRealtion.NameFormat = adress.Type.ToString();
                Debug.Assert(string.IsNullOrEmpty(infoRealtion.Description));
                if (string.IsNullOrEmpty(infoRealtion.Description))
                    infoRealtion.Description = adress.Description;

                Debug.Assert(!string.IsNullOrEmpty(infoRealtion.Name));
                _fieldsMap.Add(infoRealtion.Name, infoRealtion);
            }
        }

        #endregion  // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string FORMAT_FIRST_ELEMENT = "{1}";

        const int SHORT_NAME_LENGTH = 10;

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Table name.
        /// </summary>
        private string _name;
        /// <summary>
        /// Table type.
        /// </summary>
        private TableType _type = TableType.Schedules;
        /// <summary>
        /// Fields mapping field information by field name.
        /// </summary>
        Dictionary<string, FieldInfo> _fieldsMap = new Dictionary<string, FieldInfo>();
        /// <summary>
        /// Capacity informations.
        /// </summary>
        private CapacitiesInfo _capacityInfos;
        /// <summary>
        /// Custom order property informations.
        /// </summary>
        private OrderCustomPropertiesInfo _orderCustomPropertyInfos;
        /// <summary>
        /// Address fields.
        /// </summary>
        private AddressField[] _addressFields;

        #endregion // Private members
    }
    
    /// <summary>
    /// Export structure keeper interface.
    /// </summary>
    internal interface IExportStructureKeeper
    {
        /// <summary>
        /// Capacity informations.
        /// </summary>
        CapacitiesInfo CapacitiesInfo { get; }
        /// <summary>
        /// Order custom property informations.
        /// </summary>
        OrderCustomPropertiesInfo OrderCustomPropertiesInfo { get; }
        /// <summary>
        /// Fields that hard to generation.
        /// </summary>
        string[] HardFields { get; }

        /// <summary>
        /// Gets export pattern.
        /// </summary>
        /// <param name="type">Export type.</param>
        /// <returns>Table informations.</returns>
        ICollection<TableInfo> GetPattern(ExportType type);
        /// <summary>
        /// Gets table description.
        /// </summary>
        /// <param name="type">Export table type.</param>
        /// <returns>Table description.</returns>
        TableDescription GetTableDescription(TableType type);

        /// <summary>
        /// Checks is field name reserved for Microsoft Jet database engine.
        /// </summary>
        /// <param name="fieldName">Field name.</param>
        /// <returns>TRUE if name present in reserved words.</returns>
        bool IsNameReserved(string fieldName);
    }

    /// <summary>
    /// Application export structure reader.
    /// </summary>
    internal sealed class ExportStructureReader : IExportStructureKeeper
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>ExportStructureReader</c>.
        /// </summary>
        public ExportStructureReader()
        {
        }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Loads export settings.
        /// </summary>
        /// <param name="doc">Export keeper document.</param>
        /// <param name="capacityInfos">Capacity informations.</param>
        /// <param name="orderCustomPropertiesInfo">Order custom property informations.</param>
        /// <param name="addressFields">Address fields.</param>
        public void Load(XmlDocument doc, CapacitiesInfo capacityInfos,
                         OrderCustomPropertiesInfo orderCustomPropertiesInfo,
                         AddressField[] addressFields)
        {
            Debug.Assert(null != doc);

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_EXPORTPATTERNS, StringComparison.OrdinalIgnoreCase))
                    _LoadPatterns(node);
                else if (node.Name.Equals(NODE_NAME_TABLEDEFINITIONS, StringComparison.OrdinalIgnoreCase))
                    _LoadTableDescriptions(node, capacityInfos, orderCustomPropertiesInfo, addressFields);
                else if (node.Name.Equals(NODE_NAME_RESERVEDWORDS, StringComparison.OrdinalIgnoreCase))
                    _LoadReservedWords(node);
                else if (node.Name.Equals(NODE_NAME_HARDFIELDS, StringComparison.OrdinalIgnoreCase))
                    _LoadHardFields(node);
                else
                    throw new NotSupportedException();
            }

            _capacityInfos = capacityInfos;
            _orderCustomPropertyInfos = orderCustomPropertiesInfo;
        }

        #region IExportStructureKeeper interface
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Capacity informations.
        /// </summary>
        public CapacitiesInfo CapacitiesInfo
        {
            get { return _capacityInfos; }
        }

        /// <summary>
        /// Order custom property informations.
        /// </summary>
        public OrderCustomPropertiesInfo OrderCustomPropertiesInfo
        {
            get { return _orderCustomPropertyInfos; }
        }

        /// <summary>
        /// Gets export pattern.
        /// </summary>
        /// <param name="type">Export type.</param>
        /// <returns>Table informations.</returns>
        public ICollection<TableInfo> GetPattern(ExportType type)
        {
            if (!_listPatterns.ContainsKey(type))
                return (new List<TableInfo> ()).AsReadOnly();

            return _listPatterns[type].AsReadOnly();
        }

        /// <summary>
        /// Fields that hard to generation.
        /// </summary>
        public string[] HardFields
        {
            get
            {
                Debug.Assert(null != _hardFields);
                return _hardFields;
            }
        }

        /// <summary>
        /// Gets table description.
        /// </summary>
        /// <param name="type">Export table type.</param>
        /// <returns>Table description.</returns>
        public TableDescription GetTableDescription(TableType type)
        {
            if (!_listTables.ContainsKey(type))
                return null;

            return _listTables[type];
        }

        /// <summary>
        /// Checks is field name reserved for Microsoft Jet database engine.
        /// </summary>
        /// <param name="fieldName">Field name.</param>
        /// <returns>TRUE if name present in reserved words.</returns>
        public bool IsNameReserved(string fieldName)
        {
            Debug.Assert(!string.IsNullOrEmpty(fieldName));
            Debug.Assert(null != _reservedWords);

            bool result = false;
            foreach (string word in _reservedWords)
            {
                string[] fieldNamePart = fieldName.Split(FIELD_NAME_SPLITTERS);
                for (int index = 0; index < fieldNamePart.Length; ++index)
                {
                    if (word.Equals(fieldNamePart[index], StringComparison.OrdinalIgnoreCase))
                    {
                        result = true;
                        break; // result founded
                    }
                }

                if (result)
                {
                    break; // result founded
                }
            }

            return result;
        }

        #endregion // IExportStructureKeeper interface

        #endregion Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Parses ignored fields.
        /// </summary>
        /// <param name="nodeFields">Field's node.</param>
        /// <returns>Readed ignored field names.</returns>
        private StringCollection _ParseIgnoreFields(XmlNode nodeFields)
        {
            var collection = new StringCollection();
            foreach (XmlNode node in nodeFields.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_FIELD, StringComparison.OrdinalIgnoreCase))
                    collection.Add(node.Attributes[ATTRIBUTE_NAME_NAME].Value);
            }
            return collection;
        }

        /// <summary>
        /// Parses table indexes.
        /// </summary>
        /// <param name="nodeListIndexes">Indexes node.</param>
        /// <returns>Readed index field name.</returns>
        private StringCollection _ParseIndexFields(XmlNodeList nodeListIndexes)
        {
            var indexFields = new StringCollection();
            foreach (XmlNode node in nodeListIndexes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_FIELDS, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (XmlNode nodeField in node.ChildNodes)
                    {
                        if (nodeField.NodeType != XmlNodeType.Element)
                           continue; // skip comments and other non element nodes

                        if (nodeField.Name.Equals(NODE_NAME_FIELD, StringComparison.OrdinalIgnoreCase))
                            indexFields.Add(nodeField.Attributes[ATTRIBUTE_NAME_NAME].Value);
                    }
                }
            }

            return indexFields;
        }

        /// <summary>
        /// Checks is valid index value.
        /// </summary>
        /// <param name="type">Table index type.</param>
        /// <param name="fieldNames">Field names.</param>
        /// <returns>TRUE if valid.</returns>
        private bool _IsValidIndexValue(TableIndexType type, StringCollection fieldNames)
        {
            bool isValid = false;
            switch (type)
            {
                case TableIndexType.Primary:
                case TableIndexType.Simple:
                    isValid = (1 == fieldNames.Count);
                    break;

                case TableIndexType.Multiple:
                    isValid = (1 < fieldNames.Count);
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    isValid = false;
                    break;
            }

            return isValid;
        }

        /// <summary>
        /// Parses indexes.
        /// </summary>
        /// <param name="nodeIndexes">Indexes node.</param>
        /// <returns>Readed table indexes.</returns>
        private List<TableIndex> _ParseIndexes(XmlNode nodeIndexes)
        {
            var indexes = new List<TableIndex>();
            foreach (XmlNode node in nodeIndexes.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_INDEX, StringComparison.OrdinalIgnoreCase))
                {
                    TableIndex index = new TableIndex();
                    index.Type =
                        (TableIndexType)Enum.Parse(typeof(TableIndexType),
                                                   node.Attributes[ATTRIBUTE_NAME_TYPE].Value);
                    index.FieldNames = _ParseIndexFields(node.ChildNodes);
                    Debug.Assert(_IsValidIndexValue(index.Type, index.FieldNames));
                    indexes.Add(index);
                }
            }

            return indexes;
        }

        /// <summary>
        /// Parses table features.
        /// </summary>
        /// <param name="nodeTable">Table node.</param>
        /// <returns>Readed table information.</returns>
        private TableInfo _ParseTableFeatures(XmlNode nodeTable)
        {
            var info = new TableInfo();
            info.Type = (TableType)Enum.Parse(typeof(TableType),
                                              nodeTable.Attributes[ATTRIBUTE_NAME_TYPE].Value);
            foreach (XmlNode node in nodeTable.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_IGNORABLEFIELDS, StringComparison.OrdinalIgnoreCase))
                    info.IgnoredFields = _ParseIgnoreFields(node);
                else if (node.Name.Equals(NODE_NAME_INDEXES, StringComparison.OrdinalIgnoreCase))
                    info.Indexes = _ParseIndexes(node);
            }
            
            return info;
        }

        /// <summary>
        /// Load table informations.
        /// </summary>
        /// <param name="nodeTables">Tables node.</param>
        /// <returns>Readed table informations.</returns>
        private List<TableInfo> _LoadTables(XmlNode nodeTables)
        {
            var tables = new List<TableInfo>();
            foreach (XmlNode node in nodeTables.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_TABLE, StringComparison.OrdinalIgnoreCase))
                {
                    TableInfo info = _ParseTableFeatures(node);
                    tables.Add(info);
                }
            }

            return tables;
        }

        /// <summary>
        /// Loads export pattern.
        /// </summary>
        /// <param name="nodePattern">Pattern's node.</param>
        private void _LoadPattern(XmlNode nodePattern)
        {
            ExportType type =
                (ExportType)Enum.Parse(typeof(ExportType),
                                       nodePattern.Attributes[ATTRIBUTE_NAME_TYPE].Value);
            List<TableInfo> tables = null;
            foreach (XmlNode node in nodePattern.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_TABLES, StringComparison.OrdinalIgnoreCase))
                    tables = _LoadTables(node);
            }

            if ((null != tables) && (0 < tables.Count))
            {
                Debug.Assert(!_listPatterns.ContainsKey(type));
                _listPatterns.Add(type, tables);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        /// <summary>
        /// Loads list of reserved words.
        /// </summary>
        /// <param name="nodeReservedWords">Reserved words node.</param>
        private void _LoadReservedWords(XmlNode nodeReservedWords)
        {
            Debug.Assert(null != nodeReservedWords);

            string reservedWords = nodeReservedWords.InnerText;
            Debug.Assert(!string.IsNullOrEmpty(reservedWords));

            Debug.Assert(null == _reservedWords); // only once

            reservedWords = reservedWords.Trim();
            reservedWords = reservedWords.Replace(Environment.NewLine, ""); // remove line breaks
            reservedWords = reservedWords.Replace(" ", ""); // remove spaces
            _reservedWords = reservedWords.Split(SEPARATOR);
        }

        /// <summary>
        /// Loads list of hard fields.
        /// </summary>
        /// <param name="nodeHardFields">Hard fields node.</param>
        private void _LoadHardFields(XmlNode nodeHardFields)
        {
            Debug.Assert(null != nodeHardFields);

            var fields = new List<string>();
            foreach (XmlNode nodeField in nodeHardFields.ChildNodes)
            {
                if (nodeField.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (nodeField.Name.Equals(NODE_NAME_FIELD, StringComparison.OrdinalIgnoreCase))
                    fields.Add(nodeField.Attributes[ATTRIBUTE_NAME_NAME].Value);
            }

            _hardFields = fields.ToArray();
        }

        /// <summary>
        /// Loads export patterns.
        /// </summary>
        /// <param name="nodePatterns">Patterns node.</param>
        private void _LoadPatterns(XmlNode nodePatterns)
        {
            Debug.Assert(null != nodePatterns);

            foreach (XmlNode node in nodePatterns.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_EXPORTPATTERN, StringComparison.OrdinalIgnoreCase))
                    _LoadPattern(node);
            }
        }

        /// <summary>
        /// Loads field information.
        /// </summary>
        /// <param name="nodeFields">Fields node.</param>
        /// <returns>Readed field information.</returns>
        private List<FieldInfo> _LoadFields(XmlNode nodeFields)
        {
            var fields = new List<FieldInfo>();
            foreach (XmlNode node in nodeFields.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_FIELD, StringComparison.OrdinalIgnoreCase))
                {
                    FieldInfo field = new FieldInfo();
                    field.Name = node.Attributes[ATTRIBUTE_NAME_NAME].Value;
                    string longName = node.Attributes[ATTRIBUTE_NAME_LONGNAME].Value;
                    field.LongName = Properties.Resources.ResourceManager.GetString(longName);
                    string shortName = node.Attributes[ATTRIBUTE_NAME_SHORTNAME].Value;
                    field.ShortName = Properties.Resources.ResourceManager.GetString(shortName);

                    field.RelationType = null;
                    if (null != node.Attributes[ATTRIBUTE_NAME_RELATIONTYPE])
                        field.RelationType = node.Attributes[ATTRIBUTE_NAME_RELATIONTYPE].Value;
                    field.NameFormat =
                        (string.IsNullOrEmpty(field.RelationType)) ? null : field.Name;

                    field.Type =
                        (OleDbType)Enum.Parse(typeof(OleDbType),
                                              node.Attributes[ATTRIBUTE_NAME_ADOTYPE].Value);

                    field.Size = 0;
                    if (null != node.Attributes[ATTRIBUTE_NAME_SIZE])
                        field.Size = int.Parse(node.Attributes[ATTRIBUTE_NAME_SIZE].Value);
                    field.Precision = 0;
                    if (null != node.Attributes[ATTRIBUTE_NAME_PRECISION])
                        field.Precision = int.Parse(node.Attributes[ATTRIBUTE_NAME_PRECISION].Value);
                    field.Scale = 0;
                    if (null != node.Attributes[ATTRIBUTE_NAME_SCALE])
                        field.Scale = int.Parse(node.Attributes[ATTRIBUTE_NAME_SCALE].Value);

                    field.IsDefault = true;
                    if (null != node.Attributes[ATTRIBUTE_NAME_DEFAULT])
                        field.IsDefault = bool.Parse(node.Attributes[ATTRIBUTE_NAME_DEFAULT].Value);

                    field.IsHidden = false;
                    if (null != node.Attributes[ATTRIBUTE_NAME_HIDDEN])
                        field.IsHidden = bool.Parse(node.Attributes[ATTRIBUTE_NAME_HIDDEN].Value);

                    field.IsImage = false;
                    if (null != node.Attributes[ATTRIBUTE_NAME_IMAGE])
                        field.IsImage = bool.Parse(node.Attributes[ATTRIBUTE_NAME_IMAGE].Value);

                    field.Description = null;
                    if (null != node.Attributes[ATTRIBUTE_NAME_DESCRIPTION])
                    {
                        string description = node.Attributes[ATTRIBUTE_NAME_DESCRIPTION].Value;
                        field.Description =
                            Properties.Resources.ResourceManager.GetString(description);
                    }

                    fields.Add(field);
                }
            }
            return fields;
        }

        /// <summary>
        /// Loads table description.
        /// </summary>
        /// <param name="nodeTable">Table's node.</param>
        /// <param name="capacityInfos">Capacity informations.</param>
        /// <param name="orderCustomPropertyInfos">Order custom property informations.</param>
        /// <param name="addressFields">Address fields.</param>
        /// <returns>Readed table description.</returns>
        private TableDescription _LoadTableDescription(XmlNode nodeTable,
                                                       CapacitiesInfo capacityInfos,
                                                       OrderCustomPropertiesInfo orderCustomPropertyInfos,
                                                       AddressField[] addressFields)
        {
            TableType type =
                (TableType)Enum.Parse(typeof(TableType),
                                      nodeTable.Attributes[ATTRIBUTE_NAME_TYPE].Value);

            string name = nodeTable.Attributes[ATTRIBUTE_NAME_NAME].Value;

            List<FieldInfo> fields = null;
            foreach (XmlNode node in nodeTable.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_FIELDS, StringComparison.OrdinalIgnoreCase))
                    fields = _LoadFields(node);
            }

            return new TableDescription(name,
                                        type,
                                        fields,
                                        capacityInfos,
                                        orderCustomPropertyInfos,
                                        addressFields);
        }

        /// <summary>
        /// Loads table descriptions.
        /// </summary>
        /// <param name="nodeTables">Tables node.</param>
        /// <param name="capacityInfos">Capacity informations.</param>
        /// <param name="orderCustomPropertyInfos">Order custom property informations.</param>
        /// <param name="addressFields">Address fields.</param>
        private void _LoadTableDescriptions(XmlNode nodeTables,
                                            CapacitiesInfo capacityInfos,
                                            OrderCustomPropertiesInfo orderCustomPropertyInfos,
                                            AddressField[] addressFields)
        {
            foreach (XmlNode node in nodeTables.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_TABLEDEFINITION, StringComparison.OrdinalIgnoreCase))
                {
                    TableDescription table = _LoadTableDescription(node,
                                                                   capacityInfos,
                                                                   orderCustomPropertyInfos,
                                                                   addressFields);
                    if (null != table)
                    {
                        Debug.Assert(!_listTables.ContainsKey(table.Type));
                        _listTables.Add(table.Type, table);
                    }
                }
            }
        }

        #endregion // Private methods

        #region Provate constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string NODE_NAME_EXPORTSTRUCTURE = "ExportStructure";
        private const string NODE_NAME_RESERVEDWORDS = "ReservedWords";
        private const string NODE_NAME_HARDFIELDS = "HardFields";
        private const string NODE_NAME_EXPORTPATTERNS = "ExportPatterns";
        private const string NODE_NAME_EXPORTPATTERN = "ExportPattern";
        private const string NODE_NAME_TABLES = "Tables";
        private const string NODE_NAME_TABLE = "Table";
        private const string NODE_NAME_TABLEDEFINITIONS = "TableDefinitions";
        private const string NODE_NAME_TABLEDEFINITION = "TableDefinition";
        private const string NODE_NAME_FIELDS = "Fields";
        private const string NODE_NAME_FIELD = "Field";
        private const string NODE_NAME_IGNORABLEFIELDS = "IgnorableFields";
        private const string NODE_NAME_INDEXES = "Indexes";
        private const string NODE_NAME_INDEX = "Index";

        private const string ATTRIBUTE_NAME_TYPE = "Type";
        private const string ATTRIBUTE_NAME_NAME = "Name";
        private const string ATTRIBUTE_NAME_LONGNAME = "LongName";
        private const string ATTRIBUTE_NAME_SHORTNAME = "ShortName";
        private const string ATTRIBUTE_NAME_ADOTYPE = "ADOType";
        private const string ATTRIBUTE_NAME_SIZE = "Size";
        private const string ATTRIBUTE_NAME_PRECISION = "Precision";
        private const string ATTRIBUTE_NAME_SCALE = "Scale";
        private const string ATTRIBUTE_NAME_RELATIONTYPE = "RelationType";
        private const string ATTRIBUTE_NAME_DEFAULT = "Default";
        private const string ATTRIBUTE_NAME_IMAGE = "Image";
        private const string ATTRIBUTE_NAME_HIDDEN = "Hidden";
        private const string ATTRIBUTE_NAME_DESCRIPTION = "Description";

        private const char SEPARATOR = ',';
        readonly char[] FIELD_NAME_SPLITTERS = new char [] {' '};

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Export pattern map table informations by export type.
        /// </summary>
        private Dictionary<ExportType, List<TableInfo>> _listPatterns =
            new Dictionary<ExportType, List<TableInfo>>();
        /// <summary>
        /// Export table map table description by table type.
        /// </summary>
        private Dictionary<TableType, TableDescription> _listTables =
            new Dictionary<TableType, TableDescription>();
        /// <summary>
        /// Capacity informations.
        /// </summary>
        private CapacitiesInfo _capacityInfos;
        /// <summary>
        /// Custom order property informations.
        /// </summary>
        private OrderCustomPropertiesInfo _orderCustomPropertyInfos;
        /// <summary>
        /// List of reserved words in Jet 4.0.
        /// </summary>
        private string[] _reservedWords;
        /// <summary>
        /// Fields that hard to generation.
        /// </summary>
        private string[] _hardFields;

        #endregion // Private members
    }
}

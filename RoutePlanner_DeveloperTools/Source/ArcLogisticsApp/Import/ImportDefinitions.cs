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

using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Import
{
    #region ImportType enum

    /// <summary>
    /// Application import type.
    /// </summary>
    internal enum ImportType
    {
        Orders,
        Locations,
        Drivers,
        Vehicles,
        MobileDevices,
        DefaultRoutes,
        DriverSpecialties,
        VehicleSpecialties,
        Barriers,
        Zones

        // NOTE: if add new type - do update all switch seek "case ImportType.Orders"
    }

    #endregion // ImportType enum

    #region FieldMap struct

    /// <summary>
    /// Struct that represents a import field mapping element.
    /// </summary>
    internal struct FieldMap
    {
        /// <summary>
        /// Creates field map element.
        /// </summary>
        /// <param name="objectFieldName">Imported object field name.</param>
        /// <param name="sourceFieldName">Source field name.</param>
        public FieldMap(string objectFieldName, string sourceFieldName)
        {
            ObjectFieldName = objectFieldName;
            SourceFieldName = sourceFieldName;
        }

        /// <summary>
        /// Source field name.
        /// </summary>
        public readonly string SourceFieldName;

        /// <summary>
        /// Object field name.
        /// </summary>
        public readonly string ObjectFieldName;
    }

    #endregion // FieldMap struct

    #region DataFieldInfo struct

    /// <summary>
    /// Struct that represents a source data field info.
    /// </summary>
    internal struct DataFieldInfo
    {
        /// <summary>
        /// Creates field info
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <param name="type">Field type.</param>
        public DataFieldInfo(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Field name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Field type.
        /// </summary>
        public readonly Type Type;
    }

    #endregion // DataFieldInfo struct

    #region ObjectDataFieldInfo struct

    /// <summary>
    /// Struct that represents a object data field info.
    /// </summary>
    internal struct ObjectDataFieldInfo
    {
        /// <summary>
        /// Creates data field info.
        /// </summary>
        /// <param name="info">Field info.</param>
        /// <param name="isMandatory">Flag is field mandatory.</param>
        public ObjectDataFieldInfo(DataFieldInfo info, bool isMandatory)
        {
            Info = info;
            IsMandatory = isMandatory;
        }

        /// <summary>
        /// Field info.
        /// </summary>
        public readonly DataFieldInfo Info;

        /// <summary>
        /// Field is mandatory.
        /// </summary>
        public readonly bool IsMandatory;
    }

    #endregion // ObjectDataFieldInfo struct

    #region ImportedValueStatus enum

    /// <summary>
    /// Imported value status.
    /// </summary>
    internal enum ImportedValueStatus
    {
        Empty,
        Valid,
        Failed
    }

    #endregion // ImportType enum

    #region ImportedValueInfo struct

    /// <summary>
    /// Structure that represents a description of imported value.
    /// </summary>
    internal struct ImportedValueInfo
    {
        /// <summary>
        /// Creates imported value description.
        /// </summary>
        /// <param name="name">Name of field in source.</param>
        /// <param name="readedValue">Readed value from source.</param>
        /// <param name="value">Property value (in property type and storage units of measure).</param>
        /// <param name="status">Readed value status.</param>
        public ImportedValueInfo(string name, string readedValue, object value,
                                 ImportedValueStatus status)
        {
            Name = name;
            ReadedValue = readedValue;
            Value = value;
            Status = status;
        }

        /// <summary>
        /// Name of field in source.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Value from source.
        /// </summary>
        public readonly string ReadedValue;

        /// <summary>
        /// Property value (in property type and storage units of measure).
        /// </summary>
        public readonly object Value;

        /// <summary>
        /// Imported value status.
        /// </summary>
        public readonly ImportedValueStatus Status;
    }

    #endregion // ImportedValueInfo struct

    #region ImportResult struct

    /// <summary>
    /// Structure that represents a result of import operation.
    /// </summary>
    internal struct ImportResult
    {
        /// <summary>
        /// Create import result.
        /// </summary>
        /// <param name="obj">Imported object.</param>
        /// <param name="desciptions">Collection of import value description </param>
        public ImportResult(DataObject obj, ICollection<ImportedValueInfo> desciptions)
        {
            Object = obj;
            Desciptions = desciptions;
        }

        /// <summary>
        /// Imported object.
        /// </summary>
        public readonly DataObject Object;

        /// <summary>
        /// Collection of import value description 
        /// </summary>
        public readonly ICollection<ImportedValueInfo> Desciptions;
    }

    #endregion // ImportResult struct
}

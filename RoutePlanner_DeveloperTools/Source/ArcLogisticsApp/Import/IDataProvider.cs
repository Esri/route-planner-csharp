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
    /// Interface a data provider
    /// </summary>
    internal interface IDataProvider
    {
        /// <summary>
        /// Records count
        /// </summary>
        int RecordsCount { get; }

        /// <summary>
        /// Set cursor to first row
        /// </summary>
        void MoveFirst();

        /// <summary>
        /// Move cursor to next row
        /// </summary>
        void MoveNext();

        /// <summary>
        /// All records iterated
        /// </summary>
        bool IsEnd();

        /// <summary>
        /// Field count
        /// </summary>
        int FieldCount { get; }

        /// <summary>
        /// Obtain fields info list
        /// </summary>
        ICollection<DataFieldInfo> FieldsInfo { get; }

        /// <summary>
        /// Obtain field value
        /// </summary>
        object FieldValue(int index);

        /// <summary>
        /// Flag is current record empty
        /// </summary>
        bool IsRecordEmpty { get; }

        /// <summary>
        /// Flag is format support geometry
        /// </summary>
        bool IsGeometrySupport { get; }

        /// <summary>
        /// Geometry
        /// </summary>
        /// <remarks>if format not support geometry - return null</remarks>
        object Geometry { get; }
    }
}

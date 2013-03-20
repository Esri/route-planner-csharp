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

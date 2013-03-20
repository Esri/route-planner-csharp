using System;

namespace ESRI.ArcLogistics.Data.DataModel
{
    /// <summary>
    /// Provides access to common properties of data entities.
    /// </summary>
    public interface IDataObject
    {
        /// <summary>
        /// Gets or sets ID of the object.
        /// </summary>
        Guid Id
        {
            get;
            set;
        }
    }
}

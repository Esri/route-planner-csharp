using System;
using System.Diagnostics;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Class that manages data objects of the project.
    /// </summary>
    public class GenericDataObjectManager<T> : DataObjectManager<T>
        where T : DataObject
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal GenericDataObjectManager(DataObjectContext context, string entityName, SpecFields specFields)
            : base(context, entityName, specFields)
        {
        }

        internal GenericDataObjectManager(DataService<T> dataService)
            : base(dataService)
        {
        }

        #endregion constructors
    }
}

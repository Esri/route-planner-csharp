using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Provides access to application data collections objects.
    /// </summary>
    internal interface IProjectData
    {
        /// <summary>
        /// Gets reference to the data objects of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of data objects to get container for.</typeparam>
        /// <returns>A reference to the container for data objects of the specified type.</returns>
        IDataObjectContainer<T> GetDataObjects<T>() where T : DataObject;
    }
}

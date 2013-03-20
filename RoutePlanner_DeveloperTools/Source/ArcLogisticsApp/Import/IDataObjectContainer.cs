using System.Collections.Generic;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Provides access to a collection of data objects with ability to track elements additions.
    /// </summary>
    /// <typeparam name="T">The type of data objects in the collection.</typeparam>
    interface IDataObjectContainer<T> : ICollection<T>
        where T : DataObject
    {
        /// <summary>
        /// Checks if the data object with the specified name exists in the collection.
        /// </summary>
        /// <param name="objectName">The name of the object to check existence for.</param>
        /// <returns>True if and only if the object with the specified name exists in
        /// the collection.</returns>
        bool Contains(string objectName);

        /// <summary>
        /// Gets data object with the specified name.
        /// </summary>
        /// <param name="objectName">The name of the data object to be retrieved.</param>
        /// <param name="value">Contains reference to the data object with the specified
        /// name or null if no such object was found.</param>
        /// <returns>True if and only if the object with the specified name exists in
        /// the collection.</returns>
        bool TryGetValue(string objectName, out T value);

        /// <summary>
        /// Gets reference to the collection of added data objects.
        /// </summary>
        /// <returns>A reference to the collection of added data objects.</returns>
        IEnumerable<T> GetAddedObjects();
    }
}

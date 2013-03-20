using System;
using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Implements <see cref="I:ESRI.ArcLogistics.App.Import.IProjectDataContext"/> for
    /// the application project.
    /// </summary>
    internal sealed class ProjectDataContext : IProjectDataContext
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the ProjectDataContext class.
        /// </summary>
        /// <param name="project">The reference to the project to track changes for.</param>
        public ProjectDataContext(IProject project)
        {
            Debug.Assert(project != null);

            _AddElements(
                _projectData,
                _MakeContext(project.DefaultRoutes),
                _MakeContext(project.Drivers),
                _MakeContext(project.DriverSpecialties),
                _MakeContext(project.Locations),
                _MakeContext(project.Vehicles),
                _MakeContext(project.VehicleSpecialties),
                _MakeContext(project.MobileDevices),
                _MakeContext(project.FuelTypes),
                _MakeContext(project.Zones));
        }
        #endregion

        #region IProjectData Members
        /// <summary>
        /// Gets reference to the data objects of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of data objects to get container for.</typeparam>
        /// <returns>A reference to the container for data objects of the specified type.</returns>
        public IDataObjectContainer<T> GetDataObjects<T>()
            where T : DataObject
        {
            Debug.Assert(_projectData.ContainsKey(typeof(T)));

            var context = (RelatedDataObjectCollectionContext<T>)_projectData[typeof(T)];
            return context.DataObjects;
        }

        #endregion

        #region IDataObjectContext Members
        /// <summary>
        /// Committs changes made to the project data objects collections.
        /// </summary>
        public void Commit()
        {
            foreach (var item in _projectData.Values)
            {
                item.Commit();
            }
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Rollbacks changes made to the project data objects collections.
        /// </summary>
        public void Dispose()
        {
            foreach (var item in _projectData.Values)
            {
                item.Dispose();
            }
        }
        #endregion

        #region private members
        /// <summary>
        /// Creates data object context for the specified data objects collection.
        /// </summary>
        /// <typeparam name="T">The type of data objects in the source collection.</typeparam>
        /// <param name="source">The reference to the data objects collection to create data object
        /// context for.</param>
        /// <returns>A new key-value pair containing <typeparamref name="T"/> as a key and
        /// data object context for the <paramref name="source"/> as a value.</returns>
        private static KeyValuePair<Type, IDataObjectContext> _MakeContext<T>(
            IDataObjectCollection<T> source)
            where T : DataObject
        {
            return new KeyValuePair<Type, IDataObjectContext>(
                typeof(T),
                new RelatedDataObjectCollectionContext<T>(source));
        }

        /// <summary>
        /// Adds the specified collection of elements to the specified dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of dictionary values.</typeparam>
        /// <param name="dictionary">The reference to the dictionary to add elements to.</param>
        /// <param name="elements">A collection of elements to be added to the
        /// <paramref name="dictionary"/>.</param>
        private static void _AddElements<TKey, TValue>(
            IDictionary<TKey, TValue> dictionary,
            params KeyValuePair<TKey, TValue>[] elements)
        {
            Debug.Assert(dictionary != null);

            var collection = (ICollection<KeyValuePair<TKey, TValue>>)dictionary;
            foreach (var item in elements)
            {
                collection.Add(item);
            }
        }
        #endregion

        #region private fields
        /// <summary>
        /// Stores data contexts for project data objects collections.
        /// </summary>
        private Dictionary<Type, IDataObjectContext> _projectData =
            new Dictionary<Type, IDataObjectContext>();
        #endregion
    }
}

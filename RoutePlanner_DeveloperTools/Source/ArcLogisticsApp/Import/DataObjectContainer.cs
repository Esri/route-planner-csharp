using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Implements <see cref="T:ESRI.ArcLogistics.App.Import.IDataObjectContainer`1[T]"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class DataObjectContainer<T> : IDataObjectContainer<T>
        where T : DataObject
    {
        #region constructor
        /// <summary>
        /// Initializes a new instance of the DataObjectContainer class.
        /// </summary>
        /// <param name="source">The reference to the source collection of data objects.</param>
        public DataObjectContainer(IEnumerable<T> source)
        {
            Debug.Assert(source != null);
            Debug.Assert(source.All(item => item != null));

            _data = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in source)
            {
                var key = _GetObjectName(item);
                if (_data.ContainsKey(key))
                {
                    continue;
                }

                _data.Add(key, item);
            }

            _newObjects = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region IDataObjectContainer<T> Members
        /// <summary>
        /// Checks if the data object with the specified name exists in the collection.
        /// </summary>
        /// <param name="objectName">The name of the object to check existence for.</param>
        /// <returns>True if and only if the object with the specified name exists in
        /// the collection.</returns>
        public bool Contains(string objectName)
        {
            Debug.Assert(objectName != null);

            return _data.ContainsKey(objectName);
        }

        /// <summary>
        /// Gets data object with the specified name.
        /// </summary>
        /// <param name="objectName">The name of the data object to be retrieved.</param>
        /// <param name="value">Contains reference to the data object with the specified
        /// name or null if no such object was found.</param>
        /// <returns>True if and only if the object with the specified name exists in
        /// the collection.</returns>
        public bool TryGetValue(string objectName, out T value)
        {
            Debug.Assert(objectName != null);

            return _data.TryGetValue(objectName, out value);
        }

        /// <summary>
        /// Gets reference to the collection of added data objects.
        /// </summary>
        /// <returns>A reference to the collection of added data objects.</returns>
        public IEnumerable<T> GetAddedObjects()
        {
            return _newObjects.Values;
        }
        #endregion

        #region ICollection<T> Members
        /// <summary>
        /// Gets number of data objects in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return _data.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating if the collection is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Adds the specified item to the collection.
        /// </summary>
        /// <param name="item">The reference to the data object to be added.</param>
        public void Add(T item)
        {
            var key = _GetObjectName(item);
            if (key == null)
            {
                return;
            }

            if (_data.ContainsKey(key))
            {
                return;
            }

            _data.Add(key, item);
            _newObjects.Add(key, item);
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            _data.Clear();
            _newObjects.Clear();
        }

        /// <summary>
        /// Checks if the specified item exists in the collection.
        /// </summary>
        /// <param name="item">The item to check existence for.</param>
        /// <returns>True if and only if the specified item exists in the collection.</returns>
        public bool Contains(T item)
        {
            var key = _GetObjectName(item);
            if (key == null)
            {
                return false;
            }

            return this.Contains(key);
        }

        /// <summary>
        /// Copies collection elements to the specified array, starting at the specified index.
        /// </summary>
        /// <param name="array">The reference to an array to copy values to.</param>
        /// <param name="arrayIndex">A zero-based index in the <paramref name="array"/> to begin
        /// copying at.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array"/> is a null
        /// reference.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is
        /// less than zero.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in the collection
        /// is greater than the available space starting from <paramref name="arrayIndex"/>
        /// to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            // Cast _data.Value to ICollection and rely on it to check method arguments.
            // We cannot use _data.Value.CopyTo since it has different parameter names.
            var collection = (ICollection<T>)_data.Values;
            collection.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the specified item from the collection.
        /// </summary>
        /// <param name="item">The reference to an item to be removed.</param>
        /// <returns>True if and only if the item existed in the collection before removal.</returns>
        public bool Remove(T item)
        {
            var key = _GetObjectName(item);
            if (key == null)
            {
                return false;
            }

            _newObjects.Remove(key);
            return _data.Remove(key);
        }
        #endregion

        #region IEnumerable<T> Members
        /// <summary>
        /// Gets enumerator to iterate over collection elements.
        /// </summary>
        /// <returns>A reference to the enumerator for iterating over collection elements.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _data.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Gets enumerator to iterate over collection elements.
        /// </summary>
        /// <returns>A reference to the enumerator for iterating over collection elements.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region private static methods
        /// <summary>
        /// Gets object name for the specified data object.
        /// </summary>
        /// <param name="dataObject">The reference to the data object to get name for.</param>
        /// <returns>A name of the specified data object or null if <paramref name="dataObject"/>
        /// is a null reference.</returns>
        private static string _GetObjectName(T dataObject)
        {
            if (object.ReferenceEquals(dataObject, null))
            {
                return null;
            }

            return dataObject.ToString();
        }
        #endregion

        #region private fields
        /// <summary>
        /// Stores all data objects.
        /// </summary>
        private Dictionary<string, T> _data;

        /// <summary>
        /// Stores new data objects.
        /// </summary>
        private Dictionary<string, T> _newObjects;
        #endregion
    }
}

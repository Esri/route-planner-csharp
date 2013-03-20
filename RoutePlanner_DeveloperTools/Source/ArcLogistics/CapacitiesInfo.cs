using System;
using System.Collections;
using System.Collections.Generic;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// CapacitiesInfo class contains information about the different kinds of capacities used in the current project.
    /// </summary>
    public class CapacitiesInfo : ICollection<CapacityInfo>, ICloneable
    {
        #region constructors

        /// <summary>
        /// Creates a new instance of the <c>CapacitiesInfo</c> class.
        /// </summary>
        public CapacitiesInfo()
        {
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Clones CapacitiesInfo and makes it read-only.
        /// </summary>
        public object Clone()
        {
            CapacitiesInfo obj = new CapacitiesInfo();
            CapacityInfo[] capacities = new CapacityInfo[_capacities.Count];
            this._capacities.CopyTo(capacities, 0);
            obj._capacities.AddRange(capacities);

            return obj;
        }

        #endregion

        #region ICollection<string> Members

        /// <summary>
        /// Adds a capacity name to the collection.
        /// </summary>
        public void Add(CapacityInfo item)
        {
            if ((item == null) || (string.IsNullOrEmpty(item.Name)))
                throw new ArgumentException();

            if (_isReadOnly)
                throw new InvalidOperationException();

            _capacities.Add(item);
        }

        /// <summary>
        /// Clears all elements from the collection.
        /// </summary>
        public void Clear()
        {
            if (_isReadOnly)
                throw new InvalidOperationException();

            _capacities.Clear();
        }

        /// <summary>
        /// Returns boolean value based on whether or not it finds the
        /// requested item in the collection.
        /// </summary>
        public bool Contains(CapacityInfo item)
        {
            bool contains = _capacities.Contains(item);
            return contains;
        }

        /// <summary>
        /// Copies names from this collection into another array.
        /// </summary>
        public void CopyTo(CapacityInfo[] array, int arrayIndex)
        {
            _capacities.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns number of capacity names in the collection.
        /// </summary>
        public int Count
        {
            get { return _capacities.Count; }
        }

        /// <summary>
        /// Removes a capacity name from the collection.
        /// </summary>
        public bool Remove(CapacityInfo item)
        {
            if (_isReadOnly)
                throw new InvalidOperationException();

            return _capacities.Remove(item);
        }

        /// <summary>
        /// Returns a boolean value based on whether or not
        /// the instance is a read only collection.
        /// </summary>
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            internal set { _isReadOnly = value; }
        }

        #endregion

        #region IEnumerable<CapacityInfo> members

        /// <summary>
        /// Returns generic enumerator for this collection.
        /// </summary>
        public IEnumerator<CapacityInfo> GetEnumerator()
        {
            return (IEnumerator<CapacityInfo>)_capacities.GetEnumerator();
        }

        #endregion // IEnumerable<OrderCustomProperty> members

        #region IEnumerable members

        /// <summary>
        /// Returns non-generic enumerator for this collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)_capacities.GetEnumerator();
        }

        #endregion // IEnumerable members

        #region public properties

        /// <summary>
        /// Default accessor for the collection.
        /// </summary>
        public CapacityInfo this[int index]
        {
            get { return _capacities[index]; }
            set { throw new InvalidOperationException(); }
        }

        #endregion

        #region private members

        private List<CapacityInfo> _capacities = new List<CapacityInfo>();
        private bool _isReadOnly = false;

        #endregion
    }
}

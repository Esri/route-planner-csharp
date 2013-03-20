using System;
using System.Collections;
using System.Collections.Generic;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// OrderCustomProperies info class contains information about
	/// the different custom order properties used in the current project.
    /// </summary>
    public class OrderCustomPropertiesInfo : ICollection<OrderCustomProperty>, ICloneable
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>OrderCustomPropertiesInfo</c> class.
        /// </summary>
        public OrderCustomPropertiesInfo()
        {
            _isReadOnly = false;
        }
        #endregion // Constructors

        #region ICloneable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clone properties info and made it read-only
        /// </summary>
        public object Clone()
        {
            OrderCustomPropertiesInfo obj = new OrderCustomPropertiesInfo();

            OrderCustomProperty[] property = new OrderCustomProperty[_properies.Count];
            this._properies.CopyTo(property, 0);
            obj._properies.AddRange(property);
            obj._totalLength = this._totalLength;

            // NOTE: clonned object is ReadOnly
            obj._isReadOnly = true;

            return obj;
        }
        #endregion // ICloneable members

        #region ICollection<OrderCustomProperty> members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds a capacity name to the collection.
        /// </summary>
        public void Add(OrderCustomProperty item)
        {
            if (null == item)
                throw new ArgumentException();

            if ((null == item.Name) || (item.Length <= 0))
                throw new ArgumentException();

            if (_isReadOnly)
                throw new InvalidOperationException();

            _totalLength += item.Length;
            _properies.Add(item);
        }

        /// <summary>
        /// Clears the collection of all it's elements.
        /// </summary>
        public void Clear()
        {
            if (_isReadOnly)
                throw new InvalidOperationException();

            _properies.Clear();
        }

        /// <summary>
        /// Returns a boolean value based on whether or not it finds the
        /// requested item in the collection.
        /// </summary>
        public bool Contains(OrderCustomProperty item)
        {
            return _properies.Contains(item);
        }

        /// <summary>
        /// Copies properties from this collection into another array.
        /// </summary>
        public void CopyTo(OrderCustomProperty[] array, int arrayIndex)
        {
            _properies.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns the number of properies in the collection.
        /// </summary>
        public int Count
        {
            get { return _properies.Count; }
        }

        /// <summary>
        /// Removes a property from the collection.
        /// </summary>
        public bool Remove(OrderCustomProperty item)
        {
            if (_isReadOnly)
                throw new InvalidOperationException();

            _properies.Remove(item);
            return true;
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
        #endregion // ICollection<OrderCustomProperty> members

        #region IEnumerable<OrderCustomProperty> members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns generic enumerator for this collection.
        /// </summary>
        public IEnumerator<OrderCustomProperty> GetEnumerator()
        {
            return (IEnumerator<OrderCustomProperty>)_properies.GetEnumerator();
        }
        #endregion // IEnumerable<OrderCustomProperty> members

        #region IEnumerable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns non-generic enumerator for this collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)_properies.GetEnumerator();
        }
        #endregion // IEnumerable members

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Default accessor for the collection.
        /// </summary>
        public OrderCustomProperty this[int index]
        {
            get { return _properies[index]; }
            set { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Supported total length of all properies.
        /// </summary>
        internal long TotalLength
        {
            get { return _totalLength; }
        }
        #endregion // Public properties

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private List<OrderCustomProperty> _properies = new List<OrderCustomProperty>();
        private bool _isReadOnly;
        private long _totalLength = 0;
        #endregion // Private members
    }
}

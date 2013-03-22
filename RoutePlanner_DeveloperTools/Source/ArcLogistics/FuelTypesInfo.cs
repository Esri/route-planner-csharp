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
using System.Collections;
using System.Collections.Generic;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// FuelTypesInfo class contains information about the different fuel types used in the application.
    /// </summary>
    internal class FuelTypesInfo : ICollection<FuelTypeInfo>, ICloneable
    {
        #region constructors

        /// <summary>
        /// Creates a new instance of the <c>FuelTypesInfo</c> class.
        /// </summary>
        public FuelTypesInfo()
        {
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Clones fuel types info and makes it read-only.
        /// </summary>
        public object Clone()
        {
            FuelTypesInfo obj = new FuelTypesInfo();

            FuelTypeInfo[] fuelTypes = new FuelTypeInfo[_fuelTypes.Count];
            this._fuelTypes.CopyTo(fuelTypes, 0);
            obj._fuelTypes.AddRange(fuelTypes);

            // clonned object is ReadOnly
            obj._isReadOnly = true;

            return obj;
        }

        #endregion

        #region ICollection<FuelTypeInfo> Members

        /// <summary>
        /// Adds a fuel type to the collection.
        /// </summary>
        public void Add(FuelTypeInfo item)
        {
            if (item == null)
                throw new ArgumentException();

            if (_isReadOnly)
                throw new InvalidOperationException();

            _fuelTypes.Add(item);
        }

        /// <summary>
        /// Clears all elements from the collection.
        /// </summary>
        public void Clear()
        {
            if (_isReadOnly)
                throw new InvalidOperationException();

            _fuelTypes.Clear();
        }

        /// <summary>
        /// Returns a boolean value based on whether or not it finds the
        /// requested item in the collection.
        /// </summary>
        public bool Contains(FuelTypeInfo item)
        {
            bool contains = _fuelTypes.Contains(item);
            return contains;
        }

        /// <summary>
        /// Copies fueltypes from this collection into another array.
        /// </summary>
        public void CopyTo(FuelTypeInfo[] array, int arrayIndex)
        {
            _fuelTypes.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns number of fuel types in the collection.
        /// </summary>
        public int Count
        {
            get { return _fuelTypes.Count; }
        }

        /// <summary>
        /// Removes a fuel type from the collection.
        /// </summary>
        public bool Remove(FuelTypeInfo item)
        {
            if (_isReadOnly)
                throw new InvalidOperationException();

            _fuelTypes.Remove(item);
            return true;
        }

        /// <summary>
        /// Returns a boolean value based on whether or not
		/// the instance is a read only collection.
        /// </summary>
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }
        #endregion

        #region IEnumerable<FuelType> Members

        /// <summary>
        /// Returns FuelType generic enumerator for this collection.
        /// </summary>
        public IEnumerator<FuelTypeInfo> GetEnumerator()
        {
            return (IEnumerator<FuelTypeInfo>)_fuelTypes.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns FuelType non-generic enumerator for this collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)_fuelTypes.GetEnumerator();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Default accessor for the collection.
        /// </summary>
        public FuelTypeInfo this[int index]
        {
            get { return _fuelTypes[index]; }
            set { throw new InvalidOperationException(); }
        }

        #endregion

        #region private members

        private List<FuelTypeInfo> _fuelTypes = new List<FuelTypeInfo> ();
        private bool _isReadOnly;

        #endregion
    }
}

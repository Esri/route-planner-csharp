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
using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Represents a key to be used for detecting same orders with different planned dates.
    /// </summary>
    internal sealed class OrderKey
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the OrderKey class.
        /// </summary>
        /// <param name="order">The order to initialize key instance with.</param>
        public OrderKey(Order order)
        {
            Debug.Assert(order != null);

            _name = order.Name;
            _address = order.Address;
        }
        #endregion

        #region public static methods
        /// <summary>
        /// Compares specified objects for the equality.
        /// </summary>
        /// <param name="left">The
        /// <see cref="ESRI.ArcLogistics.App.Pages.OrderKey"/> instance
        /// to compare with the <paramref name="right"/>.</param>
        /// <param name="right">The
        /// <see cref="ESRI.ArcLogistics.App.Pages.OrderKey"/> instance
        /// to compare with the <paramref name="left"/>.</param>
        /// <returns>True if and only if specified objects are equal.</returns>
        public static bool operator ==(OrderKey left, OrderKey right)
        {
            if (object.ReferenceEquals(left, right))
            {
                return true;
            }

            if (object.ReferenceEquals(left, null))
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Compares specified objects for the inequality.
        /// </summary>
        /// <param name="left">The
        /// <see cref="ESRI.ArcLogistics.App.Pages.OrderKey"/> instance
        /// to compare with the <paramref name="right"/>.</param>
        /// <param name="right">The
        /// <see cref="ESRI.ArcLogistics.App.Pages.OrderKey"/> instance
        /// to compare with the <paramref name="left"/>.</param>
        /// <returns>True if and only if specified objects are not equal.</returns>
        public static bool operator !=(OrderKey left, OrderKey right)
        {
            return !(left == right);
        }
        #endregion

        #region Object Members
        /// <summary>
        /// Checks if the specified object is equal to the current one.
        /// </summary>
        /// <param name="obj">The reference to the <see cref="T:System.Object"/>
        /// object to compare current one with.</param>
        /// <returns>True if and only if the specified object is equal to the current
        /// one.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as OrderKey;

            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            var equals =
                NAME_COMPARER.Equals(_name, other._name) &&
                ADDRESS_COMPARER.Equals(_address, other._address);

            return equals;
        }

        /// <summary>
        /// Gets hash code for the object.
        /// </summary>
        /// <returns>Hash code value for the object.</returns>
        public override int GetHashCode()
        {
            const int initial = 1273891513;
            const int multiplier = 773891497;

            unchecked
            {
                var hashCode = initial;
                hashCode = multiplier * hashCode + _GetHashCode(_name, NAME_COMPARER);
                hashCode = multiplier * hashCode + _GetHashCode(_address, ADDRESS_COMPARER);

                return hashCode;
            }
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Gets hash code for the specified object using the specified comparer.
        /// </summary>
        /// <typeparam name="T">The type of objects to get hash codes for.</typeparam>
        /// <param name="obj">The object to get hash code for.</param>
        /// <param name="comparer">The comparer to be used for getting object hash code.</param>
        /// <returns>A hash code for the specified object.</returns>
        private static int _GetHashCode<T>(T obj, IEqualityComparer<T> comparer)
        {
            Debug.Assert(comparer != null);

            if (obj == null)
            {
                return 573891379;
            }

            return comparer.GetHashCode(obj);
        }
        #endregion

        #region private constants
        /// <summary>
        /// The reference to the equality comparer object to be used for comparing orders names.
        /// </summary>
        private static readonly IEqualityComparer<string> NAME_COMPARER =
            StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// The reference to the equality comparer object to be used for comparing orders
        /// addresses.
        /// </summary>
        private static readonly IEqualityComparer<Address> ADDRESS_COMPARER =
            EqualityComparer<Address>.Default;
        #endregion

        #region private fields
        /// <summary>
        /// The name of an order.
        /// </summary>
        private string _name;

        /// <summary>
        /// The address of an order.
        /// </summary>
        private Address _address;
        #endregion
    }
}

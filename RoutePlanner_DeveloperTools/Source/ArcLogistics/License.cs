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

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// License class.
    /// </summary>
    [Serializable]
    public sealed class License : IEquatable<License>
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public License(string productCode, int? permittedRouteNumber,
            int? permittedOrderNumber,
            DateTime? expirationDate,
            bool isRestricted,
            string description)
        {
            _productCode = productCode;
            _permittedRouteNumber = permittedRouteNumber;
            _permittedOrderNumber = permittedOrderNumber;
            _expirationDate = expirationDate;
            _isRestricted = isRestricted;
            _description = description;
        }
        
        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public bool IsRestricted
        {
            get { return _isRestricted; }
        }

        public string ProductCode
        {
            get { return _productCode; }
        }

        public int? PermittedRouteNumber
        {
            get { return _permittedRouteNumber; }
        }

        public int? PermittedOrderNumber
        {
            get { return _permittedOrderNumber; }
        }

        public DateTime? ExpirationDate
        {
            get { return _expirationDate; }
        }

        public string Description
        {
            get { return _description; }
        }

        #endregion public properties

        #region public static methods
        /// <summary>
        /// Compares specified license objects for the equality.
        /// </summary>
        /// <param name="left">The <see cref="ESRI.ArcLogistics.License"/> instance to
        /// compare with the <paramref name="right"/>.</param>
        /// <param name="right">The <see cref="ESRI.ArcLogistics.License"/> instance to
        /// compare with the <paramref name="left"/>.</param>
        /// <returns>True if and only if specified license objects are equal.</returns>
        public static bool operator ==(License left, License right)
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
        /// Compares specified license objects for the inequality.
        /// </summary>
        /// <param name="left">The <see cref="ESRI.ArcLogistics.License"/> instance to
        /// compare with the <paramref name="right"/>.</param>
        /// <param name="right">The <see cref="ESRI.ArcLogistics.License"/> instance to
        /// compare with the <paramref name="left"/>.</param>
        /// <returns>True if and only if specified license objects are not equal.</returns>
        public static bool operator !=(License left, License right)
        {
            return !(left == right);
        }
        #endregion

        #region Object Members
        /// <summary>
        /// Checks if the specified object is equal to the license.
        /// </summary>
        /// <param name="obj">The reference to the <see cref="T:System.Object"/>
        /// object to compare license with.</param>
        /// <returns>True if and only if the specified object is equal to current license.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as License;

            return this.Equals(other);
        }

        /// <summary>
        /// Gets hash code for the license.
        /// </summary>
        /// <returns>Hash code value for the license</returns>
        public override int GetHashCode()
        {
            var hash =
                _isRestricted.GetHashCode() ^
                _GetHashCode(_productCode) ^
                _permittedRouteNumber.GetHashCode() ^
                _permittedOrderNumber.GetHashCode() ^
                _expirationDate.GetHashCode() ^
                _GetHashCode(_description);

            return hash;
        }
        #endregion

        #region IEquatable<License> Members
        /// <summary>
        /// Checks if the specified license is equal to the current one.
        /// </summary>
        /// <param name="other">The reference to the <see cref="T:ESRI.ArcLogistics.License"/>
        /// object to compare current one with.</param>
        /// <returns>True if and only if the specified license is equal to the current one.</returns>
        public bool Equals(License other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            var equals =
                _isRestricted == other._isRestricted &&
                _productCode == other._productCode &&
                _permittedRouteNumber == other._permittedRouteNumber &&
                _permittedOrderNumber == other._permittedOrderNumber &&
                _expirationDate == other._expirationDate &&
                _description == other._description;

            return equals;
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Gets hash code for the specified string reference.
        /// </summary>
        /// <param name="value">A reference to the string object.</param>
        /// <returns>Zero if the reference is null and string hash code otherwise.</returns>
        private static int _GetHashCode(string value)
        {
            return value == null ? 0 : value.GetHashCode();
        }
        #endregion

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private bool _isRestricted;
        private string _productCode;
        private int? _permittedRouteNumber;
        private int? _permittedOrderNumber;
        private DateTime? _expirationDate;
        private string _description;

        #endregion private fields
    }
}

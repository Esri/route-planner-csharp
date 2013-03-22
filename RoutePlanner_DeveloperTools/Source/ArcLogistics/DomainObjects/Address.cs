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
using System.ComponentModel;

using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that contains information about a physical address.
    /// </summary>
    public class Address : ICloneable, INotifyPropertyChanged
    {
        #region public static properties

        public static string PropertyNameUnit
        {
            get { return PROP_NAME_UNIT; }
        }

        public static string PropertyNameFullAddress
        {
            get { return PROP_NAME_FULLADDRESS; }
        }

        public static string PropertyNameAddressLine
        {
            get { return PROP_NAME_ADDRESSLINE; }
        }

        public static string PropertyNameLocality1
        {
            get { return PROP_NAME_LOCALITY1; }
        }

        public static string PropertyNameLocality2
        {
            get { return PROP_NAME_LOCALITY2; }
        }

        public static string PropertyNameLocality3
        {
            get { return PROP_NAME_LOCALITY3; }
        }

        public static string PropertyNameCountryPrefecture
        {
            get { return PROP_NAME_COUNTYPREFECTURE; }
        }

        public static string PropertyNamePostalCode1
        {
            get { return PROP_NAME_POSTALCODE1; }
        }

        public static string PropertyNamePostalCode2
        {
            get { return PROP_NAME_POSTALCODE2; }
        }

        public static string PropertyNameStateProvince
        {
            get { return PROP_NAME_STATEPROVINCE; }
        }

        public static string PropertyNameCountry
        {
            get { return PROP_NAME_COUNTRY; }
        }

        public static string PropertyNameMatchMethod
        {
            get { return PROP_NAME_MATCHMETHOD; }
        }
        
        #endregion

        #region public static methods

        /// <summary>
        /// Checks if the parameter is the name of any property of the <c>Address</c> class.
        /// </summary>
        /// <param name="propName">Property name to check.</param>
        public static bool IsAddressPropertyName(string propName)
        {
            return ((PROP_NAME_FULLADDRESS == propName) || (PROP_NAME_ADDRESSLINE == propName) ||
                    (PROP_NAME_LOCALITY1 == propName) || (PROP_NAME_LOCALITY2 == propName) ||
                    (PROP_NAME_LOCALITY3 == propName) || (PROP_NAME_COUNTYPREFECTURE == propName) ||
                    (PROP_NAME_POSTALCODE1 == propName) || (PROP_NAME_POSTALCODE2 == propName) ||
                    (PROP_NAME_STATEPROVINCE == propName) || (PROP_NAME_COUNTRY == propName) ||
                    (PROP_NAME_UNIT == propName));
        }

        /// <summary>
        /// Compares specified objects for the equality.
        /// </summary>
        /// <param name="left">The <see cref="ESRI.ArcLogistics.DomainObjects.Address"/>
        /// instance to compare with the <paramref name="right"/>.</param>
        /// <param name="right">The <see cref="ESRI.ArcLogistics.DomainObjects.Address"/>
        /// instance to compare with the <paramref name="left"/>.</param>
        /// <returns>True if and only if specified objects are equal.</returns>
        public static bool operator ==(Address left, Address right)
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
        /// <param name="left">The <see cref="ESRI.ArcLogistics.DomainObjects.Address"/>
        /// instance to compare with the <paramref name="right"/>.</param>
        /// <param name="right">The <see cref="ESRI.ArcLogistics.DomainObjects.Address"/>
        /// instance to compare with the <paramref name="left"/>.</param>
        /// <returns>True if and only if specified objects are not equal.</returns>
        public static bool operator !=(Address left, Address right)
        {
            return !(left == right);
        }
        #endregion

        #region public events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Event which is fired if a property's value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion public events

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Unit name.
        /// </summary>
        public string Unit
        {
            get { return _unit; }
            set
            {
                _unit = value;
                _NotifyPropertyChanged(PROP_NAME_UNIT);
            }
        }

        /// <summary>
        /// Full address string.
        /// </summary>
        public string FullAddress
        {
            get { return _fullAddress; }
            set
            {
                _fullAddress = value;
                _NotifyPropertyChanged(PROP_NAME_FULLADDRESS);
            }
        }

        /// <summary>
        /// Street address.
        /// </summary>
        public string AddressLine
        {
            get { return _addressLine; }
            set
            {
                _addressLine = value;
                _NotifyPropertyChanged(PROP_NAME_ADDRESSLINE);
            }
        }

        /// <summary>
        /// Can represent either area in a neighborhood, district, block, or double dependent locality.
        /// </summary>
        public string Locality1
        {
            get { return _locality1; }
            set
            {
                _locality1 = value;
                _NotifyPropertyChanged(PROP_NAME_LOCALITY1);
            }
        }

        /// <summary>
        /// Can represent community, place name, municipality, settlement, or dependent locality.
        /// </summary>
        public string Locality2
        {
            get { return _locality2; }
            set
            {
                _locality2 = value;
                _NotifyPropertyChanged(PROP_NAME_LOCALITY2);
            }
        }

        /// <summary>
        /// Can represent city, town ,post town or village.
        /// </summary>
        public string Locality3
        {
            get { return _locality3; }
            set
            {
                _locality3 = value;
                _NotifyPropertyChanged(PROP_NAME_LOCALITY3);
            }
        }

        /// <summary>
        /// Can represent either county or prefecture.
        /// </summary>
        public string CountyPrefecture
        {
            get { return _countyPrefecture; }
            set
            {
                _countyPrefecture = value;
                _NotifyPropertyChanged(PROP_NAME_COUNTYPREFECTURE);
            }
        }

        /// <summary>
        /// Can represent ZIP code, Postal code or FSA.
        /// </summary>
        public string PostalCode1
        {
            get { return _postalCode1; }
            set
            {
                _postalCode1 = value;
                _NotifyPropertyChanged(PROP_NAME_POSTALCODE1);
            }
        }

        /// <summary>
        /// Can represent either ZIP+4 code or LDU code.
        /// </summary>
        public string PostalCode2
        {
            get { return _postalCode2; }
            set
            {
                _postalCode2 = value;
                _NotifyPropertyChanged(PROP_NAME_POSTALCODE2);
            }
        }

        /// <summary>
        /// Can represent either state or province.
        /// </summary>
        public string StateProvince
        {
            get { return _stateProvince; }
            set
            {
                _stateProvince = value;
                _NotifyPropertyChanged(PROP_NAME_STATEPROVINCE);
            }
        }
        
        /// <summary>
        /// Country name.
        /// </summary>
        public string Country
        {
            get { return _country; }
            set
            {
                _country = value;
                _NotifyPropertyChanged(PROP_NAME_COUNTRY);
            }
        }

        /// <summary>
        /// Describes the match method used to get this address.
        /// </summary>
        /// <remarks>
        /// Typically match method contains the name of the locator that returned the adderess. But it can
        /// be also one of the predefined values. 
        /// <para>Match method is set to "Edited X/Y" when user manually specified or edited order/location point.</para>
        /// <para>Match method is set to "Imported X/Y" when order/location wasn't geocoded but both address and point came from import database.</para>
        /// </remarks>
        public string MatchMethod
        {
            get { return _matchMethod; }
            set
            {
                _matchMethod = value;
                _NotifyPropertyChanged(PROP_NAME_MATCHMETHOD);
            }
        }

        /// <summary>
        /// Returns value of specififed address part.
        /// </summary>
        /// <param name="addressPart"></param>
        /// <returns></returns>
        public string this[AddressPart addressPart]
        {
            get 
            { 
                return _GetAddressPart(addressPart); 
            }
            set 
            { 
                _SetAddressPart(addressPart, value); 
            }
        }

        #endregion public properties

        #region public methods
        /// <summary>
        /// Returns the contents of the FullAddress property.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return FullAddress;
        }

        /// <summary>
        /// Copies address parts to the target address.
        /// </summary>
        /// <param name="address">Target address.</param>
        public void CopyTo(Address address)
        {
            address.Unit = _unit;
            address.FullAddress = _fullAddress;
            address.AddressLine = _addressLine;
            address.Locality1 = _locality1;
            address.Locality2 = _locality2;
            address.Locality3 = _locality3;
            address.CountyPrefecture = _countyPrefecture;
            address.PostalCode1 = _postalCode1;
            address.PostalCode2 = _postalCode2;
            address.StateProvince = _stateProvince;
            address.Country = _country;
            address.MatchMethod = _matchMethod;
        }

        /// <summary>
        /// clones the current Address instance.
        /// </summary>
        /// <returns>A new <see cref="T:ESRI.ArcLogistics.DomainObjects.Address"/> object equal to
        /// this one.</returns>
        public Address Clone()
        {
            Address obj = new Address();
            this.CopyTo(obj);

            return obj;
        }
        #endregion

        #region Object Members
        /// <summary>
        /// Check is the same address values.
        /// </summary>
        /// <param name="obj">Address to compare.</param>
        /// <returns>Is the same address values.</returns>
        public override bool Equals(object obj)
        {
            Address address = obj as Address;
            if (object.ReferenceEquals(address, null))
            {
                return false;
            }

            foreach (var addressPart in EnumHelpers.GetValues<AddressPart>())
            {
                string addressPartValue = this[addressPart];
                string addressPartToCompareValue = address[addressPart];

                if (!PART_COMPARER.Equals(addressPartValue, addressPartToCompareValue))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns hash code for the address object.
        /// </summary>
        /// <returns>A hash code for the address object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 1173891413;

                foreach (var addressPart in EnumHelpers.GetValues<AddressPart>())
                {
                    string addressPartValue = this[addressPart];

                    var partHash = addressPartValue == null ?
                        1873891399 : PART_COMPARER.GetHashCode(addressPartValue);

                    hashCode = 873891391 * hashCode + partHash;
                }

                return hashCode;
            }
        }
        #endregion

        #region ICloneable Members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// clones the current Address instance.
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _GetAddressPart(AddressPart addressPart)
        {
            string result = string.Empty;

            switch(addressPart)
            {
                case AddressPart.Unit:
                    {
                        result = Unit;
                        break;
                    }
                case AddressPart.FullAddress :
                    {
                        result = FullAddress;
                        break;
                    }
                case AddressPart.AddressLine :
                    {
                        result = AddressLine;
                        break;
                    }
                case AddressPart.Locality1 :
                    {
                        result = Locality1;
                        break;
                    }
                case AddressPart.Locality2 :
                    {
                        result = Locality2;
                        break;
                    }
                case AddressPart.Locality3 :
                    {
                        result = Locality3;
                        break;
                    }
                case AddressPart.CountyPrefecture :
                    {
                        result = CountyPrefecture;
                        break;
                    }
                case AddressPart.PostalCode1 :
                    {
                        result = PostalCode1;
                        break;
                    }
                case AddressPart.PostalCode2 :
                    {
                        result = PostalCode2;
                        break;
                    }
                case AddressPart.StateProvince :
                    {
                        result = StateProvince;
                        break;
                    }
                case AddressPart.Country :
                    {
                        result = Country;
                        break;
                    }
                default: 
                    throw new NotSupportedException();
            }

            return result;
        }

        private void _SetAddressPart(AddressPart addressPart, string value)
        {
            switch(addressPart)
            {
                case AddressPart.Unit:
                    {
                        Unit = value;
                        break;
                    }
                case AddressPart.FullAddress :
                    {
                        FullAddress = value;
                        break;
                    }
                case AddressPart.AddressLine :
                    {
                        AddressLine = value;
                        break;
                    }
                case AddressPart.Locality1 :
                    {
                        Locality1 = value;
                        break;
                    }
                case AddressPart.Locality2 :
                    {
                        Locality2 = value;
                        break;
                    }
                case AddressPart.Locality3 :
                    {
                        Locality3 = value;
                        break;
                    }
                case AddressPart.CountyPrefecture :
                    {
                        CountyPrefecture = value;
                        break;
                    }
                case AddressPart.PostalCode1 :
                    {
                        PostalCode1 = value;
                        break;
                    }
                case AddressPart.PostalCode2 :
                    {
                        PostalCode2 = value;
                        break;
                    }
                case AddressPart.StateProvince :
                    {
                        StateProvince = value;
                        break;
                    }
                case AddressPart.Country :
                    {
                        Country = value;
                        break;
                    }
                default: 
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Notify parent about changes
        /// </summary>
        /// <param name="info">Param name</param>
        private void _NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        #endregion private methods

        #region private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // property names
        private const string PROP_NAME_UNIT = "Unit";
        private const string PROP_NAME_FULLADDRESS = "FullAddress";
        private const string PROP_NAME_ADDRESSLINE = "AddressLine";
        private const string PROP_NAME_LOCALITY1 = "Locality1";
        private const string PROP_NAME_LOCALITY2 = "Locality2";
        private const string PROP_NAME_LOCALITY3 = "Locality3";
        private const string PROP_NAME_COUNTYPREFECTURE = "CountyPrefecture";
        private const string PROP_NAME_POSTALCODE1 = "PostalCode1";
        private const string PROP_NAME_POSTALCODE2 = "PostalCode2";
        private const string PROP_NAME_STATEPROVINCE = "StateProvince";
        private const string PROP_NAME_COUNTRY = "Country";
        private const string PROP_NAME_MATCHMETHOD = "MatchMethod";

        /// <summary>
        /// The reference to the equality comparer object to be used for comparing address parts.
        /// </summary>
        private static readonly IEqualityComparer<string> PART_COMPARER =
            StringComparer.OrdinalIgnoreCase;
        #endregion

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _unit;
        private string _fullAddress;
        private string _addressLine;
        private string _locality1;
        private string _locality2;
        private string _locality3;
        private string _countyPrefecture;
        private string _postalCode1;
        private string _postalCode2;
        private string _stateProvince;
        private string _country;
        private string _matchMethod;

        #endregion private members

    }
}

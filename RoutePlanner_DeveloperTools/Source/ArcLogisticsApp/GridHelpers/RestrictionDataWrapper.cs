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
using System.Diagnostics;
using System.Text;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// RestrictionName class
    /// </summary>
    internal class RestrictionName
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public RestrictionName(string name, string description)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(name));

            _name = name;
            _description = description;
        }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public override string ToString()
        {
            return string.Format("Name: {0}. Description: {1}", _name, _description);
        }
        #endregion // Public methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _name = null;
        private string _description = null;

        #endregion // Private members
    }

    /// <summary>
    /// Class representing restriction parameter.
    /// </summary>
    internal class Parameter
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="value">Parameter value.</param>
        public Parameter(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Parameter()
        {
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Parameter value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Parameter name.
        /// </summary>
        public string Name { get; private set; }

        #endregion

        #region Public overrided methods

        /// <summary>
        /// Check that two parameters are equal.
        /// </summary>
        /// <param name="x">First parameter to compare.</param>
        /// <param name="y">Second parameter to compare.</param>
        /// <returns>'True' if both x and y are null or if their names and values are equal, 
        /// 'false' otherwise.</returns>
        public static bool operator ==(Parameter x, Parameter y)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(x, y))
                return true;

            // If one is null, but not both, return false.
            if ((object)x == null || (object)y == null)
                return false;

            // Check that name and values are equal.
            return x.Name == y.Name && x.Value == y.Value;
        }

        /// <summary>
        /// Check that two parameters are different.
        /// </summary>
        /// <param name="x">First parameter to compare.</param>
        /// <param name="y">Second parameter to compare.</param>
        /// <returns>'False' if both x and y are null or if their names and values are equal, 
        /// 'true' otherwise.</returns>
        public static bool operator !=(Parameter x, Parameter y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">Object to compare with current.</param>
        /// <returns>'True' if object is Parameter and its name and value properties is the
        /// same as current object's.</returns>
        public override bool Equals(object obj)
        {
            var parameter = obj as Parameter;

            if (parameter == null)
                return false;
            else
                return parameter == this;
        }

        /// <summary>
        /// Get hash code for parameter.
        /// </summary>
        /// <returns>Hash code of current parameter.</returns>
        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0) ^ (Value != null ? Value.GetHashCode() : 0);
        }

        #endregion
    }

    /// <summary>
    /// Parameters class.
    /// </summary>
    internal class Parameters : IEnumerable
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of Parameters class.
        /// </summary>
        public Parameters(int length)
        {
            _values = new Parameter[length];
        }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            for (int index = 0; index < _values.Length; ++index)
            {
                result.Append(_values[index].Name);
                if (index < _values.Length - 1)
                    result.Append(SEPARATOR); // NOTE: after last not neded
            }

            return result.ToString();
        }

        /// <summary>
        /// Return fieldname for provided index.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <returns>Fieldname.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Occured when index is less then 0.</exception>
        public static string GetFieldName(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            return index.ToString();
        }

        /// <summary>
        /// Get property index by property name.
        /// </summary>
        /// <returns>Property index or -1 if it is not custom property</returns>
        public static int GetIndex(string indexString)
        {
            int index = -1;

            if (int.TryParse(indexString, out index))
                return index;
            else
                return -1;
        }

        #endregion // Public methods

        #region IEnumerable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns double non-generic enumerator for this collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        #endregion // IEnumerable members

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns number of capacity names in the collection.
        /// </summary>
        public int Count
        {
            get { return _values.Length; }
        }

        /// <summary>
        /// Default accessor for the collection.
        /// </summary>
        public Parameter this[int index]
        {
            get { return _values[index]; }
            set
            {
                if (value != _values[index])
                    _values[index] = value;
            }
        }

        #endregion // Public properties

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Parameter[] _values = null;

        private const char SEPARATOR = ',';

        #endregion // Private members
    }

    /// <summary>
    /// RestrictionDataWrapper class
    /// </summary>
    internal class RestrictionDataWrapper
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public RestrictionDataWrapper(bool IsEnabled, string name, string description, Parameters parameters)
        {
            Debug.Assert(null != parameters);

            _isEnabled = IsEnabled;
            _restrictionName = new RestrictionName(name, description);
            _parameters = parameters;
        }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        public RestrictionName Restriction
        {
            get { return _restrictionName; }
            set { _restrictionName = value; }
        }

        /// <summary>
        /// Restriction usage parameter.
        /// </summary>
        public Parameter RestrictionUsageParameter { get; set; }

        /// <summary>
        /// Collection with all other restriction attribute parameters.
        /// </summary>
        public Parameters Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        #endregion // Public methods

        #region Override methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            return string.Format("{0}. Parameters: {1}", _restrictionName.ToString(), _parameters.ToString());
        }

        #endregion // Override methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private RestrictionName _restrictionName = null;
        private Parameters _parameters = null;
        private bool _isEnabled = false;

        #endregion // Private members
    }
}

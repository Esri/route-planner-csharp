using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents a storage of capacities.
    /// </summary>
    public class Capacities : IEnumerable<double>, ICloneable, INotifyPropertyChanged
    {
        #region Static methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets capacity property name by index.
        /// </summary>
        /// <remarks>
        /// First capacity's name is "Capacity0", second capacity's name is "Capacity1", and so on.
        /// </remarks>
        public static string GetCapacityPropertyName(int propIndex)
        {
            if (propIndex < 0)
                throw new ArgumentException(Properties.Resources.InvalidCapacityIndex);

            return string.Format(CAPACITY_PROPERTY_NAME_FORMAT, propIndex);
        }

        /// <summary>
        /// Gets capacity property index by property name.
        /// </summary>
        /// <returns>Property index or -1 if <c>propName</c> is not correct capacity property name.</returns>
        public static int GetCapacityPropertyIndex(string propName)
        {
            int index = -1;
            if (propName.StartsWith(CAPACITY_PROPERTY_NAME_BASE))
            {
                string indexStr = propName.Substring(CAPACITY_PROPERTY_NAME_BASE.Length);

                int parsedIndex;
                if (int.TryParse(indexStr, out parsedIndex))
                    index = parsedIndex;
            }

            return index;
        }

        #endregion // Static methods

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>Capacities</c> class.
        /// </summary>
        public Capacities(CapacitiesInfo capacitiesInfo)
        {
            _capacitiesInfo = capacitiesInfo;
            _values = new double[_capacitiesInfo.Count];
        }

        #endregion // Constructors

        #region Public events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Event which is invoked when any of the object's properties change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion // Public events

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns values of all capacities as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _values.ToString();
        }

        /// <summary>
        /// Put all capacities to string
        /// </summary>
        /// <param name="capacities">Capacities values</param>
        internal static string AssemblyDBString(Capacities capacities)
        {
            StringBuilder result = new StringBuilder();
            for (int index = 0; index < capacities.Count; ++index)
            {
                result.Append(capacities[index].ToString(CultureInfo.GetCultureInfo(CommonHelpers.STORAGE_CULTURE)));
                if (index < capacities.Count - 1)
                    result.Append(CommonHelpers.SEPARATOR); // NOTE: after last not neded
            }

            return result.ToString();
        }

        /// <summary>
        /// Parse string and split it to capacities values
        /// </summary>
        /// <param name="value">Capacities string</param>
        /// <param name="capacitiesInfo">Project capacities info</param>
        /// <returns>Parsed capacities</returns>
        internal static Capacities CreateFromDBString(string value, CapacitiesInfo capacitiesInfo)
        {
            Capacities capacities = new Capacities(capacitiesInfo);
            if (null != value)
            {
                value = value.Replace(CommonHelpers.SEPARATOR_OLD, CommonHelpers.SEPARATOR); // NOTE: for support old projects

                char[] valuesSeparator = new char[1] { CommonHelpers.SEPARATOR };
                string[] capacitiesValues = value.Split(valuesSeparator, StringSplitOptions.None);

                System.Diagnostics.Debug.Assert(capacitiesValues.Length == capacitiesInfo.Count);

                for (int index = 0; index < capacitiesValues.Length; index++)
                {
                    double capacityValue = double.Parse(capacitiesValues[index], CultureInfo.GetCultureInfo(CommonHelpers.STORAGE_CULTURE));
                    capacities[index] = capacityValue;
                }
            }

            return capacities;
        }

        #endregion // Public methods

        #region ICloneable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Clones Capacities object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Capacities obj = new Capacities(this._capacitiesInfo);
            for (int index = 0; index < _values.Length; index++)
                obj._values[index] = this._values[index];

            return obj;
        }

        #endregion // ICloneable members

        #region IEnumerable<double> members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Returns an Enumerator for capacities values.
        /// </summary>
        /// <returns></returns>
        IEnumerator<double> IEnumerable<double>.GetEnumerator()
        {
            return (IEnumerator<double>) _values.GetEnumerator();
        }

        #endregion // IEnumerable<double> members

        #region IEnumerable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Returns an Enumerator for capacities values.
        /// </summary>
        /// <returns></returns>
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
        /// Gets number of capacities.
        /// </summary>
        public int Count
        {
            get { return _values.Length; }
        }

        /// <summary>
        /// Gets capacity value by index.
        /// </summary>
        public double this[int index]
        {
            get { return _values[index]; }
            set
            {
                if (value != _values[index])
                {
                    _values[index] = value;
                    _NotifyPropertyChanged(GetCapacityPropertyName(index));
                }
            }
        }

        /// <summary>
        /// Gets capacity value by its name.
        /// </summary>
        /// <remarks>
        /// Use names ("Capacity0", "Capacity1", etc) not titles from <c>CapacityInfo</c>.
        /// </remarks>
        public double this[string capacityName]
        {
            get
            {
                int index = _GetIndex(capacityName);
                return _values[index];
            }
            set
            {
                int index = _GetIndex(capacityName);
                if (value != _values[index])
                {
                    _values[index] = value;
                    _NotifyPropertyChanged(capacityName);
                }
            }
        }

        /// <summary>
        /// Gets information about capacities.
        /// </summary>
        public CapacitiesInfo Info
        {
            get { return _capacitiesInfo; }
        }

        #endregion // Public properties

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private int _GetIndex(string capacityName)
        {
            int index = GetCapacityPropertyIndex(capacityName);
            if ((index < 0) || (_values.Length < index))
            {
                string mes = string.Format(Properties.Resources.PropertyNameNotExists, capacityName);
                throw new ArgumentException(mes);
            }

            return index;
        }

        #endregion Private methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private double[] _values = null;
        private CapacitiesInfo _capacitiesInfo = null;

        private const string CAPACITY_PROPERTY_NAME_BASE = "Capacity";
        private const string CAPACITY_PROPERTY_NAME_FORMAT = "Capacity{0}";

        #endregion // Private members
    }
}
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

using ESRI.ArcLogistics.DomainObjects;
using AppData = ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Class for conversion from <see cref="DataObject"/> to object name.
    /// </summary>
    [ValueConversion(typeof(AppData.DataObject), typeof(string))]
    internal class DataObjectToNameConverter : IValueConverter
    {
        #region IValueConverter members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Convert <see cref="DataObject"/> to object name.
        /// </summary>
        /// <param name="value"><see cref="DataObject"/> (<see cref="Driver"/>, <see cref="Vehicle"/>, etc.).</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Driver name.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;

            _object = value as AppData.DataObject;
            Debug.Assert(_object != null);

            if (null != _object)
                // get object name.
                result = _object.ToString();

            return result;
        }

        /// <summary>
        /// Convert string name to <see cref="DataObject"/>.
        /// </summary>
        /// <param name="value">String with object name.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns><see cref="DataObject"/>.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = value as string;
            if (null != name)
            {   // set name
                Debug.Assert(null != _object);

                if (_object is Driver)
                {
                    Driver driver = _object as Driver;
                    driver.Name = name;
                }
                else if (_object is Vehicle)
                {
                    Vehicle vehicle = _object as Vehicle;
                    vehicle.Name = name;
                }
                else
                {
                    Debug.Assert(false); // not supported
                }
            }

            return _object;
        }

        #endregion // IValueConverter members

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private AppData.DataObject _object = null;

        #endregion // Private fields
    }
}

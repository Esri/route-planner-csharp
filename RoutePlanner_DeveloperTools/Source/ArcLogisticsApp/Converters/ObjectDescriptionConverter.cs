using System;
using System.Windows.Data;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

using ESRI.ArcLogistics.App.Reports;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts object  to a description string.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    internal class ObjectDescriptionConverter : IValueConverter
    {
        /// <summary>
        /// Converts report's template to description.
        /// </summary>
        /// <param name="value">Report's template wrapper.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Report's template description.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;
            if (null != value)
            {
                IDescripted desctiption = value as IDescripted;

                string desctiptionText = null;
                if (null != desctiption)
                    desctiptionText = desctiption.Description;

                if (!string.IsNullOrEmpty(desctiptionText))
                    result = desctiptionText;
            }

            return result;
        }

        /// <summary>
        /// Not used.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

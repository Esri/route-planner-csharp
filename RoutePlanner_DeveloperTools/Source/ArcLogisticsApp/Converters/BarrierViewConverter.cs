using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converter for calculating visibility.
    /// </summary>
    [ValueConversion(typeof(IDictionary<string, object>), typeof(object))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BarrierViewConverter : IValueConverter
    {
        /// <summary>
        /// Convert barrier attributes to barrier visibility.
        /// </summary>
        /// <param name="value">Barrier attributes.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Barrier visibility.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IDictionary<string, object> attributes = value as IDictionary<string, object>;

            Visibility visibility = Visibility.Visible;

            // If attribute has value - check date range.
            if (attributes != null &&
                attributes[GraphicObjects.BarrierGraphicObject.StartAttributeName] != null)
            {
                DateTime start = (DateTime)attributes[GraphicObjects.BarrierGraphicObject.StartAttributeName];
                DateTime finish = (DateTime)attributes[GraphicObjects.BarrierGraphicObject.FinishAttributeName];

                // If current date not between start and finish, than barrier is not visible.
                if (App.Current.CurrentDate < start || App.Current.CurrentDate > finish)
                {
                    visibility = Visibility.Collapsed;
                }
            }

            return visibility;
        }

        /// <summary>
        /// Convert to source.
        /// </summary>
        /// <param name="value">Ignored.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Null.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

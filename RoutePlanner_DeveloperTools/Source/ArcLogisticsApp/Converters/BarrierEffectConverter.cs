using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ESRI.ArcLogistics.App.Geocode;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;
using System.Text;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Class for setting background in address line cell.
    /// </summary>
    [ValueConversion(typeof(object), typeof(object))]
    internal class BarrierEffectConverter : IMultiValueConverter
    {
        #region IMultiValueConverter members

        /// <summary>
        /// Do convertion. Set background to textbox if empty value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Null, if background was set. Cell value otherwise.</returns>
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            Barrier barrier = value[0] as Barrier;

            string result = string.Empty;

            if (barrier != null && barrier.BarrierEffect != null)
            {
                result = CommonHelpers.ConvertBarrierEffect(barrier);
            }

            return result;
        }

        /// <summary>
        /// Not used.
        /// </summary>
        /// <param name="value">Ignored.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Null.</returns>
        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}

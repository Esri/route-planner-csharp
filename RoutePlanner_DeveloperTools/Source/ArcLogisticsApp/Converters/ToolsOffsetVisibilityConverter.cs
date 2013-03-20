using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ESRI.ArcLogistics.App.Controls;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Class for conversion from status type to status visibility.
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    internal class ToolsOffsetVisibilityConverter : IValueConverter
    {
        #region IValueConverter members

        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="value">Status.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Offset Visibility.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PaneState? paneState = value as PaneState?;

            Visibility result = Visibility.Visible;
            if (paneState != null && paneState.Value == PaneState.DockableWindow)
            {
                result = Visibility.Collapsed;
            }

            return result;
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

        #endregion
    }
}

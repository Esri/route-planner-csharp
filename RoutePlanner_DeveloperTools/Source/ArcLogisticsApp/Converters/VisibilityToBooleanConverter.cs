using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using System.Windows.Controls;
using Xceed.Wpf.DataGrid;
using ESRI.ArcLogistics.App.GridHelpers;
using System.Windows;


namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Returns "true" if visibility is Visible, otherwise returns "false"
    /// </summary>
    [ValueConversion(typeof(Visibility), typeof(bool))]
    internal class VisibilityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
                return true;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

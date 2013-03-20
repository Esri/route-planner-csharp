using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using ESRI.ArcLogistics.App.GraphicObjects;
using ESRI.ArcLogistics.DomainObjects;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Diagnostics;
using System.Reflection;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using System.Windows;

namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(Object), typeof(TextBlock))]
    internal class GanttTooltipConverter : IValueConverter
    {
        #region Convertation Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TextBlock result = new TextBlock();
            result.FontSize = (double)App.Current.FindResource("MiddleFontSize");

            if (value != null)
            {
                ESRI.ArcLogistics.App.Mapping.TipGenerator.FillTipText(result, value);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Xceed.Wpf.DataGrid;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(Row), typeof(string))]
    internal class RowTooltipConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Row row = value as Row;

            if (row == null)
                return string.Empty;

            string errorMessage = string.Empty;

            Object obj = row.DataContext;

            if (obj is IDataErrorInfo)
                errorMessage = ((IDataErrorInfo)obj).Error;
            if (string.IsNullOrEmpty(errorMessage))
                errorMessage = "";

            return errorMessage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion

    }
}

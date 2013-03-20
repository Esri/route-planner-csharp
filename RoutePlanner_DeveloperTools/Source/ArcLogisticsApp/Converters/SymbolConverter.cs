using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.OrderSymbology;

namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(object), typeof(object))]
    class SymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result = null;

            if (value != null)
            {
                SymbologyRecord sr = value as SymbologyRecord;
                if (sr != null)
                    result = sr.Symbol;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //string result = SymbologyManager.DEFAULT_TEMPLATE_NAME;
            SymbolBox symbolBox = value as SymbolBox;
            SymbologyRecord result = null;

            if (symbolBox != null)
            {
                result = symbolBox.ParentRecord;
                symbolBox.ParentRecord.SymbolFilename = symbolBox.TemplateFileName;
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.OrderSymbology;
using System.Collections.ObjectModel;

namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(object), typeof(object))]
    class SymbolsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<SymbolBox> items = new ObservableCollection<SymbolBox>();

            SymbologyRecord symbologyRecord = value as SymbologyRecord;
            if (value != null && symbologyRecord != null)
            {
                SymbolBox symbolBox = _CreateSymbolBox(SymbologyManager.DEFAULT_TEMPLATE_NAME, symbologyRecord);
                items.Add(symbolBox);

                foreach (string templateFileName in SymbologyManager.TemplatesFileNames)
                {
                    symbolBox = _CreateSymbolBox(templateFileName, symbologyRecord);
                    items.Add(symbolBox);
                }
            }

            return items;
        }

        private SymbolBox _CreateSymbolBox(string templateFileName, SymbologyRecord symbologyRecord)
        {
            SymbolBox symbolBox = new SymbolBox(templateFileName);
            symbolBox.ParentRecord = symbologyRecord;
            symbolBox.Size = symbologyRecord.Size;

            System.Drawing.Color color = symbologyRecord.Color;
            System.Windows.Media.Color mediaColor =
                System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
            symbolBox.Fill = new System.Windows.Media.SolidColorBrush(mediaColor);

            symbolBox.Height = (symbologyRecord.Size + SymbologyManager.DEFAULT_INDENT) * SymbologyManager.INCREASE_ON_HOVER;
            symbolBox.Width = (symbologyRecord.Size + SymbologyManager.DEFAULT_INDENT) * SymbologyManager.INCREASE_ON_HOVER;

            return symbolBox;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
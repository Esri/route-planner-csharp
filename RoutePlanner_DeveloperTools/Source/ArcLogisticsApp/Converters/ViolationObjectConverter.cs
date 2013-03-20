using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(DataObject), typeof(string))]
    internal class ViolationObjectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result;

            if (value != null)
            {
                try
                {
                    DataObject currentObject = (DataObject)value;
                    if (currentObject.GetType().Equals(typeof(ESRI.ArcLogistics.DomainObjects.Route)))
                        result = string.Format((string)App.Current.FindResource("ViolationRouteStringFormat"), currentObject.ToString());
                    else if (currentObject.GetType().Equals(typeof(ESRI.ArcLogistics.DomainObjects.Order)))
                        result = string.Format((string)App.Current.FindResource("ViolationOrderStringFormat"), currentObject.ToString());
                    else if (currentObject.GetType().Equals(typeof(ESRI.ArcLogistics.DomainObjects.Location)))
                        result = string.Format((string)App.Current.FindResource("ViolationLocationStringFormat"), currentObject.ToString());
                    else
                        result = "";
                }
                catch
                {
                    result = "";
                }
            }
            else
                result = "";

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

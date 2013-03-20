/*
COPYRIGHT 1995-2010 ESRI
TRADE SECRETS: ESRI PROPRIETARY AND CONFIDENTIAL
Unpublished material - all rights reserved under the 
Copyright Laws of the United States.
For additional information, contact:
Environmental Systems Research Institute, Inc.
Attn: Contracts Dept
380 New York Street
Redlands, California, USA 92373
email: contracts@esri.com
*/

using System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;


namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    public class MaxViolationTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result;

            if (value != null)
            {
                try
                {
                    double? currentValue = value as double?;

                    result = string.Format("{0} {1}", currentValue.ToString(), (string)App.Current.FindResource("MaxViolationMinText"));
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

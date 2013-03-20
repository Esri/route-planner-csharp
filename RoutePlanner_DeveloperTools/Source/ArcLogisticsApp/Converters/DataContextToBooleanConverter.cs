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
using System.Diagnostics;
using ESRI.ArcLogistics.App.Reports;


namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Retirns "true" if input context has details otherwise returns "false"
    /// </summary>
    [ValueConversion(typeof(String), typeof(bool))]
    internal class DataContextToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            if (value != null)
            {
                if (value is MessageWindowDataWrapper)
                    result = (0 < _GetDetailsCount((MessageWindowDataWrapper)value));

                else if (value is Route)
                    result = (0 < _GetStopsCount((Route)value));

                else if (value is SelectReportWrapper)
                    result = (0 < _GetSubReportsCount((SelectReportWrapper)value));

                else
                {   // ToDo - present problem
                    //      if open SchedulePage with Routes\Stops value is System.Windows.Template - way??
                    //Debug.Assert(false); // not supported
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        /// <summary>
        /// Method gets count of details
        /// </summary>
        /// <param name="wrapper"></param>
        /// <returns></returns>
        private int _GetDetailsCount(MessageWindowDataWrapper wrapper)
        {
            int result = 0;
            if (null != wrapper.Details)
            {
                foreach (object obj in wrapper.Details)
                    result++;
            }

            return result;
        }

        /// <summary>
        /// Method gets count of stops.
        /// </summary>
        /// <param name="wrapper"></param>
        /// <returns></returns>
        private int _GetStopsCount(Route route)
        {
            return (null == route.Stops)? 0 : route.Stops.Count;
        }

        private int _GetSubReportsCount(SelectReportWrapper reportWrapper)
        {
            ReportsGenerator generator = App.Current.ReportGenerator;
            Debug.Assert(null != App.Current.ReportGenerator);
            Debug.Assert(generator.GetPresentedNames(false).Contains(reportWrapper.Name.Name));

            ReportInfo report = generator.GetReportInfo(reportWrapper.Name.Name);

            int result = 0;
            if ((null != report) && (null != report.SubReports))
                result = report.SubReports.Count;

            return result;
        }
    }
}

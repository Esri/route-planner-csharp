/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

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
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Map tip generator class
    /// </summary>
    internal class TipGenerator
    {
        #region constants

        private const string BARRIER_AVAILABLE_FMT = "{0} – {1}";
        private const string ROUTE_SEPARATOR = ", ";

        #endregion

        #region constructors

        private TipGenerator()
        { }

        static TipGenerator()
        {
            _barrierAvailableCaption = (string)App.Current.FindResource("BarrierAvailableTooltipText");
            _zoneAssignmentCaption = (string)App.Current.FindResource("ZoneAssignmentTooltipText");

            _optimizeAndEditPage = (OptimizeAndEditPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.SchedulePagePath);
        }

        #endregion

        #region public static methods

        /// <summary>
        /// Fill tip text
        /// </summary>
        /// <param name="textBlock">Text block to fill</param>
        /// <param name="data">Object source for tip</param>
        internal static void FillTipText(TextBlock textBlock, object data)
        {
            _skipNewLine = true;

            textBlock.Inlines.Clear();
            if (data is Location)
            {
                _FillLocation(textBlock.Inlines, data as Location);
            }
            else if (data is Route)
            {
                _FillRoute(textBlock.Inlines, data as Route);
            }
            else if (data is Order)
            {
                _FillOrder(textBlock.Inlines, data as Order);
            }
            else if (data is Stop)
            {
                _FillStop(textBlock.Inlines, data as Stop);
            }
            else if (data is Zone)
            {
                _FillZone(textBlock.Inlines, data as Zone);
            }
            else if (data is Barrier)
            {
                _FillBarrier(textBlock.Inlines, data as Barrier);
            }

            _skipNewLine = false;
        }

        /// <summary>
        /// Fill textBlock with order\stop names, that contains in this cluster
        /// </summary>
        /// <param name="textBlock">TextBlock to fill</param>
        /// <param name="dic">Cluster attributes</param>
        internal static void FillClusterToolText(TextBlock textBlock, IDictionary<string, object> dic)
        {
            textBlock.Inlines.Clear();
            int count = (int)dic[ALClusterer.COUNT_PROPERTY_NAME];
            for (int index = 0; index < count; index++)
            {
                string attributeName = ALClusterer.GRAPHIC_PROPERTY_NAME + index.ToString();
                DataGraphicObject dataGraphic = (DataGraphicObject)dic[attributeName];
                object item = dataGraphic.Data;

                Inline inline = null;
                Stop stop = item as Stop;
                Order order = item as Order;
                if (stop != null)
                    inline = new Run(stop.Name);
                else if (order != null)
                    inline = new Run(order.Name);
                else
                    Debug.Assert(false);

                textBlock.Inlines.Add(inline);

                if (index != count - 1)
                    textBlock.Inlines.Add(new LineBreak());
            }
        }
        
        #endregion

        #region private static methods

        /// <summary>
        /// Get property caption by property name
        /// </summary>
        /// <param name="name">Property name</param>
        /// <returns>Property caption</returns>
        private static string _FindPropertyCaption(string name)
        {
            string caption = null;

            foreach (TipProperty tipProperty in App.Current.MapDisplay.StopTitles)
            {
                if (name.Equals(tipProperty.Name, StringComparison.OrdinalIgnoreCase))
                {
                    caption = tipProperty.Title;
                    break;
                }
            }

            if (caption == null)
            {
                foreach (TipProperty tipProperty in App.Current.MapDisplay.OrderTitles)
                {
                    if (name.Equals(tipProperty.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        caption = tipProperty.Title;
                        break;
                    }
                }
            }

            if (caption == null)
            {
                caption = App.Current.MapDisplay.NotSelectableProperties[name];
            }

            Debug.Assert(caption != null);
            return caption;
        }

        /// <summary>
        /// Fill location tip
        /// </summary>
        /// <param name="inlines">Inlines to fill</param>
        /// <param name="location">Source location</param>
        private static void _FillLocation(InlineCollection inlines, Location location)
        {
            string name = location.Name;
            _AddLine(_nameCaption, name, inlines, false);
        }

        /// <summary>
        /// Fill zone tip
        /// </summary>
        /// <param name="inlines">Inlines to fill</param>
        /// <param name="location">Source zone</param>
        private static void _FillZone(InlineCollection inlines, Zone zone)
        {
            string name = zone.Name;
            _AddLine(_nameCaption, name, inlines, false);

            if (App.Current.MainWindow.CurrentPage == _optimizeAndEditPage)
            {
                string zoneAssignment = "";
                foreach (Route route in _optimizeAndEditPage.CurrentSchedule.Routes)
                {
                    if (route.Zones.Contains(zone))
                    {
                        if (zoneAssignment.Length != 0)
                            zoneAssignment += ROUTE_SEPARATOR;

                        zoneAssignment += route.Name;
                    }
                }

                if (zoneAssignment.Length > 0)
                    _AddLine(_zoneAssignmentCaption, zoneAssignment, inlines, false);
            }
        }

        /// <summary>
        /// Fill barrier tip
        /// </summary>
        /// <param name="inlines">Inlines to fill</param>
        /// <param name="location">Source barrier</param>
        private static void _FillBarrier(InlineCollection inlines, Barrier barrier)
        {
            string name = barrier.Name;
            _AddLine(_nameCaption, name, inlines, false);

            string availableAt = string.Format(BARRIER_AVAILABLE_FMT,
                barrier.StartDate.Date.ToShortDateString(),
                barrier.FinishDate.Date.ToShortDateString());
            _AddLine(_barrierAvailableCaption, availableAt, inlines, false);
        }

        /// <summary>
        /// Fill route tip
        /// </summary>
        /// <param name="inlines">Inlines to fill</param>
        /// <param name="location">Source route</param>
        private static void _FillRoute(InlineCollection inlines, Route route)
        {
            string name = route.Name;
            _AddLine(_nameCaption, name, inlines, false);
        }

        /// <summary>
        /// Fill order tip
        /// </summary>
        /// <param name="inlines">Inlines to fill</param>
        /// <param name="location">Source order</param>
        private static void _FillOrder(InlineCollection inlines, Order order)
        {
            string name = order.Name;
            _AddLine(_nameCaption, name, inlines, false);

            _AddPriorityIfNeeded(order, inlines);

            _AddTimeWindow(order.TimeWindow, false, inlines, false);
            if (!order.TimeWindow.IsWideOpen)
                _AddTimeWindow(order.TimeWindow2, true, inlines, false);

            foreach (TipProperty tipProperty in App.Current.MapDisplay.OrderTitlesSelected)
            {
                _AddBindableProperty(inlines, tipProperty, order);
            }
        }

        /// <summary>
        /// Gets time window of stop by index (0 or 1).
        /// </summary>
        /// <param name="stop">Stop object (expected it has associated object either Location or Order).</param>
        /// <param name="twIndex">Index of time window (use constants FIRST_TIME_WINDOW, SECOND_TIME_WINDOW).</param>
        /// <returns>Requested time window.</returns>
        private static TimeWindow _GetTimeWindowByIndex(Stop stop, int twIndex)
        {
            Debug.Assert(stop != null);
            Debug.Assert(twIndex == FIRST_TIME_WINDOW || twIndex == SECOND_TIME_WINDOW);

            // Requested time window.
            TimeWindow timeWindow;

            // If stop is Location.
            if (stop.AssociatedObject is Location)
            {
                Location location = stop.AssociatedObject as Location;

                timeWindow = (twIndex == FIRST_TIME_WINDOW) ?
                             location.TimeWindow :
                             location.TimeWindow2;
            }
            // If stop is Order.
            else if (stop.AssociatedObject is Order)
            {
                Order order = stop.AssociatedObject as Order;

                timeWindow = (twIndex == FIRST_TIME_WINDOW) ?
                             order.TimeWindow :
                             order.TimeWindow2;
            }
            else
            {
                // Unknown object.
                Debug.Assert(false);

                // Create default time window.
                timeWindow = new TimeWindow();
            }

            return timeWindow;
        }

        /// <summary>
        /// Checks if time window includes given time.
        /// </summary>
        /// <param name="timeWindow">Time window.</param>
        /// <param name="arriveDateTime">Arrive date time.</param>
        /// <param name="plannedDateTime">Planned date time.</param>
        /// <returns>True - if arrive time is inside time window, false - otherwise.</returns>
        private static bool _DoesTimeWindowIncludeTime(TimeWindow timeWindow, DateTime? arriveDateTime, DateTime? plannedDateTime)
        {
            Debug.Assert(timeWindow != null);

            bool result = false;

            // Check if arrive and planned date time have values.
            if (arriveDateTime.HasValue && plannedDateTime.HasValue)
            {
                // Check if arrive time is inside the time window.
                result = timeWindow.DoesIncludeTime(arriveDateTime.Value, plannedDateTime.Value);
            }
            else
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Checks if time window includes stop's arrive time.
        /// </summary>
        /// <param name="timeWindow">Time window.</param>
        /// <param name="stop">Stop object (expected it has associated object either Location or Order).</param>
        /// <returns>True if stop's arrive time is inside time window, false - otherwise.</returns>
        private static bool _DoesTimeWindowIncludeStopArriveTime(TimeWindow timeWindow, Stop stop)
        {
            Debug.Assert(timeWindow != null);
            Debug.Assert(stop != null);

            // Stop's arrive time.
            DateTime? arriveDateTime = stop.ArriveTime;

            // Stop's planned time.
            DateTime? plannedDateTime = stop.Route.Schedule.PlannedDate;

            // Check if time window includes stop's arrive time.
            bool result =
                _DoesTimeWindowIncludeTime(timeWindow, arriveDateTime, plannedDateTime);

            return result;
        }

        /// <summary>
        /// Adds stop's time windows.
        /// </summary>
        /// <param name="inlines">Inlines to add time window(s).</param>
        /// <param name="stop">(expected it has associated object either Location or Order).</param>
        private static void _AddStopTimeWindows(InlineCollection inlines, Stop stop)
        {
            Debug.Assert(inlines != null);
            Debug.Assert(stop != null);

            // Get 1-st time window of the stop.
            TimeWindow timeWindow1 = _GetTimeWindowByIndex(stop, FIRST_TIME_WINDOW);

            // Get 2-nd time window of the stop.
            TimeWindow timeWindow2 = _GetTimeWindowByIndex(stop, SECOND_TIME_WINDOW);

            // Flag dfines in time window should be displayed with bold font.
            bool isTimeWindowBoldFont = false;

            // If both time windows are wide open.
            if (timeWindow1.IsWideOpen && timeWindow2.IsWideOpen)
            {
                // Add 1-st time window.
                isTimeWindowBoldFont = true;
                _AddTimeWindow(timeWindow1, false, inlines, isTimeWindowBoldFont);
            }
            // If only 2-nd time window is wide open.
            else if (!timeWindow1.IsWideOpen && timeWindow2.IsWideOpen)
            {
                // Add 1-st time window.
                isTimeWindowBoldFont = _DoesTimeWindowIncludeStopArriveTime(timeWindow1, stop);
                _AddTimeWindow(timeWindow1, false, inlines, isTimeWindowBoldFont);
            }
            // If only 1-st time window is wide open.
            else if (timeWindow1.IsWideOpen && !timeWindow2.IsWideOpen)
            {
                // Add 2-nd time window.
                isTimeWindowBoldFont = _DoesTimeWindowIncludeStopArriveTime(timeWindow2, stop);
                _AddTimeWindow(timeWindow2, true, inlines, isTimeWindowBoldFont);
            }
            // If both time windows are not wide open.
            else if (!timeWindow1.IsWideOpen && !timeWindow2.IsWideOpen)
            {
                // Add 1-st time window.
                isTimeWindowBoldFont = _DoesTimeWindowIncludeStopArriveTime(timeWindow1, stop);
                _AddTimeWindow(timeWindow1, false, inlines, isTimeWindowBoldFont);

                // Add 2-nd time window.
                isTimeWindowBoldFont = _DoesTimeWindowIncludeStopArriveTime(timeWindow2, stop);
                _AddTimeWindow(timeWindow2, true, inlines, isTimeWindowBoldFont);
            }
            else
            {
                // Not reached condition.
                Debug.Assert(false);
            }
        }

        /// <summary>
        /// Fill stop tip
        /// </summary>
        /// <param name="inlines">Inlines to fill</param>
        /// <param name="location">Source stop</param>
        private static void _FillStop(InlineCollection inlines, Stop stop)
        {
            if (stop.StopType == StopType.Lunch)
            {
                Inline inline = new Run((string)App.Current.FindResource("LunchString"));
                inlines.Add(inline);
                return;
            }

            _AddName(inlines, stop);

            if (stop.AssociatedObject is Order)
            {
                _AddPriorityIfNeeded(stop.AssociatedObject as Order, inlines);
            }

            _AddArriveTime(stop.ArriveTime, inlines);

            // Add stop's time window(s).
            _AddStopTimeWindows(inlines, stop);

            foreach (TipProperty tipProperty in App.Current.MapDisplay.StopTitlesSelected)
            {
                _AddBindableProperty(inlines, tipProperty, stop);
            }
        }

        /// <summary>
        /// Add name property value
        /// </summary>
        /// <param name="inlines">inlines to fill</param>
        /// <param name="stop">Source stop</param>
        private static void _AddName(InlineCollection inlines, Stop stop)
        {
            string nameValue = "";
            if (stop.AssociatedObject is Location)
            {
                Location location = stop.AssociatedObject as Location;
                nameValue = location.Name;
            }
            else if (stop.AssociatedObject is Order)
            {
                Order order = stop.AssociatedObject as Order;
                nameValue = order.Name;
            }

            _AddLine(_nameCaption, nameValue, inlines, false);
        }

        /// <summary>
        /// Add bindable property value
        /// </summary>
        /// <param name="inlines">Inlines to fill</param>
        /// <param name="tipProperty">Property</param>
        /// <param name="source">Source object</param>
        private static void _AddBindableProperty(InlineCollection inlines, TipProperty tipProperty, object source)
        {
            try
            {
                PropertyPath propertyPath = new PropertyPath(tipProperty.PrefixPath + tipProperty.Name);

                Binding binding = new Binding();
                binding.Path = propertyPath;
                binding.Source = source;

                FrameworkElement fe = new FrameworkElement();
                BindingOperations.SetBinding(fe, FrameworkElement.DataContextProperty, binding);

                object value = fe.DataContext;
                if (value != null)
                {
                    string valueStr;

                    if (value is double && tipProperty.ValueUnits.HasValue && tipProperty.DisplayUnits.HasValue)
                    {
                        Unit valueUnits = tipProperty.ValueUnits.Value;
                        Unit displayUnits = tipProperty.DisplayUnits.Value;
                        if (valueUnits != displayUnits)
                            value = UnitConvertor.Convert((double)value, valueUnits, displayUnits);

                        valueStr = UnitFormatter.Format((double)value, displayUnits);
                    }
                    else
                    {
                        // Special case for planned date
                        if (tipProperty.Name.Equals(Order.PropertyNamePlannedDate))
                        {
                            DateTime dateTime = (DateTime) value;
                            valueStr = dateTime.ToShortDateString();
                        }
                        else if (tipProperty.Name.Equals(Stop.PropertyNameStopType))
                        {
                            valueStr = _GetStopTypeStr((StopType)value, (Stop)source);
                        }
                        else
                        {
                            valueStr = value.ToString();
                        }
                    }

                    if (valueStr.Length > 0)
                    {
                        _AddLine(tipProperty.Title, valueStr, inlines, false);
                    }
                }

                BindingOperations.ClearBinding(fe, FrameworkElement.DataContextProperty);
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
        }

        /// <summary>
        /// Convert stop type to string
        /// </summary>
        /// <param name="stopType">Stop type</param>
        /// <param name="stop">Stop</param>
        /// <returns>Correct localized string</returns>
        private static string _GetStopTypeStr(StopType stopType, Stop stop)
        {
            string result = "";

            switch (stopType)
            {
                case StopType.Location:
                    {
                        if (stop.SequenceNumber == 1)
                        {
                            result = (string)App.Current.FindResource("StartString");
                        }
                        else if (stop.SequenceNumber == stop.Route.Stops.Count)
                        {
                            result = (string)App.Current.FindResource("FinishString");
                        }
                        else
                        {
                            result = (string)App.Current.FindResource("RenewalString");
                        }

                        break;
                    }
                case StopType.Order:
                    {
                        result = (string)App.Current.FindResource("Order");
                        break;
                    }
                case StopType.Lunch:
                    {
                        Debug.Assert(false);
                        break;
                    }
            }

            return result;
        }

        /// <summary>
        /// Add priority if it High
        /// </summary>
        /// <param name="order">Source order</param>
        /// <param name="inlines">Inlines to fill</param>
        private static void _AddPriorityIfNeeded(Order order, InlineCollection inlines)
        {
            if (order.Priority != OrderPriority.Low)
            {
                string priority = order.Priority.ToString();
                _AddLine(_priorityCaption, priority, inlines, false);
            }
        }

        /// <summary>
        /// Add arrive time
        /// </summary>
        /// <param name="dateTime">Arrive time</param>
        /// <param name="inlines">Inlines to fill</param>
        private static void _AddArriveTime(DateTime? dateTime, InlineCollection inlines)
        {
            if (dateTime.HasValue)
            {
                string arriveTime;

                if (dateTime.Value.Date == App.Current.CurrentDate)
                    arriveTime = dateTime.Value.ToShortTimeString();
                else
                    arriveTime = string.Format((string)App.Current.FindResource("FullArriveTimeCellText"),
                        dateTime.Value.ToShortDateString(), dateTime.Value.ToShortTimeString());

                _AddLine(_arriveTimeCaption, arriveTime, inlines, false);
            }
        }

        /// <summary>
        /// Add Time window
        /// </summary>
        /// <param name="timeWindow">Source time window</param>
        /// <param name="second">Is second time window</param>
        /// <param name="inlines">Inlines to fill</param>
        /// <param name="bolded">Is need to be bold inline</param>
        private static void _AddTimeWindow(TimeWindow timeWindow, bool second, InlineCollection inlines, bool bolded)
        {
            string value = timeWindow.ToString();
            string timewindowCaption = _timewindowCaption;
            if (second)
                timewindowCaption += "2";
            _AddLine(timewindowCaption, value, inlines, bolded);
        }

        /// <summary>
        /// Add inline to inlines
        /// </summary>
        /// <param name="caption">Property Caption</param>
        /// <param name="value">Property value</param>
        /// <param name="inlines">Inlines to fill</param>
        /// <param name="boldValue">Is need to be bold inline</param>
        private static void _AddLine(string caption, string value, InlineCollection inlines, bool boldValue)
        {
            Inline inline = new Run(value);
            if (boldValue)
            {
                inline = new Bold(inline);
            }
            if (!_skipNewLine)
            {
                inlines.Add(new LineBreak());
            }
            else
            {
                _skipNewLine = false;
            }

            inlines.Add(new Bold(new Run(caption + ": ")));
            inlines.Add(inline);
        }

        #endregion

        #region private constants

        /// <summary>
        /// Index of the 1-st time window.
        /// </summary>
        private const int FIRST_TIME_WINDOW = 0;

        /// <summary>
        /// Index of the 2-nd time window.
        /// </summary>
        private const int SECOND_TIME_WINDOW = 1;

        #endregion private constants

        #region private members

        private static string _nameCaption = _FindPropertyCaption("Name");
        private static string _priorityCaption = _FindPropertyCaption("Priority");
        private static string _timewindowCaption = _FindPropertyCaption("TimeWindow");
        private static string _arriveTimeCaption = _FindPropertyCaption("ArriveTime");
        private static string _barrierAvailableCaption;
        private static string _zoneAssignmentCaption;
        
        private static bool _skipNewLine = false;
        private static OptimizeAndEditPage _optimizeAndEditPage;

        #endregion
    }
}

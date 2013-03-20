using System;
using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.BreaksHelpers;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Class-converter from Breaks collection to GP Record Set with GP Breaks feature collection.
    /// </summary>
    internal class BreaksConverter
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="plannedDate">Date on which routes are planning.</param>
        /// <param name="settings">Breaks settings.</param>
        public BreaksConverter(DateTime plannedDate, BreaksSettings settings)
        {
            _plannedDate = plannedDate;
            _settings = settings;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Method converts breaks from routes to GPRecordSet.
        /// </summary>
        /// <param name="routes">Routes collection to get breaks.</param>
        /// <param name="addSequence">Flag, showing that we must 
        /// add sequence attribute to break gp feature.</param>
        /// <returns>Breaks GPRecordSet.</returns>
        public GPRecordSet ConvertBreaks(ICollection<Route> routes, bool addSequence)
        {
            Debug.Assert(routes != null);

            List<GPFeature> features = new List<GPFeature>();

            // Convert all breaks for every route to GP Features.
            foreach (Route route in routes)
            {
                Breaks breaks = (Breaks)route.Breaks.Clone();
                features.AddRange(_ConvertToGPBreaks(breaks, route, addSequence));
            }

            GPRecordSet rs = null;

            if (features.Count > 0)
            {
                rs = new GPRecordSet();
                rs.Features = features.ToArray();
            }

            return rs;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Method do conversion of separate route Breaks to GP Breaks collection.
        /// </summary>
        /// <param name="routeBreaks">Route breaks.</param>
        /// <param name="route">Route, to which breaks belong to.</param>
        /// <param name="addSequence">Flag, showing that we must add 
        /// sequence attribute to break GP feature.</param>
        /// <returns>GP Breaks collection.</returns>
        private List<GPFeature> _ConvertToGPBreaks(Breaks routeBreaks, Route route, bool addSequence)
        {
            Debug.Assert(routeBreaks != null);
            Debug.Assert(route != null);

            List<GPFeature> features = new List<GPFeature>();

            routeBreaks.Sort();

            // Precedence should always start from 1.
            int precedence = 1;
            double cumulBreakServiceTime = 0;

            // Get feature collections of route breaks.
            foreach (Break currentBreak in routeBreaks)
            {
                GPFeature feature = new GPFeature();
                AttrDictionary attrs = new AttrDictionary();

                if ((currentBreak != null) && (currentBreak.Duration > 0.0))
                {
                    // Get specific attributes for every break type.
                    if (currentBreak is TimeWindowBreak)
                    {
                        attrs = _GetTimeWindowBreakAttributes(currentBreak);
                    }
                    else if (currentBreak is DriveTimeBreak)
                    {
                        DriveTimeBreak driveTimeBreak = currentBreak as DriveTimeBreak;
                        attrs = _GetDriveTimeBreakAttributes(driveTimeBreak,
                            ref cumulBreakServiceTime);
                    }
                    else if (currentBreak is WorkTimeBreak)
                    {
                        WorkTimeBreak workTimeBreak = currentBreak as WorkTimeBreak;
                        attrs = _GetWorkTimeBreakAttributes(workTimeBreak);
                    }
                    else
                        // Unsupported breaks type.
                        Debug.Assert(false);

                    // Get common attributes.
                    _FillCommonGPBreakAttributes(route.Id, precedence++, attrs);

                    // Add sequence atribute if we must to.
                    if (addSequence)
                    {
                        var sequenceNumber = _GetSequenceNumber(route, currentBreak);
                        attrs.Add(NAAttribute.SEQUENCE, sequenceNumber);
                    }

                    feature.Attributes = attrs;
                }

                features.Add(feature);
            }

            return features;
        }

        /// <summary>
        /// Get break sequence number.
        /// </summary>
        /// <param name="route">Route to which break belong to.</param>
        /// <param name="currentBreak">Break object.</param>
        /// <returns>Sequnce number of break in route, 
        /// or null if there is no such break in route.</returns>
        private object _GetSequenceNumber(Route route, Break currentBreak)
        {
            // Find index of break in breaks collection.
            var breakIndexInBreaksCollection = currentBreak.Breaks.IndexOf(currentBreak);

            // Index, showing number of breaks before current stop.
            int breaksBeforeOrder = 0;
            foreach (Stop stop in route.Stops)
            {
                // If stop type is break.
                if (stop.StopType == StopType.Lunch)
                {
                    // And if index of break is the same as 
                    // current break index in breaks collection - we have found stop, 
                    // corresponding current break. Return its sequence number.
                    if (breaksBeforeOrder == breakIndexInBreaksCollection)
                        return stop.SequenceNumber;
                    // If index is less - this stop represents another break, increase index.
                    else
                        breaksBeforeOrder++;
                }
            }

            // If we come here - there is no breaks at route - return null.
            return null;
        }

        /// <summary>
        /// Method gets Time Window break attributes.
        /// </summary>
        /// <param name="currentBreak">Current break.</param>
        /// <returns>Attributes of Time Window break.</returns>
        private AttrDictionary _GetTimeWindowBreakAttributes(Break currentBreak)
        {
            Debug.Assert(currentBreak != null);

            AttrDictionary attrs = new AttrDictionary();

            // Break Time Window.
            _SetTimeWindowAttribute(currentBreak, NAAttribute.TW_START,
                NAAttribute.TW_END, attrs);

            // Max violation time is 0, which means route has a hard Time Window.
            attrs.Add(NAAttribute.MAX_VIOLATION_TIME, 0.0);

            attrs.Add(NAAttribute.SERVICE_TIME, currentBreak.Duration);

            return attrs;
        }

        /// <summary>
        /// Method gets Drive Time break attributes.
        /// </summary>
        /// <param name="driveTimeBreak">Break.</param>
        /// <param name="cumulServiceTime">Accumulated breaks service time.</param>
        /// <returns>Attributes of Drive Time break.</returns>
        private AttrDictionary _GetDriveTimeBreakAttributes(
            DriveTimeBreak driveTimeBreak, ref double cumulServiceTime)
        {
            Debug.Assert(driveTimeBreak != null);

            AttrDictionary attrs = new AttrDictionary();

            // Convert to Time units.
            double timeInUnits = _ConvertTimeInterval(driveTimeBreak.TimeInterval);

            // Consider accumulated breaks service time for current break.
            double actualTravelTime = timeInUnits - cumulServiceTime;
            cumulServiceTime += actualTravelTime;

            // The maximum amount of drive time that can be
            // accumulated before the break is taken.
            if(actualTravelTime > 0)
                attrs.Add(NAAttribute.MaxTravelTimeBetweenBreaks, actualTravelTime);

            attrs.Add(NAAttribute.SERVICE_TIME, driveTimeBreak.Duration);

            return attrs;
        }

        /// <summary>
        /// Method gets Work Time break attributes.
        /// </summary>
        /// <param name="workTimeBreak">Break.</param>
        /// <returns>Attributes of Work Time break.</returns>
        private AttrDictionary _GetWorkTimeBreakAttributes(WorkTimeBreak workTimeBreak)
        {
            Debug.Assert(workTimeBreak != null);

            AttrDictionary attrs = new AttrDictionary();

            attrs.Add(NAAttribute.MaxCumulWorkTime,
                _ConvertTimeInterval(workTimeBreak.TimeInterval));

            attrs.Add(NAAttribute.SERVICE_TIME, workTimeBreak.Duration);

            return attrs;
        }

        /// <summary>
        /// Method gets Common breaks attributes.
        /// </summary>
        /// <param name="routeId">Route id to be appointed break.</param>
        /// <param name="precedence">Break precedence.</param>
        /// <param name="attrs">Attributes to fill in.</param>
        private void _FillCommonGPBreakAttributes(Guid routeId, 
            int precedence, AttrDictionary attrs)
        {
            Debug.Assert(routeId != null);
            Debug.Assert(precedence >= 1);

            attrs.Add(NAAttribute.ROUTE_NAME, routeId.ToString());
            attrs.Add(NAAttribute.Precedence, precedence);

            // Paid break flag is not used in ALR, the breaks are treated as paid.
            attrs.Add(NAAttribute.IsPaid, (int)NABool.True);
        }

        /// <summary>
        /// Method fills TimeWindow attributes in correct format.
        /// </summary>
        /// <param name="routeBreak">Route break.</param> 
        /// <param name="attrNameStart">Format string for Start TW.</param>
        /// <param name="attrNameEnd">Format string for End TW</param>
        /// <param name="attributes">Attributes to fill in.</param>
        private void _SetTimeWindowAttribute(Break routeBreak, string attrNameStart,
            string attrNameEnd, AttrDictionary attributes)
        {
            Debug.Assert(routeBreak != null);
            Debug.Assert(!string.IsNullOrEmpty(attrNameStart));
            Debug.Assert(!string.IsNullOrEmpty(attrNameEnd));

            var twBreak = routeBreak as TimeWindowBreak;

            var window = new TimeWindow
            {
                IsWideOpen = false,
                From = twBreak.From,
                To = twBreak.To,
                Day = twBreak.Day
            };

            attributes.SetTimeWindow(window, _plannedDate, attrNameStart, attrNameEnd);
        }

        /// <summary>
        /// Method converts time interval in Hours to actual Time Units value
        /// which is set in the Solve settings.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns>Correctly converted Time Interval value.</returns>
        private double _ConvertTimeInterval(double value)
        {
            Debug.Assert(value >= 0);

            // Hours to minutes.
            double timeInterval = value * MINUTES_IN_HOUR;

            return timeInterval;
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Minutes in hour.
        /// </summary>
        private const int MINUTES_IN_HOUR = 60;

        #endregion

        #region Private fields

        /// <summary>
        /// Planned date.
        /// </summary>
        private DateTime _plannedDate;

        /// <summary>
        /// Breaks settings.
        /// </summary>
        private BreaksSettings _settings;

        #endregion
    }
}

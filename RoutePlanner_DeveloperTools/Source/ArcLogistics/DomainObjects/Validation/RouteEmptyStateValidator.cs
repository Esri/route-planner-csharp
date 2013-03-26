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
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.BreaksHelpers;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// Route empty state validator. 
    /// Analyzes, that route can be started and finished.
    /// For routes with timewindow breaks also check that all breaks can be visited.
    /// </summary>
    internal sealed class RouteEmptyStateValidator : Validator
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of the <c>RouteEmptyStateViolationValidator</c> class.
        /// </summary>
        public RouteEmptyStateValidator()
            : base(null, null)
        { }

        #endregion // Constructors

        #region Protected methods

        /// <summary>
        /// Does validation.
        /// </summary>
        /// <param name="objectToValidate">Object to validation.</param>
        /// <param name="currentTarget">Current target (expected only
        /// <see cref="T:ESRI.ArcLogistics.DomainObjects.Route"/>).</param>
        /// <param name="key">Ignored.</param>
        /// <param name="validationResults">Validation results.</param>
        protected override void DoValidate(object objectToValidate,
                                           object currentTarget,
                                           string key,
                                           ValidationResults validationResults)
        {
            // Save validating route, key and validation results,
            // this need for adding error messages.
            _route = currentTarget as Route;
            _key = key;
            _validationResults = validationResults;

            // Check input parameters.
            Debug.Assert(_route != null);

            // Check that assumptions about dependent properties are correct.
            if (!_DependeciesAreValid())
                return;

            // Check max total duration.
            if (!_MaxTotalDurationIsValid())
                return;

            // Try to get trip optimal time window.
            // Trip is the time from the point when vehicle leave start location
            // and up to the end of route max duration time.
            var tripOptimalTimeRange = _GetTripOptimalTimeRange();

            // If trip cannot be started - end validation.
            if (tripOptimalTimeRange == null)
                return;

            // If breaks are timewindow breaks and they are valid - check that 
            // all of breaks can be visited and that after breaks finish
            // location can be visited.
            if (_route.Breaks.Count != 0 && _route.Breaks[0] is TimeWindowBreak &&
                string.IsNullOrEmpty(_route[Route.PropertyNameBreaks]))
            {
                if (_CheckBreaksCanBeVisited(tripOptimalTimeRange) &&
                    _route.EndLocation != null)
                    _CheckFinishLocation(tripOptimalTimeRange);
            }
            else if (_route.Breaks.Count == 0)
                _CheckFinishLocation(tripOptimalTimeRange);
        }

        /// <summary>
        /// Default message template for validation.
        /// </summary>
        protected override string DefaultMessageTemplate
        {
            get { return base.MessageTemplate; }
        }

        #endregion // Protected methods

        #region Private methods

        /// <summary>
        /// If route does not have enough duration for visiting start and end locations 
        /// show error message and stop validation.
        /// </summary>
        /// <returns>'True' if maxtotalduration is valid, 'false' otherwise.</returns>
        private bool _MaxTotalDurationIsValid()
        {
            // Calculate time, that route must spend on start/end locations.
            double timeAtLocations = 0;
            if (_route.StartLocation != null)
                timeAtLocations += _route.TimeAtStart;
            if (_route.EndLocation != null)
                timeAtLocations += _route.TimeAtEnd;

            // If route max duration is less then time, which must be spend at locations,
            // log error, return false.
            if (_GetMaxTotalDuration() < timeAtLocations)
            {
                this.LogValidationResult(_validationResults,
                                Properties.Messages.Error_VRP_MaxTotalDurationIsSmall,
                                _route, _key);
                return false;
            }
            else
                return true;
        }

        /// <summary>
        /// Check that all assumptions about dependent properties are correct.
        /// </summary>
        /// <returns></returns>
        private bool _DependeciesAreValid()
        {
            // Time at end, route max total duration and
            // time at start must be non-negative.
            if (_route.TimeAtEnd < 0 && _route.TimeAtStart < 0 && _route.MaxTotalDuration < 0)
                return false;

            // Start time window must be not null.
            if (_route.StartTimeWindow == null)
                return false;

            // Breaks must be not null.
            if (_route.Breaks == null)
                return false;

            return true;
        }

        /// <summary>
        /// Returns trip optimal time window.
        /// If there was validation errors - log them and return null.
        /// </summary>
        /// <returns>Time window if it was found, null if wasn't.</returns>
        private TimeRange _GetTripOptimalTimeRange()
        {
            TimeRange tripTimeRange = null;

            // Get trip start time ranges from intersection of 
            // trip start time window and start location time windows.
            var tripStartTimeRanges = _GetTripPossibleStartTimeRanges();

            // If there is no trip start time range - route cannot be started - 
            // add erorr message.
            if (tripStartTimeRanges.Count == 0)
            {
                this.LogValidationResult(_validationResults,
                    Properties.Messages.Error_VRP_StartLocationCannotBeVisited, _route, _key);
            }
            // Calculate trip time window.
            else
            {
                // If breaks collection isnt empty and breaks are time window breaks, 
                // get optimal trip time range, analyzing first break.
                if (_route.Breaks.Count != 0 && _route.Breaks[0] is TimeWindowBreak)
                {
                    tripTimeRange = _GetTripTimeRangeOptimalForFirstBreak(tripStartTimeRanges);

                    // If we cannot calculate trip time window - all breaks are invalid.
                    if (tripTimeRange == null)
                        _AddErrorMessages(_route.Breaks);
                }
                // If there is no breaks and finish location isnt empty, get optimal trip time 
                // range, based on finish location time windows.
                else if (_route.EndLocation != null)
                {
                    tripTimeRange = _GetTripTimeRangeOptimalForEndLocation(tripStartTimeRanges);

                    // If we cannot visit finish location - add validation error.
                    if (tripTimeRange == null)
                        this.LogValidationResult(_validationResults,
                        	Properties.Messages.Error_VRP_FinishLocationCannotBeVisited, _route, _key);
                }
            }

            return tripTimeRange;
        }

        /// <summary>
        /// Get list of time ranges in which route can leave start location.
        /// </summary>
        /// <returns>List with time ranges.</returns>
        private List<TimeRange> _GetTripPossibleStartTimeRanges()
        {
            var tripStartTimeRanges = new List<TimeRange>();

            // If there is no start location - trip start time range
            // is equal to route start time range.
            if (_route.StartLocation == null)
                tripStartTimeRanges.Add(new TimeRange(_route.StartTimeWindow));
            // If there is start location - trip start time window 
            // is intersection of route start time range and start
            // location time window.
            else
            {
                // If location time windows isnt wideopen, check that theirs time range intersects.
                if (!_StartLocationIsWideopen())
                {
                    tripStartTimeRanges.Add(_GetTripStartTimeRange(_route.StartLocation.TimeWindow));
                    tripStartTimeRanges.Add(_GetTripStartTimeRange(_route.StartLocation.TimeWindow2));
                    tripStartTimeRanges.RemoveAll(x => x == null);
                }
                // If location time window is wideopen - then trip start time  
                // depends only on route's timewindow.
                else
                {
                    TimeRange routeStartTimeRange = new TimeRange(_route.StartTimeWindow);
                    tripStartTimeRanges.Add(routeStartTimeRange.Shift(
                        TimeSpan.FromMinutes(_route.TimeAtStart)));
                }
            }

            return tripStartTimeRanges;
        }

        /// <summary>
        /// Check that start location time windows are wideopen.
        /// </summary>
        /// <returns>'True' if theya re wideopen, 'false' otherwise.</returns>
        private bool _StartLocationIsWideopen()
        {
            return _route.StartLocation.TimeWindow.IsWideOpen &&
                _route.StartLocation.TimeWindow2.IsWideOpen;
        }

        /// <summary>
        /// Get time range in which trip can be started.
        /// </summary>
        /// <param name="startLocationTimeWindow">Start location time window.</param>
        /// <returns>Time range if it was found, null otherwise.</returns>
        private TimeRange _GetTripStartTimeRange(TimeWindow startLocationTimeWindow)
        {
            // If start location timewindow is wideopen - ignore it.
            if (startLocationTimeWindow == null || startLocationTimeWindow.IsWideOpen)
                return null;

            TimeRange routeStartTimeRange = new TimeRange(_route.StartTimeWindow);
            TimeRange startLocationTimeRange = new TimeRange(startLocationTimeWindow);
            TimeRange result = null;

            // Calculate the time which route can spend at location at start.
            result =
                routeStartTimeRange.Intersection(startLocationTimeRange);

            // Trip will really start later.
            if (result != null)
                result = result.Shift(
                    TimeSpan.FromMinutes(_route.TimeAtStart));

            return result;
        }
        
        /// <summary>
        /// Get route time range.
        /// </summary>
        /// <param name="realStartTimeRanges">Time window on which route really will start.</param>
        /// <returns>Time range from the moment route leave start location to the moment
        /// when route duration will be equal to max total duration.</returns>
        private TimeRange _GetTripTimeRangeOptimalForFirstBreak(List<TimeRange> realStartTimeRanges)
        {
            // Calculate trip maximum duration.
            double tripMaxDurationMinutes;
            // If we dont have start location - trip maximum duration will be equal
            // to route max total Duration.
            if (_route.StartLocation == null)
                tripMaxDurationMinutes = _GetMaxTotalDuration();
            // If we have start location - trip max duration will consist of
            // route max total duration minus time that route spend on start location.
            // We must not subtract time at finish location, because otherwise there can be 
            // a situation that error when finish location is invalid will be detected as 
            // as error that last break cannot be finished.
            else
                tripMaxDurationMinutes = _GetMaxTotalDuration() - _route.TimeAtStart;
            var tripMaxDuration = TimeSpan.FromMinutes(tripMaxDurationMinutes);

            
            // Get first route's break and normalize it.
            var firstBreak = _GetSortedBreaks()[0] as TimeWindowBreak;

            // Calculate list of starts, which is optimized for first break.
            var realStarts = new List<TimeSpan?>();
            foreach (var timeRange in realStartTimeRanges)
                realStarts.Add(_GetRealStart(timeRange, firstBreak, tripMaxDuration));

            // Select latest start.
            long maximumStart = realStarts.Max(delegate(TimeSpan? ts)
                {
                    if (ts != null)
                        return ((TimeSpan)ts).Ticks;
                    else
                        return 0;
                });
            TimeSpan tripBestStart = new TimeSpan(maximumStart);
            TimeRange result = null;

            // If best start exist - calculate trip time range.
            if (tripBestStart != TimeSpan.Zero)
            {
                var start = (TimeSpan)tripBestStart;
                result = new TimeRange(start, start + tripMaxDuration);
            }

            return result;
        }

        /// <summary>
        /// Get the time span when route must start to satisfy first break.
        /// </summary>
        /// <param name="startTimeRange">TimeRange in which route can left start location.</param>
        /// <param name="firstBreak">TimeWindowBreak.</param>
        /// <param name="tripMaxDuration">Max trip duration.</param>
        /// <returns>TimeSpan?.</returns>
        private TimeSpan? _GetRealStart(TimeRange startTimeRange, 
            TimeWindowBreak firstBreak, TimeSpan tripMaxDuration)
        {
            // Calculate trip possible time range.
            var tripPossibleTimeRange = startTimeRange.Clone() as TimeRange;
            tripPossibleTimeRange.To += tripMaxDuration;

            // Get finishTimeRange of trip and break time ranges.
            var intersection = tripPossibleTimeRange.Intersection(
                new TimeRange(firstBreak.EffectiveFrom, firstBreak.EffectiveFrom));

            // If they intersects - calculate trip start.
            if (intersection != null)
                return _MinTimeSpan(startTimeRange.To, intersection.To);
            else
                return null;
        }

        /// <summary>
        /// Select minimum of two nullable time spans.
        /// </summary>
        /// <param name="first">TimeSpan?.</param>
        /// <param name="second">TimeSpan?.</param>
        /// <returns>TimeSpan?.</returns>
        private TimeSpan? _MinTimeSpan(TimeSpan? first, TimeSpan? second)
        {
            if (first != null && second != null)
                return first < second ? first : second;
            else
                return first != null ? first : second;
        }

        /// <summary>
        /// Return trip time range, which will allow to visit finish location.
        /// </summary>
        /// <param name="tripStartTimeRange">List of time ranges, in which route can be started.</param>
        /// <returns>Trip time range if it was found, null otherwise.</returns>
        private TimeRange _GetTripTimeRangeOptimalForEndLocation(List<TimeRange> tripStartTimeRanges)
        {
            TimeRange result = null;

            // Calculate trip duration.
            var tripDuration = TimeSpan.FromMinutes(_GetMaxTotalDuration() - _route.TimeAtStart);

            // Calculate list of end locations.
            var finishLocationTimeWindows = new List<TimeWindow>();
            finishLocationTimeWindows.Add(_route.EndLocation.TimeWindow);
            finishLocationTimeWindows.Add(_route.EndLocation.TimeWindow2);

            // Try to find such trip time range, which will satisfy 
            // any of finish location time windows.
            foreach (var timeRange in tripStartTimeRanges)
                foreach (var finishTimeWindow in finishLocationTimeWindows)
                {
                    if (result == null)
                        result = _GetTripTimeRangeOptimalForEndlLocation(timeRange,
                            finishTimeWindow, tripDuration);
                }
            return result;
        }

        /// <summary>
        /// Return trip time range, which will allow to visit finish location.
        /// </summary>
        /// <param name="tripTimeRange">TimeRange.</param>
        /// <param name="endLocationTimeWindow">TimeWindow.</param>
        /// <param name="tripDuration">Trip max duration.</param>
        /// <returns>Trip time range if it was found, null otherwise.</returns>
        private TimeRange _GetTripTimeRangeOptimalForEndlLocation(
            TimeRange tripTimeRange,TimeWindow endLocationTimeWindow, TimeSpan tripDuration)
        {
            TimeRange result = null;

            // Calculate time window in which trip can be finished.
            var tripFinishTimeRange = tripTimeRange.Clone() as TimeRange;
            tripFinishTimeRange.To += tripDuration;

            // If after finishing trip end location can be visited - calculate trip time window.
            var finishTimeRange = tripFinishTimeRange.Intersection(
                new TimeRange(endLocationTimeWindow));
            if (finishTimeRange != null && finishTimeRange.Length.TotalMinutes > _route.TimeAtEnd)
            {
                var tripStart = tripTimeRange.From > finishTimeRange.From ? 
                    tripTimeRange.From : finishTimeRange.From;
                result = new TimeRange(tripStart, tripStart + tripDuration);
            }

            return result;
        }

        /// <summary>
        /// Check that all route's breaks can be visited.
        /// For invalid breaks add error messages.
        /// </summary>
        /// <param name="tripTimeRange">Trip start time window.</param>
        /// <rereturns>'True' if all breaks can be visited, 'false' otherwise.</rereturns>
        private bool _CheckBreaksCanBeVisited(TimeRange tripTimeRange)
        {
            var result = true;

            // Prepare breaks for validation.
            Breaks sortedBreaks = _GetSortedBreaks();

            // Collection with breaks, which cannot be completed at time.
            Breaks invalidBreaks = new Breaks();

            // Check that all breaks can be completed in time.
            for (int i = 0; i < sortedBreaks.Count; i++)
            {
                TimeWindowBreak breakToCheck = sortedBreaks[i] as TimeWindowBreak;

                // Try to calculate break start.
                var breakStart = tripTimeRange.Intersection(breakToCheck.EffectiveFrom, 
                    breakToCheck.EffectiveTo);

                // If break cannot be started - it is invalid. Check next break. 
                if (breakStart == null)
                {
                    invalidBreaks.Add(breakToCheck);
                }
                // Check that break can be finished.
                else
                {
                    // Try calculate break finish.
                    var breakFinish = breakStart.Shift(TimeSpan.FromMinutes(breakToCheck.Duration));
                    breakFinish = breakFinish.Intersection(tripTimeRange);

                    // If break cannot be finished - it is invalid.
                    if (breakFinish == null)
                        invalidBreaks.Add(breakToCheck);
                }
            }

            // If there was invalid breaks - show error messages.
            if (invalidBreaks.Count != 0)
            {
                _AddErrorMessages(invalidBreaks);
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Return sorted route breaks collection, which breaks are normalized.
        /// </summary>
        /// <returns>Sorted collection of normalized time window breaks.</returns>
        private Breaks _GetSortedBreaks()
        {
            Breaks sortedBreaks = _route.Breaks.Clone() as Breaks;
            sortedBreaks.Sort();

            return sortedBreaks;
        }


        /// <summary>
        /// Check that finish location will be open at routes finish.
        /// </summary>
        /// <param name="tripTimeRange">Trip time window.</param>
        private void _CheckFinishLocation(TimeRange tripTimeRange)
        {
            // If route have no end location - we have nothing to validate.
            if (_route.EndLocation == null)
                return;

            // Calculate trip finish time window.
            TimeRange tripFinishTimeRange = _GetTripPossibleFinishTimeRange(tripTimeRange);

            // Calculate time, which route can spend at finish.
            var timeAtFinish = _GetBestTimeAtFinish(tripFinishTimeRange, _route.EndLocation);

            // If finish location cannot be visited - show error.
            if (timeAtFinish == 0)
            {
                this.LogValidationResult(_validationResults,
                                Properties.Messages.Error_VRP_FinishLocationCannotBeVisited, _route, _key);
            }
            // Check that finish location can be serviced.
            else
            {
                if (timeAtFinish < _route.TimeAtEnd)
                    this.LogValidationResult(_validationResults,
                        Properties.Messages.Error_VRP_FinishLocationCannotBeServiced, _route, _key);
            }
        }

        /// <summary>
        /// Calculate time range, when trip can be finished.
        /// </summary>
        /// <param name="tripTimeRange">TimeRange.</param>
        /// <returns>TimeRange.</returns>
        private TimeRange _GetTripPossibleFinishTimeRange(TimeRange tripTimeRange)
        {
            // Calculate route finish time window.
            TimeRange routeFinishTimeRange = new TimeRange();
            // If we have breaks that must be visited. 
            if (_route.Breaks.Count != 0 && _route.Breaks[0] is TimeWindowBreak)
            {
                // Route finish time window will be from trip earliest possible finish 
                // to time when trip can be finished.

                // Calculate last break finish time window.
                TimeWindowBreak lastBreak = _GetSortedBreaks()[_route.Breaks.Count - 1] as TimeWindowBreak;
                TimeRange lastBreakTimeRange =
                    new TimeRange(lastBreak.EffectiveFrom, lastBreak.EffectiveTo);
                TimeRange lastBreakFinishTimeRange = lastBreakTimeRange.Shift(
                    TimeSpan.FromMinutes(lastBreak.Duration));
                TimeRange lastBreakRealFinishTimeRange = lastBreakFinishTimeRange.Intersection(tripTimeRange);

                // Trip earliest finish is minimum time, when last break will be completed.
                var earliestTripFinish = lastBreakRealFinishTimeRange.From;

                routeFinishTimeRange = new TimeRange(earliestTripFinish, tripTimeRange.To);
            }
            // If we have no breaks to visit route finish time window will be equal to trip time window.
            else
                routeFinishTimeRange = tripTimeRange;

            return routeFinishTimeRange;
        }

        /// <summary>
        /// Calculate the longest time, which route can spend at finish location.
        /// </summary>
        /// <param name="tripFinishTimeRange">Time range in which route can be finished.</param>
        /// <param name="location">Finish location.</param>
        /// <returns>Minutes, which route can spend at finish location.</returns>
        private double _GetBestTimeAtFinish(TimeRange tripFinishTimeRange, Location location)
        {
            // If finish is wideopen then time at finish is equal to route finish time range.
            if (location.TimeWindow.IsWideOpen && location.TimeWindow2.IsWideOpen)
                return tripFinishTimeRange.Length.TotalMinutes;
            else 
            {
                var possibleFinish = new List<TimeRange>();
                if (!location.TimeWindow.IsWideOpen)
                    possibleFinish.Add(tripFinishTimeRange.Intersection(new TimeRange(location.TimeWindow))); 
                if (!location.TimeWindow2.IsWideOpen)
                    possibleFinish.Add(tripFinishTimeRange.Intersection(new TimeRange(location.TimeWindow2)));
                possibleFinish.RemoveAll(x => x == null);

                if (possibleFinish.Count == 0)
                    return 0;
                else
                    return possibleFinish.Max(x => x.Length.TotalMinutes);
            }
        }

        /// <summary>
        /// Show error message for each invalid break.
        /// </summary>
        /// <param name="invalidBreaks">Collection with invalid breaks.</param>
        private void _AddErrorMessages(Breaks invalidBreaks)
        {
            // If there is only one break at route.
            if (_route.Breaks.Count == 1)
            {
                this.LogValidationResult(_validationResults,
                    Properties.Messages.Error_VRP_BreakCannotBeVisited, _route, _key);
            }
            // If all breaks on route are invalid.
            else if (invalidBreaks.Count == _route.Breaks.Count)
            {
                this.LogValidationResult(_validationResults,
                    Properties.Messages.Error_VRP_AllBreaksCannotBeVisited, _route, _key);
            }
            // If some of the breaks are invalid.
            else if (invalidBreaks.Count != 0)
            {
                var invalidBreak = invalidBreaks[0] as TimeWindowBreak;
                string message;
                int indexOfInvalidBreak = _IndexInUnsortedCollection(invalidBreak);

                // If only last break is invalid.
                if (indexOfInvalidBreak == _route.Breaks.Count - 1)
                    message = string.Format(Properties.Messages.Error_VRP_LastBreakCannotBeVisited);
                // If invalid some breaks, starting from the middle.
                else
                    message = string.Format(Properties.Messages.Error_VRP_TheBreakAndAllAfterItCannotBeVisited,
                        BreaksHelper.GetOrdinalNumberName(indexOfInvalidBreak));
                this.LogValidationResult(_validationResults, message, _route, _key);
            }
        }

        /// <summary>
        /// Return route duration in minutes.
        /// </summary>
        /// <returns>Route duration in minutes.</returns>
        private double _GetMaxTotalDuration()
        {
            // If route duration is unlimited return such duration, 
            // which is much more then route can need.
            return (_route.MaxTotalDuration == 0 ?
                TimeSpan.FromDays(_GetTotalDays() * 2).Minutes : _route.MaxTotalDuration);
        }

        /// <summary>
        /// Maximum amount of days which route need.
        /// </summary>
        private int _GetTotalDays()
        {
            int result = 0;
            foreach (TimeWindowBreak br in _route.Breaks)
                result += br.Day > 0 ? (int)br.Day : 1;

            // If we have start or end location, we must add 3 days if theirs time windows
            // are not wideopen, and 1 day if they are.
            if (_route.StartLocation != null)
            {
                result += _GetTimeAtLocation(_route.StartLocation.TimeWindow);
                result += _GetTimeAtLocation(_route.StartLocation.TimeWindow2);
            }
            if (_route.EndLocation != null && _route.EndLocation.TimeWindow != null)
            {
                result += _GetTimeAtLocation(_route.EndLocation.TimeWindow);
                result += _GetTimeAtLocation(_route.EndLocation.TimeWindow2);
            }

            return result;
        }

        /// <summary>
        /// Maximum amount of days which route can need at location.
        /// </summary>
        /// <param name="timeWindow">TimeWindow.</param>
        /// <returns>Maximum number of days, which route can spend at this location.</returns>
        private int _GetTimeAtLocation(TimeWindow timeWindow)
        {
            int result = 0;

            if (!timeWindow.IsWideOpen)
                result += 3;
            else
                result += 1;
            return result;
        }
            

        /// <summary>
        /// Return index of break in original route collection.
        /// </summary>
        /// <param name="breakObj">Break which index must be detected.</param>
        /// <returns></returns>
        private int _IndexInUnsortedCollection(TimeWindowBreak breakObj)
        {
            for (int i = 0; i < _route.Breaks.Count; i++)
                if ((_route.Breaks[i] as TimeWindowBreak).EffectiveFrom == breakObj.EffectiveFrom)
                    return i;

            return -1;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Route, which we are validating.
        /// </summary>
        private Route _route;

        /// <summary>
        /// Need for adding messages.
        /// </summary>
        private string _key;

        /// <summary>
        /// Need for adding messages.
        /// </summary>
        private ValidationResults _validationResults;

        #endregion
    }
}
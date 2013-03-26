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
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.Data;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// Check that Driver/Vehicle isnt used by other route.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    sealed class FreeRouteAssetValidator : Validator<DataObject>
    {
        #region Constructors

        /// <summary>
        /// Create a new instance of the <c>FreeItemValidator</c> class.
        /// </summary>
        public FreeRouteAssetValidator()
            : base(null, null)
        { }

        #endregion

        #region Protected methods

        /// <summary>
        /// Does validation.
        /// </summary>
        /// <param name="objectToValidate">Driver or Vehicle.</param>
        /// <param name="currentTarget">Current route.</param>
        /// <param name="key">Ignored.</param>
        /// <param name="validationResults">Validation results.</param>
        protected override void DoValidate(DataObject objectToValidate,
                                           object currentTarget,
                                           string key,
                                           ValidationResults validationResults)
        {
            // Check input parameters.
            Route currentRoute = currentTarget as Route;
            Debug.Assert(currentRoute != null);

            if (objectToValidate == null)
                return;

            var routesOwner = currentRoute.RoutesCollectionOwner;
            if (routesOwner == null)
            {
                return;
            }

            var routes = routesOwner.FindRoutes(objectToValidate);
            foreach (var route in routes)
            {
                if (route == currentRoute)
                {
                    continue;
                }

                // Check that routes are intersects in time.
                if (_AreRoutesIntersected(currentRoute, route))
                {
                    // Item is used in another route - log error message.
                    var errorMessage = _GetErrorMessage(currentRoute, objectToValidate);
                    this.LogValidationResult(
                        validationResults,
                        errorMessage,
                        currentRoute,
                        key);

                    return;
                }
            }
        }

        /// <summary>
        /// Default message template for validation.
        /// </summary>
        protected override string DefaultMessageTemplate
        {
            get { return null; }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get error message for Driver or Vehicle.
        /// </summary>
        /// <param name="currentRoute">Route.</param>
        /// <param name="objectToValidate">Object, already assigned to other route.</param>
        /// <returns>String with error message.</returns>
        private string _GetErrorMessage(Route currentRoute, DataObject objectToValidate)
        {
            // If route is scheduled.
            if (currentRoute.Schedule != null)
                if (objectToValidate is Driver)
                    return Properties.Messages.Error_ScheduledRouteContainsInvalidDriver;
                else
                    return Properties.Messages.Error_ScheduledRouteContainsInvalidVehicle;

            // If it is default route.
            else
                if (objectToValidate is Driver)
                    return Properties.Messages.Error_DefaultRouteContainsInvalidDriver;
                else
                    return Properties.Messages.Error_DefaultRouteContainsInvalidVehicle;
        }

        /// <summary>
        /// Check that routes are intersects in time.
        /// </summary>
        /// <param name="currentRoute">First route to check.</param>
        /// <param name="route">Second route.</param>
        /// <returns>'True' if routes are intersected, 'false' otherwise.</returns>
        private bool _AreRoutesIntersected(Route currentRoute, Route route)
        {
            // If this is default routes.
            if (currentRoute.Schedule == null)
                // Check for days intersection.
                if (!_AreDaysIntersected(currentRoute, route))
                    return false;

            // Check for timewindow intersection.
            if (_AreTimeWindowsIntersect(route, currentRoute))
                return true;

            return false;
        }

        /// <summary>
        /// Check are working days of routes intersects.
        /// </summary>
        /// <param name="currentRoute">First route.</param>
        /// <param name="route">Second route.</param>
        /// <returns>'True' if they are intersected, 'false' otherwise.</returns>
        private bool _AreDaysIntersected(Route currentRoute, Route route)
        {
            // Calculating list of days of the week on which both routes are enabled.
            List<DayOfWeek> daysOfWeek = new List<DayOfWeek>();
            for (DayOfWeek day = DayOfWeek.Sunday; day <= DayOfWeek.Saturday; day++)
                if (currentRoute.Days.IsDayEnabled(day) && route.Days.IsDayEnabled(day))
                    daysOfWeek.Add(day);

            // If there are no days of week then routes are not intersects in time.
            if (daysOfWeek.Count == 0)
                return false;

            // If both routes are wide open then routes are intersected.
            else if ((route.Days.From == null && route.Days.To == null) &&
                (currentRoute.Days.From == null && currentRoute.Days.To == null))
                return true;

            // If both routes doesnt have start date or finish date then routes are intersects.
            else if ((route.Days.From == null && currentRoute.Days.From == null) ||
                (route.Days.To == null && currentRoute.Days.To == null))
                return true;

            // Calculating Dates intersection.
            DateTime intersectionStart = (DateTime)_GetStartDate(route.Days.From, currentRoute.Days.From);
            DateTime intersectionFinish = (DateTime)_GetFinishDate(route.Days.To, currentRoute.Days.To);

            // If finish is earlier then start - routes are not intersects.
            if (intersectionFinish < intersectionStart)
                return false;

            // Check that days of week intersection doesnt contains days in dates intersection.
            for (DateTime date = intersectionStart; date <= intersectionFinish; date = date.AddDays(1))
            {
                if (daysOfWeek.Contains(date.DayOfWeek))
                    return true;
            }

            // If we come here - days are not intersects.
            return false;
        }

        /// <summary>
        /// Return latest date from two dates. One of dates for sure isn't null.
        /// </summary>
        /// <param name="day1">First date.</param>
        /// <param name="day2">Second date.</param>
        /// <returns>Latest date, if one of dates is null - it returns another.</returns>
        private DateTime? _GetStartDate(DateTime? day1, DateTime? day2)
        {
            if (day1 != null)
            {
                if (day2 != null)
                    return day1 > day2 ? day1 : day2;
                else return day1;
            }
            return day2;
        }

        /// <summary>
        /// Return earlier date from two dates. One of dates for sure isn't null.
        /// </summary>
        /// <param name="day1">First date.</param>
        /// <param name="day2">Second date.</param>
        /// <returns>Earlier date, if one of dates is null - it returns another.</returns>
        private DateTime? _GetFinishDate(DateTime? day1, DateTime? day2)
        {
            if (day1 != null)
            {
                if (day2 != null)
                    return day1 < day2 ? day1 : day2;
                return day1;
            }

            return day2;
        }
        
        /// <summary>
        /// Compare names of driver/vehicle and routes driver/vehicle.
        /// Name comparing instead of instance comparing was chosen consciously.
        /// </summary>
        /// <param name="dataObject">Driver or vehicle.</param>
        /// <param name="route">Route which property will be check.</param>
        /// <returns>True if names are equal, false otherwise.</returns>
        private bool _AreNamesEqual(DataObject dataObject, Route route)
        {
            // Get the name of route's property with corresponding type.
            if (dataObject is Driver && route.Driver != null)
                return dataObject.Name == route.Driver.Name;
            else if (dataObject is Vehicle && route.Vehicle != null)
                return dataObject.Name == route.Vehicle.Name;
            
            // If route driver/vehicle is null - then return false.
            return false;
        }

        /// <summary>
        /// Checks are time windows intersects or not.
        /// </summary>
        /// <param name="route1">First route to check.</param>
        /// <param name="route2">Second route to check.</param>
        /// <returns>'True' if time windows intersects.</returns>
        private bool _AreTimeWindowsIntersect(Route route1, Route route2)
        {
            // Check that routes are wideopen.
            if (route1.StartTimeWindow.IsWideOpen ||
                (0 == route1.MaxTotalDuration) ||
                route2.StartTimeWindow.IsWideOpen ||
                (0 == route2.MaxTotalDuration))
                return true;
            // Check for time window intersection.
            else
            {
                TimeRange totalTimeRange1 = _CalculateTotalTimeWindow(route1);
                TimeRange totalTimeRange2 = _CalculateTotalTimeWindow(route2);
                return totalTimeRange1.Intersects(totalTimeRange2);
            }
        }

        /// <summary>
        /// Calculates full route time window.
        /// </summary>
        /// <param name="route">Source route to calcutaion.</param>
        private TimeRange _CalculateTotalTimeWindow(Route route)
        {
            TimeSpan From = route.StartTimeWindow.From;
            TimeSpan To = route.StartTimeWindow.To + TimeSpan.FromMinutes(route.MaxTotalDuration);
            TimeRange totalTimeRange = new TimeRange (From, To);
            
            return totalTimeRange;
        }

        #endregion
    }
}

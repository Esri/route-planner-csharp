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
using ESRI.ArcLogistics.App.Commands.Utility;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Tracking;
using ESRI.ArcLogistics.Utility;
using System.ComponentModel;
using System.Collections.Specialized;

namespace ESRI.ArcLogistics.App.Commands.TrackingCommandHelpers
{
    /// <summary>
    /// Provides facilities for sending routes to the tracking server.
    /// </summary>
    internal sealed class SendRoutesTask :
        RoutesManagementTaskBase,
        ISendRoutesTask
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the SendRoutesTask class.
        /// </summary>
        /// <param name="workflowManagementStateTracker">Reference to the
        /// workflow management connection state tracking service.</param>
        /// <param name="solverStateTracker">Reference to the VRP solver state
        /// tracking service.</param>
        /// <param name="optimizeAndEditPage">Reference to the "Optimize And Edit"
        /// page object.</param>
        /// <param name="dateTimeProvider">The reference to the date time provider
        /// object.</param>
        /// <param name="routesSender">The reference to the routes sender object.</param>
        public SendRoutesTask(
            IStateTrackingService workflowManagementStateTracker,
            IStateTrackingService solverStateTracker,
            IOptimizeAndEditPage optimizeAndEditPage,
            ICurrentDateProvider dateTimeProvider,
            IRoutesSender routesSender)
            : base(workflowManagementStateTracker, solverStateTracker, optimizeAndEditPage)
        {
            Debug.Assert(dateTimeProvider != null);
            Debug.Assert(routesSender != null);

            _dateTimeProvider = dateTimeProvider;
            _routesSender = routesSender;
        }
        #endregion

        #region ISendRoutesTask Members

        /// <summary>
        /// Retrieves a collection of routes which will be send with the <see cref="Execute"/>
        /// method.
        /// </summary>
        /// <returns>A collection of routes to be sent with the <see cref="Execute"/>
        /// method.</returns>
        public IEnumerable<Route> QueryRoutesToBeSent()
        {
            if (_schedule == null)
            {
                return Enumerable.Empty<Route>();
            }

            var notSentRoutes = _GetRoutesForSending(_schedule.Routes).ToList();

            return notSentRoutes;
        }

        /// <summary>
        /// Sends routes to the tracking service.
        /// </summary>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">Failed
        /// to authenticate within tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public void Execute()
        {
            if (!this.IsEnabled)
            {
                throw new InvalidOperationException();
            }

            try
            {
                var deploymentDate = (DateTime)GetDeploymentDate();
                var notSentRoutes = this.QueryRoutesToBeSent();

                _routesSender.Send(notSentRoutes, deploymentDate);
            }
            finally
            {
                this.UpdatedEnabledState();
            }
        }

        /// <summary>
        /// Get date for which routes would be sended.
        /// </summary>
        /// <returns>Date for which routes would be sended.</returns>
        public DateTime? GetDeploymentDate()
        {
            return (DateTime)_schedule.PlannedDate;
        }

        #endregion

        #region RoutesManagementTaskBase Members
        
        /// <summary>
        /// Checks if the task should be in enabled state.
        /// </summary>
        /// <returns>True if and only if the task should be in enabled state.</returns>
        protected override bool CheckIsEnabled()
        {
            // Check that we have current schedule and base class is in enabled state.
            return _schedule != null && base.CheckIsEnabled() && 
                // Check that we have at least one route which could be sent.
                _schedule.Routes.Any(route => _RouteShouldBeSent(route));
        }

        /// <summary>
        /// Handles current schedule changes.
        /// </summary>
        /// <param name="newSchedule">Reference to the new current schedule.</param>
        protected override void OnCurrentScheduleChanged(Schedule newSchedule)
        {
            if (_routeEventsAggreagator != null)
            {
                _routeEventsAggreagator.Dispose();
            }

            // Subscribe to routes property changes.
            _routeEventsAggreagator = new ScheduleRoutesEventAggregator(newSchedule);
            _routeEventsAggreagator.RoutePropertyChanged += _RoutePropertyChanged;
            _routeEventsAggreagator.RoutesCollectionChanged += _RoutesCollectionChanged;

            _schedule = newSchedule;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Handles route property values changes.
        /// </summary>
        /// <param name="sender">Event sender object.</param>
        /// <param name="e">Event arguments object.</param>
        private void _RoutePropertyChanged(
            object sender,
            PropertyChangedEventArgs e)
        {
            this.UpdatedEnabledState();
        }

        /// <summary>
        /// Handles current schedule routes collection changes.
        /// </summary>
        /// <param name="sender">Event sender object.</param>
        /// <param name="e">Event arguments instance.</param>
        private void _RoutesCollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            this.UpdatedEnabledState();
        }

        /// <summary>
        /// Checks if the specified route should be sent to the tracking service. Returns false
        /// for already sent routes.
        /// </summary>
        /// <param name="route">The route object to be checked.</param>
        /// <returns>True if and only if the route should be sent.</returns>
        private bool _RouteShouldBeSent(Route route)
        {
            Debug.Assert(route != null);

            var device = TrackingHelper.GetDeviceByRoute(route);

            return
                device != null && device.SyncType == SyncType.WMServer;
        }

        /// <summary>
        /// Gets collection of not sent routes.
        /// </summary>
        /// <param name="routes">Collection of routes to search for not sent ones.</param>
        /// <returns>Collection of not sent routes.</returns>
        private IEnumerable<Route> _GetRoutesForSending(IEnumerable<Route> routes)
        {
            Debug.Assert(routes != null);

            var sentRoutes = routes
                .Where(route =>
                    _RouteShouldBeSent(route));

            return sentRoutes;
        }

        #endregion

        #region private fields
        /// <summary>
        /// Provides current date/time.
        /// </summary>
        private ICurrentDateProvider _dateTimeProvider;

        /// <summary>
        /// Sends routes to the workflow management server.
        /// </summary>
        private IRoutesSender _routesSender;

        /// <summary>
        /// Current schedule to send routes for.
        /// </summary>
        private Schedule _schedule;

        /// <summary>
        /// Property changed events aggregator for routes of the current schedule.
        /// </summary>
        private ScheduleRoutesEventAggregator _routeEventsAggreagator;

        #endregion
    }
}

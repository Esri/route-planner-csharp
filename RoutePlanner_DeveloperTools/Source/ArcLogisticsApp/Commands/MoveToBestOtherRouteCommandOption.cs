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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Class implements logic for "MoveToBestRoute" option.
    /// </summary>
    internal class MoveToBestOtherRouteCommandOption : MoveToCommandOptionBase
    {
        #region Constructors

        /// <summary>
        /// Crates new MoveToBestOtherRouteCommandOption. Initializes all main class fields.
        /// </summary>
        /// <param name="groupId">Option group ID (to set in separate group in UI).</param>
        public MoveToBestOtherRouteCommandOption(int groupId)
        {
            GroupID = groupId;
            EnabledTooltip = (string)App.Current.FindResource(ENABLED_TOOLTIP_RESOURCE);
            DisabledTooltip = (string)App.Current.FindResource(DISABLED_TOOLTIP_RESOURCE);
        }

        #endregion

        #region CommandBase Members

        /// <summary>
        /// Gets command option title.
        /// </summary>
        public override string Title
        {
            get { return (string)App.Current.FindResource(BEST_OTHER_ROUTE); }
        }

        /// <summary>
        /// Gets/sets tooltip.
        /// </summary>
        public override string TooltipText
        {
            get;
            protected set;
        }

        #endregion

        #region RoutingCommandOptionBase Members

        /// <summary>
        /// Define whether option is enabled.
        /// </summary>
        protected override void _CheckEnabled(OptimizeAndEditPage schedulePage)
        {
            Debug.Assert(schedulePage != null);

            // Option is enabled if all selected stops belong to the same route.
            if (!_AreAllSelectedOrdersAssignedToOneRoute(schedulePage.SelectedItems))
            {
                IsEnabled = false;
                return;
            }

            var sourceRoute = _GetSourceRoute(schedulePage.SelectedItems);

            var result =
                schedulePage.CurrentSchedule != null &&
                schedulePage.CurrentSchedule.Routes.Any(
                    route => !route.IsLocked && route != sourceRoute);

            IsEnabled = result;
        }

        /// <summary>
        /// Starts option process.
        /// </summary>
        /// <param name="args">Operation args. Empty there.</param>
        protected override void _Execute(params object[] args)
        {
            OptimizeAndEditPage schedulePage = (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);
            Schedule schedule = schedulePage.CurrentSchedule;
            ICollection<Route> targetRoutes = _GetOtherRoutes(schedule, schedulePage.SelectedItems);
            base._Execute(targetRoutes, (string)App.Current.FindResource(BEST_OTHER_ROUTE));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method gets routes that are present for the day, except the route that orders are currently assigned to
        /// </summary>
        /// <param name="selection">Selected items.</param>
        /// <returns>All selected routes except current stops parent.</returns>
        private ICollection<Route> _GetOtherRoutes(Schedule schedule, IList selection)
        {
            Collection<Route> otherRoutes = new Collection<Route>();
            Collection<Route> usedRoutes = new Collection<Route>();

            foreach (Object obj in selection)
            {
                if (obj is Stop && !usedRoutes.Contains(((Stop)obj).Route))
                    usedRoutes.Add(((Stop)obj).Route);
            }

            foreach (Route route in schedule.Routes)
            {
                if (!usedRoutes.Contains(route))
                    otherRoutes.Add(route);
            }

            return otherRoutes;
        }

        /// <summary>
        /// Checks whether al selected object assigned to same route.
        /// </summary>
        /// <param name="selection">Selected objects.</param>
        /// <returns>Bool value.</returns>
        private bool _AreAllSelectedOrdersAssignedToOneRoute(IList selection)
        {
            bool result = false;
            Route parentRoute = null;

            foreach (Object obj in selection)
            {
                if (obj is Stop)
                {
                    // If object is stop and remembered route is empty - remember new parent route.
                    if (((Stop)obj).Route != parentRoute && parentRoute == null)
                    {
                        parentRoute = ((Stop)obj).Route;
                        result = true;
                    }
                    else if (((Stop)obj).Route != parentRoute && parentRoute != null)
                    {
                        // If stops route is different from remembered - return false.
                        result = false;
                        break;
                    }
                }
                else if (obj is Order) // If object in selection not assigned to any route - return false.
                {
                    result = false;
                    break;
                }
                else if (obj is Route && selection.Count == 1) // If selection contains 1 object and this is Route - return true.
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets reference to the route object containing all specified selected
        /// items.
        /// </summary>
        /// <param name="selectedItems">The collection of selected items to
        /// get route for.</param>
        /// <returns>A reference to the route containing all selected items.</returns>
        private Route _GetSourceRoute(IList selectedItems)
        {
            Debug.Assert(selectedItems != null);

            foreach (var item in selectedItems)
            {
                var stop = item as Stop;
                if (stop != null)
                {
                    return stop.Route;
                }

                var route = item as Route;
                if (route != null)
                {
                    return route;
                }
            }

            return null;
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// BestOtherRoute string.
        /// </summary>
        private const string BEST_OTHER_ROUTE = "BestOtherRouteOption";

        /// <summary>
        /// Enabled tooltip resource.
        /// </summary>
        private const string ENABLED_TOOLTIP_RESOURCE = "AssignToOtherRouteCommandEnabledTooltip";

        /// <summary>
        /// Disabled tooltip resource.
        /// </summary>
        private const string DISABLED_TOOLTIP_RESOURCE = "AssignToOtherRouteCommandDisabledTooltip";

        #endregion
    }
}

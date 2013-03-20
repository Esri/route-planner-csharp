using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Dialogs
{
    /// <summary>
    /// Class, which control default routes update.
    /// </summary>
    internal class DefaultRoutesController
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public DefaultRoutesController(IDataObjectCollection<Route> routes)
        {
            // Check input parameter.
            Debug.Assert(routes != null);

            // Save routes as old default collection.
            _oldDefaultRoutes = new List<Route>();
            _oldDefaultRouteLinks = new List<RouteLinks>();
            foreach (Route route in routes)
            {
                var tempRoute = route.CloneNoResults() as Route;
                _oldDefaultRoutes.Add(tempRoute);

                // Workaround: when application will call Project.Save method next time 
                // all reference properties of tempRoute which points on database object
                // will be set to null, so we need to remeber this links. Do it by using
                // special structure with corresponding fields.
                RouteLinks tempRouteLinks = new RouteLinks();
                tempRouteLinks.StartLocation = route.StartLocation;
                tempRouteLinks.EndLocation = route.EndLocation;
                tempRouteLinks.Vehicle = route.Vehicle;
                tempRouteLinks.Driver = route.Driver;
                tempRouteLinks.Zones = route.Zones;
                tempRouteLinks.RenewalLocations = route.RenewalLocations;
                tempRouteLinks.savedRouteID = tempRoute.Id;
                _oldDefaultRouteLinks.Add(tempRouteLinks);
            }

        }

        #endregion

        #region Public Method

        /// <summary>
        /// Method, comparing current default routes and "_oldDefaultRoutes" collection.
        /// If they differs then it ask/dont ask user about updating scheduled routes, 
        /// </summary>
        public void CheckDefaultRoutesForUpdates()
        {
            // If default routes have changed, new values are valid and we havent checked
            // oldRoutes collections for updates before - then check settings.
            if(_DefaultRoutesAreValid() && _RoutesWasUpdated() && _WasntChecked)
            {
                // If corresponding property is true - ask user about updating.
                if (Settings.Default.IsAlwaysAskAboutApplyingDefaultRoutesToSheduledRoutes)
                {
                    // If he pressed "Yes" button - update scheduled routes.
                    if (_ShowDialog())
                        _ApplyDefaultRoutesToSheduledRoutes();
                }
                // If we dont have to ask user and must update routes - update them.
                else if (Settings.Default.ApplyDefaultRoutesToSheduledRoutes)
                    _ApplyDefaultRoutesToSheduledRoutes();

                // Remeber that we have checked this old Routes collection.
                _WasntChecked = false;
            }
        }

        #endregion

        #region Private Structure

        /// <summary>
        /// Structure for saving links to database objects on which points Default route's properties.
        /// </summary>
        private struct RouteLinks
        {
            public Location StartLocation;
            public Location EndLocation;
            public Vehicle Vehicle;
            public Driver Driver;
            public IDataObjectCollection<Zone> Zones;
            public IDataObjectCollection<Location> RenewalLocations;
            public Guid savedRouteID;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check that default routes are valid.
        /// </summary>
        /// <returns>'True' if routes are valid, 'false' otherwise.</returns>
        private bool _DefaultRoutesAreValid()
        {
            // If we find error message for any route then default routes are not valid.
            foreach (var route in App.Current.Project.DefaultRoutes)
                if (!string.IsNullOrEmpty(route.Error))
                    return false;

            // All routes are valid.
            return true;
        }

        /// <summary>
        /// Check that scheduled routes must be updated. Such situation occures when some route
        /// in default routes was updated. Deleted/added routes doesnt matter.
        /// </summary>
        /// <returns>'True' if collection needs to be updated, 'false' otherwise.</returns>
        private bool _RoutesWasUpdated()
        {
            // For each previously saved route.
            foreach (var route in _oldDefaultRoutes)
            {
                // Find corresponding new route.
                var newRoute = App.Current.Project.DefaultRoutes.FirstOrDefault(x => x.Id == route.DefaultRouteID);

                // If corresponding new route, differs from old
                // route - collection needs to be updated.
                if (newRoute != null && 
                    (!route.EqualsByValue(newRoute) || !RouteLinksAreEqual(route, newRoute)))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check that routes links are equal.
        /// </summary>
        /// <param name="oldRoute">Previously saved route.</param>
        /// <param name="newRoute">Corresponding route, which links will be checked on equality.</param>
        /// <returns>'True' if routes links are equal, 'false' otherwise.</returns>
        private bool RouteLinksAreEqual(Route oldRoute, Route newRoute)
        {
            // Find RouteLink, corresponding to old route.
            var oldRouteLink = _oldDefaultRouteLinks.Find(x => x.savedRouteID == oldRoute.Id);

            // It must exist.
            Debug.Assert(_oldDefaultRouteLinks != null);

            // Compare fields of structure and new route.
            return oldRouteLink.Vehicle == newRoute.Vehicle &&
                oldRouteLink.StartLocation == newRoute.StartLocation &&
                oldRouteLink.EndLocation == newRoute.EndLocation &&
                oldRouteLink.Driver == newRoute.Driver &&
                DataObjectCollectionsComparer<Zone>.AreEqual(oldRouteLink.Zones, newRoute.Zones) &&
                DataObjectCollectionsComparer<Location>.AreEqual
                    (oldRouteLink.RenewalLocations, newRoute.RenewalLocations);
        }

        /// <summary>
        /// Show dialog, where user can choose update routes or not.
        /// </summary>
        /// <returns>"True" if user decided to update routes, "false" otherwise.</returns>
        private bool _ShowDialog()
        {
            // Get strings from resources.
            string question = (string)App.Current.FindResource("DefaultRoutesApplyingDialogText");
            string checboxLabel = (string)App.Current.FindResource("BreaksApplyingDialogCheckbox");
            string title = (string)App.Current.FindResource("DefaultRoutesApplyingDialogTitle");

            // Flag - show this dialog in future or not.
            bool dontAsk = true;

            // Show dialog and convert result to bool.
            MessageBoxExButtonType pressedButton = MessageBoxEx.Show(App.Current.MainWindow, question,
                title, System.Windows.Forms.MessageBoxButtons.YesNo, MessageBoxImage.Question,
                checboxLabel, ref dontAsk);
            bool result = (pressedButton == MessageBoxExButtonType.Yes);

            // If user checked checbox - we dont need to show this dialog in future, so
            // update corresponding settings.
            if (dontAsk)
            {
                Properties.Settings.Default.
                    IsAlwaysAskAboutApplyingDefaultRoutesToSheduledRoutes = false;
                Settings.Default.ApplyDefaultRoutesToSheduledRoutes = result;
                Properties.Settings.Default.Save();
            }

            // Return user choice.
            return result;
        }

        /// <summary>
        /// Apply default routes to all scheduled routes on current 
        /// and future dates, which have no assigned orders.
        /// </summary>
        private void _ApplyDefaultRoutesToSheduledRoutes()
        {
            List<DateTime> _daysWithRoutesWithOrders = new List<DateTime>();
            List<Route> routesWhichMustBeUpdated = new List<Route>();

            // Search for current schedules for all days starting from today.
            var allSchedules = (App.Current.
                Project.Schedules.SearchRange(DateTime.Now, null, ScheduleType.Current));

            // Check each schedule.
            foreach (Schedule schedule in allSchedules)
            {
                var listOfRoutes = new List<Route>(schedule.Routes);

                // If at least one of this schedule's routes has orders
                // then cannot update all routes from this schedule.
                if (listOfRoutes.Find(route => route.OrderCount > 0) != null)
                    _daysWithRoutesWithOrders.Add((DateTime)schedule.PlannedDate);
                else
                    // Add routes, which has corresponding default route to update list.
                    foreach (var route in schedule.Routes)
                        if (route.DefaultRouteID != null)
                            routesWhichMustBeUpdated.Add(route);
            }

            // For each route in default routes update corresponding scheduled routes.
            foreach (Route route in App.Current.Project.DefaultRoutes)
            {
                var result = routesWhichMustBeUpdated.FindAll(x => x.DefaultRouteID == route.Id);
                foreach(var oldRoute in result)
                    route.CopyTo(oldRoute);
            }

            // Show message with updating result in message window.
            _ShowResultMessage(_daysWithRoutesWithOrders);

            // Save current project.
            App.Current.Project.Save();
        }

        /// <summary>
        /// Show dialog with result of scheduled routes update.
        /// </summary>
        /// <param name="daysWithRoutesWithOrders">List with dates on which routes wasnt updated.</param>
        private void _ShowResultMessage(List<DateTime> daysWithRoutesWithOrders)
        {
            // If ther is no "bad" days - show successfull message.
            if (daysWithRoutesWithOrders.Count == 0)
                App.Current.Messenger.AddMessage(MessageType.Information,
                    (string)App.Current.FindResource("RoutesUpdatedSuccessfullMessage"));
            // Otherwise - show message with a short list of dates.
            else
            {
                // Construct message string.
                StringBuilder message = new StringBuilder();
                message.Append((string)App.Current.FindResource("NotAllRoutesWasUpdatedMessage"));
                
                // Append several first dates of days, which routes wasnt updated.
                daysWithRoutesWithOrders.Sort();
                int quantity = Math.Min(daysWithRoutesWithOrders.Count, QUANTITY);
                for (int i = 0; i < quantity; i++)
                    message.Append(daysWithRoutesWithOrders[i].ToString(DATE_FORMAT) + DATES_SPLITTER);
                
                // If we have shown all "bad" days - replace last comma with point.
                if (daysWithRoutesWithOrders.Count == quantity)
                    message.Replace(DATES_SPLITTER, END_OF_MESSAGE, message.Length - 2, 2);
                // If there are more such days - append "etc" to the end of the message.
                else
                    message.Replace(DATES_SPLITTER, ET_CETERA, message.Length - 2, 2);

                // Show message in message window.
                App.Current.Messenger.AddMessage(MessageType.Warning, message.ToString());
            }
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Format for DateTime conversion.
        /// </summary>
        private const string DATE_FORMAT = "m";

        /// <summary>
        /// Splitter for dates.
        /// </summary>
        private const string DATES_SPLITTER = ", ";

        /// <summary>
        /// Dot for the end of message.
        /// </summary>
        private const string END_OF_MESSAGE = ".";

        /// <summary>
        /// Dot for the end of message.
        /// </summary>
        private const string ET_CETERA = ", etc.";

        /// <summary>
        /// Number of unupdated dates to show.
        /// </summary>
        private const int QUANTITY = 3;

        #endregion

        #region Private Fields

        /// <summary>
        /// Collection with routes, which needs to be compared to current default
        /// routes collection. 
        /// </summary>
        private List<Route> _oldDefaultRoutes;

        /// <summary>
        /// Collection with structures, which will be used for comparing.
        /// </summary>
        private List<RouteLinks> _oldDefaultRouteLinks;

        /// <summary>
        /// Flag, shows that current oldDefaultRoute collection wasnt checked.
        /// </summary>
        private bool _WasntChecked = true;

        #endregion
    }
}


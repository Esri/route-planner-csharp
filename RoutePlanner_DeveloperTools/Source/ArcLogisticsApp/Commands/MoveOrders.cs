using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Pages;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ESRI.ArcLogistics.Routing;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Commands
{
    internal class MoveOrders : CommandBase, ISupportOptions
    {
        #region CommandBase Members

        /// <summary>
        /// Gets command name.
        /// </summary>
        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        /// <summary>
        /// Gets command title.
        /// </summary>
        public override string Title
        {
            get { return (string)App.Current.FindResource(COMMAND_TITLE); }
        }

        /// <summary>
        /// Gets/sets tooltip text.
        /// </summary>
        public override string TooltipText
        {
            get { return null; }
            protected set { }
        }

        /// <summary>
        /// Gets/sets IsEnabled property.
        /// </summary>
        public override bool IsEnabled
        {
            get { return _isEnabled; }
            protected set
            {
                _isEnabled = value;
                _NotifyPropertyChanged(IS_ENABLED_PROPERTY_NAME);
            }
        }

        /// <summary>
        /// Initializes command.
        /// </summary>
        /// <param name="app">Current application.</param>
        public override void Initialize(App app)
        {
            base.Initialize(app);
            app.ApplicationInitialized += new EventHandler(_ApplicationInitialized);
        }

        #endregion

        #region ISupportOptions Members

        public ICommandOption[] Options
        {
            get;
            private set;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Starts command process.
        /// </summary>
        /// <param name="args"></param>
        protected override void _Execute(params object[] args)
        {
            // Define clicked option and call it's Execute method.
            RoutingCommandBase option = (RoutingCommandBase)args[0];
            Debug.Assert(option != null);

            option.Execute();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method creates options.
        /// </summary>
        private void _CreateOptions(Schedule currentSchedule)
        {
            if (currentSchedule.Routes != null)
            {
                // Used to show horisontal lines in menu.
                int groupID = 0;

                List<ICommandOption> options = new List<ICommandOption>();

                // Add MoveToUnassigned orders option.
                options.Add(new MoveToUnassignedOrdersOption(groupID));

                // Start next group.
                groupID++;

                // Add MoveToBestRoute option.
                options.Add(new MoveToBestRouteCommandOption(groupID));

                // Add MoveToBestOtherRoute option.
                options.Add(new MoveToBestOtherRouteCommandOption(groupID));

                // Start next group.
                groupID++;

                // Add MoveToRoute option for each route.
                SortedDataObjectCollection<Route> sortedRoutes = new SortedDataObjectCollection<Route>(currentSchedule.Routes, new RoutesComparer());

                foreach (Route route in sortedRoutes)
                    options.Add(new MoveToRouteCommandOption(groupID, route));

                foreach (CommandBase option in options)
                    option.Initialize(App.Current);

                Options = options.ToArray();

                // Notify about option collection changed.
                _NotifyPropertyChanged(OPTIONS_PROPERTY_NAME);
            }
        }

        /// <summary>
        /// Define whether command is enabled.
        /// </summary>
        private void _CheckEnabled()
        {
            Debug.Assert(_schedulePage != null);

            bool isEnabled = false;

            // Check IsEnabled when selection contains routes.
            if (_DoesSelectionContainsRoutes(_schedulePage.SelectedItems))
            {
                isEnabled = ((_schedulePage.CurrentSchedule != null && _schedulePage.CurrentSchedule.Routes.Count > 0) &&
                   !_HasLockedStopsOrRoutes(_schedulePage.SelectedItems) &&
                   _GetUnlockedOrdersFromSelectedRoutes(_schedulePage.SelectedItems).Count > 0);
            }
            else // Check IsEnabled when selection contains orders/stops.
            {
                isEnabled = ((_schedulePage.CurrentSchedule != null && _schedulePage.CurrentSchedule.Routes.Count > 0) &&
                    (_GetOrdersFromSelection(_schedulePage.SelectedItems).Count > 0 &&
                    !_HasLockedStopsOrRoutes(_schedulePage.SelectedItems)));
            }

            if (_schedulePage.IsLocked || _schedulePage.IsEditingInProgress)
                isEnabled = false;

            IsEnabled = isEnabled;
        }

        /// <summary>
        /// Method gets orders collection from selection collection.
        /// </summary>
        /// <param name="selection">Selected items.</param>
        /// <returns>Selected orders.</returns>
        private Collection<Order> _GetOrdersFromSelection(IList selection)
        {
            Collection<Order> orders = new Collection<Order>();
            foreach (Object obj in selection)
            {
                if (obj is Order)
                    orders.Add((Order)obj);
                else if (obj is Stop && ((Stop)obj).StopType == StopType.Order)
                    orders.Add((Order)((Stop)obj).AssociatedObject);
            }

            return orders;
        }

        /// <summary>
        /// Method gets collection of unlocked orders from selected routes.
        /// </summary>
        /// <param name="selection">Selected items.</param>
        /// <returns>Selected orders.</returns>
        private Collection<Order> _GetUnlockedOrdersFromSelectedRoutes(IList selection)
        {
            Collection<Order> orders = new Collection<Order>();
            foreach (object obj in selection)
            {
                if (obj is Route)
                    foreach (Stop stop in ((Route)obj).Stops)
                    {
                        if (!stop.IsLocked && stop.AssociatedObject is Order)
                            orders.Add((Order)stop.AssociatedObject);
                    }
            }

            return orders;
        }

        /// <summary>
        /// Checks whether collection of selected items contains routes.
        /// </summary>
        /// <param name="selectedItems">Selected items.</param>
        /// <returns>Bool value.</returns>
        private bool _DoesSelectionContainsRoutes(IList selectedItems)
        {
            bool doesSelectionCintainsRoutes = false;

            foreach (object obj in selectedItems)
            {
                if (obj is Route)
                {
                    doesSelectionCintainsRoutes = true;
                    break;
                }
            }

            return doesSelectionCintainsRoutes;
        }

        /// <summary>
        /// Method checks whether selection has locked stops or routes.
        /// </summary>
        /// <param name="selection">Selected items.</param>
        /// <returns>True is selection contains locked stops or routes and false otherwise.</returns>
        private bool _HasLockedStopsOrRoutes(IList selection)
        {
            bool hasLockedStops = false;
            foreach (Object obj in selection)
            {
                if ((obj is Stop && ((Stop)obj).IsLocked) || 
                    (obj is Stop && ((Stop)obj).Route != null && ((Stop)obj).Route.IsLocked) || 
                    (obj is Route && ((Route)obj).IsLocked))
                {
                    hasLockedStops = true;
                    break;
                }
            }
            return hasLockedStops;
        }

        #endregion

        #region Private Event handlers

        /// <summary>
        /// Creates all necessary event handlers when application is initialized.
        /// </summary>
        /// <param name="sender">Application.</param>
        /// <param name="e">Event args.</param>
        private void _ApplicationInitialized(object sender, EventArgs e)
        {
            // Add handlers to all events to enable/disable command and update collection of options when necessary.
            App.Current.ProjectLoaded += new EventHandler(_ProjectLoaded);
            App.Current.ProjectClosing += new EventHandler(_ProjectClosing);

            _schedulePage = (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);
            Debug.Assert(_schedulePage != null);

            if (null != _schedulePage.CurrentSchedule)
                _CreateOptions(_schedulePage.CurrentSchedule);

            _schedulePage.SelectionChanged += new EventHandler(_SchedulePageSelectionChanged);
            _schedulePage.CurrentScheduleChanged += new EventHandler(_CurrentScheduleChanged);
            _schedulePage.EditBegun += new DataObjectEventHandler(_SchedulePageEditBegun);
            _schedulePage.EditCanceled += new DataObjectEventHandler(_SchedulePageEditCanceled);
            _schedulePage.EditCommitted += new DataObjectEventHandler(_SchedulePageEditCommitted);

            App.Current.Solver.AsyncSolveCompleted += new AsyncSolveCompletedEventHandler(_AsyncSolveCompleted);
        }

        /// <summary>
        /// Updates options collection and check whether command enbled when schedule changed.
        /// </summary>
        /// <param name="sender">OptimizeAndEdit page.</param>
        /// <param name="e">Event args.</param>
        private void _CurrentScheduleChanged(object sender, EventArgs e)
        {
            Debug.Assert(_schedulePage != null);
            ((INotifyCollectionChanged)_schedulePage.CurrentSchedule.Routes).CollectionChanged -= _RoutesCollectionChanged;

            _CreateOptions(_schedulePage.CurrentSchedule);
            _CheckEnabled();

            ((INotifyCollectionChanged)_schedulePage.CurrentSchedule.Routes).CollectionChanged += new NotifyCollectionChangedEventHandler(_RoutesCollectionChanged);
        }

        /// <summary>
        /// Removes handler to SaveChangesCompleted event.
        /// </summary>
        /// <param name="sender">Current project.</param>
        /// <param name="e">Event args.</param>
        private void _ProjectLoaded(object sender, EventArgs e)
        {
            App.Current.Project.SaveChangesCompleted += new SaveChangesCompletedEventHandler(_SaveChangesCompleted);
        }

        /// <summary>
        /// Removes handler to SaveChangesCompleted event.
        /// </summary>
        /// <param name="sender">Current project.</param>
        /// <param name="e">Event args.</param>
        private void _ProjectClosing(object sender, EventArgs e)
        {
            App.Current.Project.SaveChangesCompleted -= _SaveChangesCompleted;
        }

        /// <summary>
        /// Checks whether command is enabled when solve operation cancelled.
        /// </summary>
        /// <param name="sender">Solver.</param>
        /// <param name="e">Event args.</param>
        private void _AsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            if (e.Cancelled)
                _CheckEnabled();
        }

        /// <summary>
        /// Checks whether command is enabled when editing started.
        /// </summary>
        /// <param name="sender">OptimizeAndEdit page.</param>
        /// <param name="e">Event args.</param>
        private void _SchedulePageEditBegun(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        /// <summary>
        /// Checks whether command is enabled and updates collection of options when editing saved.
        /// </summary>
        /// <param name="sender">OptimizeAndEdit page.</param>
        /// <param name="e">Event args.</param>
        private void _SchedulePageEditCommitted(object sender, DataObjectEventArgs e)
        {
            if (null != _schedulePage.CurrentSchedule)
                _CreateOptions(_schedulePage.CurrentSchedule);

            _CheckEnabled();
        }

        /// <summary>
        /// Checks whether command is enabled when editing cancelled.
        /// </summary>
        /// <param name="sender">OptimizeAndEdit page.</param>
        /// <param name="e">Event args.</param>
        private void _SchedulePageEditCanceled(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        /// <summary>
        /// Checks whether command is enabled when editing saved.
        /// </summary>
        /// <param name="sender">OptimizeAndEdit page.</param>
        /// <param name="e">Event args.</param>
        private void _SchedulePageSelectionChanged(object sender, EventArgs e)
        {
            Debug.Assert(_schedulePage != null); // Must be initialized before.

            if (_schedulePage.CurrentSchedule != null)
                _CheckEnabled();
        }

        /// <summary>
        /// Updates options collection when routes collection changed.
        /// </summary>
        /// <param name="sender">Routes collection.</param>
        /// <param name="e">Event args.</param>
        private void _RoutesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(_schedulePage != null);
            _CreateOptions(_schedulePage.CurrentSchedule);
        }

        /// <summary>
        /// Checks whether command is enabled when editing saved.
        /// </summary>
        /// <param name="sender">OptimizeAndEdit page.</param>
        /// <param name="e">Event args.</param>
        private void _SaveChangesCompleted(object sender, SaveChangesCompletedEventArgs e)
        {
            _CheckEnabled();
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Command name.
        /// </summary>
        private const string COMMAND_NAME = "ArcLogistics.Commands.MoveOrders";

        /// <summary>
        /// Command title.
        /// </summary>
        private const string COMMAND_TITLE = "MoveOrdersCommandTitle";

        /// <summary>
        /// Options string.
        /// </summary>
        private const string OPTIONS_PROPERTY_NAME = "Options";

        /// <summary>
        /// IsEnabled string.
        /// </summary>
        private const string IS_ENABLED_PROPERTY_NAME = "IsEnabled";

        #endregion

        #region Private Fields

        /// <summary>
        /// Schedule page.
        /// </summary>
        private OptimizeAndEditPage _schedulePage = null;

        /// <summary>
        /// IsEnabled value.
        /// </summary>
        private bool _isEnabled = false;

        #endregion
    }
}

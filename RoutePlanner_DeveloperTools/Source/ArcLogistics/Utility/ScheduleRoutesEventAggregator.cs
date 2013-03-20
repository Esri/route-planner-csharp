using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Simplifies observing <see cref="P:ESRI.ArcLogistics.DomainObjects.Route.PropertyChanged"/>
    /// event for a collection of routes in a <see cref="T:ESRI.ArcLogistics.DomainObjects.Schedule"/>
    /// object.
    /// </summary>
    internal sealed class ScheduleRoutesEventAggregator : IDisposable
    {
        #region constructos
        /// <summary>
        /// Initializes a new instance of the ScheduleRoutesEventAggregator class.
        /// </summary>
        public ScheduleRoutesEventAggregator()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ScheduleRoutesEventAggregator class.
        /// </summary>
        /// <param name="schedule">Reference to the schedule object to aggregate
        /// route events for.</param>
        public ScheduleRoutesEventAggregator(Schedule schedule)
        {
            _schedule = schedule;

            if (_schedule != null)
            {
                _schedule.Routes.CollectionChanged += _RoutesCollectionChanged;
                _SubscribeToPropertyChangedEvent(_schedule.Routes);
                _existingRoutes = _schedule.Routes.ToList();
            }
        }
        #endregion

        #region public events
        /// <summary>
        /// Fired when property of one of the schedule routes is changed.
        /// </summary>
        public event PropertyChangedEventHandler RoutePropertyChanged = delegate { };

        /// <summary>
        /// Fired when routes collection is changed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler RoutesCollectionChanged = delegate { };
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Unsubscribes from all events allowing this object to be garbadge collected.
        /// </summary>
        public void Dispose()
        {
            if (_schedule != null)
            {
                _UnsubscribeFromPropertyChangedEvent(_schedule.Routes);
                _schedule.Routes.CollectionChanged -= _RoutesCollectionChanged;
            }
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
            RoutePropertyChanged(this, e);
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
            _UpdateRoutesSubscirptions(e);
            this.RoutesCollectionChanged(this, e);
        }

        /// <summary>
        /// Updates subscriptions to routes event.
        /// </summary>
        /// <param name="e">Describes changes in the routes collection.</param>
        private void _UpdateRoutesSubscirptions(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _UnsubscribeFromPropertyChangedEvent(_existingRoutes);
                _existingRoutes = new List<Route>();

                return;
            }

            _existingRoutes = _schedule.Routes.ToList();

            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                return;
            }

            var oldRoutes = e.OldItems ?? new List<Route>();
            _UnsubscribeFromPropertyChangedEvent(oldRoutes.Cast<Route>());

            var newRoutes = e.NewItems ?? new List<Route>();
            _SubscribeToPropertyChangedEvent(newRoutes.Cast<Route>());
        }

        /// <summary>
        /// Subsribes to the property changed event for each route in the collection.
        /// </summary>
        /// <param name="routes">Collection of routes to subscribe for events.</param>
        private void _SubscribeToPropertyChangedEvent(IEnumerable<Route> routes)
        {
            foreach (var route in routes)
            {
                route.PropertyChanged += _RoutePropertyChanged;
            }
        }

        /// <summary>
        /// Unsubsribes from the property changed event for each route in the collection.
        /// </summary>
        /// <param name="routes">Collection of routes to unsubscribe for events.</param>
        private void _UnsubscribeFromPropertyChangedEvent(IEnumerable<Route> routes)
        {
            foreach (var route in routes)
            {
                route.PropertyChanged -= _RoutePropertyChanged;
            }
        }
        #endregion

        #region private fields
        /// <summary>
        /// Schedule to observe routes events for.
        /// </summary>
        private Schedule _schedule;

        /// <summary>
        /// Collection of all routes in a current schedule.
        /// </summary>
        private List<Route> _existingRoutes = new List<Route>();
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.Utility.CoreEx;

namespace ESRI.ArcLogistics.DomainObjects.Utility
{
    /// <summary>
    /// Implements <see cref="IRoutesCollectionOwner"/> interface for default routes or routes
    /// from specific schedule.
    /// </summary>
    internal sealed class RoutesCollectionOwner : IRoutesCollectionOwner
    {
        #region constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RoutesCollectionOwner"/> class.
        /// </summary>
        /// <param name="routes">A collection of routes owned by this instance.</param>
        public RoutesCollectionOwner(IDataObjectCollection<Route> routes)
        {
            CodeContract.RequiresNotNull("routes", routes);

            _routes = routes;
            _routes.CollectionChanged += _RoutesCollectionChanged;
            _TrackAssociations();
        }
        #endregion

        #region IRoutesCollectionOwner Members
        /// <summary>
        /// Finds all routes associated with the specified object.
        /// </summary>
        /// <param name="associatedObject">The object to find routes associated with.</param>
        /// <returns>A collection of routes associated with the specified object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="associatedObject"/> is
        /// a null reference.</exception>
        public IEnumerable<Route> FindRoutes(DataObject associatedObject)
        {
            CodeContract.RequiresNotNull("associatedObject", associatedObject);

            var tracker = default(RouteAssociationTracker);
            if (!_associationTrackers.TryGetValue(associatedObject.GetType(), out tracker))
            {
                return Enumerable.Empty<Route>();
            }

            return tracker.FindRoutes(associatedObject);
        }

        /// <summary>
        /// Updates association between the specified route and related object accessed by the
        /// route property with the specified name.
        /// </summary>
        /// <param name="route">The route object to update association for.</param>
        /// <param name="propertyName">The name of the route property to update association
        /// with.</param>
        /// <exception cref="ArgumentNullException"><paramref name="route"/> or
        /// <paramref name="propertyName"/> is a null reference.</exception>
        public void UpdateRouteAssociation(Route route, string propertyName)
        {
            CodeContract.RequiresNotNull("route", route);
            CodeContract.RequiresNotNull("propertyName", propertyName);

            var tracker = default(RouteAssociationTracker);
            if (!_associationTrackersByPropertyName.TryGetValue(propertyName, out tracker))
            {
                return;
            }

            tracker.UnregisterRoute(route);
            tracker.RegisterRoute(route);
        }
        #endregion

        #region private methods
        /// <summary>
        /// Handles routes collection changes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event data object.</param>
        private void _RoutesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add: // Fall through.
                case NotifyCollectionChangedAction.Replace: // Fall through.
                case NotifyCollectionChangedAction.Remove:
                    var newItems = e.NewItems == null ?
                        Enumerable.Empty<Route>() : e.NewItems.Cast<Route>();
                    foreach (var route in newItems)
                    {
                        _RegisterAssociations(route);
                    }

                    var oldItems = e.OldItems == null ?
                        Enumerable.Empty<Route>() : e.OldItems.Cast<Route>();
                    foreach (var route in oldItems)
                    {
                        _UnregisterAssociations(route);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _TrackAssociations();
                    break;

                case NotifyCollectionChangedAction.Move: // Fall through.
                default:
                    break;
            }
        }

        /// <summary>
        /// Starts association tracking for all schedule routes.
        /// </summary>
        private void _TrackAssociations()
        {
            var associationTrackers = new List<RouteAssociationTracker>
            {
                RouteAssociationTracker.Create(_ => _.Driver),
                RouteAssociationTracker.Create(_ => _.Vehicle),
            };

            _associationTrackers = associationTrackers
                .ToDictionary(tracker => tracker.AssociationType);
            _associationTrackersByPropertyName = associationTrackers
                .ToDictionary(tracker => tracker.AssociationPropertyName);

            foreach (var route in _routes)
            {
                _RegisterAssociations(route);
            }
        }

        /// <summary>
        /// Starts associations tracking for the specified route.
        /// </summary>
        /// <param name="route">The route to start associations tracking for.</param>
        private void _RegisterAssociations(Route route)
        {
            Debug.Assert(route != null);

            foreach (var item in _associationTrackers.Values)
            {
                item.RegisterRoute(route);
            }

            route.RoutesCollectionOwner = this;
        }

        /// <summary>
        /// Unregisters all associations for the specified route.
        /// </summary>
        /// <param name="route">The route to stop associations tracking for.</param>
        private void _UnregisterAssociations(Route route)
        {
            Debug.Assert(route != null);

            foreach (var item in _associationTrackers.Values)
            {
                item.UnregisterRoute(route);
            }

            route.RoutesCollectionOwner = null;
        }
        #endregion

        #region private fields
        /// <summary>
        /// A collection of association trackers for routes owned by this instance.
        /// </summary>
        private Dictionary<Type, RouteAssociationTracker> _associationTrackers =
            new Dictionary<Type, RouteAssociationTracker>();

        /// <summary>
        /// Maps route property names into association trackers for these properties.
        /// </summary>
        private Dictionary<string, RouteAssociationTracker> _associationTrackersByPropertyName =
            new Dictionary<string, RouteAssociationTracker>();
        
        /// <summary>
        /// The reference to the collection of routes owned by this instance.
        /// </summary>
        private IDataObjectCollection<Route> _routes;
        #endregion
    }
}

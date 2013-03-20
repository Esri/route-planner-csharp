using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.Utility.CoreEx;
using ESRI.ArcLogistics.Utility.Reflection;

namespace ESRI.ArcLogistics.DomainObjects.Utility
{
    /// <summary>
    /// Tracks route association with a related object like Driver or Vehicle.
    /// </summary>
    internal sealed class RouteAssociationTracker
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RouteAssociationTracker"/> class.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> of the Route property
        /// to track association with.</param>
        private RouteAssociationTracker(PropertyInfo propertyInfo)
        {
            Debug.Assert(propertyInfo != null);

            _propertyValueProvider = (Func<Route, DataObject>)Delegate.CreateDelegate(
                typeof(Func<Route, DataObject>),
                propertyInfo.GetGetMethod());

            this.AssociationType = propertyInfo.PropertyType;
            this.AssociationPropertyName = propertyInfo.Name;
        }
        #endregion

        #region public static methods
        /// <summary>
        /// Creates a new instance of the <see cref="RouteAssociationTracker"/> class.
        /// </summary>
        /// <typeparam name="TProperty">The type of the Route property to track association
        /// with.</typeparam>
        /// <param name="expression">The member expression specifying Route property to
        /// track.</param>
        /// <returns>A new instance of the <see cref="RouteAssociationTracker"/> class for the
        /// specified Route property.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is
        /// a null reference.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="expression"/> node
        /// type is not <see cref="ExpressionType.MemberAccess"/>.</exception>
        public static RouteAssociationTracker Create<TProperty>(
            Expression<Func<Route, TProperty>> expression)
        {
            CodeContract.RequiresNotNull("expression", expression);

            var propertyInfo = TypeInfoProvider<Route>.GetPropertyInfo(expression);

            return new RouteAssociationTracker(propertyInfo);
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets a type of the route related object the association is tracked with.
        /// </summary>
        public Type AssociationType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets name of the property used for accessing object related to a route.
        /// </summary>
        public string AssociationPropertyName
        {
            get;
            private set;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Finds all routes associated with the specified associated object.
        /// </summary>
        /// <param name="associatedObject">The object for which to find associated
        /// routes.</param>
        /// <returns>A collection of routes referencing the specified object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="associatedObject"/> is
        /// a null reference.</exception>
        public IEnumerable<Route> FindRoutes(DataObject associatedObject)
        {
            CodeContract.RequiresNotNull("associatedObject", associatedObject);

            var routes = default(HashSet<Route>);
            if (!_backAssociation.TryGetValue(associatedObject, out routes))
            {
                return Enumerable.Empty<Route>();
            }

            return routes;
        }

        /// <summary>
        /// Registers association for the specified route.
        /// </summary>
        /// <param name="route">The route to track association for.</param>
        public void RegisterRoute(Route route)
        {
            Debug.Assert(route != null);

            var associatedObject = _propertyValueProvider(route);

            _associations[route] = associatedObject;
            if (associatedObject == null)
            {
                return;
            }

            if (!_backAssociation.ContainsKey(associatedObject))
            {
                _backAssociation[associatedObject] = new HashSet<Route>();
            }

            _backAssociation[associatedObject].Add(route);
        }

        /// <summary>
        /// Unregisters association for the specified route.
        /// </summary>
        /// <param name="route">The route to stop association tracking for.</param>
        public void UnregisterRoute(Route route)
        {
            Debug.Assert(route != null);

            var associatedObject = default(DataObject);
            _associations.TryGetValue(route, out associatedObject);
            _associations.Remove(route);

            if (associatedObject == null)
            {
                return;
            }

            if (!_backAssociation.ContainsKey(associatedObject))
            {
                return;
            }

            var routes = _backAssociation[associatedObject];
            routes.Remove(route);
            if (routes.Count == 0)
            {
                _backAssociation.Remove(associatedObject);
            }
        }
        #endregion

        #region private fields
        /// <summary>
        /// Tracks a link from route to it's associated object.
        /// </summary>
        private Dictionary<Route, DataObject> _associations = new Dictionary<Route, DataObject>();

        /// <summary>
        /// Tracks links from a associated object to all routes referencing it.
        /// </summary>
        private Dictionary<DataObject, HashSet<Route>> _backAssociation =
            new Dictionary<DataObject, HashSet<Route>>();

        /// <summary>
        /// A function returning value of the Route property returning the associated object.
        /// </summary>
        private Func<Route, DataObject> _propertyValueProvider;
        #endregion
    }
}

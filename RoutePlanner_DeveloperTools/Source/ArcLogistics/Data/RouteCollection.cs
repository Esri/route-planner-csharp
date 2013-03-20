using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.DomainObjects.Utility;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// RouteCollection class.
    /// </summary>
    internal class RouteCollection : DataObjectOwnerCollection<Route, DataModel.Routes>
    {
        #region Constructors
        public RouteCollection(DataService<Route> ds, bool isDefault)
            : base(ds, false)
        {
            _isDefault = isDefault;
        }

        #endregion // Constructors

        #region DataObjectCollection<Route, DataModel.Routes> Members
        /// <summary>
        /// Adds data object to the collection.
        /// </summary>
        public override void Add(Route route)
        {
            route.Default = _isDefault;
            base.Add(route);
        }

        /// <summary>
        /// Handles collection initialization completion.
        /// </summary>
        protected override void OnCollectionInitialized()
        {
            base.OnCollectionInitialized();

            _routesCollectionOwner = new RoutesCollectionOwner(this);
        }
        #endregion

        #region Private fields
        /// <summary>
        /// The reference to the routes collection owner for routes in this collection.
        /// </summary>
        private IRoutesCollectionOwner _routesCollectionOwner;

        private bool _isDefault = true;
        #endregion // Private fields
    }
}

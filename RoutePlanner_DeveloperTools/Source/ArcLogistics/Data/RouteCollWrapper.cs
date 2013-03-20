using System;
using System.Data.Objects.DataClasses;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// RouteCollWrapper class.
    /// </summary>
    internal class RouteCollWrapper : EntityCollWrapper<Route, DataModel.Routes>
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of RouteCollWrapper class.
        /// </summary>
        public RouteCollWrapper(EntityCollection<DataModel.Routes> entities,
            DataObject owner, bool isReadOnly)
            : base(entities, owner, isReadOnly)
        {
        }

        /// <summary>
        /// Initializes a new instance of RouteCollWrapper class.
        /// </summary>
        /// <param name="entities">Entities collection to be wrapped.</param>
        /// <param name="owner">The owner of the entities collection.</param>
        /// <param name="isReadOnly">Indicates if the collection should be read-only.</param>
        /// <param name="collectionInitializedCallback">A callback to be called after collection
        /// initialization.</param>
        public RouteCollWrapper(
            EntityCollection<DataModel.Routes> entities,
            DataObject owner,
            bool isReadOnly,
            Action collectionInitializedCallback)
            : base(entities, owner, isReadOnly, collectionInitializedCallback)
        {
        }
        #endregion constructors

        #region protected overrides
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected override IDataObjectCollection<Route> CreateCollection(
            EntityCollection<DataModel.Routes> entities,
            DataObject owner,
            bool isReadOnly)
        {
            return new RelatedRouteCollection(entities, owner, isReadOnly);
        }
        
        #endregion protected overrides
    }
}

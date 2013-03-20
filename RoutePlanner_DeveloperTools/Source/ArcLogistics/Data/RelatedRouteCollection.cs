using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// RelatedRouteCollection class.
    /// </summary>
    internal class RelatedRouteCollection :
        RelationObjectCollection<Route, DataModel.Routes>
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of RelatedRouteCollection class.
        /// </summary>
        public RelatedRouteCollection(EntityCollection<DataModel.Routes> entities,
            DataObject owner,
            bool isReadOnly) :
            base(entities, owner, isReadOnly)
        {
        }

        #endregion constructors

        #region overrides
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds data object to the collection.
        /// </summary>
        public override void Add(Route dataObject)
        {
            base.Add(dataObject);

            Debug.Assert(base._Owner != null);
            if (base._Owner.CanSave || base._Owner.IsStored)
                dataObject.CanSave = true;
        }

        /// <summary>
        /// Removes data object from the collection.
        /// </summary>
        public override bool Remove(Route route)
        {
            bool res = base.Remove(route);
            if (res)
                _RemoveRoute(route);

            return res;
        }

        /// <summary>
        /// Clear the collection of all it's elements.
        /// </summary>
        public override void Clear()
        {
            List<Route> routes = new List<Route>(base.DataObjects);

            base.Clear();
            foreach (Route route in routes)
                _RemoveRoute(route);
        }

        #endregion overrides

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _RemoveRoute(Route route)
        {
            if (route.Stops.Count > 0)
            {
                List<Stop> stops = new List<Stop>(route.Stops);

                route.Stops.Clear();
                foreach (Stop stop in stops)
                {
                    stop.AssociatedObject = null;
                    _RemoveObject(stop);
                }
            }

            route.StartLocation = null;
            route.EndLocation = null;
            route.Driver = null;
            route.Vehicle = null;

            _RemoveObject(route);
        }

        private void _RemoveObject(DataObject obj)
        {
            DataObjectContext ctx = null;
            if (GetObjectContext(out ctx))
                ContextHelper.RemoveObject(ctx, obj);
            else
                obj.CanSave = false;
        }

        #endregion private methods
    }
}

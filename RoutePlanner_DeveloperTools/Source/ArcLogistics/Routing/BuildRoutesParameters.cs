using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;
using System.ComponentModel;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Contains parameters for build routes command.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BuildRoutesParameters
    {
        #region public properties
        /// <summary>
        /// Gets or sets collection of orders to build routes for.
        /// </summary>
        public ICollection<Order> OrdersToAssign
        {
            get
            {
                return _ordersToAssign;
            }
            set
            {
                Debug.Assert(value != null, "Orders collection should not be null");
                _ordersToAssign = value;
            }
        }

        /// <summary>
        /// Gets or sets collection of routes to be built.
        /// </summary>
        public ICollection<Route> TargetRoutes
        {
            get
            {
                return _targetRoutes;
            }
            set
            {
                Debug.Assert(value != null, "Routes collection should not be null");
                _targetRoutes = value;
            }
        }
        #endregion

        #region private fields
        /// <summary>
        /// Stores collection of orders to build routes for.
        /// </summary>
        private ICollection<Order> _ordersToAssign = new List<Order>();

        /// <summary>
        /// Stores collection of routes to be built.
        /// </summary>
        private ICollection<Route> _targetRoutes = new List<Route>();
        #endregion
    }
}

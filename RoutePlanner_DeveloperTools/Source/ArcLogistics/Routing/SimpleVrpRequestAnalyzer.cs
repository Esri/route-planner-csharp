using System.Diagnostics;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Simple implementation of the <see cref="T:ESRI.ArcLogistics.Routing.IVrpRequestAnalyzer"/>
    /// interface using number of orders in the request to choose between
    /// synchronous and asynchronous request submission.
    /// </summary>
    internal sealed class SimpleVrpRequestAnalyzer : IVrpRequestAnalyzer
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="routesCount">The maximum number of routes 
        /// which should be processed synchronously. 
        /// If number is more then 1 - default value will be used.</param>
        /// <param name="ordersCount">The maximum number of orders 
        /// which should be processed synchronously.</param>
        /// If number is more then 1 - default value will be used.</param>
        public SimpleVrpRequestAnalyzer(int routesCount, int ordersCount)
        {
            _maxRoutes = routesCount > 0 ? routesCount : DEFAULT_MAX_ROUTES_LENGTH;
            _maxOrders = ordersCount > 0 ? ordersCount : DEFAULT_MAX_ORDERS_LENGTH;
        }

        #endregion

        #region IVrpRequestAnalyzer Members
        /// <summary>
        /// Checks if the VRP request is short enough to be services with the
        /// synchronous VRP service.
        /// </summary>
        /// <param name="request">The reference to the request object to be
        /// analyzed.</param>
        /// <returns>True if the request could be executed with a synchronous VRP
        /// service.</returns>
        public bool CanExecuteSyncronously(SubmitVrpJobRequest request)
        {
            Debug.Assert(request != null);
            Debug.Assert(request.Orders != null);
            Debug.Assert(request.Orders.Features != null);
            Debug.Assert(request.Routes != null);
            Debug.Assert(request.Routes.Features != null);

            if (request.Orders.Features.Length > _maxOrders || 
                request.Routes.Features.Length > _maxRoutes)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region private constants
        /// <summary>
        /// Default maximum number of orders which should be processed synchronously.
        /// </summary>
        private const int DEFAULT_MAX_ORDERS_LENGTH = 200;

        /// <summary>
        /// Default maximum number of routes which should be processed synchronously.
        /// </summary>
        private const int DEFAULT_MAX_ROUTES_LENGTH = 50;
        #endregion

        #region private fields

        /// <summary>
        /// The maximum number of orders which should be processed synchronously.
        /// </summary>
        private int _maxOrders;

        /// <summary>
        /// The maximum number of routes which should be processed synchronously.
        /// </summary>
        private int _maxRoutes;

        #endregion
    }
}

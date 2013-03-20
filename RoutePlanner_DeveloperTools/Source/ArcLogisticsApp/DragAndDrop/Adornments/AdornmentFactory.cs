using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.DragAndDrop.Adornments
{
    /// <summary>
    /// Class that instantiates adornments.
    /// </summary>
    internal static class AdornmentFactory
    {
        /// <summary>
        /// Creates adornment.
        /// </summary>
        /// <param name="ordersAndStops">Collection of orders and stops.</param>
        /// <param name="source">Source where orders are dragged from.</param>
        /// <returns>Adornment.</returns>
        public static IAdornment CreateAdornment(IList<object> ordersAndStops, DragSource source)
        {
            Debug.Assert(ordersAndStops != null);
            Debug.Assert(ordersAndStops.Count > 0);

            IAdornment adornment = null;

            switch (source)
            {
                case DragSource.FindView:
                case DragSource.OrdersView:
                    adornment = _CreateOrdersViewAdornment(ordersAndStops);
                    break;
                case DragSource.RoutesView:
                    adornment = _CreateRoutesViewAdornment(ordersAndStops);
                    break;
                case DragSource.MapView:
                    adornment = _CreateMapViewAdornment(ordersAndStops);
                    break;
                case DragSource.TimeView:
                    adornment = _CreateGanttViewAdornment(ordersAndStops);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            return adornment;
        }

        /// <summary>
        /// Create adornemnt for the case when order is dragged from orders and find views.
        /// </summary>
        /// <param name="ordersAndStops">Stops and orders.</param>
        /// <returns>Adornment.</returns>
        private static IAdornment _CreateOrdersViewAdornment(IList<object> ordersAndStops)
        {
            IAdornment adornment;
            if (ordersAndStops.Count == 1) // Single order case.
            {
                Order order = AdornHelpers.GetOrder(ordersAndStops[0]);
                Debug.Assert(order != null);

                adornment = new SheetAdornment(order);
            }
            else // Multiple orders case.
                adornment = new MultiSheetAdornment(ordersAndStops);

            return adornment;
        }

        /// <summary>
        /// Create adornemnt for the case when order is dragged from routes views.
        /// </summary>
        /// <param name="ordersAndStops">Stops and orders.</param>
        /// <returns>Adornment.</returns>
        private static IAdornment _CreateRoutesViewAdornment(IList<object> ordersAndStops)
        {
            IAdornment adornment;
            if (ordersAndStops.Count == 1) // Single order case.
            {
                adornment = new DotAdornment(ordersAndStops[0] as Stop);
            }
            else // Multiple orders case.
                adornment = new MultiDotAdornment(ordersAndStops);

            return adornment;
        }

        /// <summary>
        /// Create adornemnt for the case when order is dragged from map view.
        /// </summary>
        /// <param name="ordersAndStops">Stops and orders.</param>
        /// <returns>Adornment.</returns>
        private static IAdornment _CreateMapViewAdornment(IList<object> ordersAndStops)
        {
            IAdornment adornment;
            if (ordersAndStops.Count == 1) // Single order case.
            {
                object orderOrStop = ordersAndStops[0];

                // Label sequence is turned on and object is stop.
                if (orderOrStop is Stop && App.Current.MapDisplay.LabelingEnabled)
                    adornment = new LabelSequenceAdornment(orderOrStop as Stop);
                else // Use custom order symbol adornment.
                {
                    adornment = new CustomOrderSymbolAdornment(orderOrStop);
                }
            }
            else // Multiple orders case.
                adornment = new MultiMapSymbolAdornment(ordersAndStops);

            return adornment;
        }

        /// <summary>
        /// Create adornemnt for the case when order is dragged from time view.
        /// </summary>
        /// <param name="ordersAndStops">Stops and orders.</param>
        /// <returns>Adornment.</returns>
        private static IAdornment _CreateGanttViewAdornment(IList<object> ordersAndStops)
        {
            IAdornment adornment;
            if (ordersAndStops.Count == 1) // Single order case.
            {
                object orderOrStop = ordersAndStops[0];
                Debug.Assert(orderOrStop is Stop);
                adornment = new GanttElementAdornment(orderOrStop as Stop);
            }
            else // Multiple order case.
                adornment = new MultiGanttElementAdornment(ordersAndStops);

            return adornment;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using ESRI.ArcLogistics.App.DragAndDrop.Adornments.Controls;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.DragAndDrop.Adornments
{
    /// <summary>
    /// Represents multi rectangle adornment.
    /// </summary>
    internal class MultiGanttElementAdornment : MultiOrderAdornmentBase
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of <c>MultiGanttElementAdornment</c> class.
        /// </summary>
        public MultiGanttElementAdornment(IList<object> ordersAndStops)
            : base (ordersAndStops)
        {

        }

        #endregion

        #region Protected Overriden Methods

        protected override FrameworkElement CreateOrderElement(object orderOrStop)
        {
            Debug.Assert(orderOrStop != null && orderOrStop is Stop);

            // Create and return gantt item element.
            return new GanttElementFrameworkElement(orderOrStop as Stop);
        }

        #endregion
    }
}

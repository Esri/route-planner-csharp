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
    /// Class that represents gantt item element adornment.
    /// </summary>
    class GanttElementAdornment : SingleOrderAdornmentBase
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of <c>GanttElementAdorner</c> class.
        /// </summary>
        public GanttElementAdornment(Stop stop)
            : base (stop)
        {

        }

        #endregion

        #region Protected Overriden Methods

        protected override FrameworkElement CreateOrderElement(object orderOrStop)
        {
            Debug.Assert(orderOrStop != null && orderOrStop is Stop);

 	        // Create order sheet image.
            return new GanttElementFrameworkElement(orderOrStop as Stop);
        }

        #endregion
    }
}

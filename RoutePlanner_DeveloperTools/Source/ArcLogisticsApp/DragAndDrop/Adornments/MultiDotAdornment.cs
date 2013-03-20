using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.DragAndDrop.Adornments
{
    /// <summary>
    /// Represents multi dot adornment.
    /// </summary>
    internal class MultiDotAdornment : MultiOrderAdornmentBase
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of <c>MultiDotAdornment</c> class.
        /// </summary>
        public MultiDotAdornment(IList<object> ordersAndStops)
            : base (ordersAndStops)
        {

        }

        #endregion

        #region Protected Overriden Methods

        protected override FrameworkElement CreateOrderElement(object orderOrStop)
        {
            Debug.Assert(orderOrStop != null && orderOrStop is Stop);

            // Create and return dot element.
            return AdornHelpers.CreateDotSymbol(orderOrStop as Stop);
        }

        #endregion
    }
}

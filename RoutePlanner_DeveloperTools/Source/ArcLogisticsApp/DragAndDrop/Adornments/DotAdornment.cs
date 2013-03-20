using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.DragAndDrop.Adornments
{
    /// <summary>
    /// Adornment that represents a dot like on map control.
    /// </summary>
    internal class DotAdornment : SingleOrderAdornmentBase
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of <c>DotAdorner</c> class.
        /// </summary>
        public DotAdornment(Stop stop)
            : base(stop)
        {

        }

        #endregion
        
        #region Protected Overriden Methods

        protected override FrameworkElement CreateOrderElement(object orderOrStop)
        {
            Debug.Assert(orderOrStop != null && orderOrStop is Stop);

 	        // Create order sheet image.
            return AdornHelpers.CreateDotSymbol(orderOrStop as Stop);
        }

        #endregion
    }
}

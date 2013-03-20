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
    /// Adornment that represents label sequence symbol.
    /// </summary>
    class LabelSequenceAdornment : SingleOrderAdornmentBase
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of <c>GanttElementAdorner</c> class.
        /// </summary>
        public LabelSequenceAdornment(Stop stop)
            : base (stop)
        {

        }

        #endregion

        #region Protected Overriden Methods

        protected override FrameworkElement CreateOrderElement(object orderOrStop)
        {
            Debug.Assert(orderOrStop != null && orderOrStop is Stop);

 	        // Create order label sequence image.
            return AdornHelpers.CreateLabelSequenceSymbol(orderOrStop as Stop);
        }

        #endregion
    }
}

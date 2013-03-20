using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.DragAndDrop.Adornments
{
    /// <summary>
    /// Represents multiple custom order symbols adornment.
    /// </summary>
    class MultiMapSymbolAdornment : MultiOrderAdornmentBase
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of <c>MultiMapSymbolAdornment</c> class.
        /// </summary>
        public MultiMapSymbolAdornment(IList<object> ordersAndStops)
            : base (ordersAndStops)
        {

        }

        #endregion

        #region Protected Overriden Methods

        protected override FrameworkElement CreateOrderElement(object orderOrStop)
        {
            if (orderOrStop is Stop && App.Current.MapDisplay.LabelingEnabled)
                return AdornHelpers.CreateLabelSequenceSymbol(orderOrStop as Stop);
            else
                return AdornHelpers.CreateCustomOrderSymbol(orderOrStop);
        }

        #endregion
    }
}

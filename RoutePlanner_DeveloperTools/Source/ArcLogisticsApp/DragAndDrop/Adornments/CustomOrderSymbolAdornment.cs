using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.DragAndDrop.Adornments
{
    /// <summary>
    /// Adornment that represents custom order symbol (uses map symbology).
    /// </summary>
    internal class CustomOrderSymbolAdornment : SingleOrderAdornmentBase
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of <c>CustomOrderSymbolAdornment</c> class.
        /// </summary>
        public CustomOrderSymbolAdornment(object orderOrStop)
            : base(orderOrStop)
        {

        }

        #endregion
        
        #region Protected Overriden Methods

        protected override FrameworkElement CreateOrderElement(object orderOrStop)
        {
            Debug.Assert(orderOrStop != null);

 	        // Create order sheet image.
            return AdornHelpers.CreateCustomOrderSymbol(orderOrStop);
        }

        #endregion
    }
}

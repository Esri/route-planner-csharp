using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ESRI.ArcLogistics.App.DragAndDrop.Adornments
{
    /// <summary>
    /// Class that represents adornment in form of order sheets.
    /// </summary>
    internal class MultiSheetAdornment : MultiOrderAdornmentBase
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of <c>MultiSheetAdornment</c> class.
        /// </summary>
        public MultiSheetAdornment(IList<object> ordersAndStops)
            : base (ordersAndStops)
        {

        }

        #endregion

        #region Protected Overriden Methods

        protected override FrameworkElement CreateOrderElement(object orderOrStop)
        {
 	        // Create order sheet image.
            return AdornHelpers.CreateSheetImage();
        }

        #endregion
    }
}

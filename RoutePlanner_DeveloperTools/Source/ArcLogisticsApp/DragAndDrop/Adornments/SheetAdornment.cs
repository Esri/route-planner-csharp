using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.DragAndDrop.Adornments
{
    /// <summary>
    /// Class that represents adornment in form of order sheet.
    /// </summary>
    internal class SheetAdornment : SingleOrderAdornmentBase
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of <c>SheetAdornment</c> class.
        /// </summary>
        public SheetAdornment(Order order)
            : base (order)
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

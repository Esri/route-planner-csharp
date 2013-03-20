using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.App.DragAndDrop.Adornments
{
    /// <summary>
    /// Interface of drag and drop adornment.
    /// </summary>
    internal interface IAdornment
    {
        /// <summary>
        /// Gets adornment element as a canvas.
        /// </summary>
        Canvas Adornment { get; }
    }
}

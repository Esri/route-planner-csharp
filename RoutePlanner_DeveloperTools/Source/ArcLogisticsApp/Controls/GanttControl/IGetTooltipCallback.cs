using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interface for create Tooltip helpers.
    /// </summary>
    interface IGetTooltipCallback
    {
        /// <summary>
        /// Returns tooltip content for hovered object.
        /// </summary>
        /// <param name="hoveredObject">Hovered object.</param>
        /// <returns>Tooltip content.</returns>
        object GetTooltip(object hoveredObject);
    }
}

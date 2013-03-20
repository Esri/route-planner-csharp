using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interface contains common properties/methods for gantt glyphs.
    /// </summary>
    internal interface IGanttGlyph
    {
        /// <summary>
        /// Draws element in necessary context. 
        /// </summary>
        /// <param name="context">Darwing Context.</param>
        void Draw(DrawingContext context);
    }
}

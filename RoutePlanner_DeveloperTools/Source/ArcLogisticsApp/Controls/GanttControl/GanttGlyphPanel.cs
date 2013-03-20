using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Panel that shows additional glyphs over GanttControl.
    /// </summary>
    internal class GanttGlyphPanel : Canvas
    {
        #region Public Methods

        /// <summary>
        /// Adds glyph to collection and redraws panel.
        /// </summary>
        /// <param name="ganttItemElement">Key object element.</param>
        /// <param name="glyph">Adding glyph.</param>
        public void AddGlyph(object keyObject, IGanttGlyph glyph)
        {
            if (_glyphs == null)
                _glyphs = new Dictionary<object, IGanttGlyph>();

            if (!_glyphs.Keys.ToList<object>().Contains(keyObject))
                _glyphs.Add(keyObject, glyph);

            // Call Invalidate visual to redraw control.
            InvalidateVisual();
        }

        /// <summary>
        /// Removes glyph from collection by key and redraws panel.
        /// </summary>
        /// <param name="ganttItemElement">Key object element.</param>
        public void RemoveGlyphByKey(object keyObject)
        {
            if (_glyphs == null)
                return;

            if (_glyphs.Keys.ToList<object>().Contains(keyObject))
                _glyphs.Remove(keyObject);

            // Call Invalidate visual to redraw control.
            InvalidateVisual();
        }

        #endregion

        #region Protected overriden methods

        ///<summary>
        ///Draws all necessary elements.
        ///</summary>
        ///<param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_glyphs == null)
                return;

            // Redraw all glyphs in collection.
            foreach (IGanttGlyph glyph in (ICollection<IGanttGlyph>)_glyphs.Values)
                glyph.Draw(drawingContext);
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Collection af all glyphs.
        /// </summary>
        private Dictionary<object, IGanttGlyph> _glyphs;

        #endregion
    }
}

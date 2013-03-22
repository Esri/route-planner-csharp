/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Glyph that visible when user drags anything over gantt control.
    /// </summary>
    internal class StopDragOverGlyph : IGanttGlyph
    {
        #region Constructor

        /// <summary>
        /// Creates ne glyph.
        /// </summary>
        /// <param name="parentElementBounds">Stop element bounds.</param>
        /// <param name="isStopFirst">Bool flag defines whether parent stop is first in route's sequence.</param>
        public StopDragOverGlyph(Rect parentElementBounds, bool isStopFirst)
        {
            _parentBounds = parentElementBounds;
            _isStopFirst = isStopFirst;
        }

        #endregion

        #region IGanttGlyph Members

        /// <summary>
        /// Draws glyph.
        /// </summary>
        /// <param name="context">Drawing context.</param>
        public void Draw(System.Windows.Media.DrawingContext context)
        {
            if (_isStopFirst)
                _DrawRightDraggedOverGlyph(context);
            else
                _DrawLeftDraggedOverGlyph(context);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Draw left dragged over glyph.
        /// </summary>
        /// <param name="drawingContext">Context where glyph should be draw.</param>
        private void _DrawLeftDraggedOverGlyph(DrawingContext drawingContext)
        {
            // Define glyph's area.
            Rect glyphRect = new Rect(_parentBounds.Left - GLYPH_WIDTH, _parentBounds.Top - GLYPH_GAP, GLYPH_WIDTH, GLYPH_HEIGHT);

            // Define brushes.
            Brush insertionBrush = (DrawingBrush)Application.Current.FindResource(LEFT_GLYPH_BRUSH);
            insertionBrush.Freeze();
            Pen insertionPen = new Pen(insertionBrush, 0);
            insertionPen.Freeze();

            // Draw glyph.
            drawingContext.DrawRectangle(insertionBrush, insertionPen, glyphRect);
        }

        /// <summary>
        /// Draw right dragged over glyph.
        /// </summary>
        /// <param name="drawingContext">Context where glyph should be draw.</param>
        private void _DrawRightDraggedOverGlyph(DrawingContext drawingContext)
        {
            // Define glyph's area.
            Rect glyphRect = new Rect(_parentBounds.Left + _parentBounds.Width + GLYPH_GAP, _parentBounds.Top - GLYPH_GAP, GLYPH_WIDTH, GLYPH_HEIGHT);

            // Define brushes.
            Brush insertionBrush = (DrawingBrush)Application.Current.FindResource(RIGHT_GLYPH_BRUSH);
            insertionBrush.Freeze();
            Pen insertionPen = new Pen(insertionBrush, 0);
            insertionPen.Freeze();

            // Draw glyph.
            drawingContext.DrawRectangle(insertionBrush, insertionPen, glyphRect);
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Parent element's rect bounds.
        /// </summary>
        private Rect _parentBounds;

        /// <summary>
        /// Bool flag shows whether this is left drag over glyph.
        /// </summary>
        private bool _isStopFirst;

        #endregion

        #region Private Constants

        /// <summary>
        /// Left glyph brush resource.
        /// </summary>
        private const string LEFT_GLYPH_BRUSH = "InsertionLeftDrawingBrush";

        /// <summary>
        /// Right glyph brush resource.
        /// </summary>
        private const string RIGHT_GLYPH_BRUSH = "InsertionRightDrawingBrush";

        /// <summary>
        /// Glyph width.
        /// </summary>
        private const double GLYPH_WIDTH = 14;

        /// <summary>
        /// Glyph height.
        /// </summary>
        private const double GLYPH_HEIGHT = 23;

        /// <summary>
        /// Glyph gap.
        /// </summary>
        private const double GLYPH_GAP = 1;

        #endregion
    }
}

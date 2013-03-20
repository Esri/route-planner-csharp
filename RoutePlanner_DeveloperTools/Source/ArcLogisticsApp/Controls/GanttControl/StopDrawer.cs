using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.DomainObjects;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Media.Effects;
using ESRI.ArcLogistics.App.DragAndDrop;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class that contains code that draws a stop on gantt control. 
    /// </summary>
    /// <remarks>
    /// Drawing code is moved into this class to be reused from drag and drop classes, which need to show
    /// the same image while dragging.
    /// </remarks>
    internal static class StopDrawer
    {
        #region Public Methods

        /// <summary>
        /// Stracture that contains reference to stop and to stop's route. Passing this structure
        /// to DrawStop method instead of stop reference can help to increase performacne.
        /// </summary>
        public struct StopInfo
        {
            /// <summary>
            /// Stop.
            /// </summary>
            public Stop Stop
            {
                get;
                set;
            }

            /// <summary>
            /// Stop's route.
            /// </summary>
            public Route Route
            {
                get;
                set;
            }
        }

        /// <summary>
        /// Draws stop.
        /// </summary>
        /// <param name="stopInfo">Information about stop. Used for optimizing performance.</param>
        /// <param name="context">DrawingContext.</param>
        /// <remarks>
        /// Getting some relation properties (like Route, AssociatedObject) from stop takes some time
        /// due to the data layer implementation specifics. To improve perofmrance on rendering of
        /// large amount of stops use this method passing cached value of stop's properties.
        /// </remarks>
        public static void DrawStop(StopInfo stopInfo, GanttItemElementDrawingContext context)
        {
            Debug.Assert(stopInfo.Stop != null);
            Debug.Assert(stopInfo.Route != null);
            Debug.Assert(context != null);

            Brush fillBrush = _GetFillBrush(stopInfo, context);

            // Draw element.
            _DrawRectangle(fillBrush, context);

            // Draw violated icon.
            if (stopInfo.Stop.IsViolated)
            {
                Rect elementRect = _GetElementRect(context);
                Rect violatedIconRect = new Rect(elementRect.Left + GAP_SIZE, elementRect.Top + GAP_SIZE, ICON_SIZE, ICON_SIZE);

                violatedIconRect.Width = (violatedIconRect.Width > elementRect.Width) ? elementRect.Width : violatedIconRect.Width;

                ImageBrush brush = GanttControlHelper.GetViolatedBrush();
                context.DrawingContext.DrawImage(brush.ImageSource, violatedIconRect);
            }
        }

        /// <summary>
        /// Draw stop element which has specific color that is passed a parameter.
        /// </summary>
        /// <param name="color">Color.</param>
        /// <param name="context">Context.</param>
        /// <remarks>
        /// This methods is used when it is necessary to draw stop without any stop specifics. Just colored 
        /// rectangle.
        /// </remarks>
        public static void DrawStop(System.Drawing.Color color, GanttItemElementDrawingContext context)
        {
            Color wpfColor = Color.FromRgb(color.R, color.G, color.B);
            SolidColorBrush brush = new SolidColorBrush(wpfColor);

            _DrawRectangle(brush, context);
        }

        /// <summary>
        /// Returns elemnet rect.
        /// </summary>
        /// <param name="context">Drawing context.</param>
        /// <returns>Element's rect bounds.</returns>
        public static Rect GetElementRect(GanttItemElementDrawingContext context)
        {
            return _GetElementRect(context);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Defines brush to draw stop element.
        /// </summary>
        /// <param name="stop">Stop.</param>
        /// <param name="context">Drawing context.</param>
        /// <returns></returns>
        private static Brush _GetFillBrush(StopInfo stopInfo, GanttItemElementDrawingContext context)
        {
            Debug.Assert(context != null);

            // Define selected color.
            if (context.DrawSelected)
                return GanttControlHelper.GetSelectedBrush();

            // Define location color.
            if (stopInfo.Stop.StopType == StopType.Location)
                return GanttControlHelper.GetLocationBrush();

            // Define Break color.
            if (stopInfo.Stop.StopType == StopType.Lunch)
                return GanttControlHelper.GetBreakBrush();

            // Define locked color.
            if (stopInfo.Stop.IsLocked || (stopInfo.Route != null && stopInfo.Route.IsLocked))
                return GanttControlHelper.GetLockedBrush(stopInfo.Route.Color);

            // If stop is Order, not locked, not selected and not dragged over - return normal fill color.
            return GanttControlHelper.GetFillBrush(stopInfo.Route.Color);
        }
        
        /// <summary>
        /// Draws gantt item element.
        /// </summary>
        /// <param name="fillBrush">Filling brush.</param>
        /// <param name="context">Context.</param>
        private static void _DrawRectangle(Brush fillBrush, GanttItemElementDrawingContext context)
        {
            // Draw shadow effect.
            _DrawShadowEffect(context);

            Rect elementRect = _GetElementRect(context);
            Pen drawPen = GanttControlHelper.GetPen();

            // Draw rectangle.
            context.DrawingContext.DrawRoundedRectangle(fillBrush, drawPen, elementRect, ROUND_RADIUS, ROUND_RADIUS);

            // Draw top gradient effect.
            _DrawGradientEffect(context);
        }

        /// <summary>
        /// Draws drop shadow effect.
        /// </summary>
        /// <param name="context">Drawing context.</param>
        private static void _DrawShadowEffect(GanttItemElementDrawingContext context)
        {
            Rect elementRect = _GetShadowRect(context);
            Brush effectBrush = GanttControlHelper.GetShadowEffectBrush();
            Pen drawPen = GanttControlHelper.GetPen();

            context.DrawingContext.DrawRoundedRectangle(effectBrush, drawPen, elementRect, ROUND_RADIUS, ROUND_RADIUS);
        }

        /// <summary>
        /// Draws internal gradient effect.
        /// </summary>
        /// <param name="context">Drawing context.</param>
        private static void _DrawGradientEffect(GanttItemElementDrawingContext context)
        {
            Rect elementRect = _GetGradientRect(context);
            Brush gradientBrush = GanttControlHelper.GetGradientEffectBrush();
            Pen drawPen = GanttControlHelper.GetPen();

            context.DrawingContext.DrawRoundedRectangle(gradientBrush, drawPen, elementRect, ROUND_RADIUS, ROUND_RADIUS);     
        }

        /// <summary>
        /// Returns element gradient drawing rectangle.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <returns>Drawing rectangle.</returns>
        private static Rect _GetGradientRect(GanttItemElementDrawingContext context)
        {
            // Add gap bentween base elemnt and gradient.
            double xPos = context.DrawingArea.X + GRADIENT_GAP_SIZE;
            double yPos = context.DrawingArea.Top + GAP_SIZE + GRADIENT_GAP_SIZE;
            double height = context.DrawingArea.Height - GAP_SIZE - GRADIENT_GAP_SIZE*2;
            double width = Math.Max(context.DrawingArea.Width - GRADIENT_GAP_SIZE*2, 0);

            // Define gradient size.
            return new Rect(xPos, yPos, width, height);
        }

        /// <summary>
        /// Returns element shadow drawing rectangle.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <returns>Drawing rectangle.</returns>
        private static Rect _GetShadowRect(GanttItemElementDrawingContext context)
        {
            double gapSize = Math.Min(context.DrawingArea.Width, GAP_SIZE);

            // Add gap bentween base elemnt and shadow.
            double xPos = context.DrawingArea.X + gapSize;
            double yPos = context.DrawingArea.Top + gapSize + gapSize/2;
            double height = context.DrawingArea.Height - gapSize;
            double width = Math.Max(context.DrawingArea.Width - gapSize/2, 0);

            // Define shadow size.
            return new Rect(xPos, yPos, width, height);
        }

        /// <summary>
        /// Returns element drawing rectangle.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <returns>Drawing rectangle.</returns>
        private static Rect _GetElementRect(GanttItemElementDrawingContext context)
        {
            // Add gap bentween gantt rows.
            double yPos = context.DrawingArea.Top + GAP_SIZE;
            double height = context.DrawingArea.Height - GAP_SIZE;

            // Define element size.
            return new Rect(context.DrawingArea.X, yPos, context.DrawingArea.Width, height);
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Radius of rounded rectangle corners.
        /// </summary>
        private const int ROUND_RADIUS = 2;

        /// <summary>
        /// Violated icon size.
        /// </summary>
        private const int ICON_SIZE = 16;

        /// <summary>
        /// Size of gap between gantt rows.
        /// </summary>
        private const int GAP_SIZE = 2;

        /// <summary>
        /// Size of gap between rectangle and gradient effect.
        /// </summary>
        private const int GRADIENT_GAP_SIZE = 1;

        /// <summary>
        /// Size of gap between rectangle and gradient effect.
        /// </summary>
        private const int SHADOW_GAP_SIZE = 1;

        /// <summary>
        /// Shadow effect opacity.
        /// </summary>
        private const double SHADOW_EFFECT_OPACITY = 0.5;


        #endregion
    }
}

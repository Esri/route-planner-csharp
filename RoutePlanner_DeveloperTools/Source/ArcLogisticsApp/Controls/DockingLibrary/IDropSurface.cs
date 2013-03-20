using System.Windows;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interface drop surface
    /// </summary>
    interface IDropSurface
    {
        /// <summary>
        /// Get surface rectangle
        /// </summary>
        /// <returns>Returns a rectangle where this surface is active</returns>
        Rect SurfaceRectangle { get; }
        /// <summary>
        /// Handles this sourface mouse entering
        /// </summary>
        /// <param name="point">Current mouse position</param>
        void OnDragEnter(Point point);
        /// <summary>
        /// Handles mouse overing this surface
        /// </summary>
        /// <param name="point">Current mouse position</param>
        void OnDragOver(Point point);
        /// <summary>
        /// Handles mouse leave event during drag
        /// </summary>
        /// <param name="point">Current mouse position</param>
        void OnDragLeave(Point point);
        /// <summary>
        /// Handler drop events
        /// </summary>
        /// <param name="point">Current mouse position</param>
        bool OnDrop(Point point);
    }
}

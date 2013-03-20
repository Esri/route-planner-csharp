using System;
using System.Windows.Input;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Provides helper facilities for the <see cref="System.Windows.Input.Mouse"/>
    /// class.
    /// </summary>
    internal static class MouseHelper
    {
        /// <summary>
        /// Temporarily overrides mouse cursor for the entire application.
        /// </summary>
        /// <param name="newCursor">The new cursor to be used.</param>
        /// <returns>The reference to the <see cref="System.IDisposable"/> object
        /// restoring current mouse cursor upon call to the
        /// <see cref="M:System.IDisposable.Dispose"/> method.</returns>
        public static IDisposable OverrideCursor(Cursor newCursor)
        {
            var currentCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = newCursor;

            return new DelegateDisposable(() => Mouse.OverrideCursor = currentCursor);
        }
    }
}

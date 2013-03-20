using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ESRI.ArcLogistics.App.DragAndDrop.Adornments;

namespace ESRI.ArcLogistics.App.DragAndDrop
{
    /// <summary>
    /// Class that provides feedback during drag and drop. It changes cursor depending on current effect.
    /// </summary>
    internal class DragDropFeedbacker : UIElement, IDisposable
    {
        #region Constructor

        /// <summary>
        /// Creates isntance of <c>DragDropFeedbacker</c> class.
        /// </summary>
        /// <param name="adornment">Drag and drop adornment to show below cursor.</param>
        public DragDropFeedbacker(IAdornment adornment)
        {
            Debug.Assert(adornment != null);

            _adornment = adornment;

            // Subscribe on feedback from drag and drop.
            this.GiveFeedback += new GiveFeedbackEventHandler(_GiveFeedbackHandler);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Deletes created cursors.
        /// </summary>
        public void Dispose()
        {
            // Release cursors.
            if (_moveCursor != null)
                _moveCursor.Dispose();
            if (_noneCursor != null)
                _noneCursor.Dispose();

            // Unsubscribe from d&d feedback.
            this.GiveFeedback -= _GiveFeedbackHandler;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Changes cursor for drag and drop current effect.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _GiveFeedbackHandler(object sender, GiveFeedbackEventArgs e)
        {
            try
            {
                // Set custom cursor.
                _SetCursor(e.Effects);

                // Don't use default cursor.
                e.UseDefaultCursors = false;

                e.Handled = true; // We handled this event.
            }
            catch (Exception ex)
            {
                // Ignore exceptions. In case cursor cannot be created - no problem, show common cursor.
                Logger.Warning(ex);
            }
        }

        /// <summary>
        /// Method sets current cursor depending on input drag and drop effect.
        /// </summary>
        /// <param name="effects">Current drag and drop effect.</param>
        private void _SetCursor(DragDropEffects effects)
        {
            if (effects == DragDropEffects.Move)
                System.Windows.Forms.Cursor.Current = _GetMoveCursor();
            else
                System.Windows.Forms.Cursor.Current = _GetNoneCursor();
        }

        /// <summary>
        /// Returns move cursor. Methods creates it if necessary.
        /// </summary>
        /// <returns>Returns move cursor.</returns>
        private System.Windows.Forms.Cursor _GetMoveCursor()
        {
            if (_moveCursor == null)
                _moveCursor = _CreateCursor(System.Windows.Forms.Cursors.Arrow);

            return _moveCursor;
        }

        /// <summary>
        /// Returns none cursor. Methods creates it if necessary.
        /// </summary>
        /// <returns>Returns none cursor.</returns>
        private System.Windows.Forms.Cursor _GetNoneCursor()
        {
            if (_noneCursor == null)
                _noneCursor = _CreateCursor(System.Windows.Forms.Cursors.No);

            return _noneCursor;
        }

        /// <summary>
        /// Create cursor for drag and drop.
        /// </summary>
        /// <param name="baseCursor">Base cursor that is show in the top left corner.</param>
        /// <returns>Create cursor.</returns>
        private System.Windows.Forms.Cursor _CreateCursor(System.Windows.Forms.Cursor baseCursor)
        {
            // Create cursor canvas.
            Canvas cursorCanvas = new Canvas();

            // Get base cursor image.
            System.Drawing.Point hotSpotPoint;
            Image baseCursorImage = _CreateImageFromCursor(baseCursor, out hotSpotPoint);

            // Add base cursor.
            cursorCanvas.Children.Add(baseCursorImage);
            
            // Add adornment.
            cursorCanvas.Children.Add(_adornment.Adornment);

            // Move adornment below base cusor.
            cursorCanvas.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            // Get base cursor image last visible pixel.
            Point cursorLastVisiblePoint = _GetBitmapLastVisiblePixel(baseCursorImage.Source as BitmapSource);

            Canvas.SetTop(_adornment.Adornment, cursorLastVisiblePoint.Y + SPACE_SIZE);
            Canvas.SetLeft(_adornment.Adornment, cursorLastVisiblePoint.X + SPACE_SIZE);

            // Arrange cursor canvas to get its actual height and width.
            double cursorWidth = cursorLastVisiblePoint.X + SPACE_SIZE + _adornment.Adornment.Width;
            double cursorHeight = cursorLastVisiblePoint.Y + SPACE_SIZE + _adornment.Adornment.Height;
            Rect cursorRect = new Rect(0, 0, cursorWidth, cursorHeight);

            cursorCanvas.Arrange(cursorRect);
            cursorCanvas.UpdateLayout();

            // Create cursor.
            System.Windows.Forms.Cursor cursor = _CreateCursorFromCanvas(cursorCanvas, hotSpotPoint);

            // Remove all canvas elements.
            cursorCanvas.Children.Clear();

            return cursor;
        }

        /// <summary>
        /// Creates image from input cursor for adding to WPF canvas.
        /// </summary>
        /// <param name="cursor">Cursor to convert.</param>
        /// <param name="hotSpot">Output cursor hotspot point.</param>
        /// <returns>Create cursor image.</returns>
        private Image _CreateImageFromCursor(System.Windows.Forms.Cursor cursor, out System.Drawing.Point hotSpot)
        {
            // Get info about cursor.
            WinAPIHelpers.IconInfo iconInfo = new WinAPIHelpers.IconInfo();
            if (WinAPIHelpers.GetIconInfo(cursor.Handle, ref iconInfo))
            {
                // Set hotspot point.
                hotSpot = new System.Drawing.Point { X = iconInfo.xHotspot, Y = iconInfo.yHotspot };

                // Free bitmap objects.
                if (iconInfo.hbmColor != IntPtr.Zero)
                    WinAPIHelpers.DeleteObject(iconInfo.hbmColor);
                if (iconInfo.hbmMask != IntPtr.Zero)
                    WinAPIHelpers.DeleteObject(iconInfo.hbmMask);
            }
            else
            {
                // We didn't get info about cursor for some reason - set hot spot to (0; 0).
                hotSpot = new System.Drawing.Point { X = 0, Y = 0 };
            }

            // Create bitmap source from cursor (cursor handle is icon handle as well).
            BitmapSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(cursor.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            // Create image control.
            Image image = new Image();
            image.Source = source;

            return image;
        }

        /// <summary>
        /// Returns coordinates of the last visible pixel (most right-bottom that is not fully transparent).
        /// </summary>
        /// <param name="bitmap">Bitmap.</param>
        /// <returns>Coordinates of the last visible pixel.</returns>
        private Point _GetBitmapLastVisiblePixel(BitmapSource bitmap)
        {
            Debug.Assert(bitmap != null);

            // If format is not BGRA32 then we need to convert bitmap.
            BitmapSource bmp;
            if (bitmap.Format != PixelFormats.Bgra32)
                bmp = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
            else
                bmp = bitmap;

            // Get bitmap extent.
            int width = bmp.PixelWidth;
            int height = bmp.PixelHeight;

            // Get bitmap pixels.
            byte[] pixels = new byte[width * BYTES_PER_PIXEL_IN_BGRA32_FORMAT * height];
            bmp.CopyPixels(pixels, width * BYTES_PER_PIXEL_IN_BGRA32_FORMAT, 0);

            // Find a pixel searching from bitmap right-bottom corner which alpha chanel is not 0 - opaque pixel.
            Point? pt = null;
            for (int j = height - 1; j >= 0 && pt == null; j--)
                for (int i = width * BYTES_PER_PIXEL_IN_BGRA32_FORMAT - 1; i >= 0 && pt == null; i -= BYTES_PER_PIXEL_IN_BGRA32_FORMAT)
                {
                    if (pixels[j * width * BYTES_PER_PIXEL_IN_BGRA32_FORMAT + i] != 0)
                    {
                        // We found this point.
                        pt = new Point((i + 1) / BYTES_PER_PIXEL_IN_BGRA32_FORMAT, j);
                    }
                }

            if (!pt.HasValue)
                pt = new Point(0, 0); // Probably bitmap doesn't have anything drawn.

            return pt.Value;
        }

        /// <summary>
        /// Create cursor from canvas.
        /// </summary>
        /// <param name="canvas">Input canvas.</param>
        /// <param name="hotSpotPoint">Hotspot cursor point.</param>
        /// <returns>Created cursor.</returns>
        private System.Windows.Forms.Cursor _CreateCursorFromCanvas(Canvas canvas, System.Drawing.Point hotSpotPoint)
        {
            // Render canvas to bitmap.
            RenderTargetBitmap targetBitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, BASIC_DPI, BASIC_DPI, PixelFormats.Pbgra32);
            targetBitmap.Render(canvas);

            // Create PNG encoder.
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(targetBitmap));

            // Render target bitmap to System.Drawing bitmap.
            System.Drawing.Bitmap bmp;
            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Save(stream);
                bmp = new System.Drawing.Bitmap(stream);
            }

            // Create cursor.
            WinAPIHelpers.IconInfo iconInfo = new WinAPIHelpers.IconInfo();
            IntPtr hIcon = bmp.GetHicon();
            WinAPIHelpers.GetIconInfo(hIcon, ref iconInfo);
            iconInfo.xHotspot = hotSpotPoint.X;
            iconInfo.yHotspot = hotSpotPoint.Y;
            iconInfo.fIcon = false;
            IntPtr cursorHandle = WinAPIHelpers.CreateIconIndirect(ref iconInfo);
            
            // Destroy icon obtained from GetHicon method.
            if (hIcon != IntPtr.Zero)
                WinAPIHelpers.DestroyIcon(hIcon);

            // Free resources.
            if (iconInfo.hbmColor != IntPtr.Zero)
                WinAPIHelpers.DeleteObject(iconInfo.hbmColor);
            if (iconInfo.hbmMask != IntPtr.Zero)
                WinAPIHelpers.DeleteObject(iconInfo.hbmMask);

            System.Windows.Forms.Cursor cursor = null;
            if (cursorHandle != null)
                cursor = new System.Windows.Forms.Cursor(cursorHandle);

            return cursor;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Size of space between cursor and adornment in pixels.
        /// </summary>
        private const int SPACE_SIZE = 3;

        /// <summary>
        /// Bytes per pixel in BGRA32 pixel format.
        /// </summary>
        private const int BYTES_PER_PIXEL_IN_BGRA32_FORMAT = 4;

        /// <summary>
        /// Presumable cursor height.
        /// </summary>
        private const double DEFAULT_CURSOR_HEIGHT = 18;

        /// <summary>
        /// Presumable cursor width.
        /// </summary>
        private const double DEFAULT_CURSOR_WIDTH = 18;

        /// <summary>
        /// Basic monitor DPI.
        /// </summary>
        private const double BASIC_DPI = 96; 

        /// <summary>
        /// Move cursor.
        /// </summary>
        System.Windows.Forms.Cursor _moveCursor;

        /// <summary>
        /// None cursor.
        /// </summary>
        System.Windows.Forms.Cursor _noneCursor;

        /// <summary>
        /// Adornment that is shown below cursor.
        /// </summary>
        private IAdornment _adornment;

        #endregion
    }
}

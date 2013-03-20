using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ESRI.ArcLogistics.App.Controls.BitmapEx
{
    /// <summary>
    /// Bitmap class inherited from UIElement which is able to do pixel alignment.
    /// Therefore bitmaps are displayed without blurry effect.
    /// To achieve it this class performs two things:
    ///     1. Adjusts size to real pixel sizes.
    ///     2. Adjusts it's position to be on pixel boundaries.
    /// </summary>
    public class BitmapUIElement : UIElement
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <c>BitmapUIElement</c>.
        /// </summary>
        public BitmapUIElement()
        {
            LayoutUpdated += _OnLayoutUpdated;
        }

        #endregion Constructors

        #region Public static properties

        /// <summary>
        /// Dependency property Source.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source",
                                        typeof(BitmapSource),
                                        typeof(BitmapUIElement),
                                        new FrameworkPropertyMetadata(null,
                                            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                                            BitmapUIElement._OnSourceChanged)
                                       );

        #endregion Public static properties

        #region Public properties

        /// <summary>
        /// Gets or sets bitmap source.
        /// </summary>
        public BitmapSource Source
        {
            get { return (BitmapSource) GetValue(SourceProperty); }

            set { SetValue(SourceProperty, value); }
        }

        #endregion Public properties

        #region Overridden methods of the base class

        /// <summary>
        /// Provides measurement logic for sizing this element properly, with consideration of the size of any
        /// child element content.
        /// Measures size to be the size needed to display the bitmap pixels.
        /// </summary>
        /// <param name="availableSize">The available size that the parent element can allocate for the child.</param>
        /// <returns>The desired size of this element in layout.</returns>
        protected override Size MeasureCore(Size availableSize)
        {
            Size measuredSize = new Size();

            // Get bitmap source.
            BitmapSource bitmapSource = Source;

            if (bitmapSource != null)
            {
                // Get presentation source.
                PresentationSource presentationSource = PresentationSource.FromVisual(this);

                if (presentationSource != null)
                {
                    // Get transformation matrix fom device.
                    Matrix fromDeviceMatrix = presentationSource.CompositionTarget.TransformFromDevice;

                    // Get bitmap size in pixels.
                    Vector bitmapSizeInPixels = new Vector(bitmapSource.PixelWidth, bitmapSource.PixelHeight);

                    // Get measured size vector by transforming the bitmap's size in pixels vector
                    // using transformation matrix.
                    Vector measuredSizeV = fromDeviceMatrix.Transform(bitmapSizeInPixels);

                    // Get measured size.
                    measuredSize = new Size(measuredSizeV.X, measuredSizeV.Y);
                }
            }

            return measuredSize;
        }

        /// <summary>
        /// Participates in rendering operations that are directed by the layout system.
        /// The rendering instructions for this element are not used directly when this method is invoked,
        /// and are instead preserved for later asynchronous use by layout and drawing.
        /// </summary>
        /// <param name="drawingContext">The drawing instructions for a specific element.
        /// This context is provided to the layout system.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            BitmapSource bitmapSource = Source;
            if (bitmapSource != null)
            {
                _pixelOffset = _GetPixelOffset();

                // Render the bitmap offset by the needed amount to align on pixels boundary.
                drawingContext.DrawImage(bitmapSource, new Rect(_pixelOffset, DesiredSize));
            }
        }

        #endregion Overridden methods of the base class

        #region Private static methods

        /// <summary>
        /// Invoked when bitmap's Source property is changed.
        /// </summary>
        /// <param name="dependencyObject">Dependency object - bitmap.</param>
        /// <param name="eventArgs">Event data.</param>
        private static void _OnSourceChanged(DependencyObject dependencyObject,
                                             DependencyPropertyChangedEventArgs eventArgs)
        {
            // Get bitmap.
            BitmapUIElement bitmap = (BitmapUIElement)dependencyObject;

            // Get old bitmap source value.
            BitmapSource oldBitmapSourceValue = (BitmapSource)eventArgs.OldValue;

            // Get new bitmap source value.
            BitmapSource newBitmapSourceValue = (BitmapSource)eventArgs.NewValue;

            // Unsubscribe from events for old bitmap source value.
            if (oldBitmapSourceValue != null && 
                !oldBitmapSourceValue.IsFrozen)
            {
                oldBitmapSourceValue.DownloadCompleted -= bitmap._OnBitmapSourceDownloadCompleted;
                oldBitmapSourceValue.DownloadFailed -= bitmap._OnBitmapSourceDownloadFailed;
            }

            // Subscribe to events for the new bitmap source value.
            if (newBitmapSourceValue != null && !newBitmapSourceValue.IsFrozen)
            {
                newBitmapSourceValue.DownloadCompleted += bitmap._OnBitmapSourceDownloadCompleted;
                newBitmapSourceValue.DownloadFailed += bitmap._OnBitmapSourceDownloadFailed;
            }
        }

        /// <summary>
        /// Gets transformation matrix.
        /// Matrix is used to convert a point from "above" the coordinate space of a visual
        /// into the the coordinate space "below" the visual.
        /// </summary>
        /// <param name="visual">Visual object which provides rendering support.</param>
        /// <returns>Transformation matrix.</returns>
        private static Matrix _GetVisualTransformMatrix(Visual visual)
        {
            Matrix resultMatrix;

            // If visual object is not null.
            if (visual != null)
            {
                // Get transform object from visual.
                Transform transform = VisualTreeHelper.GetTransform(visual);

                if (transform != null)
                {
                    // Get matrix which is multiplication of identity matrix and transform's matrix.
                    resultMatrix = Matrix.Multiply(Matrix.Identity, transform.Value);
                }
                else
                {
                    resultMatrix = Matrix.Identity;
                }

                // Get offset vector from visual object.
                Vector offset = VisualTreeHelper.GetOffset(visual);

                // Translate result matrix using given offset.
                resultMatrix.Translate(offset.X, offset.Y);
            }
            // Visual object is null.
            else
            {
                // Get identity matrix.
                resultMatrix = Matrix.Identity;
            }

            return resultMatrix;
        }

        /// <summary>
        /// Tries to apply transformation of given point.
        /// </summary>
        /// <param name="point">Point to be transformed.</param>
        /// <param name="visual">Visual object which provides rendering support to get transformation matrix from it.</param>
        /// <param name="inverse">Flag defines if transformation matrix should be inverted.</param>
        /// <param name="success">Output parameter indicating whether transformation was successfull or not.</param>
        /// <returns>Transformed point.</returns>
        private static Point _TryApplyVisualTransform(Point point, Visual visual, bool inverse, out bool success)
        {
            Point transformedPoint;

            if (visual != null)
            {
                // Get transformation matrix.
                Matrix visualTransformMatrix = _GetVisualTransformMatrix(visual);

                // If transformation matrix should be inverted.
                if (inverse)
                {
                    // If transformation matrix is invertible.
                    if (visualTransformMatrix.HasInverse)
                    {
                        visualTransformMatrix.Invert();
                        transformedPoint = visualTransformMatrix.Transform(point);
                        success = true;
                    }
                    // Transformation matrix can't be inverted.
                    else
                    {
                        transformedPoint = new Point(0, 0);
                        success = false;
                    }
                }
                // Transformation matrix is used without inversion.
                else
                {
                    transformedPoint = visualTransformMatrix.Transform(point);
                    success = true;
                }
            } // if (visual != null)
            // Can't apply visual transformation.
            else
            {
                transformedPoint = point;
                success = false;
            }

            return transformedPoint;
        }

        /// <summary>
        /// Applies transformation of given point.
        /// </summary>
        /// <param name="point">Point to be transformed.</param>
        /// <param name="visual">Visual object which provides rendering support to get transformation matrix from it.</param>
        /// <param name="inverse">Flag defines if transformation matrix should be inverted.</param>
        /// <returns>Transformed point.</returns>
        private static Point _ApplyVisualTransform(Point point, Visual visual, bool inverse)
        {
            bool success = false;

            Point transformedPoint = _TryApplyVisualTransform(point, visual, inverse, out success);

            Debug.Assert(success);

            return transformedPoint;
        }

        /// <summary>
        /// Checks if points are close enough (distance between points is not more than given limit).
        /// </summary>
        /// <param name="point1">First point.</param>
        /// <param name="point2">Second point.</param>
        /// <returns>True - if points are close enough, otherwise - false.</returns>
        private static bool _PointsAreCloseEnough(Point point1, Point point2)
        {
            bool result =
                _CoordinatesAreCloseEnough(point1.X, point2.X) && _CoordinatesAreCloseEnough(point1.Y, point2.Y);

            return result;
        }

        /// <summary>
        /// Checks if coordinates are close enough (difference between coordinates is not more than given limit).
        /// </summary>
        /// <param name="value1">First value.</param>
        /// <param name="value2">Second value.</param>
        /// <returns></returns>
        private static bool _CoordinatesAreCloseEnough(double value1, double value2)
        {
            double delta = Math.Abs(value1 - value2);

            bool result = delta <= MAX_COORDINATES_DEVIATION;

            return result;
        }

        #endregion Private static methods

        #region Prvate methods

        /// <summary>
        /// Invoked when event BitmapSource.DownloadCompleted is raised.
        /// Invalidates the measurement state (layout) for the element and rendering of the element.
        /// </summary>
        /// <param name="sender">Source of an event (Ignored).</param>
        /// <param name="eventArgs">Event data (ignored).</param>
        private void _OnBitmapSourceDownloadCompleted(object sender, EventArgs eventArgs)
        {
            InvalidateMeasure();
            InvalidateVisual();
        }

        /// <summary>
        /// Invoked when event BitmapSource.DownloadFailed is raised.
        /// Nulls bitmap source and raises BitmapFailed event.
        /// </summary>
        /// <param name="sender">Source of an event (Ignored).</param>
        /// <param name="eventArgs">Event data.</param>
        private void _OnBitmapSourceDownloadFailed(object sender, ExceptionEventArgs eventArgs)
        {
            Source = null;
        }

        /// <summary>
        /// Invoked when event LayoutUpdated is raised.
        /// Checks offfset of bitmap pixels and invalidates the rendering of the element
        /// if offset is more than threshold.
        /// </summary>
        /// <param name="sender">Source of an event (Ignored).</param>
        /// <param name="eventArgs">Event data (Ignored).</param>
        private void _OnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            // Get offset of bitmap pixels.
            Point pixelOffset = _GetPixelOffset();

            // If offset is more than threshold - invalidate rendering of the element.
            if (!_PointsAreCloseEnough(pixelOffset, _pixelOffset))
            {
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Gets offset of bitmap pixels.
        /// </summary>
        /// <returns>Pixels offset in X and Y direction.</returns>
        private Point _GetPixelOffset()
        {
            // Pixel offset.
            Point pixelOffset = new Point();

            // Get presentation source from visual object.
            PresentationSource presentationSource = PresentationSource.FromVisual(this);

            if (presentationSource != null)
            {
                Visual rootVisual = presentationSource.RootVisual;

                // Transform (0,0) from this element up to pixels.
                pixelOffset = TransformToAncestor(rootVisual).Transform(pixelOffset);
                pixelOffset = _ApplyVisualTransform(pixelOffset, rootVisual, false);
                pixelOffset = presentationSource.CompositionTarget.TransformToDevice.Transform(pixelOffset);

                // Round the origin to the nearest whole pixel.
                pixelOffset.X = Math.Round(pixelOffset.X);
                pixelOffset.Y = Math.Round(pixelOffset.Y);

                // Transform the whole-pixel back to this element.
                pixelOffset = presentationSource.CompositionTarget.TransformFromDevice.Transform(pixelOffset);
                pixelOffset = _ApplyVisualTransform(pixelOffset, rootVisual, true);
                pixelOffset = rootVisual.TransformToDescendant(this).Transform(pixelOffset);
            }

            return pixelOffset;
        }

        #endregion Prvate methods

        #region Private constants

        /// <summary>
        /// Maximum deviation between coordinates.
        /// </summary>
        private const double MAX_COORDINATES_DEVIATION = 1.53E-06;

        #endregion Private constants

        #region Private fields

        /// <summary>
        /// Pixel offset.
        /// </summary>
        private Point _pixelOffset;

        #endregion Private fields
    }
}

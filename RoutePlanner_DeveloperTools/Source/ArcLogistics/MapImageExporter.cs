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
using System.IO;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SysWindows = System.Windows;
using SysControls = System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;

using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.MapService;
using ESRI.ArcLogistics.DomainObjects;
using AppGeometry = ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Class that represents a map images exporter.
    /// </summary>
    internal sealed class MapImageExporter
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>MapImageExporter</c> class.
        /// </summary>
        /// <param name="showLeadingStemTime">Show leading stem time.</param>
        /// <param name="showTrailingStemTime">Show trailing stem time.</param>
        public MapImageExporter(bool showLeadingStemTime,
                                bool showTrailingStemTime)
        {
            _showLeadingStemTime = showLeadingStemTime;
            _showTrailingStemTime = showTrailingStemTime;
        }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes routine (support reinit).
        /// </summary>
        /// <param name="mapLayer">Layer for creating images.</param>
        public void Init(MapLayer mapLayer)
        {
            Debug.Assert(null != mapLayer);
            Debug.Assert(!_serviceInWorkedState); // only once

            AgsServer server = ((AgsMapLayer)mapLayer).Server;
            ServiceHelper.ValidateServerState(server);

            _mapService = new MapServiceClient(mapLayer.Url, server.OpenConnection());

            _mapInfo = _mapService.GetServerInfo(_mapService.GetDefaultMapName());
            _mapDescription = _mapInfo.DefaultMapDescription;

            var imgType = new ImageType();
            imgType.ImageFormat = esriImageFormat.esriImagePNG;
            imgType.ImageReturnType = esriImageReturnType.esriImageReturnMimeData;

            _imgDescription = new ImageDescription();
            _imgDescription.ImageType = imgType;

            _serviceInWorkedState = true;
        }

        /// <summary>
        /// Gets route image with drawn stops.
        /// </summary>
        /// <param name="route">Route for draw.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="dpi">Image DPI.</param>
        /// <returns>Created route image, or null if not inited.</returns>
        public Image GetRouteImage(Route route, int width, int height, int dpi)
        {
            Debug.Assert(null != route);
            Debug.Assert(0 < width);
            Debug.Assert(0 < height);
            Debug.Assert(0 < dpi);

            Image image = null;
            if (_serviceInWorkedState)
            {
                if (_IsCallFromSta())
                    image = _GetRouteImage(route, width, height, dpi);

                else
                {   // work with controls must be only in STA
                    var worker = new Thread(new ParameterizedThreadStart(delegate(object obj)
                    {
                        image = _GetRouteImage(route, width, height, dpi);
                    }));
                    worker.SetApartmentState(ApartmentState.STA);
                    worker.Start();
                    worker.Join();

                    // free symbol templates - next call need use new in new thread
                    _routeSymbolTemplate =
                        _sequenceSymbolTemplate =
                            _stopSymbolTemplate = null;
                }
            }

            return image;
        }

        /// <summary>
        /// Gets route's stop image.
        /// </summary>
        /// <param name="route">Route for draw.</param>
        /// <param name="stop">Stop for draw.</param>
        /// <param name="extentRadius">Radius of extent indent of stop.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="dpi">Image DPI.</param>
        /// <returns>Created route image, or null if not inited.</returns>
        public Image GetStopImage(Route route,
                                  Stop stop,
                                  double extentRadius,
                                  int width,
                                  int height,
                                  int dpi)
        {
            Debug.Assert(null != route);
            Debug.Assert(null != stop);
            Debug.Assert(0 < extentRadius);
            Debug.Assert(0 < width);
            Debug.Assert(0 < height);
            Debug.Assert(0 < dpi);

            Image image = null;

            if (_serviceInWorkedState &&
                (stop.AssociatedObject is Order || stop.AssociatedObject is Location))
            {
                if (_IsCallFromSta())
                    image = _GetStopImage(route, stop, extentRadius, height, width, dpi);

                else
                {   // work with controls must be only in STA
                    var worker = new Thread(new ParameterizedThreadStart(delegate(object obj)
                    {
                        image = _GetStopImage(route,
                                              stop,
                                              extentRadius,
                                              height,
                                              width,
                                              dpi);
                    }));
                    worker.SetApartmentState(System.Threading.ApartmentState.STA);
                    worker.Start();
                    worker.Join();

                    // free symbol templates - next call need use new in new thread
                    _routeSymbolTemplate =
                        _sequenceSymbolTemplate =
                            _stopSymbolTemplate = null;
                }
            }

            return image;
        }

        #endregion

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Checks call routine from STA.
        /// </summary>
        /// <returns>TRUE if routine apartment state is STA.</returns>
        private bool _IsCallFromSta()
        {
            return (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA);
        }

        /// <summary>
        /// Loads symbol templates.
        /// </summary>
        private void _LoadSymbolTemplates()
        {
            if (null == _routeSymbolTemplate)
            {
                _routeSymbolTemplate =
                    _LoadTemplateFromResource(RESOURCE_NAME_PREFIX + "RouteLineSymbol.xaml");
            }

            if (null == _sequenceSymbolTemplate)
            {
                _sequenceSymbolTemplate =
                    _LoadTemplateFromResource(RESOURCE_NAME_PREFIX + "LabelSequenceSymbol.xaml");
            }

            if (null == _stopSymbolTemplate)
            {
                _stopSymbolTemplate =
                    _LoadTemplateFromResource(RESOURCE_NAME_PREFIX + "LocationSymbol.xaml");
            }
        }

        /// <summary>
        /// Gets image by service.
        /// </summary>
        /// <param name="extent">Map area extent.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="dpi">Image DPI.</param>
        /// <returns>Getted image.</returns>
        private MapImage _GetImage(EnvelopeN extent, int width, int height, int dpi)
        {
            Debug.Assert(null != extent);
            Debug.Assert(0 < width);
            Debug.Assert(0 < height);
            Debug.Assert(0 < dpi);

            MapImage mapImage = null;
            try
            {
                var imgdDisplay = new ImageDisplay();
                imgdDisplay.ImageHeight = height;
                imgdDisplay.ImageWidth = width;
                imgdDisplay.ImageDPI = dpi;

                _imgDescription.ImageDisplay = imgdDisplay;
                _mapDescription.MapArea.Extent = extent;

                mapImage = _mapService.ExportMapImage(_mapDescription, _imgDescription);
            }
            catch (Exception ex)
            {
                if (ex is AuthenticationException || ex is CommunicationException)
                    _serviceInWorkedState = false;

                throw; // exception
            }

            _imgDescription.ImageDisplay = null;

            return mapImage;
        }

        /// <summary>
        /// Creates image.
        /// </summary>
        /// <param name="route">Route for draw.</param>
        /// <param name="sortedRouteStops">Sorted stops from route.</param>
        /// <param name="extent">Image extent.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="dpi">Image DPI.</param>
        /// <returns>Created image.</returns>
        private Image _CreateImage(Route route,
                                   IList<Stop> sortedRouteStops,
                                   EnvelopeN extent,
                                   int width,
                                   int height,
                                   int dpi)
        {
            Debug.Assert(null != route);
            Debug.Assert(null != extent);
            Debug.Assert(0 < width);
            Debug.Assert(0 < height);
            Debug.Assert(0 < dpi);

            _LoadSymbolTemplates();

            MapImage mapImage = _GetImage(extent, width, height, dpi);

            SysControls.Canvas canvas = _CreateRouteCanvas(route, sortedRouteStops, mapImage);
            Image image = _CreateImage(mapImage, canvas);

            return image;
        }

        /// <summary>
        /// Gets route image with drawn stops.
        /// </summary>
        /// <param name="route">Route for draw.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="dpi">Image DPI.</param>
        /// <returns>Created route image, or null if not inited.</returns>
        private Image _GetRouteImage(Route route,
                                     int width,
                                     int height,
                                     int dpi)
        {
            Debug.Assert(null != route);
            Debug.Assert(0 < width);
            Debug.Assert(0 < height);
            Debug.Assert(0 < dpi);

            IList<Stop> sortedRouteStops = CommonHelpers.GetSortedStops(route);

            EnvelopeN extent = _GetExtent(route, sortedRouteStops);
            Image image = _CreateImage(route, sortedRouteStops, extent, width, height, dpi);

            return image;
        }

        /// <summary>
        /// Gets route's stop image.
        /// </summary>
        /// <param name="route">Route for draw.</param>
        /// <param name="stop">Stop for draw.</param>
        /// <param name="extentRadius">Radius of extent indent of stop.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="dpi">Image DPI.</param>
        /// <returns>Created route image, or null if not inited.</returns>
        private Image _GetStopImage(Route route,
                                    Stop stop,
                                    double extentRadius,
                                    int height,
                                    int width,
                                    int dpi)
        {
            Debug.Assert(null != route);
            Debug.Assert(null != stop);
            Debug.Assert(0 < extentRadius);
            Debug.Assert(0 < width);
            Debug.Assert(0 < height);
            Debug.Assert(0 < dpi);

            Image image = null;

            IList<Stop> routeStops = CommonHelpers.GetSortedStops(route);

            if (((stop.AssociatedObject != routeStops[0].AssociatedObject) || _showLeadingStemTime) &&
                ((stop.AssociatedObject != routeStops[routeStops.Count - 1].AssociatedObject) || _showTrailingStemTime))
            {
                EnvelopeN extent = _GetExtent(stop, extentRadius);

                image = _CreateImage(route, routeStops, extent, width, height, dpi);
            }

            return image;
        }

        /// <summary>
        /// Creates brush by map bitmap.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private ImageBrush _CreateImageBrush(Bitmap bitmap)
        {
            Debug.Assert(null != bitmap);

            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageBrush imageBrush = null;
            try
            {
                ImageSource bitmapImage =
                    Imaging.CreateBitmapSourceFromHBitmap(hBitmap,
                                                          IntPtr.Zero,
                                                          Int32Rect.Empty,
                                                          BitmapSizeOptions.FromEmptyOptions());
                imageBrush = new ImageBrush(bitmapImage);
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return imageBrush;
        }

        /// <summary>
        /// Creates image from canvas with map background.
        /// </summary>
        /// <param name="mapImage">Map image.</param>
        /// <param name="canvas">Canvas to draw.</param>
        /// <returns>Created image.</returns>
        private Image _CreateImage(MapImage mapImage, SysControls.Canvas canvas)
        {
            Debug.Assert(null != mapImage);
            Debug.Assert(null != canvas);

            Image imageWithData = null;

            RenderTargetBitmap bmp = null;
            SysControls.Canvas outer = null;

            Image sourceImage = null;
            using (MemoryStream sourceStream = new MemoryStream((byte[])mapImage.ImageData))
                sourceImage = Image.FromStream(sourceStream);

            try
            {

                var bitmap = sourceImage as Bitmap;
                Debug.Assert(null != bitmap);
                ImageBrush imageBrush = _CreateImageBrush(bitmap);

                outer = new SysControls.Canvas();

                outer.Width = mapImage.ImageWidth;
                outer.Height = mapImage.ImageHeight;
                outer.Children.Add(canvas);
                outer.Background = (ImageBrush)imageBrush.GetCurrentValueAsFrozen();
                outer.Arrange(new Rect(0, 0, outer.Width, outer.Height));

                bmp = new RenderTargetBitmap((int)outer.Width,
                                             (int)outer.Height,
                                             mapImage.ImageDPI,
                                             mapImage.ImageDPI,
                                             PixelFormats.Pbgra32);
                bmp.Render(outer);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));

                using (MemoryStream stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    imageWithData = Image.FromStream(stream);
                }
            }
            finally
            {   // Clear and dispose all used stuff
                if (outer != null)
                {
                    outer.UpdateLayout();
                    outer.Children.Clear();
                }

                canvas.UpdateLayout();
                foreach (object child in canvas.Children)
                {
                    var symbolControl = child as SymbolControl;
                    if (symbolControl != null)
                        symbolControl.Template = null;
                }
                canvas.Children.Clear();

                if (bmp != null)
                    bmp.Clear();

                if (sourceImage != null)
                    sourceImage.Dispose();
            }

            return imageWithData;
        }

        /// <summary>
        /// Creates canvas with invalidated route and its stops.
        /// </summary>
        /// <param name="route">Route to draw.</param>
        /// <param name="sortedRouteStops">Sorted stops from route.</param>
        /// <param name="mapImage">Map image.</param>
        /// <returns>Canvas with invalidated route and its stops.</returns>
        private SysControls.Canvas _CreateRouteCanvas(Route route, IList<Stop> sortedRouteStops, MapImage mapImage)
        {
            Debug.Assert(null != route);
            Debug.Assert(null != mapImage);
            Debug.Assert(null != sortedRouteStops);

            // create canvas
            var canvas = new SysControls.Canvas();
            canvas.InvalidateVisual();
            canvas.Height = mapImage.ImageHeight;
            canvas.Width = mapImage.ImageWidth;

            // init route brush from route color
            System.Drawing.Color color = route.Color;
            var mediaColor =
                SysWindows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
            var fillingBrush = new SolidColorBrush(mediaColor);

            // create and init route image
            SymbolControl routeBox = new SymbolControl(_routeSymbolTemplate);
            routeBox.Geometry = _CreatePath(route,
                                            sortedRouteStops,
                                            mapImage.Extent,
                                            mapImage.ImageWidth,
                                            mapImage.ImageHeight);
            routeBox.Fill = (SolidColorBrush)fillingBrush.GetCurrentValueAsFrozen();
            routeBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            routeBox.VerticalAlignment = VerticalAlignment.Stretch;
            // draw route
            canvas.Children.Add(routeBox);

            // draw stops - from last to first (visual elements)
            int stopLastIndex = sortedRouteStops.Count - 1;
            for (int index = stopLastIndex; index >= 0; --index)
            {
                Stop stop = sortedRouteStops[index];
                if (!stop.MapLocation.HasValue)
                    continue; // skip empty

                bool isVisible = true;
                if (stop.AssociatedObject is Location)
                {
                    if (index == 0)
                    {
                        isVisible = _showLeadingStemTime;
                    }
                    else if (index == stopLastIndex)
                    {
                        isVisible = _showTrailingStemTime;
                    }
                    // else do nothing
                }

                if (isVisible)
                {
                    UIElement element = _CreateStopUIElement(stop, mapImage, fillingBrush);
                    if (element != null)
                        canvas.Children.Add(element);
                }
            }

            return canvas;
        }

        /// <summary>
        /// Creates UIElement for stop.
        /// </summary>
        /// <param name="stop">Stop as source.</param>
        /// <param name="mapImage">Map image.</param>
        /// <param name="fillingBrush">Filling brush.</param>
        /// <returns>Created element fro stop.</returns>
        private UIElement _CreateStopUIElement(Stop stop,
                                               MapImage mapImage,
                                               SolidColorBrush fillingBrush)
        {
            Debug.Assert(null != stop);
            Debug.Assert(null != mapImage);
            Debug.Assert(null != fillingBrush);

            UIElement element = null;
            if (stop.AssociatedObject is Order)
            {
                SysWindows.Point position =
                    _ConvertPoint(stop.MapLocation.Value,
                                  (EnvelopeN)mapImage.Extent,
                                  mapImage.ImageWidth,
                                  mapImage.ImageHeight);

                var stopSymbolBox = new SymbolControl(_sequenceSymbolTemplate);
                stopSymbolBox.Margin = new Thickness(position.X, position.Y, 0, 0);
                stopSymbolBox.SequenceNumber = stop.OrderSequenceNumber.Value.ToString();
                stopSymbolBox.Fill = fillingBrush;

                element = stopSymbolBox;
            }

            else if (stop.AssociatedObject is Location)
            {
                SysWindows.Point position =
                    _ConvertPoint(stop.MapLocation.Value,
                                  (EnvelopeN)mapImage.Extent,
                                  mapImage.ImageWidth,
                                  mapImage.ImageHeight);

                var control = new SysControls.Control();
                control.Template = _stopSymbolTemplate;
                control.Margin = new Thickness(position.X, position.Y, 0, 0);

                element = control;
            }

            // else Do nothing

            return element;
        }

        /// <summary>
        /// Loads control template from resource.
        /// </summary>
        /// <param name="name">Name of manifest resource being requested.</param>
        /// <returns>Loaded control template</returns>
        private SysControls.ControlTemplate _LoadTemplateFromResource(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            SysControls.ControlTemplate controlTemplate = null;

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                using (XmlTextReader xmlReader = new XmlTextReader(stream))
                    controlTemplate = XamlReader.Load(xmlReader) as SysControls.ControlTemplate;
           }

            return controlTemplate;
        }

        /// <summary>
        /// Creates route path geometry.
        /// </summary>
        /// <param name="route">Route to get points.</param>
        /// <param name="sortedRouteStops">Stops as points source.</param>
        /// <param name="extent">Map image extent.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <returns>Created path geometry by stop point.</returns>
        private PathGeometry _CreatePath(Route route,
                                         IList<Stop> sortedRouteStops,
                                         Envelope extent,
                                         double width,
                                         double height)
        {
            Debug.Assert(null != route);
            Debug.Assert(null != sortedRouteStops);
            Debug.Assert(null != extent);
            Debug.Assert(0 < width);
            Debug.Assert(0 < height);

            var pathFigure = new PathFigure();

            int startIndex = _GetStartIndex(route, sortedRouteStops);
            int processCount = _GetProcessStopCount(route, sortedRouteStops);

            bool isStartFound = false;
            for (int index = startIndex; index < processCount; ++index)
            {
                Stop stop = sortedRouteStops[index];
                if (!stop.MapLocation.HasValue)
                {
                    continue; // NOTE: skip empty
                }

                // not show path to first stop
                if (isStartFound &&
                    (stop.Path != null) &&
                    !stop.Path.IsEmpty)
                {
                    for (int pointsIndex = 0; pointsIndex < stop.Path.Groups.Length; ++pointsIndex)
                    {
                        AppGeometry.Point[] pointsArray =
                            stop.Path.GetGroupPoints(pointsIndex);

                        foreach (AppGeometry.Point point in pointsArray)
                        {
                            SysWindows.Point segmentPoint =
                                _ConvertPoint(point, (EnvelopeN)extent, width, height);

                            var segment = new LineSegment(segmentPoint, true);
                            pathFigure.Segments.Add(segment);
                        }
                    }
                }

                if (!isStartFound &&
                    stop.MapLocation.HasValue)
                {
                    // init figure start point
                    pathFigure.StartPoint =
                        _ConvertPoint(stop.MapLocation.Value, (EnvelopeN)extent, width, height);
                    pathFigure.IsClosed = false;

                    isStartFound = true;
                }
            }

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }

        /// <summary>
        /// Converts from map coordinates to screen.
        /// </summary>
        /// <param name="mapPoint">Point to conversion (in map coordinates).</param>
        /// <param name="extent">Map extent.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <returns>Converted point in screen coordinates.</returns>
        private SysWindows.Point _ConvertPoint(AppGeometry.Point mapPoint,
                                               EnvelopeN extent,
                                               double width,
                                               double height)
        {
            Debug.Assert(null != mapPoint);
            Debug.Assert(null != extent);
            Debug.Assert(0 < width);
            Debug.Assert(0 < height);

            AppGeometry.Point projectedPointFrom =
                WebMercatorUtil.ProjectPointToWebMercator(mapPoint, _mapInfo.SpatialReference.WKID);

            double x = width * (projectedPointFrom.X - extent.XMin) / (extent.XMax - extent.XMin);
            double y =
                height - height * (projectedPointFrom.Y - extent.YMin) / (extent.YMax - extent.YMin);

            SysWindows.Point pointTo = new SysWindows.Point(x, y);

            return pointTo;
        }

        /// <summary>
        /// Gets map extent for stop with selected radius around.
        /// </summary>
        /// <param name="stop">Stop to getting position.</param>
        /// <param name="extentRadiusInUnits">Extent radius [units].</param>
        /// <returns>Map extent for stop.</returns>
        private EnvelopeN _GetExtent(Stop stop, double extentRadiusInUnits)
        {
            Debug.Assert(null != stop);
            Debug.Assert(0 < extentRadiusInUnits);

            AppGeometry.Point point = stop.MapLocation.Value;
            double stopX = point.X;
            double stopY = point.Y;

            double extentRadius = DistCalc.GetExtentRadius(stopX, stopY, extentRadiusInUnits);

            // project extent to map spatial reference
            AppGeometry.Point leftTop =
                new AppGeometry.Point(stopX - extentRadius, stopY + extentRadius);
            AppGeometry.Point rightBottom =
                new AppGeometry.Point(stopX + extentRadius, stopY - extentRadius);

            int spatialRefId = _mapInfo.SpatialReference.WKID;
            leftTop =
                WebMercatorUtil.ProjectPointToWebMercator(leftTop, spatialRefId);
            rightBottom =
                WebMercatorUtil.ProjectPointToWebMercator(rightBottom, spatialRefId);

            // create extent in correct projection
            EnvelopeN extent = new EnvelopeN();
            extent.XMin = leftTop.X;
            extent.XMax = rightBottom.X;
            extent.YMin = rightBottom.Y;
            extent.YMax = leftTop.Y;

            return extent;
        }

        /// <summary>
        /// Gets map extent for route.
        /// </summary>
        /// <param name="route">Route to getting points.</param>
        /// <param name="sortedRouteStops">Sorted stops from route.</param>
        /// <returns>Map extent for route (all points on map).</returns>
        private EnvelopeN _GetExtent(Route route, IList<Stop> sortedRouteStops)
        {
            Debug.Assert(null != route);
            Debug.Assert(null != sortedRouteStops);

            int spatialRefId = _mapInfo.SpatialReference.WKID;

            int startIndex = _GetStartIndex(route, sortedRouteStops);
            int processCount = _GetProcessStopCount(route, sortedRouteStops);

            var points = new List<AppGeometry.Point>();
            bool isStartFound = false;
            for (int stopIndex = startIndex; stopIndex < processCount; ++stopIndex)
            {
                Stop stop = sortedRouteStops[stopIndex];

                // NOTE: path to first stop not showing
                if (isStartFound &&
                    (null != stop.Path) &&
                    !stop.Path.IsEmpty)
                {
                    for (int index = 0; index < stop.Path.Groups.Length; ++index)
                    {
                        AppGeometry.Point[] pointsArray = stop.Path.GetGroupPoints(index);
                        foreach (AppGeometry.Point point in pointsArray)
                        {
                            AppGeometry.Point pt =
                                WebMercatorUtil.ProjectPointToWebMercator(point, spatialRefId);
                            points.Add(pt);
                        }
                    }
                }

                if (stop.MapLocation.HasValue)
                {
                    AppGeometry.Point location = stop.MapLocation.Value;
                    AppGeometry.Point loc =
                        WebMercatorUtil.ProjectPointToWebMercator(location, spatialRefId);
                    points.Add(loc);

                    if (!isStartFound)
                    {
                        isStartFound = true;
                    }
                }
            }

            var rect = new AppGeometry.Envelope();
            rect.SetEmpty();
            foreach (AppGeometry.Point point in points)
                rect.Union(point);

            // increase extent
            double heightInc = ROUTE_EXTENT_INDENT * rect.Height;
            if (heightInc == 0)
                heightInc = ROUTE_EXTENT_INDENT;
            double widthInc = ROUTE_EXTENT_INDENT * rect.Width;
            if (widthInc == 0)
                widthInc = ROUTE_EXTENT_INDENT;

            rect.left -= widthInc;
            rect.right += widthInc;
            rect.top += heightInc;
            rect.bottom -= heightInc;

            var extent = new EnvelopeN();
            extent.XMax = rect.right;
            extent.XMin = rect.left;
            extent.YMax = rect.top;
            extent.YMin = rect.bottom;

            return extent;
        }

        /// <summary>
        /// Gets start stop index.
        /// Checks need process start depot.
        /// </summary>
        /// <param name="route">Route to getting points.</param>
        /// <param name="sortedRouteStops">Sorted stops from route.</param>
        /// <returns>Start stop index.</returns>
        private int _GetStartIndex(Route route, IList<Stop> sortedRouteStops)
        {
            Debug.Assert(null != route);
            Debug.Assert(null != sortedRouteStops);

            int startIndex = 0;
            if ((sortedRouteStops[startIndex].AssociatedObject is Location) &&
                !_showLeadingStemTime)
            {
                ++startIndex;
            }

            return startIndex;
        }

        /// <summary>
        /// Gets process stop count.
        /// Checks need process end depot.
        /// </summary>
        /// <param name="route">Route to getting points.</param>
        /// <param name="sortedRouteStops">Sorted stops from route.</param>
        /// <returns>Process stop count.</returns>
        private int _GetProcessStopCount(Route route, IList<Stop> sortedRouteStops)
        {
            Debug.Assert(null != route);
            Debug.Assert(null != sortedRouteStops);

            int processCount = sortedRouteStops.Count;
            if ((sortedRouteStops[processCount - 1].AssociatedObject is Location) &&
                !_showTrailingStemTime)
            {
                --processCount;
            }

            return processCount;
        }

        #endregion // Private methods

        #region Extern methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        #endregion // Extern methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Indent for route extent.
        /// </summary>
        private const double ROUTE_EXTENT_INDENT = 0.05f;

        /// <summary>
        /// Resource name prefix.
        /// </summary>
        private const string RESOURCE_NAME_PREFIX = "ESRI.ArcLogistics.Resources.";

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Map service client.
        /// </summary>
        private MapServiceClient _mapService;
        /// <summary>
        /// Map server info.
        /// </summary>
        private MapServerInfo _mapInfo;
        /// <summary>
        /// Map description.
        /// </summary>
        private MapDescription _mapDescription;
        /// <summary>
        /// Image description.
        /// </summary>
        private ImageDescription _imgDescription;

        /// <summary>
        /// Route line symbol template.
        /// </summary>
        private SysControls.ControlTemplate _routeSymbolTemplate;
        /// <summary>
        /// Label sequrnce symbol template.
        /// </summary>
        private SysControls.ControlTemplate _sequenceSymbolTemplate;
        /// <summary>
        /// Stop symbol yemplate.
        /// </summary>
        private SysControls.ControlTemplate _stopSymbolTemplate;

        /// <summary>
        /// Is service in worked state flag.
        /// </summary>
        private bool _serviceInWorkedState;

        /// <summary>
        /// Show leading stem time.
        /// </summary>
        private bool _showLeadingStemTime;
        /// <summary>
        /// Show trailing stem time.
        /// </summary>
        private bool _showTrailingStemTime;

        #endregion // Private members
    }
}

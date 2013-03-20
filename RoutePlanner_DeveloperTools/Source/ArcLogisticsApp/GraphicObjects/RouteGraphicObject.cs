using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media;
using ESRI.ArcLogistics.App.OrderSymbology;
using ESRI.ArcLogistics.App.Symbols;
using ESRI.ArcLogistics.DomainObjects;

using ArcGISGeometry = ESRI.ArcGIS.Client.Geometry;
using ArcLogisticsGeometry = ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.GraphicObjects
{
    /// <summary>
    /// Graphic object for showing routes
    /// </summary> 
    class RouteGraphicObject : DataGraphicObject
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="route">Route to show.</param>
        private RouteGraphicObject(Route route)
            : base(route)
        {
            _route = route;

            _InitEventHandlers();

            _CreateAndSetPolyline();
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Create graphic object for route.
        /// </summary>
        /// <param name="route">Source route.</param>
        /// <returns>Graphic object for route.</returns>
        public static RouteGraphicObject Create(Route route)
        {
            RouteGraphicObject graphic = null;

            // Calculate route color.
            System.Drawing.Color oldColor = route.Color;
            Color color = Color.FromArgb(DefaultAlphaValue, oldColor.R, oldColor.G, oldColor.B);

            // Create route symbol.
            RouteLineSymbol simpleLineSymbol = new RouteLineSymbol();

            graphic = new RouteGraphicObject(route);

            graphic.Attributes.Add(SymbologyContext.IS_LOCKED_ATTRIBUTE_NAME, route.IsLocked);
            graphic.Attributes.Add(SymbologyContext.FILL_ATTRIBUTE_NAME, new SolidColorBrush(color));
            graphic.Symbol = simpleLineSymbol;
            graphic._SetPolyline();

            return graphic;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Unsubscribe from all events.
        /// </summary>
        public override void UnsubscribeOnChange()
        {
            _route.PropertyChanged -= new PropertyChangedEventHandler(_RoutePropertyChanged);

            App.Current.MapDisplay.TrueRouteChanged -= new EventHandler(_MapDisplayTrueRouteChanged);
            App.Current.MapDisplay.ShowLeadingStemTimeChanged -= new EventHandler(_MapDisplayShowStemTimeChanged);
            App.Current.MapDisplay.ShowTrailingStemTimeChanged -= new EventHandler(_MapDisplayShowStemTimeChanged);
        }

        /// <summary>
        /// Project geometry to map spatial reference
        /// </summary>
        public override void ProjectGeometry()
        {
            _CreatePolyline(_route);
            _SetPolyline();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets start stop index.
        /// Checks need process start depot.
        /// </summary>
        /// <param name="sortedRouteStops">Sorted stops from route.</param>
        /// <returns>Start stop index.</returns>
        private int _GetStartIndex(IList<Stop> sortedRouteStops)
        {
            Debug.Assert(null != sortedRouteStops);

            int startIndex = 0;
            if ((sortedRouteStops[startIndex].AssociatedObject is Location) &&
                !App.Current.MapDisplay.ShowLeadingStemTime)
            {
                ++startIndex;
            }

            return startIndex;
        }

        /// <summary>
        /// Gets process stop count.
        /// Checks need process end depot.
        /// </summary>
        /// <param name="sortedRouteStops">Sorted stops from route.</param>
        /// <returns>Process stop count.</returns>
        private int _GetProcessStopCount(IList<Stop> sortedRouteStops)
        {
            Debug.Assert(null != sortedRouteStops);

            int processCount = sortedRouteStops.Count;
            if ((sortedRouteStops[processCount - 1].AssociatedObject is Location) &&
                !App.Current.MapDisplay.ShowTrailingStemTime)
            {
                --processCount;
            }

            return processCount;
        }

        /// <summary>
        /// Creates all common event handlers.
        /// </summary>
        private void _InitEventHandlers()
        {
            _route.PropertyChanged += new PropertyChangedEventHandler(_RoutePropertyChanged);

            App.Current.MapDisplay.TrueRouteChanged += new EventHandler(_MapDisplayTrueRouteChanged);
            App.Current.MapDisplay.ShowLeadingStemTimeChanged += new EventHandler(_MapDisplayShowStemTimeChanged);
            App.Current.MapDisplay.ShowTrailingStemTimeChanged += new EventHandler(_MapDisplayShowStemTimeChanged);
        }

        /// <summary>
        /// Create polyline of route and set it to geometry if visible.
        /// </summary>
        private void _CreateAndSetPolyline()
        {
            _CreatePolyline(_route);
            _SetPolyline();
        }

        /// <summary>
        /// Create needed polyline.
        /// </summary>
        /// <param name="route">Route.</param>
        private void _CreatePolyline(Route route)
        {
            if (route.Stops.Count != 0)
            {
                // Create True route or straight polyline depends on map display settings.
                if (App.Current.MapDisplay.TrueRoute)
                {
                    _routeLine = _CreateTruePolyline(route);
                }
                else
                {
                    _routeLine = _CreateStraightPolyline(route);
                }
            }
        }

        /// <summary>
        /// Create true line geometry.
        /// </summary>
        /// <param name="route">Route.</param>
        /// <returns>True polyline route geometry.</returns>
        private ArcGISGeometry.Polyline _CreateTruePolyline(Route route)
        {
            // Create follow street route.
            ArcGISGeometry.PointCollection pointCollection = new ArcGISGeometry.PointCollection();

            IList<Stop> routeStops = CommonHelpers.GetSortedStops(route);

            int startIndex = _GetStartIndex(routeStops);
            int processCount = _GetProcessStopCount(routeStops);

            // add path to stop to drawing
            bool isStartFound = false;
            for (int stopIndex = startIndex; stopIndex < processCount; ++stopIndex)
            {
                Stop stop = routeStops[stopIndex];

                // not show path to first stop
                if (isStartFound &&
                    stop.Path != null &&
                    !stop.Path.IsEmpty)
                {
                    for (int index = 0; index < stop.Path.Groups.Length; ++index)
                    {
                        ArcLogisticsGeometry.Point[] points = stop.Path.GetGroupPoints(index);
                        foreach (ArcLogisticsGeometry.Point point in points)
                        {
                            pointCollection.Add(_CreateProjectedMapPoint(point));
                        }
                    }
                }

                if (!isStartFound)
                {
                    isStartFound = (stop.StopType != StopType.Lunch);
                }
            }

            ArcGISGeometry.Polyline routeLine;
            if (pointCollection.Count > 0)
            {
                routeLine = new ArcGISGeometry.Polyline();
                routeLine.Paths.Add(pointCollection);
            }
            else
            {
                routeLine = _CreateStraightPolyline(route);
            }

            return routeLine;
        }

        /// <summary>
        /// Create map point and project to webmercator if needed.
        /// </summary>
        /// <param name="point">Source point.</param>
        /// <returns>Map point in correct projection.</returns>
        private ArcGISGeometry.MapPoint _CreateProjectedMapPoint(ArcLogisticsGeometry.Point point)
        {
            ArcLogisticsGeometry.Point projectedPoint = new ArcLogisticsGeometry.Point(point.X, point.Y);
            
            if (ParentLayer != null && ParentLayer.SpatialReferenceID != null)
            {
                projectedPoint = WebMercatorUtil.ProjectPointToWebMercator(projectedPoint, ParentLayer.SpatialReferenceID.Value);
            }

            ArcGISGeometry.MapPoint mapPoint = new ArcGISGeometry.MapPoint(projectedPoint.X, projectedPoint.Y);
            return mapPoint;
        }

        /// <summary>
        /// Create straight line geometry.
        /// </summary>
        /// <param name="route">Route.</param>
        /// <returns>Straight line route geometry.</returns>
        private ArcGISGeometry.Polyline _CreateStraightPolyline(Route route)
        {
            ArcGISGeometry.Polyline routeLine = new ArcGISGeometry.Polyline();
            ArcGISGeometry.PointCollection pointCollection = new ArcGISGeometry.PointCollection();

            List<Stop> routeStops = CommonHelpers.GetSortedStops(route);

            int startIndex = _GetStartIndex(routeStops);
            int processCount = _GetProcessStopCount(routeStops);

            // add stop map points
            for (int index = startIndex; index < processCount; ++index)
            {
                Stop stop = routeStops[index];
                if (stop.MapLocation != null)
                {
                    ArcGISGeometry.MapPoint mapPoint = _CreateProjectedMapPoint(stop.MapLocation.Value);
                    pointCollection.Add(mapPoint);
                }
            }

            routeLine.Paths.Add(pointCollection);

            return routeLine;
        }

        /// <summary>
        /// Set correct geometry depends on graphic visibility.
        /// </summary>
        private void _SetPolyline()
        {
            IsVisible = _route.IsVisible;
            Geometry = _routeLine;
        }
        
        /// <summary>
        /// React on true route setting changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapDisplayTrueRouteChanged(object sender, EventArgs e)
        {
            _CreateAndSetPolyline();
        }

        /// <summary>
        /// React on leading\trailing option changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapDisplayShowStemTimeChanged(object sender, EventArgs e)
        {
            _CreateAndSetPolyline();
        }

        /// <summary>
        /// React on route property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Route property changed event args.</param>
        private void _RoutePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Route.PropertyNameColor)
            {
                Color color = Color.FromArgb(DefaultAlphaValue, _route.Color.R, _route.Color.G, _route.Color.B);
                Attributes[SymbologyContext.FILL_ATTRIBUTE_NAME] = new SolidColorBrush(color);
                RouteLineSymbol simpleLineSymbol = new RouteLineSymbol();
                Symbol = simpleLineSymbol;
            }
            else if (e.PropertyName == Route.PropertyNameIsVisible)
            {
                _SetPolyline();
            }
            else if (e.PropertyName.Equals(Route.PropertyNameIsLocked, StringComparison.OrdinalIgnoreCase))
            {
                Attributes[SymbologyContext.IS_LOCKED_ATTRIBUTE_NAME] = _route.IsLocked;
                Symbol = new RouteLineSymbol();
            }
        }

        #endregion

        #region Private static fields

        /// <summary>
        /// Default alpha value of route graphic.
        /// </summary>
        private static byte DefaultAlphaValue = (byte)App.Current.FindResource("RouteAlphaValue");

        #endregion
        
        #region Private fields

        /// <summary>
        /// Route, which this graphic is shows.
        /// </summary>
        private Route _route;

        /// <summary>
        /// Route geometry to show.
        /// </summary>
        private ArcGISGeometry.Polyline _routeLine;

        #endregion
    }
}

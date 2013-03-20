using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.App.OrderSymbology;
using ESRI.ArcLogistics.App.Symbols;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.GraphicObjects
{
    /// <summary>
    /// Graphic object for showing zones.
    /// </summary>
    class ZoneGraphicObject : DataGraphicObject
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="zone">Zone to show.</param>
        private ZoneGraphicObject(Zone zone)
            : base(zone)
        {
            _zone = zone;
            _zone.PropertyChanged += new PropertyChangedEventHandler(_ZonePropertyChanged);

            ESRI.ArcGIS.Client.Geometry.Geometry geometry = _CreateGeometry(zone);
            Geometry = geometry;

            Attributes.Add(SymbologyContext.FILL_ATTRIBUTE_NAME, null);

            _CreateSymbol();
        }

        #endregion Constructors

        #region Public static methods

        /// <summary>
        /// Create graphic object for zone.
        /// </summary>
        /// <param name="zone">Source zone.</param>
        /// <returns>Graphic object for zone.</returns>
        public static ZoneGraphicObject Create(Zone zone)
        {
            ZoneGraphicObject graphic = null;

            graphic = new ZoneGraphicObject(zone);

            return graphic;
        }

        #endregion

        #region Public members

        /// <summary>
        /// Object, depending on whose properties graphics changes their view.
        /// </summary>
        public override object ObjectContext
        {
            get
            {
                return _schedule;
            }
            set
            {
                if (_schedule != null)
                    _schedule.Routes.CollectionChanged -= new NotifyCollectionChangedEventHandler(_RoutesCollectionChanged);

                if (value != null)
                {
                    if (!(value is Schedule))
                        throw new ArgumentException();

                    _schedule = value as Schedule;
                }
                else
                {
                    _schedule = null;
                }

                if (_schedule != null)
                    _schedule.Routes.CollectionChanged += new NotifyCollectionChangedEventHandler(_RoutesCollectionChanged);

                _ReactOnScheduleChanged();
            }
        }

        #endregion Public members

        #region Public methods

        /// <summary>
        /// Unsubscribe from all events.
        /// </summary>
        public override void UnsubscribeOnChange()
        {
            _zone.PropertyChanged -= new PropertyChangedEventHandler(_ZonePropertyChanged);

            // unsubscribe from routes and schedule events
            ObjectContext = null;
        }

        /// <summary>
        /// Project geometry to map spatial reference.
        /// </summary>
        public override void ProjectGeometry()
        {
            Geometry = _CreateGeometry(_zone);
        }

        #endregion Public methods

        #region Private methods

        /// <summary>
        /// Create zone geometry.
        /// </summary>
        /// <param name="zone">Zone for creating geometry.</param>
        /// <returns>Created geometry.</returns>
        private ESRI.ArcGIS.Client.Geometry.Geometry _CreateGeometry(Zone zone)
        {
            ESRI.ArcGIS.Client.Geometry.Geometry geometry = null;

            if (zone.Geometry != null)
            {
                if (zone.Geometry is ESRI.ArcLogistics.Geometry.Point?)
                {
                    ESRI.ArcLogistics.Geometry.Point? point = zone.Geometry as ESRI.ArcLogistics.Geometry.Point?;
                    geometry = new ESRI.ArcGIS.Client.Geometry.MapPoint(point.Value.X, point.Value.Y);
                }
                else
                {
                    int? spatialReference = null;
                    if (ParentLayer != null)
                    {
                        spatialReference = ParentLayer.SpatialReferenceID;
                    }

                    geometry = MapHelpers.ConvertToArcGISPolygon(
                        zone.Geometry as ESRI.ArcLogistics.Geometry.Polygon, spatialReference);
                }
            }
            else
            {
                geometry = null;
            }

            return geometry;
        }

        /// <summary>
        /// React on zone property changes.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _ZonePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Zone.PropertyNameGeometry)
            {
                // If zone position changed - show zone in new place.
                Geometry = _CreateGeometry(_zone);

                _CreateSymbol();
            }
        }

        /// <summary>
        /// Create graphic symbol.
        /// </summary>
        private void _CreateSymbol()
        {
            Brush brush;

            if (_multiRoute)
            {
                // Create brush for case of zone assigned to more than one route.
                brush = (Brush)Application.Current.FindResource("GrayColor");
            }
            else
            {
                SolidColorBrush defaultBrush = (SolidColorBrush)Application.Current.FindResource("ZonePolylineFillColor");

                if (_assignedRoute != null)
                {
                    // Create brush for case of zone assigned to one route.
                    Color color = System.Windows.Media.Color.FromArgb(defaultBrush.Color.A,
                        _assignedRoute.Color.R, _assignedRoute.Color.G, _assignedRoute.Color.B);
                    SolidColorBrush solidColorBrush = new SolidColorBrush(color);
                    brush = solidColorBrush;
                }
                else
                {
                    // Create brush for case of zone not assigned to routes.
                    brush = defaultBrush;
                }
            }

            Attributes[SymbologyContext.FILL_ATTRIBUTE_NAME] = brush;
            Symbol = new ZonePolygonSymbol();
        }

        /// <summary>
        /// Subscribe to new schedule events and unsubscribe from old schedule events.
        /// </summary>
        private void _ReactOnScheduleChanged()
        {
            while (_routes.Count > 0)
            {
                _DeleteRouteFromInnerCollection(_routes[0]);
            }

            _routes = new List<Route>();

            if (_schedule != null)
            {
                foreach (Route route in _schedule.Routes)
                {
                    _AddRouteToInnerCollection(route);
                }

                _SetAssignedRoute();
                _CreateSymbol();
            }
        }

        /// <summary>
        /// Check routes, assigned to zone.
        /// </summary>
        private void _SetAssignedRoute()
        {
            _assignedRoute = null;
            _multiRoute = false;

            foreach (Route route in _schedule.Routes)
            {
                if (route.Zones.Contains(_zone))
                {
                    if (_assignedRoute == null)
                    {
                        _assignedRoute = route;
                    }
                    else
                    {
                        _multiRoute = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Add route to inner collection and subscribe to route property changed.
        /// </summary>
        /// <param name="route">Route to add.</param>
        private void _AddRouteToInnerCollection(Route route)
        {
            _routes.Add(route);
            route.PropertyChanged += new PropertyChangedEventHandler(_RoutePropertyChanged);
        }

        /// <summary>
        /// Remove route from inner collection and unsubscribe from route property changed.
        /// </summary>
        /// <param name="route">Route to remove.</param>
        private void _DeleteRouteFromInnerCollection(Route route)
        {
            if (_routes.Contains(route))
            {
                _routes.Remove(route);
                route.PropertyChanged -= new PropertyChangedEventHandler(_RoutePropertyChanged);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        /// <summary>
        /// React on route property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _RoutePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(Route.PropertyNameColor) && _assignedRoute != null)
            {
                _CreateSymbol();
            }
            else if (e.PropertyName.Equals(Route.PropertyNameZonesCollection))
            {
                _SetAssignedRoute();
                _CreateSymbol();
            }
        }

        /// <summary>
        /// React on changes in schedule routes collection.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _RoutesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (Route route in e.NewItems)
                        {
                            _AddRouteToInnerCollection(route);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Route route in e.OldItems)
                        {
                            _DeleteRouteFromInnerCollection(route);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        foreach (Route route in _routes)
                        {
                            _DeleteRouteFromInnerCollection(route);
                        }
                        break;
                    }
                default:
                    throw new NotSupportedException();
            }

            // Change zones color, according to new routes collection.
            _SetAssignedRoute();
            _CreateSymbol();
        }

        #endregion Private methods

        #region Private members

        /// <summary>
        /// Object, assigned to this graphic.
        /// </summary>
        private Zone _zone;

        /// <summary>
        /// Schedule to get routes, to which zone can be assigned.
        /// </summary>
        private Schedule _schedule;

        /// <summary>
        /// Inner routes collection to subscribe on color and zones changing.
        /// </summary>
        private List<Route> _routes = new List<Route>();

        /// <summary>
        /// Is zone assigned to more than one route.
        /// </summary>
        private bool _multiRoute;

        /// <summary>
        /// Route to which zone assigned in case of this zone assigned to only one route. Otherwise null.
        /// </summary>
        private Route _assignedRoute;

        #endregion Private members
    }
}

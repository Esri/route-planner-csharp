using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using ESRI.ArcLogistics.App.OrderSymbology;
using ESRI.ArcLogistics.App.Symbols;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.GraphicObjects
{
    /// <summary>
    /// Graphic object for showing stops.
    /// </summary>
    class StopGraphicObject : DataGraphicObject
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stop">Source stop for graphic object.</param>
        private StopGraphicObject(Stop stop)
            : base(stop)
        {
            _stop = stop;
            _route = stop.Route;

            Geometry = _CreatePoint(stop);

            IsVisible = stop.Route.IsVisible;

            stop.PropertyChanged += new PropertyChangedEventHandler(_StopPropertyChanged);

            if (stop.AssociatedObject is Order)
            {
                Order order = (Order)stop.AssociatedObject;
                order.PropertyChanged += new PropertyChangedEventHandler(_StopAssociatedObjectPropertyChanged);

                _SetOrderAttributes(stop);
                _CreateOrderSymbol(stop);
                App.Current.MapDisplay.LabelingChanged += new EventHandler(_MapDisplayLabelingChanged);
            }
            else if (stop.AssociatedObject is Location)
            {
                Location location = (Location)stop.AssociatedObject;
                location.PropertyChanged += new PropertyChangedEventHandler(_StopAssociatedObjectPropertyChanged);

                App.Current.MapDisplay.ShowLeadingStemTimeChanged += new EventHandler(_MapDisplayShowStemTimeChanged);
                App.Current.MapDisplay.ShowTrailingStemTimeChanged += new EventHandler(_MapDisplayShowStemTimeChanged);

                Symbol = new LocationSymbol();
                IsVisible = _IsLocationVisible();
            }
            else
                Debug.Assert(false);

            stop.Route.PropertyChanged += new PropertyChangedEventHandler(_StopRoutePropertyChanged);
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Create graphic object for order.
        /// </summary>
        /// <param name="stop">Source order.</param>
        /// <returns>Graphic object for stop.</returns>
        public static StopGraphicObject Create(Stop stop)
        {
            StopGraphicObject graphic = new StopGraphicObject(stop);
            return graphic;
        }

        /// <summary>
        /// Unsubscribe graphic from all events.
        /// </summary>
        public override void UnsubscribeOnChange()
        {
            _stop.PropertyChanged -= new PropertyChangedEventHandler(_StopPropertyChanged);

            if (_stop.AssociatedObject is Order)
            {
                Order order = (Order) _stop.AssociatedObject;
                order.PropertyChanged -= new PropertyChangedEventHandler(_StopAssociatedObjectPropertyChanged);
                App.Current.MapDisplay.LabelingChanged -= new EventHandler(_MapDisplayLabelingChanged);
            }
            else if (_stop.AssociatedObject is Location)
            {
                Location location = (Location) _stop.AssociatedObject;
                location.PropertyChanged -= new PropertyChangedEventHandler(_StopAssociatedObjectPropertyChanged);
                App.Current.MapDisplay.ShowLeadingStemTimeChanged -= new EventHandler(_MapDisplayShowStemTimeChanged);
                App.Current.MapDisplay.ShowTrailingStemTimeChanged -= new EventHandler(_MapDisplayShowStemTimeChanged);
            }

            _route.PropertyChanged -= new PropertyChangedEventHandler(_StopRoutePropertyChanged);
        }

        /// <summary>
        /// Process order symbology logic.
        /// </summary>
        public void InitSymbology()
        {
            if (_stop.AssociatedObject is Order)
            {
                _CreateOrderSymbol(_stop);
            }
        }

        /// <summary>
        /// Project geometry to map spatial reference.
        /// </summary>
        public override void ProjectGeometry()
        {
            Geometry = _CreatePoint(_stop);
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// Create stop point.
        /// </summary>
        /// <param name="stop">Stop, from which create geolocation point.</param>
        /// <returns>Geolocation point.</returns>
        private ESRI.ArcGIS.Client.Geometry.MapPoint _CreatePoint(Stop stop)
        {
            ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = null;
            // if order geocoded - create point
            if (stop.MapLocation != null)
            {
                ESRI.ArcLogistics.Geometry.Point geoLocation = stop.MapLocation.Value;

                // Project point from WGS84 to Web Mercator if spatial reference of map is Web Mercator.
                if (ParentLayer != null && ParentLayer.SpatialReferenceID != null)
                {
                    geoLocation = WebMercatorUtil.ProjectPointToWebMercator(geoLocation, ParentLayer.SpatialReferenceID.Value);
                }

                mapPoint = new ESRI.ArcGIS.Client.Geometry.MapPoint(geoLocation.X, geoLocation.Y);
            }
            else
            {
                mapPoint = null;
            }

            return mapPoint;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Set orders attributes to graphic.
        /// </summary>
        /// <param name="stop">Stop, associated with order.</param> 
        private void _SetOrderAttributes(Stop stop)
        {
            Order order = (Order)stop.AssociatedObject;

            Route route = stop.Route;
            Color color = Color.FromRgb(route.Color.R, route.Color.G, route.Color.B);
            SolidColorBrush solidColorBrush = new SolidColorBrush(color);
            Attributes.Add(SymbologyContext.FILL_ATTRIBUTE_NAME, solidColorBrush);
            Attributes.Add(SymbologyContext.OFFSETX_ATTRIBUTE_NAME,
                -(SymbologyManager.DEFAULT_SIZE - SymbologyManager.DEFAULT_INDENT / 2));
            Attributes.Add(SymbologyContext.IS_VIOLATED_ATTRIBUTE_NAME, stop.IsViolated);
            Attributes.Add(SymbologyContext.IS_LOCKED_ATTRIBUTE_NAME, stop.IsLocked);
            Attributes.Add(SymbologyContext.OFFSETY_ATTRIBUTE_NAME,
                -(SymbologyManager.DEFAULT_SIZE - SymbologyManager.DEFAULT_INDENT / 2));
            Attributes.Add(SymbologyContext.SIZE_ATTRIBUTE_NAME, SymbologyManager.DEFAULT_SIZE);
            Attributes.Add(SymbologyContext.FULLSIZE_ATTRIBUTE_NAME,
                SymbologyManager.DEFAULT_SIZE + SymbologyManager.DEFAULT_INDENT);

            string sequenceNumber = stop.OrderSequenceNumber.ToString();
            Attributes.Add(SymbologyContext.SEQUENCE_NUMBER_ATTRIBUTE_NAME, sequenceNumber);
        }

        /// <summary>
        /// Create order symbols.
        /// </summary>
        /// <param name="stop">Stop, associated with order.</param>
        private void _CreateOrderSymbol(Stop stop)
        {
            if (stop.AssociatedObject is Order)
            {
                if (App.Current.MapDisplay.LabelingEnabled)
                {
                    Color color = Color.FromRgb(_stop.Route.Color.R, _stop.Route.Color.G, _stop.Route.Color.B);
                    SolidColorBrush solidColorBrush = new SolidColorBrush(color);
                    Attributes[SymbologyContext.FILL_ATTRIBUTE_NAME] = solidColorBrush;
                    Symbol = new LabelSequenceSymbol();
                }
                else
                    SymbologyManager.InitGraphic(this);
            }
        }

        /// <summary>
        /// React on labeling setting changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapDisplayLabelingChanged(object sender, EventArgs e)
        {
            _CreateOrderSymbol(_stop);
        }

        /// <summary>
        /// React on leading\trailing option changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapDisplayShowStemTimeChanged(object sender, EventArgs e)
        {
            if (_stop.Route == null)
                return;

            IsVisible = _IsLocationVisible();
        }

        /// <summary>
        /// Is location visible.
        /// </summary>
        /// <returns>Is location visible.</returns>
        private bool _IsLocationVisible()
        {
            bool isVisible = false;

            Route route = _stop.Route;
            if (route.IsVisible &&
                (null != route.Stops))
            {
                isVisible = true;
            }

            return isVisible;
        }

        /// <summary>
        /// React on stop property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _StopPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(Stop.PropertyNameIsLocked, StringComparison.OrdinalIgnoreCase))
            {
                Attributes[SymbologyContext.IS_LOCKED_ATTRIBUTE_NAME] = _stop.IsLocked;
                if (_stop.AssociatedObject is Order)
                {
                    _CreateOrderSymbol(_stop);
                }
            }
        }

        /// <summary>
        /// React on route of stop property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _StopRoutePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_stop.Route == null)
                return;

            if (e.PropertyName.Equals(Route.PropertyNameIsVisible))
            {
                if (_stop.AssociatedObject is Order)
                {
                    IsVisible = _stop.Route.IsVisible;
                }
                else if (_stop.AssociatedObject is Location)
                {
                     // in case of location we should check visibility of leading\trailing
                     IsVisible = _IsLocationVisible() && _stop.Route.IsVisible;
                }
            }
            else if (e.PropertyName.Equals(Route.PropertyNameColor))
            {
                _CreateOrderSymbol(_stop);
            }
        }

        /// <summary>
        /// React on order property changes.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _StopAssociatedObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Order.PropertyNameGeoLocation)
            {
                // if order geocoded position changed - show order in new place
                Geometry = _CreatePoint(_stop);
            }

            Order order = _stop.AssociatedObject as Order;
            if (order != null)
            {
                if (e.PropertyName.Equals(SymbologyManager.FieldName) && !App.Current.MapDisplay.LabelingEnabled)
                {
                    SymbologyManager.InitGraphic(this);
                }
            }
        }

        #endregion

        #region Private members

        /// <summary>
        /// Source stop for graphic object.
        /// </summary>
        private Stop _stop;

        /// <summary>
        /// Parent of source stop for graphic object.
        /// </summary>
        private Route _route;

        #endregion private members
    }
}

using System.ComponentModel;
using System.Windows.Media;
using ESRI.ArcLogistics.App.OrderSymbology;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.GraphicObjects
{
    /// <summary>
    /// Graphic object for showing orders
    /// </summary>
    class OrderGraphicObject : DataGraphicObject
    {        
        #region constructors

        private OrderGraphicObject(Order order)
            : base(order)
        {
            _order = order;
            _order.PropertyChanged += new PropertyChangedEventHandler(_order_PropertyChanged);

            Geometry = _CreatePoint(order);
        }

        #endregion constructors

        #region public methods

        /// <summary>
        /// Create graphic object for order
        /// </summary>
        /// <param name="order">Source order</param>
        /// <returns>Graphic object for order</returns>
        public static OrderGraphicObject Create(Order order)
        {
            OrderGraphicObject graphic = null;

            graphic = new OrderGraphicObject(order);

            Color color = Color.FromRgb(0, 0, 0);
            graphic.Attributes.Add(SymbologyContext.FILL_ATTRIBUTE_NAME, new SolidColorBrush(color));
            graphic.Attributes.Add(SymbologyContext.OFFSETX_ATTRIBUTE_NAME,
                -(SymbologyManager.DEFAULT_SIZE - SymbologyManager.DEFAULT_INDENT / 2));
            graphic.Attributes.Add(SymbologyContext.OFFSETY_ATTRIBUTE_NAME,
                -(SymbologyManager.DEFAULT_SIZE - SymbologyManager.DEFAULT_INDENT / 2));
            graphic.Attributes.Add(SymbologyContext.SIZE_ATTRIBUTE_NAME, SymbologyManager.DEFAULT_SIZE);
            graphic.Attributes.Add(SymbologyContext.FULLSIZE_ATTRIBUTE_NAME, 
                SymbologyManager.DEFAULT_SIZE + SymbologyManager.DEFAULT_INDENT);

            SymbologyManager.InitGraphic(graphic);

            return graphic;
        }

        /// <summary>
        /// Unsubscribe from all events
        /// </summary>
        public override void UnsubscribeOnChange()
        {
            _order.PropertyChanged -= new PropertyChangedEventHandler(_order_PropertyChanged);
        }

        /// <summary>
        /// Project geometry to map spatial reference
        /// </summary>
        public override void ProjectGeometry()
        {
            Geometry = _CreatePoint(_order);
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// Create map point for order
        /// </summary>
        /// <param name="order">Order to get geometry</param>
        /// <returns>Map point of order position</returns>
        private ESRI.ArcGIS.Client.Geometry.MapPoint _CreatePoint(Order order)
        {
            ESRI.ArcGIS.Client.Geometry.MapPoint point = null;

            // if order geocoded - create point
            if (order.GeoLocation.HasValue)
            {
                ESRI.ArcLogistics.Geometry.Point geoLocation = order.GeoLocation.Value;

                // Project point from WGS84 to Web Mercator if spatial reference of map is Web Mercator
                if (ParentLayer != null && ParentLayer.SpatialReferenceID != null)
                {
                    geoLocation = WebMercatorUtil.ProjectPointToWebMercator(geoLocation, ParentLayer.SpatialReferenceID.Value);
                }

                point = new ESRI.ArcGIS.Client.Geometry.MapPoint(geoLocation.X, geoLocation.Y);
            }
            else
            {
                point = null;
            }

            return point;
        }

        /// <summary>
        /// React on order property changes
        /// </summary>
        private void _order_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Order.PropertyNameGeoLocation)
            {
                // if order geocoded position changed - show order in new place
                Geometry = _CreatePoint(_order);
            }

            if (e.PropertyName.Equals(SymbologyManager.FieldName))
                SymbologyManager.InitGraphic(this);
        }

        #endregion private methods

        #region private members

        // Order, which this graphic is shows
        private Order _order;

        #endregion private members
    }
}

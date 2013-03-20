using System.ComponentModel;
using ESRI.ArcLogistics.App.Symbols;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.App.GraphicObjects
{
    /// <summary>
    /// Graphic object for showing locations.
    /// </summary>
    class LocationGraphicObject : DataGraphicObject
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="location">Location object to show.</param>
        private LocationGraphicObject(Location location)
            : base(location)
        {
            _location = location;
            _location.PropertyChanged += new PropertyChangedEventHandler(_LocationPropertyChanged);

            Geometry = _CreatePoint(location);

            Symbol = new LocationSymbol();
        }

        #endregion constructors

        #region Public methods

        /// <summary>
        /// Create graphic object for location.
        /// </summary>
        /// <param name="location">Source location.</param>
        /// <returns>Graphic object for location.</returns>
        public static LocationGraphicObject Create(Location location)
        {
            LocationGraphicObject graphic = new LocationGraphicObject(location);

            return graphic;
        }

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        public override void UnsubscribeOnChange()
        {
            _location.PropertyChanged -= new PropertyChangedEventHandler(_LocationPropertyChanged);
        }

        /// <summary>
        /// Project geometry to map spatial reference.
        /// </summary>
        public override void ProjectGeometry()
        {
            Geometry = _CreatePoint(_location);
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// Create location point geometry.
        /// </summary>
        /// <param name="location">Location.</param>
        /// <returns>Location geometry.</returns>
        private ESRI.ArcGIS.Client.Geometry.MapPoint _CreatePoint(Location location)
        {
            ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = null;

            // If location geocoded - create point.
            if (location.GeoLocation.HasValue)
            {
                ESRI.ArcLogistics.Geometry.Point geoLocation = location.GeoLocation.Value;

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

        /// <summary>
        /// React on location property changes.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _LocationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Location.PropertyNameGeoLocation)
            {
                // if location geocoded position changed - show location in new place
                Geometry = _CreatePoint(_location);
            }
        }

        #endregion private methods

        #region private members

        /// <summary>
        /// Location, which this graphic is shows.
        /// </summary>
        private Location _location;

        #endregion private members
    }
}

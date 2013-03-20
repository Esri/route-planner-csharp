using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Client.Symbols;
using System.ComponentModel;
using ESRI.ArcLogistics.App.Symbols;
using ESRI.ArcGIS.Client;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.App.Mapping;

namespace ESRI.ArcLogistics.App.GraphicObjects
{
    /// <summary>
    /// Graphic object for showing candidates
    /// </summary>
    class CandidateGraphicObject : DataGraphicObject
    {
        #region constructors

        private CandidateGraphicObject(AddressCandidate candidate)
            : base(candidate)
        {
            _candidate = candidate;
            _candidate.PropertyChanged += new PropertyChangedEventHandler(_candidate_PropertyChanged);

            Geometry = _CreatePoint(candidate);
        }

        #endregion constructors

        #region public static methods

        /// <summary>
        /// Create graphic object for candidate
        /// </summary>
        /// <param name="candidate">Source candidate</param>
        /// <returns>Graphic object for candidate</returns>
        public static CandidateGraphicObject Create(AddressCandidate candidate)
        {
            CandidateGraphicObject graphic = new CandidateGraphicObject(candidate)
            {
                Symbol = new CandidateSymbol()
            };

            graphic.SetZIndex(ObjectLayer.BACKZINDEX);

            return graphic;
        }

        /// <summary>
        /// Unsubscribe from all events
        /// </summary>
        public override void UnsubscribeOnChange()
        {
            _candidate.PropertyChanged -= new PropertyChangedEventHandler(_candidate_PropertyChanged);
        }

        /// <summary>
        /// Project geometry to map spatial reference
        /// </summary>
        public override void ProjectGeometry()
        {
            Geometry = _CreatePoint(_candidate);
        }

        #endregion public static methods

        #region public members

        /// <summary>
        /// React on candidate property changes
        /// </summary>
        private void _candidate_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // if candidate geocoded position changed - show candidate in new place
            ESRI.ArcGIS.Client.Geometry.MapPoint point = _CreatePoint(_candidate);
            Geometry = point;
        }

        #endregion public members

        #region private static methods

        /// <summary>
        /// Create map point for candidate
        /// </summary>
        /// <param name="candidate">Candidate to get geometry</param>
        /// <returns>Map point of candidate position</returns>
        private ESRI.ArcGIS.Client.Geometry.MapPoint _CreatePoint(AddressCandidate candidate)
        {
            ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = null;

            ESRI.ArcLogistics.Geometry.Point geoLocation = candidate.GeoLocation;

            // Project point from WGS84 to Web Mercator if spatial reference of map is Web Mercator
            if (ParentLayer != null && ParentLayer.SpatialReferenceID != null)
            {
                geoLocation = WebMercatorUtil.ProjectPointToWebMercator(geoLocation, ParentLayer.SpatialReferenceID.Value);
            }

            mapPoint = new ESRI.ArcGIS.Client.Geometry.MapPoint(geoLocation.X, geoLocation.Y);

            return mapPoint;
        }

        #endregion private static methods

        #region private members

        private AddressCandidate _candidate;

        #endregion private members
    }
}

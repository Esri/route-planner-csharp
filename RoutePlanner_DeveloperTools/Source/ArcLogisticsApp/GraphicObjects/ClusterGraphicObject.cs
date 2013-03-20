using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcLogistics.App.Symbols;
using ESRI.ArcLogistics.App.OrderSymbology;
using ESRI.ArcLogistics.DomainObjects;
using System.Diagnostics;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.App.GraphicObjects
{
    /// <summary>
    /// Graphic object for showing clusters
    /// </summary>
    class ClusterGraphicObject : Graphic, IDisposable
    {
        private ClusterGraphicObject(MapPoint mapPoint, IList<Graphic> cluster)
        {
            Geometry = mapPoint;
            Symbol = new ClusterSymbol();

            SolidColorBrush fillColor = _GetFillColor(cluster);
            Attributes.Add(SymbologyContext.FILL_ATTRIBUTE_NAME, fillColor);

            _cluster = cluster;
            foreach(Graphic graphic in _cluster)
                graphic.PropertyChanged += new PropertyChangedEventHandler(graphic_PropertyChanged);

            _SelectIfNeeded();
        }

        #region public methods

        /// <summary>
        /// Create graphic object for order
        /// </summary>
        /// <param name="location">Source order</param>
        /// <returns>Graphic object for order</returns>
        public static ClusterGraphicObject Create(MapPoint mapPoint, IList<Graphic> cluster)
        {
            ClusterGraphicObject graphic = new ClusterGraphicObject(mapPoint, cluster);

            // ClusterGraphicObject contains static list of all clusters.
            // This list used for disposing after new clustering process is started.
            _graphics.Add(graphic);
            return graphic;
        }

        /// <summary>
        /// Unsubscribe all cluster graphics from all events and clear graphics list
        /// </summary>
        public static void DisposeAll()
        {
            foreach (ClusterGraphicObject clusterGraphicObject in _graphics)
                clusterGraphicObject.Dispose();

            _graphics.Clear();
        }

        /// <summary>
        /// Unsubscribe from all events
        /// </summary>
        public void Dispose()
        {
            if (_route != null)
                _route.PropertyChanged -= new PropertyChangedEventHandler(route_PropertyChanged);

            foreach (Graphic graphic in _cluster)
                graphic.PropertyChanged -= new PropertyChangedEventHandler(graphic_PropertyChanged);
        }

        #endregion public methods

        #region public members

        /// <summary>
        /// List of all clusters
        /// </summary>
        public static IList<ClusterGraphicObject> Graphics
        {
            get { return _graphics; }
        }

        #endregion

        #region private methods

        /// <summary>
        /// React on property changed. Works with "Selected" property.
        /// </summary>
        private void graphic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(Graphic.SelectedProperty.Name))
            {
                _SelectIfNeeded();
            }
        }

        /// <summary>
        /// Set cluster selection depends on "selected" property of it graphics
        /// </summary>
        private void _SelectIfNeeded()
        {
            bool selected = false;

            foreach (Graphic graphic in _cluster)
            {
                if (graphic.Selected)
                {
                    selected = true;
                    break;
                }
            }

            if (selected && !this.Selected)
                Select();

            if (!selected && this.Selected)
                this.UnSelect();
        }

        /// <summary>
        /// Get fill color for cluster graphics. OrdersColor in case of only orders.
        /// Route color in case of unique route. GrayColor otherwise.
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
        private SolidColorBrush _GetFillColor(IList<Graphic> cluster)
        {
            ClusterSymbol clusterSymbol = (ClusterSymbol)Symbol;
            SolidColorBrush brush = (SolidColorBrush)clusterSymbol.ControlTemplate.Resources["GrayColor"];

            bool isOrders = true;
            bool isUniqueRoute = true;

            foreach (Graphic graphic in cluster)
            {
                DataGraphicObject dataGraphic = (DataGraphicObject)graphic;
                if (dataGraphic.Data is Order)
                    isUniqueRoute = false;
                else 
                    isOrders = false;

                if (isUniqueRoute)
                {
                    Stop stop = (Stop)dataGraphic.Data;
                    DataGraphicObject firstDataGraphic = (DataGraphicObject)cluster[0];
                    Stop firstStop = (Stop)firstDataGraphic.Data;
                    isUniqueRoute = stop.Route == firstStop.Route;
                }
            }

            if (isOrders)
            {
                Debug.Assert(!isUniqueRoute);
                brush = (SolidColorBrush)clusterSymbol.ControlTemplate.Resources["OrdersColor"];
            }
            
            if( isUniqueRoute)
            {
                Debug.Assert(!isOrders);
                DataGraphicObject firstDataGraphic = (DataGraphicObject)cluster[0];
                Stop firstStop = (Stop)firstDataGraphic.Data;
                Route route = firstStop.Route;

                // stop will not assign to route in case of not updated schedule
                if (route != null)
                {
                    Color mediaColor = System.Windows.Media.Color.FromArgb(route.Color.A,
                        route.Color.R, route.Color.G, route.Color.B);
                    brush = new SolidColorBrush(mediaColor);

                    _route = route;
                    route.PropertyChanged += new PropertyChangedEventHandler(route_PropertyChanged);
                }
            }

            return brush;
        }

        /// <summary>
        /// React on route color changed.
        /// </summary>
        void route_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(Route.PropertyNameColor))
            {
                Color mediaColor = System.Windows.Media.Color.FromArgb(_route.Color.A,
                    _route.Color.R, _route.Color.G, _route.Color.B);
                SolidColorBrush brush = new SolidColorBrush(mediaColor);
                Attributes[SymbologyContext.FILL_ATTRIBUTE_NAME] = brush;
                Symbol = new ClusterSymbol();
            }
        }

        #endregion

        #region private members

        private Route _route;
        private IList<Graphic> _cluster;
        private static List<ClusterGraphicObject> _graphics = new List<ClusterGraphicObject>();
        
        #endregion
    }
}

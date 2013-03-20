using System;
using System.Diagnostics;
using System.ComponentModel;
using ESRI.ArcLogistics.Services;
using ESRI.ArcGIS.Client;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics
{
    public enum AgsLayerType
    {
        Cached,
        Dynamic
    };
    // APIREV: expose type of layer: cached or dynamic using enum

    /// <summary>
    /// MapLayer class.
    /// </summary>
    public class MapLayer : INotifyPropertyChanged
    {
        #region constants
        
        /// <summary>
        /// Name of the Opacity property
        /// </summary>
        public static string PropertyNameOpacity
        {
            get { return OPACITY_PROPERTY_NAME;}
        }

        /// <summary>
        /// Name of the Visible property
        /// </summary>
        public static string PropertyNameVisible
        {
            get { return VISIBLE_PROPERTY_NAME; }
        }

        #endregion

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal MapLayer(MapServiceInfoWrap serviceInfo, AgsServer server, Map map)
        {
            Debug.Assert(serviceInfo != null);

            _id = Guid.NewGuid();
            _serviceInfo = serviceInfo;
            _map = map;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public Guid Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _serviceInfo.Name; }
        }

        public bool IsBaseMap
        {
            get { return _serviceInfo.IsBaseMap; }
        }

        public virtual bool IsVisible
        {
            get { return _serviceInfo.IsVisible; }
            set
            {
                if (IsBaseMap)
                {
                    if (value == true)
                        _map.SelectedBaseMapLayer = this;
                }
                else
                    SetVisible(value);
            }
        }

        public virtual double Opacity
        {
            get { return _serviceInfo.Opacity; }
            set
            {
                _serviceInfo.Opacity = value;
                NotifyPropertyChanged(OPACITY_PROPERTY_NAME);
            }
        }

        internal MapServiceInfoWrap MapServiceInfo
        {
            get
            {
                return _serviceInfo;
            }
        }

        internal void SetVisible(bool isVisible)
        {
            if (_serviceInfo.IsVisible != isVisible)
            {
                _serviceInfo.IsVisible = isVisible;
                NotifyPropertyChanged(VISIBLE_PROPERTY_NAME);
            }
        }

        /// <summary>
        /// Parent Map
        /// </summary>
        internal Map Map
        {
            get
            {
                return _map;
            }
        }

        // APIREV: need to expose both URLs: RestUrl or SoapUrl. They should present in serivces.xml file as well.
        public string Url
        {
            get { return _serviceInfo.Url; }
        }

        #endregion public properties

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Fire when object property changed.
        /// </summary>
        protected virtual void NotifyPropertyChanged(string info)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        #endregion private methods

        #region private constants

        /// <summary>
        /// Name of the Opacity property
        /// </summary>
        private const string OPACITY_PROPERTY_NAME = "Opacity";

        /// <summary>
        /// Name of the Visible property
        /// </summary>
        private const string VISIBLE_PROPERTY_NAME = "Visible";

        #endregion

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private MapServiceInfoWrap _serviceInfo;
        private Guid _id;
        private Map _map;

        #endregion private members
    }
}

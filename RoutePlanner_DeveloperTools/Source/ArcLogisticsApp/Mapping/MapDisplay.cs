using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using ESRI.ArcLogistics.DomainObjects;
using System.Reflection;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using System.Xml.Serialization;
using System.IO;
using ESRI.ArcLogistics.App.Properties;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.App.Mapping;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// MapDisplay class
    /// </summary>
    public class MapDisplay
    {
        #region constants

        private const string ARRIVETIME_PROP_NAME = "ArriveTime";

        #endregion

        #region constructors

        public MapDisplay()
        {
            _LoadConfig();

            bool firstRun = false;
            if (_mapDisplayConfig == null)
            {
                _mapDisplayConfig = new MapDisplayConfig();
                TrueRoute = true;
                LabelingEnabled = true;
                ShowLeadingStemTime = true;
                ShowTrailingStemTime = true;
                AutoZoom = true;

                firstRun = true;
            }

            _CreateOrdersTipsConfig("", _orderTitles, _orderTitlesSelected, _mapDisplayConfig.OrderSelectedProp);

            _CreateStopsTipsConfig(_stopTitles, _stopTitlesSelected, _mapDisplayConfig.StopSelectedProp);
            _CreateOrdersTipsConfig("AssociatedObject.", _stopTitles, _stopTitlesSelected, _mapDisplayConfig.StopSelectedProp);

            // select all by default
            if (firstRun)
            {
                string xPropName = Order.PropertyNameGeoLocation+ '.' + Order.PropertyNameX;
                string yPropName = Order.PropertyNameGeoLocation + '.' + Order.PropertyNameY;

                foreach (TipProperty property in _orderTitles)
                {
                    if (!property.Name.Equals(Order.PropertyNamePlannedDate) &&
                        !property.Name.Equals(xPropName) && !property.Name.Equals(yPropName))
                    {
                         _orderTitlesSelected.Add(property);
                        _mapDisplayConfig.OrderSelectedProp.Add(property.Name);
                    }
                }

                foreach (TipProperty property in _stopTitles)
                {
                    if (!property.Name.Equals(Order.PropertyNamePlannedDate) &&
                        !property.Name.Equals(xPropName) && !property.Name.Equals(yPropName))
                    {
                        _stopTitlesSelected.Add(property);
                        _mapDisplayConfig.StopSelectedProp.Add(property.Name);
                    }
                }

                _FillInCorrectDirection();

                Save();
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Save config
        /// </summary>
        public void Save()
        {
            _FillInCorrectDirection();

            DataContractSerializer ser = new DataContractSerializer(typeof(MapDisplayConfig));

            string serialized;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                ser.WriteObject(memoryStream, _mapDisplayConfig);
                serialized = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            Settings settings = Settings.Default;
            settings.MapDisplayConfig = serialized;
            settings.Save();
        }

        #endregion

        #region public members

        /// <summary>
        /// Order properties list
        /// </summary>
        public IList<object> OrderTitles
        {
            get
            {
                return _orderTitles;
            }
        }

        /// <summary>
        /// Selected order properties list
        /// </summary>
        public IList<object> OrderTitlesSelected
        {
            get { return _orderTitlesSelected; }
        }

        /// <summary>
        /// Stop properties list
        /// </summary>
        public IList<object> StopTitles
        {
            get
            {
                return _stopTitles;
            }
        }

        /// <summary>
        /// Selected stop properties list
        /// </summary>
        public IList<object> StopTitlesSelected
        {
            get { return _stopTitlesSelected; }
        }

        /// <summary>
        /// Shows true route setting
        /// </summary>
        public bool TrueRoute
        {
            get
            {
                return _mapDisplayConfig.TrueRoute;
            }
            set
            {
                bool _oldValue = _mapDisplayConfig.TrueRoute;
                _mapDisplayConfig.TrueRoute = value;
                if (_oldValue != value)
                {
                    if (TrueRouteChanged != null)
                        TrueRouteChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Labeling enabled setting
        /// </summary>
        public bool LabelingEnabled
        {
            get
            {
                return _mapDisplayConfig.LabelingEnabled;
            }
            set
            {
                bool _oldValue = _mapDisplayConfig.LabelingEnabled;
                _mapDisplayConfig.LabelingEnabled = value;
                if (_oldValue != value)
                {
                    if (LabelingChanged != null)
                        LabelingChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Show barriers setting
        /// </summary>
        public bool ShowBarriers
        {
            get
            {
                return _mapDisplayConfig.ShowBarriers;
            }
            set
            {
                bool _oldValue = _mapDisplayConfig.ShowBarriers;
                _mapDisplayConfig.ShowBarriers = value;
                if (_oldValue != value)
                {
                    if (ShowBarriersChanged != null)
                        ShowBarriersChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Show zones setting
        /// </summary>
        public bool ShowZones
        {
            get
            {
                return _mapDisplayConfig.ShowZones;
            }
            set
            {
                bool _oldValue = _mapDisplayConfig.ShowZones;
                _mapDisplayConfig.ShowZones = value;
                if (_oldValue != value)
                {
                    if (ShowZonesChanged != null)
                        ShowZonesChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Show leading stem time setting
        /// </summary>
        public bool ShowLeadingStemTime
        {
            get
            {
                return _mapDisplayConfig.ShowLeadingStemTime;
            }
            set
            {
                bool _oldValue = _mapDisplayConfig.ShowLeadingStemTime;
                _mapDisplayConfig.ShowLeadingStemTime = value;
                if (_oldValue != value)
                {
                    if (ShowLeadingStemTimeChanged != null)
                        ShowLeadingStemTimeChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Show trailing stem time setting
        /// </summary>
        public bool ShowTrailingStemTime
        {
            get
            {
                return _mapDisplayConfig.ShowTrailingStemTime;
            }
            set
            {
                bool _oldValue = _mapDisplayConfig.ShowTrailingStemTime;
                _mapDisplayConfig.ShowTrailingStemTime = value;
                if (_oldValue != value)
                {
                    if (ShowTrailingStemTimeChanged != null)
                        ShowTrailingStemTimeChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Auto zoom for orders and routes
        /// </summary>
        public bool AutoZoom
        {
            get
            {
                return _mapDisplayConfig.AutoZoom;
            }
            set
            {
                _mapDisplayConfig.AutoZoom = value;
            }
        }

        /// <summary>
        /// Dictionary of name-title of properties, that visible always.
        /// So, that properties will not used in selectable list.
        /// </summary>
        public Dictionary<string, string> NotSelectableProperties
        {
            get { return _notSelectableProperties; }
        }

        #endregion

        #region public events

        public event EventHandler TrueRouteChanged;
        public event EventHandler LabelingChanged;
        public event EventHandler ShowLeadingStemTimeChanged;
        public event EventHandler ShowTrailingStemTimeChanged;
        public event EventHandler ShowBarriersChanged;
        public event EventHandler ShowZonesChanged;

        #endregion

        #region private methods

        static private Type _GetEffectiveType(Type type)
        {   
            // NOTE: type can be nullabled
            Type effectiveType = type;

            Type typeReal = Nullable.GetUnderlyingType(type);
            if (null != typeReal)
                effectiveType = typeReal;

            return effectiveType;
        }

        /// <summary>
        /// Add property tip to list
        /// </summary>
        private void _AddPropertyTip(string prePath, string name, string title, IList<object> mapProperties,
            IList<object> selectedMapProperties, StringCollection selectedConfig, Unit? valueUnits, Unit? displayUnits)
        {
            if (name.Equals(Order.PropertyNameName, StringComparison.OrdinalIgnoreCase) ||
                name.Equals(ARRIVETIME_PROP_NAME, StringComparison.OrdinalIgnoreCase) ||
                name.Equals(Order.PropertyNameTimeWindow, StringComparison.OrdinalIgnoreCase) ||
                name.Equals(Order.PropertyNameTimeWindow2, StringComparison.OrdinalIgnoreCase))
            {
                if (!_notSelectableProperties.Keys.Contains(name))
                    _notSelectableProperties.Add(name, title);

                return;
            }

            foreach (TipProperty property in mapProperties)
            {
                if (title.Equals(property.ToString(), StringComparison.OrdinalIgnoreCase))
                    return;
            }

            if (title.Length != 0)
            {
                TipProperty tipProperty = new TipProperty(name, title, valueUnits, displayUnits);
                tipProperty.PrefixPath = prePath;
                mapProperties.Add(tipProperty);
                if (selectedConfig.Contains(name))
                {
                    selectedMapProperties.Add(tipProperty);
                }
            }
        }

        /// <summary>
        /// Fill Orders maptips list
        /// </summary>
        // REV: can be switched to new static methods of Order class that allow to get order properties
        private void _CreateOrdersTipsConfig(string prePath, IList<object> mapProperties, IList<object> selectedMapProperties, 
            StringCollection selectedConfig)
        {
            Type type = typeof(Order);

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (Attribute.IsDefined(property, typeof(DomainPropertyAttribute)))
                {
                    DomainPropertyAttribute attribute = (DomainPropertyAttribute)Attribute.GetCustomAttribute(property, typeof(DomainPropertyAttribute));
                    System.Diagnostics.Debug.Assert(null != attribute);
                    Type typeProperty = _GetEffectiveType(property.PropertyType);

                    if (typeof(OrderCustomProperties) == typeProperty)
                    {
                        _AddCustomOrderProperties(prePath, mapProperties, selectedMapProperties, selectedConfig);
                    }
                    else if (typeof(Capacities) == typeProperty)
                    {
                        _AddCapacityProperties(prePath, mapProperties, selectedMapProperties, selectedConfig); 
                    }
                    else if (typeof(Address) == typeProperty)
                    {   // specials type: address
                        ESRI.ArcLogistics.Geocoding.AddressField[] fields = App.Current.Geocoder.AddressFields;
                        for (int i = 0; i < fields.Length; ++i)
                            _AddPropertyTip(prePath, "Address." + fields[i].Type, fields[i].Title, mapProperties, 
                                selectedMapProperties, selectedConfig, null, null);
                    }
                    else if (typeof(ESRI.ArcLogistics.Geometry.Point) == typeProperty)
                    {   // specials type: Point
                        _AddPropertyTip(prePath, "GeoLocation.X", "X", mapProperties,
                            selectedMapProperties, selectedConfig, null, null);
                        _AddPropertyTip(prePath, "GeoLocation.Y", "Y", mapProperties,
                            selectedMapProperties, selectedConfig, null, null);
                    }
                    else
                    {
                        _AddCoreProperty(property, prePath, attribute, mapProperties, selectedMapProperties, selectedConfig);
                    }
                }
            }
        }

        /// <summary>
        /// Add Capacities properties
        /// </summary>
        private void _AddCapacityProperties(string prePath, IList<object> mapProperties, 
            IList<object> selectedMapProperties, StringCollection selectedConfig)
        {
            // specials type: capacities
            if (App.Current.Project != null)
            {
                CapacitiesInfo info = App.Current.Project.CapacitiesInfo;
                for (int i = 0; i < info.Count; ++i)
                {
                    CapacityInfo capacityInfo = info[i];

                    Unit units;
                    if (RegionInfo.CurrentRegion.IsMetric)
                        units = capacityInfo.DisplayUnitMetric;
                    else
                        units = capacityInfo.DisplayUnitUS;

                    _AddPropertyTip(prePath, "Capacities.[" + i.ToString() + "]", info[i].Name,
                        mapProperties, selectedMapProperties, selectedConfig, units, units);
                }
            }
        }

        /// <summary>
        /// Add custom order properties
        /// </summary>
        private void _AddCustomOrderProperties(string prePath, IList<object> mapProperties,
            IList<object> selectedMapProperties, StringCollection selectedConfig)
        {
            // specials type: order custom property
            if (App.Current.Project != null)
            {
                OrderCustomPropertiesInfo info = App.Current.Project.OrderCustomPropertiesInfo;
                for (int i = 0; i < info.Count; ++i)
                {
                    _AddPropertyTip(prePath, "CustomProperties.[" + i.ToString() + "]", info[i].Name,
                        mapProperties, selectedMapProperties, selectedConfig, null, null);
                }
            }
        }

        /// <summary>
        /// Add core properties of class
        /// </summary>
        private void _AddCoreProperty(PropertyInfo property, string prePath, DomainPropertyAttribute attribute,
            IList<object> mapProperties, IList<object> selectedMapProperties, StringCollection selectedConfig)
        {
            Unit? displayUnits = null;
            Unit? valueUnits = null;
            if (Attribute.IsDefined(property, typeof(UnitPropertyAttribute)))
            {
                UnitPropertyAttribute unitAttribute = (UnitPropertyAttribute)Attribute.GetCustomAttribute(
                    property, typeof(UnitPropertyAttribute));

                displayUnits = (RegionInfo.CurrentRegion.IsMetric) ? unitAttribute.DisplayUnitMetric : unitAttribute.DisplayUnitUS;
                valueUnits = unitAttribute.ValueUnits;
            }

            _AddPropertyTip(prePath, property.Name, attribute.Title, mapProperties,
                selectedMapProperties, selectedConfig, valueUnits, displayUnits);
        }

        /// <summary>
        /// Fill stops maptips list
        /// </summary>
        private void _CreateStopsTipsConfig(List<object> mapProperties, IList<object> selectedMapProperties, StringCollection selectedConfig)
        {
            Type type = typeof(Stop);

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (Attribute.IsDefined(property, typeof(DomainPropertyAttribute)))
                {
                    DomainPropertyAttribute attribute = (DomainPropertyAttribute)Attribute.GetCustomAttribute(property, typeof(DomainPropertyAttribute));
                    System.Diagnostics.Debug.Assert(null != attribute);
                    Type typeProperty = _GetEffectiveType(property.PropertyType);

                    Unit? displayUnits = null;
                    Unit? valueUnits = null;
                    if (Attribute.IsDefined(property, typeof(UnitPropertyAttribute)))
                    {
                        UnitPropertyAttribute unitAttribute = (UnitPropertyAttribute)Attribute.GetCustomAttribute(
                            property, typeof(UnitPropertyAttribute));

                        displayUnits = (RegionInfo.CurrentRegion.IsMetric) ? unitAttribute.DisplayUnitMetric : unitAttribute.DisplayUnitUS;
                        valueUnits = unitAttribute.ValueUnits;
                    }

                    if (!property.Name.Equals(Stop.PropertyNameMapLocation) && !property.Name.Equals(Stop.PropertyNameSequenceNumber))
                    {
                        _AddPropertyTip("", property.Name, attribute.Title, mapProperties,
                            selectedMapProperties, selectedConfig, valueUnits, displayUnits);
                    }
                }
            }
        }

        /// <summary>
        /// Load Config 
        /// </summary>
        private void _LoadConfig()
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(MapDisplayConfig));

            Settings settings = Settings.Default;
            string configText = settings.MapDisplayConfig;

            if (!string.IsNullOrEmpty(configText))
            {
                MemoryStream stream = null;
                
                try
                {
                    stream = new MemoryStream(Encoding.UTF8.GetBytes(configText));
                    _mapDisplayConfig = (MapDisplayConfig)ser.ReadObject(stream);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
            }
        }

        /// <summary>
        /// Set tipproperties to correct direction
        /// </summary>
        private void _FillInCorrectDirection()
        {
            // Fill correct values
            _mapDisplayConfig.OrderSelectedProp.Clear();

            object[] orderTitlesSelected = _orderTitlesSelected.ToArray();
            _orderTitlesSelected.Clear();

            foreach (TipProperty tipProperty in _orderTitles)
            {
                foreach (TipProperty selectedTipProperty in orderTitlesSelected)
                {
                    if (tipProperty.Title.Equals(selectedTipProperty.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        _mapDisplayConfig.OrderSelectedProp.Add(tipProperty.Name);
                        _orderTitlesSelected.Add(selectedTipProperty);
                        break;
                    }
                }
            }

            _mapDisplayConfig.StopSelectedProp.Clear();
            object[] stopTitlesSelected = _stopTitlesSelected.ToArray();
            _stopTitlesSelected.Clear();

            foreach (TipProperty tipProperty in _stopTitles)
            {
                foreach (TipProperty selectedTipProperty in stopTitlesSelected)
                {
                    if (tipProperty.Title.Equals(selectedTipProperty.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        _mapDisplayConfig.StopSelectedProp.Add(tipProperty.Name);
                        _stopTitlesSelected.Add(selectedTipProperty);
                        break;
                    }
                }
            }
        }

        #endregion

        #region private members

        private List<object> _orderTitles = new List<object>();
        private ObservableCollection<object> _orderTitlesSelected = new ObservableCollection<object>();

        private List<object> _stopTitles = new List<object>();
        private ObservableCollection<object> _stopTitlesSelected = new ObservableCollection<object>();

        private MapDisplayConfig _mapDisplayConfig;
        private Dictionary<string, string> _notSelectableProperties = new Dictionary<string, string>();

        #endregion
    }
}

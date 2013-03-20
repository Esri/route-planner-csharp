using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Drawing;
using System.Xml;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Windows;
using ESRI.ArcLogistics.App.GraphicObjects;
using ESRI.ArcLogistics.App.Symbols;
using System.Reflection;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client;
using System.Windows.Media;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.App.OrderSymbology
{
    internal enum SymbologyType
    {
        CategorySymbology,
        QuantitySymbology
    }

    /// <summary>
    /// Order Symbology main class
    /// </summary>
    static class SymbologyManager
    {
        #region constants

        private const string CONFIG_PROPERTY_NAME = "SymbologyConfig";
        private const string SYMBOLS_FOLDER = "Symbols";

        public const string DEFAULT_TEMPLATE_NAME = "<Default Template>";

        public const int DEFAULT_SIZE = 16;
        public const int DEFAULT_INDENT = 8;
        public const double INCREASE_ON_HOVER = 1.2;

        #endregion

        #region constructors

        static SymbologyManager()
        {
            DefaultValueString = (string)App.Current.FindResource("AllOtherValues");
            _LoadTemplates();
            App.Current.ProjectLoaded += new EventHandler(App_ProjectLoaded);
            App.Current.ProjectClosed += new EventHandler(App_ProjectClosed);
            if (App.Current.Project != null)
                _InitProjectSymbology();
        }

        #endregion

        #region public methods

        /// <summary>
        /// Init Project Symbology
        /// </summary>
        public static void Init()
        {
            if (!_inited)
                _InitProjectSymbology();
        }

        /// <summary>
        /// Save symbology config
        /// </summary>
        public static void SaveConfig()
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(SymbologyConfig));

            string serialized;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                ser.WriteObject(memoryStream, _symbologyConfig);
                serialized = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            App.Current.Project.ProjectProperties.UpdateProperty(CONFIG_PROPERTY_NAME, serialized);
            App.Current.Project.Save();

            if (OnSettingsChanged != null)
                OnSettingsChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// Add default category
        /// </summary>
        public static void AddDefaultCategory()
        {
            OrderCategory orderCategory = new OrderCategory(true)
            {
                Value = DefaultValueString,
                SymbolFilename = DEFAULT_TEMPLATE_NAME
            };
            _symbologyConfig.OrderCategories.Add(orderCategory);
        }

        /// <summary>
        /// Add default quantity
        /// </summary>
        public static void AddDefaultQuantity()
        {
            OrderQuantity orderQuantity = new OrderQuantity(true)
            {
                SymbolFilename = DEFAULT_TEMPLATE_NAME
            };
            _symbologyConfig.OrderQuantities.Add(orderQuantity);
        }

        /// <summary>
        /// Init Graphic. Set it's attributes and symbol
        /// </summary>
        /// <param name="graphicObject">Graphic to init</param>
        public static void InitGraphic(DataGraphicObject graphicObject)
        {
            if (!_inited)
                _InitProjectSymbology();

            if (FieldName.Length == 0)
            {
                System.Windows.Media.Color mediaColor;

                Stop stop = graphicObject.Data as Stop;
                if (stop == null)
                    mediaColor = System.Windows.Media.Color.FromRgb(0, 0, 0);
                else
                {
                    System.Drawing.Color color = stop.Route.Color;
                    mediaColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                }
                graphicObject.Attributes[SymbologyContext.FILL_ATTRIBUTE_NAME] = new SolidColorBrush(mediaColor);

                graphicObject.Symbol = new CustomOrderSymbol();
            }
            else
            {
                Order order = graphicObject.Data as Order;
                if (order == null)
                {
                    Stop stop = (Stop)graphicObject.Data;
                    order = (Order)stop.AssociatedObject;
                }

                object value = Order.GetPropertyValue(order, FieldName);
                Symbol orderSymbol;
                if (SymbologyType == SymbologyType.CategorySymbology)
                    orderSymbol = _InitGraphicByCategory(value, graphicObject);
                else
                    orderSymbol = _InitGraphicByQuantity(value, graphicObject);

                graphicObject.Symbol = orderSymbol;
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Load all available templates
        /// </summary>
        private static void _LoadTemplates()
        {
            string templatesFolderPath = Path.Combine(DataFolder.Path, SYMBOLS_FOLDER);

            if (Directory.Exists(templatesFolderPath))
            {
                DirectoryInfo dir = new DirectoryInfo(templatesFolderPath);
                foreach (FileInfo file in dir.GetFiles("*.xaml", SearchOption.AllDirectories))
                {
                    ControlTemplate controlTemplate = _LoadTemplateFromFile(file.FullName);
                    if (controlTemplate != null)
                    {
                        _templates.Add(controlTemplate);
                        _templatesFileNames.Add(file.Name);
                    }
                }
            }

            _defaultTemplate = _LoadTemplateFromResource("ESRI.ArcLogistics.App.Symbols.CustomOrderSymbol.xaml");
        }

        /// <summary>
        /// Init project symbology
        /// </summary>
        private static void _InitProjectSymbology()
        {
            FillQuantityRequiredFields();
            _categoryFieldNames.AddRange(Order.GetPropertyNames(App.Current.Project.CapacitiesInfo,
                App.Current.Project.OrderCustomPropertiesInfo, App.Current.Geocoder.AddressFields));
            _categoryFieldTitles.AddRange(Order.GetPropertyTitles(App.Current.Project.CapacitiesInfo,
                App.Current.Project.OrderCustomPropertiesInfo, App.Current.Geocoder.AddressFields));
            int index = _categoryFieldNames.IndexOf(Order.PropertyNameX);
            _categoryFieldNames.RemoveAt(index);
            _categoryFieldTitles.RemoveAt(index);

            index = _categoryFieldNames.IndexOf(Order.PropertyNameY);
            _categoryFieldNames.RemoveAt(index);
            _categoryFieldTitles.RemoveAt(index);

            string configText = "";
            try
            {
                configText = App.Current.Project.ProjectProperties.GetPropertyByName(CONFIG_PROPERTY_NAME);
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
            _LoadConfig(configText);

            _inited = true;
        }

        /// <summary>
        /// Load Config 
        /// </summary>
        private static void _LoadConfig(string configText)
        {
            _symbologyConfig = null;

            DataContractSerializer ser = new DataContractSerializer(typeof(SymbologyConfig));

            if (!string.IsNullOrEmpty(configText))
            {
                MemoryStream stream = null;
                try
                {
                    stream = new MemoryStream(Encoding.UTF8.GetBytes(configText));
                    _symbologyConfig = (SymbologyConfig)ser.ReadObject(stream);
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

            if (_symbologyConfig == null)
                _CreateConfig();
        }

        /// <summary>
        /// Create empty config
        /// </summary>
        private static void _CreateConfig()
        {
            _symbologyConfig = new SymbologyConfig();
            _symbologyConfig.OrderSymbologyType = SymbologyType.CategorySymbology;

            _symbologyConfig.OrderCategories = new ObservableCollection<OrderCategory>();
            AddDefaultCategory();

            _symbologyConfig.OrderQuantities = new ObservableCollection<OrderQuantity>();
            AddDefaultQuantity();

            SaveConfig();
        }

        private static void App_ProjectLoaded(object sender, EventArgs e)
        {
            if (!_inited)
                _InitProjectSymbology();
        }

        private static void App_ProjectClosed(object sender, EventArgs e)
        {
            _inited = false;
            _symbologyConfig = null;
        }

        /// <summary>
        /// Load ControlTemplate from file
        /// </summary>
        /// <param name="filepath">Filename of ControlTemplate</param>
        /// <returns>Loaded ControlTemplate</returns>
        private static ControlTemplate _LoadTemplateFromFile(string filepath)
        {
            ControlTemplate controlTemplate = null;
            try
            {
                FileStream stream = new FileStream(filepath, FileMode.Open);
                string template = new StreamReader(stream).ReadToEnd();
                StringReader stringReader = new StringReader(template);

                XmlTextReader xmlReader = new XmlTextReader(stringReader);
                controlTemplate = XamlReader.Load(xmlReader) as ControlTemplate;
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
            return controlTemplate;
        }

        /// <summary>
        /// Load ControlTemplate from application resource
        /// </summary>
        /// <param name="key">Resource name</param>
        /// <returns>Loaded ControlTemplate</returns>
        private static ControlTemplate _LoadTemplateFromResource(string key)
        {
            ControlTemplate controlTemplate = null;
            try
            {
                Stream stream = Application.Current.GetType().Assembly.GetManifestResourceStream(key);
                string template = new StreamReader(stream).ReadToEnd();
                StringReader stringReader = new StringReader(template);
                XmlTextReader xmlReader = new XmlTextReader(stringReader);
                controlTemplate = XamlReader.Load(xmlReader) as ControlTemplate;
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
            return controlTemplate;
        }

        #endregion

        #region public members

        /// <summary>
        /// Available ControlTemplates
        /// </summary>
        public static List<ControlTemplate> Templates
        {
            get { return _templates; }
        }

        /// <summary>
        /// FileNames of available ControlTemplates
        /// </summary>
        public static List<string> TemplatesFileNames
        {
            get { return _templatesFileNames; }
        }

        /// <summary>
        /// Default ControlTemplate
        /// </summary>
        public static ControlTemplate DefaultTemplate
        {
            get { return _defaultTemplate; }
        }

        /// <summary>
        /// Symbology type
        /// </summary>
        public static SymbologyType SymbologyType
        {
            get { return _symbologyConfig.OrderSymbologyType; }
            set
            {
                _symbologyConfig.OrderSymbologyType = value;
            }
        }

        /// <summary>
        /// Order categories
        /// </summary>
        public static ObservableCollection<OrderCategory> OrderCategories
        {
            get { return _symbologyConfig.OrderCategories; }
        }

        /// <summary>
        /// Order quantities
        /// </summary>
        public static ObservableCollection<OrderQuantity> OrderQuantities
        {
            get { return _symbologyConfig.OrderQuantities; }
        }

        /// <summary>
        /// Symbology field for categories
        /// </summary>
        public static string CategoryOrderField
        {
            get { return _symbologyConfig.CategoryOrderField; }
            set { _symbologyConfig.CategoryOrderField = value; }
        }

        /// <summary>
        /// Symbology field for quantities
        /// </summary>
        public static string QuantityOrderField
        {
            get { return _symbologyConfig.QuantityOrderField; }
            set { _symbologyConfig.QuantityOrderField = value; }
        }

        public static string FieldTitle
        {
            get
            {
                if (SymbologyType == SymbologyType.CategorySymbology)
                    return CategoryOrderField;
                else
                    return QuantityOrderField;
            }
        }

        /// <summary>
        /// Order Symbology FieldName
        /// Empty in case of symbology off
        /// </summary>
        public static string FieldName
        {
            get
            {
                if (SymbologyType == SymbologyType.CategorySymbology)
                {
                    int index = _categoryFieldTitles.IndexOf(FieldTitle);
                    if (index == -1)
                        return string.Empty;
                    return _categoryFieldNames[index];
                }
                else
                {
                    int index = _quantityFieldTitles.IndexOf(FieldTitle);
                    if (index == -1)
                        return string.Empty;
                    return _quantityFieldNames[index];
                }
            }
        }

        /// <summary>
        /// Field titles, used in category symbology
        /// </summary>
        public static IList<string> CategoryFieldTitles
        {
            get { return _categoryFieldTitles; }
        }

        /// <summary>
        /// Field titles, used in quantity symbology
        /// </summary>
        public static IList<string> QuantityFieldTitles
        {
            get { return _quantityFieldTitles; }
        }

        /// <summary>
        /// String with default value
        /// </summary>
        public static string DefaultValueString
        {
            get;
            private set;
        }

        public static event EventHandler OnSettingsChanged;

        #endregion

        /// <summary>
        /// Get numeric and capacities field names
        /// </summary>
        /// <param name="fieldNames">Field names array</param>
        private static void FillQuantityRequiredFields()
        {
            _quantityFieldTitles.Clear();
            _quantityFieldNames.Clear();
            _quantityFieldProperties.Clear();

            Type type = typeof(Order);

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (Attribute.IsDefined(property, typeof(DomainPropertyAttribute)))
                {
                    DomainPropertyAttribute attribute = (DomainPropertyAttribute)Attribute.GetCustomAttribute(property, typeof(DomainPropertyAttribute));
                    System.Diagnostics.Debug.Assert(null != attribute);
                    Type typeProperty = GetEffectiveType(property.PropertyType);

                    if (typeof(OrderCustomProperties) == typeProperty)
                    {
                        _AddCustomOrderProperties(property);
                    }
                    else if (typeof(Capacities) == typeProperty)
                    {
                        _AddCapacitiesInfos(property);
                    }
                    else
                    {
                        _AddProperty(property, typeProperty, attribute);
                    }
                }
            }
        }

        private static void _AddCustomOrderProperties(PropertyInfo property)
        {
            OrderCustomPropertiesInfo info = App.Current.Project.OrderCustomPropertiesInfo;
            for (int i = 0; i < info.Count; ++i)
            {
                _quantityFieldTitles.Add(info[i].Name);
                _quantityFieldNames.Add(OrderCustomProperties.GetCustomPropertyName(i));
                _quantityFieldProperties.Add(property);
            }
        }

        private static void _AddCapacitiesInfos(PropertyInfo property)
        {
            if (App.Current.Project != null)
            {
                CapacitiesInfo info = App.Current.Project.CapacitiesInfo;
                for (int i = 0; i < info.Count; ++i)
                {
                    _quantityFieldTitles.Add(info[i].Name);
                    _quantityFieldNames.Add(Capacities.GetCapacityPropertyName(i));
                    _quantityFieldProperties.Add(property);
                }
            }
        }

        private static void _AddProperty(PropertyInfo property, Type typeProperty, DomainPropertyAttribute attribute)
        {
            if (typeProperty == typeof(int) || typeProperty == typeof(double))
            {
                _quantityFieldTitles.Add(attribute.Title);
                _quantityFieldNames.Add(property.Name);
                _quantityFieldProperties.Add(property);
            }
        }

        private static Type GetEffectiveType(Type type)
        {
            // NOTE: type can be nullabled
            Type effectiveType = type;

            Type typeReal = Nullable.GetUnderlyingType(type);
            if (null != typeReal)
                effectiveType = typeReal;

            return effectiveType;
        }

        private static Symbol _InitGraphicByCategory(object value, DataGraphicObject graphicObject)
        {
            SymbologyRecord record = null;
            foreach (OrderCategory orderCategory in OrderCategories)
                if (!orderCategory.DefaultValue && value != null && orderCategory.Value == value.ToString())
                {
                    record = orderCategory;
                    break;
                }

            if (record == null)
                foreach (OrderCategory orderCategory in OrderCategories)
                    if (orderCategory.DefaultValue)
                        record = orderCategory;

            return _InitGraphicByRecord(record, graphicObject);
        }

        private static Symbol _InitGraphicByQuantity(object value, DataGraphicObject graphicObject)
        {
            SymbologyRecord record = null;
            if (value != null)
            {
                double? numValue = null;
                if (value is string)
                {
                    double result;
                    if (double.TryParse((string)value, out result))
                        numValue = result;
                }
                else
                    numValue = (double)value;

                if (numValue.HasValue)
                {
                    foreach (OrderQuantity orderQuantity in OrderQuantities)
                    {
                        if (!orderQuantity.DefaultValue && value != null &&
                            (orderQuantity.MinValue == numValue ||
                            orderQuantity.MinValue < numValue && orderQuantity.MaxValue > numValue))
                        {
                            record = orderQuantity;
                        }
                    }
                }
            }

            if (record == null)
                foreach (OrderQuantity orderQuantity in OrderQuantities)
                    if (orderQuantity.DefaultValue)
                        record = orderQuantity;

            return _InitGraphicByRecord(record, graphicObject);
        }

        private static Symbol _InitGraphicByRecord(SymbologyRecord record, DataGraphicObject graphicObject)
        {
            Symbol symbol = new MarkerSymbol();

            symbol.ControlTemplate = GetTemplateByFileName(record.SymbolFilename);
            graphicObject.Attributes[SymbologyContext.SIZE_ATTRIBUTE_NAME] = record.Size;
            graphicObject.Attributes[SymbologyContext.OFFSETX_ATTRIBUTE_NAME] =
                -record.Size / 2 - SymbologyManager.DEFAULT_INDENT / 2;
            graphicObject.Attributes[SymbologyContext.OFFSETY_ATTRIBUTE_NAME] =
                -record.Size / 2 - SymbologyManager.DEFAULT_INDENT / 2;
            graphicObject.Attributes[SymbologyContext.FULLSIZE_ATTRIBUTE_NAME] =
                record.Size + SymbologyManager.DEFAULT_INDENT;

            if (!record.UseRouteColor)
            {
                System.Drawing.Color color = (System.Drawing.Color)record.Color;
                System.Windows.Media.Color mediaColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                graphicObject.Attributes[SymbologyContext.FILL_ATTRIBUTE_NAME] = new SolidColorBrush(mediaColor);
            }
            else
            {
                System.Windows.Media.Color mediaColor;

                Stop stop = graphicObject.Data as Stop;
                if (stop == null)
                    mediaColor = System.Windows.Media.Color.FromRgb(0, 0, 0);
                else
                {
                    System.Drawing.Color color = stop.Route.Color;
                    mediaColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                }
                graphicObject.Attributes[SymbologyContext.FILL_ATTRIBUTE_NAME] = new SolidColorBrush(mediaColor);
            }

            return symbol;
        }

        private static ControlTemplate GetTemplateByFileName(string filename)
        {
            int index = TemplatesFileNames.IndexOf(filename);
            if (index != -1)
                return Templates[index];

            return _defaultTemplate;
        }

        #region private members

        private static List<ControlTemplate> _templates = new List<ControlTemplate>();
        private static List<string> _templatesFileNames = new List<string>();

        private static ControlTemplate _defaultTemplate;

        private static SymbologyConfig _symbologyConfig;
        private static bool _inited;

        private static List<string> _categoryFieldNames = new List<string>();
        private static List<string> _quantityFieldNames = new List<string>();

        private static List<string> _categoryFieldTitles = new List<string>();
        private static List<string> _quantityFieldTitles = new List<string>();

        private static List<PropertyInfo> _categoryFieldProperties = new List<PropertyInfo>();
        private static List<PropertyInfo> _quantityFieldProperties = new List<PropertyInfo>();

        #endregion
    }
}

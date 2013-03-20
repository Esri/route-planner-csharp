using System;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Views;

using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// Class reads xaml file and loads collections of grid parts from there
    /// </summary>
    internal class GridStructureInitializer
    {
        #region Constructors

        public GridStructureInitializer(string sourceXAML)
        {
            _LoadStructureFromXAML(sourceXAML);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Builds collectionSource ItemPorperties collection and collection of xceedGrid columns.
        /// </summary>
        /// <param name="collectionSource"></param>
        /// <param name="xceedControl"></param>
        public void BuildGridStructure(DataGridCollectionViewSource collectionSource, DataGridControl xceedGrid)
        {
            _BuildCollectionSource(collectionSource.ItemProperties);
            _BuildColumnsCollection(xceedGrid.Columns);
            _InitPrintConfiguration(xceedGrid);
        }

        /// <summary>
        /// Builds collectionSource DetailDescriptions.ItemProperties and collection of detail columns.
        /// </summary>
        /// <param name="collectionSource"></param>
        /// <param name="xceedControl"></param>
        /// <param name="detail"></param>
        public void  BuildDetailStructure(DataGridCollectionViewSource collectionSource, DataGridControl xceedGrid,
                                          DetailConfiguration detail)
        {
            Debug.Assert(null != collectionSource.DetailDescriptions);
            Debug.Assert(1 == collectionSource.DetailDescriptions.Count);

            _BuildCollectionSource(collectionSource.DetailDescriptions[0].ItemProperties);
            _BuildColumnsCollection(detail.Columns);

            // Add stops as detail of route.
            xceedGrid.DetailConfigurations.Clear();
            xceedGrid.DetailConfigurations.Add(detail);

            // NOTE: Set this property so that columns with custom order properties from default
            // settings were not added to grid automatically.
            xceedGrid.DetailConfigurations[0].AutoCreateColumns = false;

            // Collapse all detail and reexpand it.
            List<DataGridContext> dataGridContexts = new List<DataGridContext>(xceedGrid.GetChildContexts());
            foreach (DataGridContext dataGridContext in dataGridContexts)
            {
                dataGridContext.ParentDataGridContext.CollapseDetails(dataGridContext.ParentItem);
                dataGridContext.ParentDataGridContext.ExpandDetails(dataGridContext.ParentItem);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method inits grid's print configuration
        /// </summary>
        /// <param name="xceedGrid"></param>
        private void _InitPrintConfiguration(DataGridControl xceedGrid)
        {
            // if current page contains "Print" button and print configuration defined in structure
            if (_headerTemplate != null)
            {
                PrintTableView view = new PrintTableView();
                view.PageHeaders.Add(_headerTemplate);
                view.PageFooters.Add((DataTemplate)App.Current.FindResource(PRINT_FOOTER_RESOURCE_NAME));
                xceedGrid.PrintView = view;
            }
        }

        /// <summary>
        /// Builds collection of columns.
        /// </summary>
        /// <param name="columns"></param>
        private void _BuildColumnsCollection(ColumnCollection columns)
        {
            columns.Clear();
            foreach (Column column in _columns)
            {
                if (column.FieldName.Equals("Capacities") || column.FieldName.Equals("CustomOrderProperties"))
                {
                    Collection<Column> dynamicColumns = column.FieldName.Equals("Capacities") ?
                                                            _GetDynamicCapacitiesColumns(column.ReadOnly) :
                                                            _GetDynamicCustomOrderColumns(column.ReadOnly);
                    foreach (Column dynamicColumn in dynamicColumns)
                    {
                        if (column.CellEditor != null)
                            dynamicColumn.CellEditor = column.CellEditor;
                        dynamicColumn.Width = column.Width;
                        columns.Add(dynamicColumn);
                    }
                }
                else if (column.FieldName.Equals("AddressFields"))
                {
                    Collection<Column> dynamicColumns = _GetDynamicAddressColumns(column.ReadOnly);
                    foreach (Column dynamicColumn in dynamicColumns)
                        columns.Add(dynamicColumn);
                }
                else
                    columns.Add(column);
            }
        }

        /// <summary>
        /// Builds collection of item properties
        /// </summary>
        private void _BuildCollectionSource(ObservableCollection<DataGridItemPropertyBase> itemProperties)
        {
            itemProperties.Clear();
            foreach (DataGridItemProperty property in _itemPorpertiesCollection)
            {
                if (property.Name.Equals("Capacities") ||
                    property.Name.Equals("AddressFields") ||
                    property.Name.Equals("CustomOrderProperties"))
                {
                    Collection<DataGridItemProperty> dynamicProperties = null;
                    if (property.Name.Equals("Capacities"))
                        dynamicProperties = _GetDynamicCapacitiesProperties(property.ValuePath, property.IsReadOnly);
                    else if (property.Name.Equals("AddressFields"))
                        dynamicProperties = _GetDynamicAddressProperties(property.ValuePath, property.IsReadOnly);
                    else
                    {
                        Debug.Assert(property.Name.Equals("CustomOrderProperties"));
                        dynamicProperties = _GetDynamicCustomOrderProperties(property.ValuePath, property.IsReadOnly);
                    }

                    Debug.Assert(null != dynamicProperties);
                    foreach (DataGridItemProperty dynamicProperty in dynamicProperties)
                        itemProperties.Add(dynamicProperty);
                }
                else
                    itemProperties.Add(property);
            }
        }

        /// <summary>
        /// Loads grid structure (ItemPorperties and Columns) from xaml
        /// </summary>
        /// <param name="key"></param>
        private void _LoadStructureFromXAML(string key)
        {
            try
            {
                Stream stream = this.GetType().Assembly.GetManifestResourceStream(key);
                string template = new StreamReader(stream).ReadToEnd();
                StringReader stringReader = new StringReader(template);
                XmlTextReader xmlReader = new XmlTextReader(stringReader);
                ResourceDictionary resource = XamlReader.Load(xmlReader) as ResourceDictionary;
                _itemPorpertiesCollection = resource[ITEM_PROPERTIES_RESOURCE_NAME] as ArrayList;
                _columns = resource[COLUMNS_RESOURCE_NAME] as ArrayList;
                _headerTemplate = resource[PRINT_HEADER_RESOURCE_NAME] as DataTemplate;
            }
            catch (Exception ex)
            {
                Logger.Info(ex.Message);
            }
        }

        /// <summary>
        /// Gets collection of dynamic capacities columns
        /// </summary>
        /// <returns></returns>
        private Collection<Column> _GetDynamicCapacitiesColumns(bool isReadOnly)
        {
            Collection<Column> dynamicColumns = new Collection<Column>();

            for (int i = 0; i < App.Current.Project.CapacitiesInfo.Count; i++)
            {
                Column col = new Column();
                col.FieldName = Capacities.GetCapacityPropertyName(i);
                col.Title = App.Current.Project.CapacitiesInfo[i].Name;
                col.ReadOnly = isReadOnly;
                col.CellContentTemplate = (DataTemplate)App.Current.FindResource("UnitCellContentTemplate");
                col.CellEditor = (CellEditor)App.Current.FindResource("UnitEditorTemplate");
                dynamicColumns.Add(col);
            }
            return dynamicColumns;
        }

        /// <summary>
        /// Gets collection of dynamic address columns
        /// </summary>
        /// <returns></returns>
        private Collection<Column> _GetDynamicAddressColumns(bool isReadOnly)
        {
            Collection<Column> dynamicColumns = new Collection<Column>();

            for (int i = 0; i < App.Current.Geocoder.AddressFields.Length; i++)
            {
                AddressField addressField = App.Current.Geocoder.AddressFields[i];

                if (addressField.Visible)
                {
                    Column col = new Column();
                    col.FieldName = addressField.Type.ToString();
                    col.Title = addressField.Title;
                    col.ReadOnly = isReadOnly;
                    col.CellEditor = _GetAddressColumnEditor(addressField.Type);
                    dynamicColumns.Add(col);
                }
            }
            return dynamicColumns;
        }

        private CellEditor _GetAddressColumnEditor(AddressPart addressPart)
        {
            string keyName = string.Format(ADDRESS_COLUMN_EDITOR_KEY_FORMAT, addressPart.ToString());

            CellEditor editor = (CellEditor)App.Current.FindResource(keyName);
            Debug.Assert(editor != null);

            return editor;
        }

        /// <summary>
        /// Gets collection of dynamic custom oreder properties columns
        /// </summary>
        /// <returns></returns>
        private Collection<Column> _GetDynamicCustomOrderColumns(bool isReadOnly)
        {
            Collection<Column> dynamicColumns = new Collection<Column>();

            OrderCustomPropertiesInfo infos = App.Current.Project.OrderCustomPropertiesInfo;

            for (int i = 0; i < infos.Count; i++)
            {
                OrderCustomProperty info = infos[i];

                Column col = new Column();
                col.FieldName = OrderCustomProperties.GetCustomPropertyName(i);
                col.Title = info.Name;

                if (info.Type == OrderCustomPropertyType.Text)
                    col.CellEditor = (CellEditor)App.Current.FindResource("CustomOrderPropertyTextEditor");
                else if (info.Type == OrderCustomPropertyType.Numeric)
                {
                    col.CellEditor = (CellEditor)App.Current.FindResource("CustomOrderPropertyNumericEditor");
                    col.CellContentTemplate = (DataTemplate)App.Current.FindResource("DefaultStringTemplate");
                }
                else
                {
                    Debug.Assert(false); // NOTE: not supported
                }

                col.ReadOnly = isReadOnly;
                dynamicColumns.Add(col);
            }
            return dynamicColumns;
        }

        /// <summary>
        /// Builds collection of dynamic capacities
        /// </summary>
        /// <returns></returns>
        private Collection<DataGridItemProperty> _GetDynamicCapacitiesProperties(string baseValuePath, bool isReadonly)
        {
            Collection<DataGridItemProperty> dynamicProperties = new Collection<DataGridItemProperty>();

            for (int i = 0; i < App.Current.Project.CapacitiesInfo.Count; i++)
            {
                string valuePath = baseValuePath + string.Format("Capacities[{0}]", i);
                string valueName = Capacities.GetCapacityPropertyName(i);

                DataGridItemProperty newProperty = new DataGridItemProperty(valueName, valuePath, typeof(double));
                newProperty.IsReadOnly = isReadonly;
                dynamicProperties.Add(newProperty);
            }
            return dynamicProperties;
        }

        /// <summary>
        /// Builds collection of dynamic address fields
        /// </summary>
        /// <returns></returns>
        private Collection<DataGridItemProperty> _GetDynamicAddressProperties(string baseValuePath, bool isReadOnly)
        {
            Collection<DataGridItemProperty> dynamicProperties = new Collection<DataGridItemProperty>();

            AddressField[] addressFields = App.Current.Geocoder.AddressFields;
            for (int i = 0; i < addressFields.Length; i++)
            {
                if (addressFields[i].Visible)
                {
                    string valuePath = baseValuePath + string.Format("Address[{0}]", addressFields[i].Type.ToString());
                    string propertyName = addressFields[i].Type.ToString();

                    DataGridItemProperty newProperty = new DataGridItemProperty(propertyName, valuePath, typeof(string));
                    newProperty.IsReadOnly = isReadOnly;
                    dynamicProperties.Add(newProperty);
                }
            }
            return dynamicProperties;
        }

        /// <summary>
        /// Builds collection of dynamic custom oreder properties fields
        /// </summary>
        /// <returns></returns>
        private Collection<DataGridItemProperty> _GetDynamicCustomOrderProperties(string baseValuePath, bool isReadOnly)
        {
            Collection<DataGridItemProperty> dynamicProperties = new Collection<DataGridItemProperty>();

            OrderCustomPropertiesInfo infos = App.Current.Project.OrderCustomPropertiesInfo;
            for (int i = 0; i < infos.Count; i++)
            {
                string valuePath = baseValuePath + string.Format("CustomProperties[{0}]", i);
                string valueName = OrderCustomProperties.GetCustomPropertyName(i);
                Type type = (infos[i].Type == OrderCustomPropertyType.Text)? typeof(string) : typeof(double);

                DataGridItemProperty newProperty = new DataGridItemProperty(valueName, valuePath, type);
                newProperty.IsReadOnly = isReadOnly;
                dynamicProperties.Add(newProperty);
            }
            return dynamicProperties;
        }

        #endregion

        #region Private Fields

        private ArrayList _itemPorpertiesCollection = new ArrayList();
        private ArrayList _columns = null;
        private DataTemplate _headerTemplate = null;

        private const string ADDRESS_COLUMN_EDITOR_KEY_FORMAT = "{0}Editor";
        private const string ITEM_PROPERTIES_RESOURCE_NAME = "itemProperties";
        private const string COLUMNS_RESOURCE_NAME = "columns";
        private const string PRINT_HEADER_RESOURCE_NAME = "printHeader";
        private const string PRINT_FOOTER_RESOURCE_NAME = "printFooter";

        #endregion
    }
}

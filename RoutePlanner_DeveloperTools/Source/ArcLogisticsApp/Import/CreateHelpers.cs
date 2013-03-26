/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using EsriGeometry = ESRI.ArcGIS.Client.Geometry;
using ArcLogisticsGeometry = ESRI.ArcLogistics.Geometry;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using ESRI.ArcLogistics.Geocoding;
using AppGeometry = ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Class provide helper functions for creation import object.
    /// </summary>
    internal sealed class CreateHelpers : PropertyHelpers
    {
        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates object from DB.
        /// </summary>
        /// <param name="type">Type of import.</param>
        /// <param name="references">Dictionary property name to field position in data source.</param>
        /// <param name="data">Data source provider.</param>
        /// <param name="projectData">The reference to the data of the project to create objects
        /// for.</param>
        /// <param name="defaultDate">Default date.</param>
        /// <returns>Import results.</returns>
        static public ImportResult Create(ImportType type,
                                          Dictionary<string, int> references,
                                          IDataProvider data,
                                          IProjectData projectData,
                                          DateTime defaultDate)
        {
            Debug.Assert(null != references);
            Debug.Assert(null != projectData);
            Debug.Assert(null != defaultDate);
            Debug.Assert(null != data);

            ESRI.ArcLogistics.Data.DataObject objData = _CreateObj(type, defaultDate);

            string[] listSeparators = _GetListSeparators();

            object obj = objData as object;
            ICollection<ImportedValueInfo> descriptions = _InitObjectFromDB(null,
                                                                            listSeparators,
                                                                            references,
                                                                            data,
                                                                            projectData,
                                                                            defaultDate,
                                                                            ref obj);
            return new ImportResult(objData, descriptions);
        }

        /// <summary>
        /// Special initialization for imported object.
        /// </summary>
        /// <typeparam name="T">Application DataObject.</typeparam>
        /// <param name="collection">Application object collection.</param>
        /// <param name="importedObject">Imported object.</param>
        static public void SpecialInit<T>(ICollection<T> collection,
                                          DataObject importedObject)
            where T : DataObject
        {
            if (typeof(T) == typeof(Route))
            {   // special issue for import routes
                //      color set by special algorithm
                var route = importedObject as Route;
                if (route.Color.IsEmpty) // use next color by special algorithm
                {
                    route.Color =
                        RouteColorManager.Instance.NextRouteColor(collection as ICollection<Route>);
                }
                else
                {   // for imported colors Alpha value ignored
                    if (255 != route.Color.A)
                        route.Color = Color.FromArgb(255, route.Color);
                }
            }
            // else Do nothing - other object do not need complementary initialization
        }

        #endregion // Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets text list separators.
        /// </summary>
        /// <returns>List of text list separators.</returns>
        static private string[] _GetListSeparators()
        {
            string currentListSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            var listSeparators = new List<string> ();
            listSeparators.Add(COMMA_SEPARATOR);
            listSeparators.Add(SEMICOLON_SEPARATOR);
            if ((SEMICOLON_SEPARATOR != currentListSeparator) &&
                (COMMA_SEPARATOR != currentListSeparator))
                listSeparators.Add(currentListSeparator);

            return listSeparators.ToArray();
        }

        /// <summary>
        /// Checks is point imported.
        /// </summary>
        /// <param name="pt">Input point.</param>
        /// <returns>TRUE is point update properies during import process.</returns>
        static private bool _IsPointImported(AppGeometry.Point pt)
        {
            return ((pt.X != pt.Y) || (0 != pt.X));
        }

        /// <summary>
        /// Creates object.
        /// </summary>
        /// <param name="type">Import type.</param>
        /// <param name="defaultDate">Default date.</param>
        /// <returns>Created data object.</returns>
        static private DataObject _CreateObj(ImportType type, DateTime defaultDate)
        {
            Debug.Assert(null != defaultDate);

            DataObject obj = null;

            switch (type)
            {
                case ImportType.Orders:
                    Project project = App.Current.Project;
                    obj = new Order(project.CapacitiesInfo, project.OrderCustomPropertiesInfo);
                    break;

                case ImportType.Locations:
                    obj = new Location();
                    break;

                case ImportType.Vehicles:
                    obj = new Vehicle(App.Current.Project.CapacitiesInfo);
                    break;

                case ImportType.Drivers:
                    obj = new Driver();
                    break;

                case ImportType.MobileDevices:
                    obj = new MobileDevice();
                    break;

                case ImportType.DefaultRoutes:
                    Project currentProject = App.Current.Project;
                    obj = currentProject.CreateRoute();
                    break;

                case ImportType.DriverSpecialties:
                    obj = new DriverSpecialty();
                    break;

                case ImportType.VehicleSpecialties:
                    obj = new VehicleSpecialty();
                    break;

                case ImportType.Barriers:
                    obj = CommonHelpers.CreateBarrier(defaultDate);
                    break;

                case ImportType.Zones:
                    obj = new Zone();
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported type
                    break;
            }

            return obj;
        }

        /// <summary>
        /// Converts readed value to color.
        /// </summary>
        /// <param name="readValue">Readed value.</param>
        /// <returns>Color object or null if convert failed.</returns>
        static private object _Convert2Color(string readValue)
        {
            object obj = null;

            try
            {   // try first format
                obj = ColorTranslator.FromHtml(readValue);
            }
            catch { }

            if (null == obj)
            {   // try other format
                try
                {
                    string value = readValue.Replace(',', '.');
                    obj = ColorTranslator.FromOle((int)double.Parse(value));
                }
                catch { }
            }

            if (null == obj)
            {   // last chance
                string value = readValue.Replace(',', '.');
                obj = ColorTranslator.FromWin32((int)double.Parse(value));
            }

            return obj;
        }

        /// <summary>
        /// Finds first letter position in readed text.
        /// </summary>
        /// <param name="text">Text for checking.</param>
        /// <returns>-1 if letter not founded or position in input text.</returns>
        static private int _FindFirstLetterPosition(string text)
        {
            Debug.Assert(!string.IsNullOrEmpty(text));

            // find first letter
            int textPosition = -1;
            NumberFormatInfo nfi = CultureInfo.CurrentCulture.NumberFormat;
            string currencySymbol = nfi.CurrencySymbol;

            for (int index = 0; index < text.Length; ++index)
            {
                char curChar = text[index];
                if (char.IsLetter(curChar) || char.IsWhiteSpace(curChar) ||
                    currencySymbol.StartsWith(new string(new char[] {curChar}),
                                              StringComparison.CurrentCultureIgnoreCase))
                {
                    textPosition = index;
                    break; // result founded
                }
            }

            return textPosition;
        }

        /// <summary>
        /// Converts value to double value.
        /// </summary>
        /// <param name="value">Readed value.</param>
        /// <param name="unitAttribute">Unit Attribute.</param>
        /// <returns>Numeric value.</returns>
        static private double _ConvertToDouble(string value, UnitPropertyAttribute unitAttribute)
        {
            Debug.Assert(!string.IsNullOrEmpty(value));

            double numValue = 0.0;

            NumberFormatInfo nfi = CultureInfo.CurrentCulture.NumberFormat;
            value = value.Replace(nfi.NumberDecimalSeparator, ".");
            var nfiClone = nfi.Clone() as NumberFormatInfo;
            nfiClone.NumberDecimalSeparator = ".";

            // split readed text to numeric and unit symbol
            int textPosition = _FindFirstLetterPosition(value);

            if ((0 == textPosition) && (unitAttribute.ValueUnits == Unit.Currency))
            {   // special case for Currency (value must start with Currency symbol)
                numValue = double.Parse(value, NumberStyles.Any, nfiClone);
            }
            else
            {
                string unitSymbol = null;
                string numeric = null;
                if (-1 == textPosition)
                    numeric = value;
                else
                {
                    if (0 == textPosition)
                        throw new NotSupportedException();

                    char[] chars = value.ToCharArray();
                    numeric = new string(chars, 0, textPosition);
                    unitSymbol = new string(chars, textPosition, value.Length - textPosition);
                }

                // convert numeric
                numValue = double.Parse(numeric, NumberStyles.Any, nfiClone);

                if (null == unitAttribute)
                {
                    if (!string.IsNullOrEmpty(unitSymbol))
                        throw new NotSupportedException();
                }
                else
                {   // conver to application unit
                    Unit unit = Unit.Unknown;
                    if (!string.IsNullOrEmpty(unitSymbol))
                        unit = UnitFormatter.GetUnitBySymbol(unitSymbol, unitAttribute.ValueUnits);
                    else
                        unit = (RegionInfo.CurrentRegion.IsMetric) ?
                                    unitAttribute.DisplayUnitMetric : unitAttribute.DisplayUnitUS;
                    numValue = UnitConvertor.Convert(numValue, unit, unitAttribute.ValueUnits);
                }
            }

            return numValue;
        }

        /// <summary>
        /// Converts readed value to property type.
        /// </summary>
        /// <param name="value">Readed value.</param>
        /// <param name="type">Property type.</param>
        /// <param name="unitAttribute">Unit attribute.</param>
        /// <returns>Property type value or null if convert failed.</returns>
        static private object _ConvertValue(object value, Type type,
                                            UnitPropertyAttribute unitAttribute)
        {
            Debug.Assert(null != value);

            object obj = null;
            try
            {
                string text = value.ToString().Trim(); // convert readed value to string
                if (typeof(string) == type)
                    obj = text;

                else if (typeof(double) == type)
                    obj = _ConvertToDouble(text, unitAttribute);

                else if (typeof(TimeSpan) == type)
                {
                    double minutes = 0;
                    if (double.TryParse(text, out minutes))
                        obj = new TimeSpan((long)(minutes * TimeSpan.TicksPerMinute));
                    else
                    {
                        TimeSpan time;
                        if (TimeSpan.TryParse(text, out time))
                            obj = time;
                        else
                            obj = DateTime.Parse(text).TimeOfDay;
                    }
                }

                else if (typeof(DateTime) == type)
                    obj = DateTime.Parse(text);

                else if (typeof(Color) == type)
                    obj = _Convert2Color(text);

                else if ((typeof(SyncType) == type) ||
                         (typeof(OrderPriority) == type) ||
                         (typeof(OrderType) == type))
                    obj = CustomEnumParser.Parse(type, text);

                else
                {   // other type (Enums, etc.)
                    TypeConverter tc = TypeDescriptor.GetConverter(type);
                    Debug.Assert(tc.CanConvertFrom(typeof(string)));
                    obj = tc.ConvertFromInvariantString(text);
                }
            }
            catch
            { }

            return obj;
        }

        /// <summary>
        /// Reads value from data source.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <param name="references">Dictionary property name to field position in data source.</param>
        /// <param name="data">Data source provider.</param>
        /// <returns>Readed value or null if can't read or value is DBNull.</returns>
        static private object _ReadValue(string name, Dictionary<string, int> references,
                                         IDataProvider data)
        {
            object value = null;
            int pos = 0;
            if (references.TryGetValue(name, out pos))
            {
                object fieldValue = data.FieldValue(pos);
                if (!(fieldValue is DBNull))
                    value = fieldValue;
            }

            return value;
        }

        /// <summary>
        /// Gets value form data source. With detect conversion problem.
        /// </summary>
        /// <param name="type">Property type.</param>
        /// <param name="name">Field name.</param>
        /// <param name="unitAttribute">Unit attribute.</param>
        /// <param name="references">Dictionary property name to field position in data source.</param>
        /// <param name="data">Data source provider.</param>
        /// <returns>Imported value description.</returns>
        static private ImportedValueInfo _GetValue(Type type, string name,
                                                   UnitPropertyAttribute unitAttribute,
                                                   Dictionary<string, int> references,
                                                   IDataProvider data)
        {
            // read value
            object readedValue = _ReadValue(name, references, data);

            // convert to property type
            object obj = null;
            var status = ImportedValueStatus.Empty;
            if (null != readedValue)
            {
                obj = _ConvertValue(readedValue, type, unitAttribute);
                status = (null == obj)? ImportedValueStatus.Failed : ImportedValueStatus.Valid;
            }

            return new ImportedValueInfo(name, readedValue as string, obj, status);
        }

        /// <summary>
        /// Create relative object with readed name.
        /// </summary>
        /// <typeparam name="T">DataObject type (only relative).</typeparam>
        /// <param name="name">Object init name.</param>
        /// <returns>Created data object.</returns>
        static private T _CreateObject<T>(string name)
            where T : DataObject
        {
            return (T)_CreateObject(typeof(T), name);
        }

        /// <summary>
        /// Create relative object with readed name.
        /// </summary>
        /// <param name="type">DataObject type (only relative).</param>
        /// <param name="name">Object init name.</param>
        /// <returns>Created data object.</returns>
        static private DataObject _CreateObject(Type type, string name)
        {
            DataObject createdObject = null;
            if (typeof(DriverSpecialty) == type)
            {
                var driverSpec = new DriverSpecialty();
                driverSpec.Name = name;
                createdObject = driverSpec;
            }

            else if (typeof(VehicleSpecialty) == type)
            {
                var vehicleSpec = new VehicleSpecialty();
                vehicleSpec.Name = name;
                createdObject = vehicleSpec;
            }

            else if (typeof(Location) == type)
            {
                var fuelType = new Location();
                fuelType.Name = name;
                createdObject = fuelType;
            }

            else if (typeof(Zone) == type)
            {
                var zone = new Zone();
                zone.Name = name;
                createdObject = zone;
            }

            else if (typeof(Driver) == type)
            {
                var driver = new Driver();
                driver.Name = name;
                createdObject = driver;
            }

            else if (typeof(Vehicle) == type)
            {
                var vehicle = new Vehicle(App.Current.Project.CapacitiesInfo);
                vehicle.Name = name;
                createdObject = vehicle;
            }

            else if (typeof(MobileDevice) == type)
            {
                var mobileDevice = new MobileDevice();
                mobileDevice.Name = name;
                createdObject = mobileDevice;
            }

            else if (typeof(FuelType) == type)
            {
                var fuelType = new FuelType();
                fuelType.Name = name;
                createdObject = fuelType;
            }

            else
            {
                Debug.Assert(false); // NOTE: not supported
            }

            return createdObject;
       }

        /// <summary>
        /// Inits related object.
        /// </summary>
        /// <typeparam name="T">DataObject type.</typeparam>
        /// <param name="importedName">Readed initialization object name.</param>
        /// <param name="appCollection">Application object collection.</param>
        /// <returns>Data object.</returns>
        static private T _InitRelatedObject<T>(string importedName,
                                               IDataObjectContainer<T> appCollection)
            where T : DataObject
        {
            Debug.Assert(!string.IsNullOrEmpty(importedName));

            // find object with input name
            T importedObj = null;
            if (!appCollection.TryGetValue(importedName, out importedObj))
            {
                importedObj = _CreateObject<T>(importedName);
                appCollection.Add(importedObj);
            }

            return importedObj;
        }

        /// <summary>
        /// Sets object property.
        /// </summary>
        /// <typeparam name="T">Readed value type.</typeparam>
        /// <param name="value">Readed value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="obj">Parent object.</param>
        static private void _SetObjectProperty<T>(T value, string propertyName, ref object obj)
        {
            bool isValueSet = false;
            Type type = obj.GetType();

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (Attribute.IsDefined(property, typeof(DomainPropertyAttribute)))
                {
                    Type propertyType = _GetEffectiveType(property.PropertyType);
                    if (value.GetType() != propertyType)
                        continue; // do not - check next

                    if (!string.IsNullOrEmpty(propertyName) && (propertyName != property.Name))
                        continue; // do not - check next

                    property.SetValue(obj, value, null);
                    isValueSet = true;
                    break;
                }
            }

            Debug.Assert(isValueSet);
        }

        /// <summary>
        /// Imports related object.
        /// </summary>
        /// <typeparam name="T">Related oject type.</typeparam>
        /// <param name="propertyName">Property name.</param>
        /// <param name="references">Dictionary property name to field position in data source.</param>
        /// <param name="data">Data source provider.</param>
        /// <param name="appCollection">Related object collecion.</param>
        /// <param name="obj">Parent object.</param>
        /// <returns>Imported value description.</returns>
        static private ImportedValueInfo _ImportRelatedObject<T>(string propertyName,
                                                                 Dictionary<string, int> references,
                                                                 IDataProvider data,
                                                                 IDataObjectContainer<T> appCollection,
                                                                 ref object obj)
            where T : DataObject
        {
            ImportedValueInfo description = _GetValue(typeof(string), propertyName, null,
                                                      references, data);
            string importedName = description.Value as string;
            if (!string.IsNullOrEmpty(importedName))
            {
                Debug.Assert(!string.IsNullOrEmpty(propertyName));

                DataObject importedObj = _InitRelatedObject(importedName, appCollection);
                _SetObjectProperty(importedObj, propertyName, ref obj);
            }

            return description;
        }

        /// <summary>
        /// Adds new readed object collection to application collection.
        /// </summary>
        /// <typeparam name="T">Related object type.</typeparam>
        /// <param name="newObjects">New readed object collection.</param>
        /// <param name="collection">Application collection.</param>
        static private void _AddObjects2Collection<T>(IEnumerable<T> newObjects,
                                                      ICollection<T> collection)
        {
            foreach (T obj in newObjects)
                collection.Add(obj);
        }

        /// <summary>
        /// Inits related objects.
        /// </summary>
        /// <typeparam name="T">Related object type.</typeparam>
        /// <param name="importedNames">Readed relative object collection name.</param>
        /// <param name="appCollection">Application collection.</param>
        /// <returns>Imported object collection.</returns>
        static private ICollection<T> _InitRelatedObjects<T>(string[] importedNames,
                                                             IDataObjectContainer<T> appCollection)
            where T : DataObject
        {
            Collection<T> importedObjects = new Collection<T>();

            for (int i = 0; i < importedNames.Length; ++i)
            {
                string name = importedNames[i].Trim();
                if (string.IsNullOrEmpty(name))
                    continue; // skip empty names

                // find in application collection
                T dataObject;
                if (!appCollection.TryGetValue(name, out dataObject))
                {
                    dataObject = _CreateObject<T>(name);
                    appCollection.Add(dataObject);
                }

                importedObjects.Add(dataObject);
            }

            return importedObjects;
        }

        /// <summary>
        /// Imports related objects
        /// </summary>
        /// <param name="propertyType">Related property type.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="listSeparators">Text list separators.</param>
        /// <param name="references">Dictionary property name to field position in data source.</param>
        /// <param name="data">Data source provider.</param>
        /// <param name="projectData">Project data.</param>
        /// <param name="obj">Parent object.</param>
        /// <returns>Imported value description.</returns>
        static private ImportedValueInfo _ImportRelatedObjects(Type propertyType,
                                                               string propertyName,
                                                               string[] listSeparators,
                                                               Dictionary<string, int> references,
                                                               IDataProvider data,
                                                               IProjectData projectData,
                                                               ref object obj)
        {
            // read related objects property name list
            ImportedValueInfo description = _GetValue(typeof(string),
                                                      propertyName,
                                                      null,
                                                      references,
                                                      data);
            string namesList = description.Value as string;
            if (!string.IsNullOrEmpty(namesList))
            {
                string[] importedNames = namesList.Split(listSeparators,
                                                         StringSplitOptions.RemoveEmptyEntries);
                if (0 < importedNames.Length)
                {
                    if (typeof(IDataObjectCollection<VehicleSpecialty>) == propertyType)
                    {
                        ICollection<VehicleSpecialty> importSpecialities =
                            _InitRelatedObjects(importedNames,
                                                projectData.GetDataObjects<VehicleSpecialty>());
                        if (obj is Order)
                        {
                            _AddObjects2Collection(importSpecialities,
                                                   (obj as Order).VehicleSpecialties);
                        }
                        else if (obj is Vehicle)
                        {
                            _AddObjects2Collection(importSpecialities, (obj as Vehicle).Specialties);
                        }
                        else
                        {
                            Debug.Assert(false); // NOTE: not supported
                        }
                    }

                    else if (typeof(IDataObjectCollection<DriverSpecialty>) == propertyType)
                    {
                        ICollection<DriverSpecialty> importSpecialities =
                            _InitRelatedObjects(importedNames,
                                                projectData.GetDataObjects<DriverSpecialty>());
                        if (obj is Order)
                        {
                            _AddObjects2Collection(importSpecialities,
                                                   (obj as Order).DriverSpecialties);
                        }
                        else if (obj is Driver)
                        {
                            _AddObjects2Collection(importSpecialities, (obj as Driver).Specialties);
                        }
                        else
                        {
                            Debug.Assert(false); // NOTE: not supported
                        }
                    }

                    else if (typeof(IDataObjectCollection<Location>) == propertyType)
                    {
                        ICollection<Location> importLocations = _InitRelatedObjects(
                            importedNames,
                            projectData.GetDataObjects<Location>());
                        if (obj is Route)
                            _AddObjects2Collection(importLocations, (obj as Route).RenewalLocations);
                        else
                        {
                            Debug.Assert(false); // NOTE: not supported
                        }
                    }

                    else if (typeof(IDataObjectCollection<Zone>) == propertyType)
                    {
                        ICollection<Zone> importZones =
                            _InitRelatedObjects(importedNames, projectData.GetDataObjects<Zone>());
                        if (obj is Route)
                            _AddObjects2Collection(importZones, (obj as Route).Zones);
                        else
                        {
                            Debug.Assert(false); // NOTE: not supported
                        }
                    }

                    else
                    {
                        Debug.Assert(false); // NOTE: not supported
                    }
                }
            }

            return description;
        }

        /// <summary>
        /// Checks is special geometry property.
        /// </summary>
        /// <param name="obj">Parent object.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>TRUE if property is "Geometry".</returns>
        static private bool _IsSpecialGeometryProperty(object obj, string propertyName)
        {
            Debug.Assert(!string.IsNullOrEmpty(propertyName));
            return (("Geometry" == propertyName) &&
                    ((obj is Zone) || (obj is Barrier)));
        }

        /// <summary>
        /// Inits geometry.
        /// </summary>
        /// <param name="property">Property info.</param>
        /// <param name="data">Data source provider.</param>
        /// <param name="obj">Parent object.</param>
        static private void _InitGeometry(PropertyInfo property, IDataProvider data, ref object obj)
        {
            if (data.IsGeometrySupport)
            {
                if (null != data.Geometry)
                {
                    object geometry = null;
                    if (data.Geometry is ArcLogisticsGeometry.Point)
                        geometry = (ArcLogisticsGeometry.Point)data.Geometry;
                    else if (data.Geometry is ArcLogisticsGeometry.Polygon)
                        geometry = data.Geometry as ArcLogisticsGeometry.Polygon;
                    else if (data.Geometry is ArcLogisticsGeometry.Polyline)
                        geometry = data.Geometry as ArcLogisticsGeometry.Polyline;
                    else
                    {
                        Debug.Assert(false); // NOTE: not supported
                    }

                    if (null != geometry)
                        property.SetValue(obj, geometry, null);
               }
            }
        }

        /// <summary>
        /// Inits system type property.
        /// </summary>
        /// <param name="propertyType">Property type.</param>
        /// <param name="name">Property name.</param>
        /// <param name="unitAttribute">Unit attribute.</param>
        /// <param name="references">Dictionary property name to field position in data source.</param>
        /// <param name="data">Data source provider.</param>
        /// <param name="property">Property info.</param>
        /// <param name="defaultDate">Default date.</param>
        /// <param name="obj">Parent object.</param>
        /// <returns>Imported value description.</returns>
        static private ImportedValueInfo _InitSystemTypeProperty(Type propertyType, string name,
                                                                 UnitPropertyAttribute unitAttribute,
                                                                 Dictionary<string, int> references,
                                                                 IDataProvider data,
                                                                 PropertyInfo property,
                                                                 DateTime defaultDate,
                                                                 ref object obj)
        {
            ImportedValueInfo description = _GetValue(propertyType,
                                                      name,
                                                      unitAttribute,
                                                      references,
                                                      data);
            // NOTE: special case - if date not set use current date
            object value = description.Value;
            if ((null == value) && (typeof(DateTime) == propertyType))
            {
                if ("FinishDate" == property.Name)
                {   // NOTE: special routine for Barrier.FinishDate
                    var barrier = obj as Barrier;
                    Debug.Assert(null != barrier);
                    DateTime date = barrier.StartDate;
                    barrier.FinishDate = date.AddDays(1);
                }
                else
                    value = defaultDate;
            }

            if (null != value)
                property.SetValue(obj, value, null);

            return description;
        }

        /// <summary>
        /// Inits custom order properties.
        /// </summary>
        /// <param name="references">Dictionary property name to field position in data source.</param>
        /// <param name="data">Data source provider.</param>
        /// <param name="property">Property info.</param>
        /// <param name="obj">Parent object.</param>
        /// <returns>Collection of imported value description.</returns>
        static private ICollection<ImportedValueInfo> _InitOrderCustomProperties(Dictionary<string, int> references,
                                                                                 IDataProvider data,
                                                                                 PropertyInfo property,
                                                                                 ref object obj)
        {
            var descriptions = new List<ImportedValueInfo>();

            OrderCustomPropertiesInfo infos = App.Current.Project.OrderCustomPropertiesInfo;
            var customProperties = new OrderCustomProperties(infos);
            for (int i = 0; i < infos.Count; ++i)
            {
                bool isPropertyText = (infos[i].Type == OrderCustomPropertyType.Text);

                Type type = isPropertyText? typeof(string) : typeof(double);
                ImportedValueInfo description = _GetValue(type,
                                                          OrderCustomProperties.GetCustomPropertyName(i),
                                                          null, references, data);
                if (isPropertyText)
                    customProperties[i] = (string)description.Value;
                else
                {   // OrderCustomPropertyType.Numeric
                    object val = description.Value;
                    double value = 0.0;
                    if (null != val)
                        value = (double)val;
                    customProperties[i] = value;
                }

                descriptions.Add(description);
            }

            property.SetValue(obj, customProperties, null);

            return descriptions.AsReadOnly();
        }

        /// <summary>
        /// Inits capacities.
        /// </summary>
        /// <param name="references">Dictionary property name to field position in data source.</param>
        /// <param name="data">Data source provider.</param>
        /// <param name="property">Property info.</param>
        /// <param name="obj">Parent object.</param>
        /// <returns>Collection of imported value description.</returns>
        static private ICollection<ImportedValueInfo> _InitCapacities(Dictionary<string, int> references,
                                                                      IDataProvider data,
                                                                      PropertyInfo property,
                                                                      ref object obj)
        {
            var descriptions = new List<ImportedValueInfo>();

            CapacitiesInfo infos = App.Current.Project.CapacitiesInfo;
            var capacities = new Capacities(infos);
            for (int i = 0; i < infos.Count; ++i)
            {
                CapacityInfo info = infos[i];

                Unit unit = (RegionInfo.CurrentRegion.IsMetric)?
                                info.DisplayUnitMetric : info.DisplayUnitUS;
                var unitAttribute = new UnitPropertyAttribute(unit,
                                                              info.DisplayUnitUS,
                                                              info.DisplayUnitMetric);

                ImportedValueInfo description = _GetValue(typeof(double),
                                                          Capacities.GetCapacityPropertyName(i),
                                                          unitAttribute, references, data);
                if (null != description.Value) // NOTE: null not supported
                    capacities[i] = (double)description.Value;

                descriptions.Add(description);
            }

            property.SetValue(obj, capacities, null);

            return descriptions.AsReadOnly();
        }

        /// <summary>
        /// Replaces space sequence to space.
        /// </summary>
        /// <param name="input">String to replace.</param>
        /// <returns>String with removed spaces sequences.</returns>
        private static string _RemoveDuplicateWhiteSpace(string input)
        {
            string result = string.Empty;
            if (null != input)
            {
                var sb = new StringBuilder();
                char[] separators = new char[] { SPACE };
                string[] parts = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                    sb.AppendFormat(WORD_WITH_SPACE_FMT, parts[i].Trim());
                result = sb.ToString().Trim();
            }

            return result;
        }

        /// <summary>
        /// Inits address.
        /// </summary>
        /// <param name="references">Dictionary property name to field position in data source.</param>
        /// <param name="data">Data source provider.</param>
        /// <param name="property">Property info.</param>
        /// <param name="obj">Parent object.</param>
        /// <returns>Collection of imported value description.</returns>
        static private ICollection<ImportedValueInfo> _InitAddress(Dictionary<string, int> references,
                                                                   IDataProvider data,
                                                                   PropertyInfo property,
                                                                   ref object obj)
        {
            var descriptions = new List<ImportedValueInfo>();

            var address = new Address();
            AddressField[] fields = App.Current.Geocoder.AddressFields;
            for (int i = 0; i < fields.Length; ++i)
            {
                ImportedValueInfo description = _GetValue(typeof(string),
                                                          fields[i].Type.ToString(),
                                                          null,
                                                          references,
                                                          data);
                string value = (string)description.Value;
                address[fields[i].Type] = _RemoveDuplicateWhiteSpace(value);

                descriptions.Add(description);
            }

            property.SetValue(obj, address, null);

            return descriptions;
        }

        /// <summary>
        /// Checks is properties changed.
        /// </summary>
        /// <param name="obj">Imported object.</param>
        /// <returns>TRUE if properties of object changed after creation.</returns>
        static private bool _IsPropertiesChanged(object obj)
        {
            Type type = obj.GetType();
            object defaultObject = Activator.CreateInstance(type); // create new object

            bool isChanges = false;

            PropertyInfo[] sourceProperties = type.GetProperties();
            foreach (PropertyInfo pi in sourceProperties)
            {
                object defValue = type.GetProperty(pi.Name).GetValue(defaultObject, null);
                object curValue = type.GetProperty(pi.Name).GetValue(obj, null);
                if (((null == defValue) && (null != curValue)) ||
                    ((null != defValue) && (null == curValue)) ||
                    ((null != defValue) && (null != curValue) &&
                     (defValue.ToString() != curValue.ToString())))
                {
                    isChanges = true;
                    break; // NOTE: stop process - result founded
                }
            }

            return isChanges;
        }

        /// <summary>
        /// Post inits property.
        /// </summary>
        /// <param name="property">Property.</param>
        static private void _PostInitProperty(object property)
        {
            if (typeof(TimeWindow) == property.GetType())
            {
                TimeWindow timeWindow = property as TimeWindow;
                TimeSpan timeEmpty = new TimeSpan();
                timeWindow.IsWideOpen = ((timeEmpty == timeWindow.From) && (timeEmpty == timeWindow.To));

                timeWindow.Day = Math.Min(MAX_TIME_WINDOW_DAY, timeWindow.Day);
            }
        }

        /// <summary>
        /// Inits new instance.
        /// </summary>
        /// <param name="propertyType">Property type.</param>
        /// <param name="propertySub">Sub property object.</param>
        /// <param name="data">Data source provider.</param>
        /// <param name="property">Property info.</param>
        /// <param name="obj">Parent object.</param>
        static private void _InitNewInstance(Type propertyType, object propertySub, IDataProvider data,
                                             PropertyInfo property, ref object obj)
        {
            if (typeof(AppGeometry.Point) == propertyType)
            {   // special case for Point
                if (_IsPointImported((AppGeometry.Point)propertySub))
                    property.SetValue(obj, propertySub, null);
                else if (data.IsGeometrySupport)
                {
                    if (null != data.Geometry)
                    {
                        Debug.Assert(data.Geometry is ArcLogisticsGeometry.Point);
                        propertySub = (ArcLogisticsGeometry.Point)data.Geometry;
                        property.SetValue(obj, propertySub, null);
                    }
                }
            }
            else if ((typeof(AppGeometry.Polygon) == propertyType) &&
                     data.IsGeometrySupport)
            {   // special case for Polygon
                if (null != data.Geometry)
                {
                    Debug.Assert(data.Geometry is ArcLogisticsGeometry.Polygon);
                    propertySub = data.Geometry as ArcLogisticsGeometry.Polygon;
                    property.SetValue(obj, propertySub, null);
                }
            }
            else
            {   // other types
                _PostInitProperty(propertySub);
                if (_IsPropertiesChanged(propertySub))
                    property.SetValue(obj, propertySub, null);
            }
        }

        /// <summary>
        /// Gets property name.
        /// </summary>
        /// <param name="expression">LinQ expression.</param>
        /// <returns>Propery name.</returns>
        private static string _GetPropertyName<T, TReturn>(Expression<Func<T, TReturn>> expression)
        {
            var body = expression.Body as MemberExpression;
            Debug.Assert(null != body);
            return body.Member.Name;
        }

        /// <summary>
        /// Post validates related properties.
        /// </summary>
        /// <param name="firstPropertyName">First property name to validation.</param>
        /// <param name="secondPropertyName">Secind property name to validation.</param>
        /// <param name="descriptions">Collection of imported value description.</param>
        /// <param name="obj">Parent object.</param>
        private static void _ValidateRelatedProperties(string firstPropertyName,
                                                       string secondPropertyName,
                                                       ICollection<ImportedValueInfo> descriptions,
                                                       ref object obj)
        {
            bool isReadedFirst = false;
            bool isReadedSecond = false;
            foreach (ImportedValueInfo description in descriptions)
            {
                if (description.Name == firstPropertyName)
                    isReadedFirst = (description.Status == ImportedValueStatus.Valid);
                else if (description.Name == secondPropertyName)
                    isReadedSecond = (description.Status == ImportedValueStatus.Valid);
            }

            if (isReadedFirst != isReadedSecond)
            {   // only if one of related properties invalid imported
                PropertyInfo firstPi = obj.GetType().GetProperty(firstPropertyName);
                Debug.Assert(firstPi.PropertyType == typeof(double));
                double firstValue = (double)firstPi.GetValue(obj, null);

                PropertyInfo secondPi = obj.GetType().GetProperty(secondPropertyName);
                Debug.Assert(secondPi.PropertyType == typeof(double));
                double secondValue = (double)secondPi.GetValue(obj, null);

                if (firstValue < secondValue)
                {   // and values compartion not valid
                    if (isReadedFirst) // set not mapped property from readed
                        secondPi.SetValue(obj, firstValue, null);
                    else
                        firstPi.SetValue(obj, secondValue, null);
                }
            }
        }

        /// <summary>
        /// Post inits object.
        /// </summary>
        /// <param name="descriptions">Collection of imported value description.</param>
        /// <param name="obj">Imported object.</param>
        static private void _PostInitObject(ICollection<ImportedValueInfo> descriptions,
                                            ref object obj)
        {
            // special cases: see PropertyComparisonValidator
            if (obj is Route)
            {   // post init for route
                string maxTravelDurationName = _GetPropertyName((Route r) => r.MaxTravelDuration);
                string maxTotalDurationName = _GetPropertyName((Route r) => r.MaxTotalDuration);
                _ValidateRelatedProperties(maxTotalDurationName, maxTravelDurationName,
                                           descriptions, ref obj);

                var route = obj as Route;
                if (route.StartTimeWindow.IsWideOpen) // route start time window can't sets as wide open
                    route.StartTimeWindow.IsWideOpen = false;
            }
            else if (obj is Driver)
            {   // post init for driver
                string perHourOTSalaryName = _GetPropertyName((Driver d) => d.PerHourOTSalary);
                string perHourSalaryName = _GetPropertyName((Driver d) => d.PerHourSalary);
                _ValidateRelatedProperties(perHourOTSalaryName, perHourSalaryName,
                                           descriptions, ref obj);
            }
            else if (obj is Order)
            {
                Order order = obj as Order;
                // We need to discard time component of order planned date if it was to set
                // to the value other than "12:00 AM" in the source of import.
                // Otherwise the problem appears on looking for orders assigned to the given date.
                if (order.PlannedDate.HasValue)
                {
                    order.PlannedDate = order.PlannedDate.Value.Date;
                }
            }
            // else Do nothing
        }

        /// <summary>
        /// Inits object from data sources.
        /// </summary>
        /// <param name="name">Property name (can be null [for parent object]).</param>
        /// <param name="listSeparators">Text list separators.</param>
        /// <param name="references">Dictionary property name to field position in data source.</param>
        /// <param name="data">Data source provider.</param>
        /// <param name="projectData">Project's data.</param>
        /// <param name="defaultDate">Default date.</param>
        /// <param name="obj">Object to import properties.</param>
        /// <returns>Collection of imported value description.</returns>
        static private ICollection<ImportedValueInfo> _InitObjectFromDB(string name,
                                                                        string[] listSeparators,
                                                                        Dictionary<string, int> references,
                                                                        IDataProvider data,
                                                                        IProjectData projectData,
                                                                        DateTime defaultDate,
                                                                        ref object obj)
        {
            //
            // NOTE: function use recursion
            //
            var descriptions = new List<ImportedValueInfo>();

            Type type = obj.GetType();
            Type attributeType = typeof(DomainPropertyAttribute);

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                bool isGeometryProperty = _IsSpecialGeometryProperty(obj, property.Name);
                if (Attribute.IsDefined(property, attributeType) || isGeometryProperty)
                {
                    if (isGeometryProperty)
                        // NOTE: special case
                        _InitGeometry(property, data, ref obj);

                    else
                    {
                        Type propertyType = _GetEffectiveType(property.PropertyType);
                        bool isPrefixed = _IsPrefixed(type, name);
                        if (_IsSystemType(propertyType))
                        {
                            var unitAttribute =
                                Attribute.GetCustomAttribute(property, typeof(UnitPropertyAttribute))
                                    as UnitPropertyAttribute;

                            string fullName = _AssemblyPropertyFullName(property.Name,
                                                                        isPrefixed,
                                                                        name);
                            descriptions.Add(_InitSystemTypeProperty(propertyType,
                                                                     fullName,
                                                                     unitAttribute,
                                                                     references,
                                                                     data,
                                                                     property,
                                                                     defaultDate,
                                                                     ref obj));
                        }

                        else if (typeof(OrderCustomProperties) == propertyType)
                        {
                            descriptions.AddRange(_InitOrderCustomProperties(references,
                                                                             data,
                                                                             property,
                                                                             ref obj));
                        }

                        else if (typeof(Capacities) == propertyType)
                        {
                            descriptions.AddRange(_InitCapacities(references,
                                                                  data,
                                                                  property,
                                                                  ref obj));
                        }

                        else if (typeof(Address) == propertyType)
                        {
                            descriptions.AddRange(_InitAddress(references,
                                                               data,
                                                               property,
                                                               ref obj));
                        }

                        else if (typeof(Driver) == propertyType)
                        {
                            descriptions.Add(_ImportRelatedObject(property.Name,
                                                                  references,
                                                                  data,
                                                                  projectData.GetDataObjects<Driver>(),
                                                                  ref obj));
                        }
                        else if (typeof(Vehicle) == propertyType)
                        {
                            descriptions.Add(_ImportRelatedObject(property.Name,
                                                                  references,
                                                                  data,
                                                                  projectData.GetDataObjects<Vehicle>(),
                                                                  ref obj));
                        }
                        else if (typeof(MobileDevice) == propertyType)
                        {
                            descriptions.Add(_ImportRelatedObject(property.Name,
                                                                  references,
                                                                  data,
                                                                  projectData.GetDataObjects<MobileDevice>(),
                                                                  ref obj));
                        }
                        else if (typeof(FuelType) == propertyType)
                        {
                            descriptions.Add(_ImportRelatedObject(property.Name,
                                                                  references,
                                                                  data,
                                                                  projectData.GetDataObjects<FuelType>(),
                                                                  ref obj));
                        }
                        else if (typeof(Location) == propertyType)
                        {
                            descriptions.Add(_ImportRelatedObject(property.Name,
                                                                  references,
                                                                  data,
                                                                  projectData.GetDataObjects<Location>(),
                                                                  ref obj));
                        }
                        else if ((typeof(IDataObjectCollection<VehicleSpecialty>) == propertyType) ||
                                 (typeof(IDataObjectCollection<DriverSpecialty>) == propertyType) ||
                                 (typeof(IDataObjectCollection<Location>) == propertyType) ||
                                 (typeof(IDataObjectCollection<Zone>) == propertyType))
                            descriptions.Add(_ImportRelatedObjects(propertyType,
                                                                   property.Name,
                                                                   listSeparators,
                                                                   references,
                                                                   data,
                                                                   projectData,
                                                                   ref obj));

                        else
                        {   // application internal declaration type
                            object propertySub = property.GetValue(obj, null);
                            // NOTE: if type nullable, GetValue() return NULL
                            bool isNewInstance = false;
                            if ((null == propertySub) || (typeof(TimeWindow) == propertyType))
                            {   // NOTE: for TimeWindow init need special routine -
                                //       force create new instance
                                propertySub = Activator.CreateInstance(propertyType);
                                isNewInstance = true;
                            }

                            Debug.Assert(null != propertySub);
                            string fullName = _AssemblyPropertyFullName(property.Name,
                                                                        isPrefixed,
                                                                        name);

                            ICollection<ImportedValueInfo> infos =
                                _InitObjectFromDB(fullName,
                                                  listSeparators,
                                                  references,
                                                  data,
                                                  projectData,
                                                  defaultDate,
                                                  ref propertySub);
                            descriptions.AddRange(infos);

                            if (isNewInstance)
                                _InitNewInstance(propertyType, propertySub, data, property, ref obj);
                        }
                    }
                }
            }

            ICollection<ImportedValueInfo> descrs = descriptions.AsReadOnly();
            _PostInitObject(descrs, ref obj);

            return descrs;
        }

        #endregion // Private methods

        #region Const definition
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Word with space format.
        /// </summary>
        private const string WORD_WITH_SPACE_FMT = "{0} ";
        /// <summary>
        /// Semicolon separator list text separator.
        /// </summary>
        private const string SEMICOLON_SEPARATOR = ";";
        /// <summary>
        /// Comma separator list text separator.
        /// </summary>
        private const string COMMA_SEPARATOR = ",";
        /// <summary>
        /// Space char.
        /// </summary>
        private const char SPACE = ' ';

        /// <summary>
        /// Maximum value of time window day.
        /// </summary>
        private const int MAX_TIME_WINDOW_DAY = 1;

        #endregion // Const definition
    }
}

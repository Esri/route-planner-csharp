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

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using ESRI.ArcLogistics.Geocoding;
using AppGeometry = ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Class provide helper functions for import process.
    /// </summary>
    internal abstract class PropertyHelpers
    {
        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Checks is geocoding used for this <see cref="ImportType"/>.
        /// </summary>
        /// <param name="type">Type for check.</param>
        /// <returns>TRUE if geocoding used.</returns>
        static public bool IsGeocodeSupported(ImportType type)
        {
            Type objType = _Convert2ObjectType(type);
            return typeof(IGeocodable).IsAssignableFrom(objType);
        }

        /// <summary>
        /// Gets collection of destination property for import.
        /// </summary>
        /// <param name="type">Type of import.</param>
        /// <param name="isSourceShape">Flag show is source shape file.</param>
        /// <returns>Collection of destination property.</returns>
        static public ICollection<ObjectDataFieldInfo> GetDestinationProperties(ImportType type,
                                                                                bool isSourceShape)
        {
            var list = new List<ObjectDataFieldInfo>();
            _GetFieldInfos(_Convert2ObjectType(type), null, isSourceShape, ref list);

            _ReorderFieldInfos(type, list);

            return list.AsReadOnly();
        }

        /// <summary>
        /// Gets collection of destination property title for import.
        /// </summary>
        /// <param name="type">Type of import.</param>
        /// <param name="isSourceShape">Flag show is source shape file.</param>
        /// <returns>Collection of destination property title.</returns>
        static public StringCollection GetDestinationPropertiesTitle(ImportType type,
                                                                     bool isSourceShape)
        {
            ICollection<ObjectDataFieldInfo> infos = GetDestinationProperties(type, isSourceShape);

            var titles = new StringCollection();
            foreach (ObjectDataFieldInfo info in infos)
                titles.Add(info.Info.Name);

            return titles;
        }

        /// <summary>
        /// Gets collection of destination property name for import.
        /// </summary>
        /// <param name="type">Import object type (<see cref="DataObject"/>).</param>
        /// <returns>Collection of destination property name.</returns>
        static public StringCollection GetDestinationPropertiesName(Type type)
        {
            StringDictionary map = GetTitle2NameMap(type);

            var results = new StringCollection();
            foreach (string elem in map.Values)
                results.Add(elem);

            return results;
        }

        /// <summary>
        /// Gets collection of property title for import.
        /// </summary>
        /// <param name="type">Type of import.</param>
        /// <returns>Collection of property title.</returns>
        static public StringDictionary GetTitle2NameMap(ImportType type)
        {
            return GetTitle2NameMap(_Convert2ObjectType(type));
        }

        /// <summary>
        /// Gets collection of property title for import.
        /// </summary>
        /// <param name="type">Import object type (<see cref="DataObject"/>).</param>
        /// <returns>Collection of property title.</returns>
        static public StringDictionary GetTitle2NameMap(Type type)
        {
            var map = new StringDictionary();
            _GetTitle2NameMap(type, null, null, ref map);

            return map;
        }

        /// <summary>
        /// Adds oblygatory indication sign.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>Property name with oblygatory indicator.</returns>
        static public string AddOblygatoryIndication(string name)
        {
            return name + OBLIGATORY_INDICATION;
        }

        /// <summary>
        /// Removs oblygatory indication sign.
        /// </summary>
        /// <param name="name">Property name with oblygatory indicator.</param>
        /// <returns>Property name.</returns>
        static public string ClearOblygatoryIndication(string name)
        {
            int lastChar = name.Length - 1;
            if (OBLIGATORY_INDICATION == name[lastChar])
                return name.Remove(lastChar);

            return name;
        }

        /// <summary>
        /// Creates import field map.
        /// </summary>
        /// <param name="fieldMaps">Import field map (Imported object field name to Data Source
        /// field name).</param>
        /// <param name="data">Data source provider.</param>
        /// <returns>Reference Object property name to Data Source field position.</returns>
        static public Dictionary<string, int> CreateImportMap(ICollection<FieldMap> fieldMaps,
                                                              IDataProvider data)
        {
            Debug.Assert(null != fieldMaps);
            Debug.Assert(null != data);

            var references = new Dictionary<string, int>();

            ICollection<DataFieldInfo> fieldsInfo = data.FieldsInfo;
            ICollection<FieldMap> connectedPairs = fieldMaps;

            // do reference Object property name to Data Source field position
            foreach (FieldMap map in connectedPairs)
            {
                if (string.IsNullOrEmpty(map.SourceFieldName))
                    continue; // skip empty

                int index = 0;
                foreach (DataFieldInfo field in fieldsInfo)
                {
                    if (field.Name == map.SourceFieldName)
                    {
                        references.Add(map.ObjectFieldName, index);
                        break; // result founded
                    }

                    ++index;
                }
            }

            return references;
        }

        #endregion // Public methods

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets effective type.
        /// </summary>
        /// <param name="type">Type to process.</param>
        /// <returns>Effective type.</returns>
        static protected Type _GetEffectiveType(Type type)
        {
            Type effectiveType = type;

            Type typeReal = Nullable.GetUnderlyingType(type);
            if (null != typeReal)
                effectiveType = typeReal;

            return effectiveType;
        }

        /// <summary>
        /// Checks is prefixed property (subproperty).
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <param name="parentName">Parent property name.</param>
        /// <returns>TRUE for prefixed property.</returns>
        static protected bool _IsPrefixed(Type type, string parentName)
        {
            return (((typeof(Break) == type) ||
                     (typeof(TimeWindow) == type) ||
                     (typeof(BarrierEffect) == type)) &&
                        (!string.IsNullOrEmpty(parentName)));
        }

        /// <summary>
        /// Assemblys property full name ([parent name +] current property name).
        /// </summary>
        /// <param name="name">Propery name.</param>
        /// <param name="isPrefixed">Flag of subproprty.</param>
        /// <param name="baseName">Parent property name.</param>
        /// <returns>Property full name.</returns>
        static protected string _AssemblyPropertyFullName(string name,
                                                          bool isPrefixed,
                                                          string baseName)
        {
            return (!isPrefixed) ? name :
                                   App.Current.GetString("ImportPropertyNamePrefixedFormat",
                                                         baseName,
                                                         name);
        }

        /// <summary>
        /// Checks is system type (string, float, int and etc).
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>TRUE if input type is system type.</returns>
        static protected bool _IsSystemType(Type type)
        {
            if (null == type.BaseType)
                return false;

            return (!type.FullName.Contains("ESRI.ArcLogistic") || type.IsEnum);
        }

        #endregion // Protected methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Converts <see cref="ImportType"/> to <see cref="DataObject"/> type.
        /// </summary>
        /// <param name="type">Type to conversion.</param>
        /// <returns>Type of imported objects (<see cref="DataObject"/>).</returns>
        static private Type _Convert2ObjectType(ImportType type)
        {
            Type objectType = null;
            switch (type)
            {
                case ImportType.Orders:
                    objectType = typeof(Order);
                    break;

                case ImportType.Locations:
                    objectType = typeof(Location);
                    break;

                case ImportType.Drivers:
                    objectType = typeof(Driver);
                    break;

                case ImportType.Vehicles:
                    objectType = typeof(Vehicle);
                    break;

                case ImportType.MobileDevices:
                    objectType = typeof(MobileDevice);
                    break;

                case ImportType.DefaultRoutes:
                    objectType = typeof(Route);
                    break;

                case ImportType.DriverSpecialties:
                    objectType = typeof(DriverSpecialty);
                    break;

                case ImportType.VehicleSpecialties:
                    objectType = typeof(VehicleSpecialty);
                    break;

                case ImportType.Barriers:
                    objectType = typeof(Barrier);
                    break;

                case ImportType.Zones:
                    objectType = typeof(Zone);
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported type
                    break;
            }

            return objectType;
        }

        /// <summary>
        /// Reorders field info.
        /// </summary>
        /// <param name="type">Import type.</param>
        /// <param name="list">Object data field info list to reording.</param>
        static private void _ReorderFieldInfos(ImportType type, List<ObjectDataFieldInfo> list)
        {
            if ((ImportType.Orders == type) || (ImportType.Locations == type))
            {   // reorder Addresses field - start after name
                AddressField[] fields = App.Current.Geocoder.AddressFields;

                // find index first address field in infos
                int startAddressesFieldIndex = -1;
                for (int index = 0; index < list.Count; ++index)
                {
                    if (fields[0].Title == list[index].Info.Name)
                    {
                        startAddressesFieldIndex = index;
                        break; // result founded
                    }
                }
                Debug.Assert(-1 != startAddressesFieldIndex);

                // copy address fields
                List<ObjectDataFieldInfo> addressFieldList =
                    list.GetRange(startAddressesFieldIndex, fields.Length);
                // remove address fields in old position
                list.RemoveRange(startAddressesFieldIndex, fields.Length);
                // insert to new position (after name)
                list.InsertRange(NAME_FIELD_POSITION + 1, addressFieldList);
            }
        }

        /// <summary>
        /// Gets list of object data field for application types.
        /// </summary>
        /// <param name="type">Propery type.</param>
        /// <param name="title">Property title.</param>
        /// <param name="isSourceShape">Flag is source shape file.</param>
        /// <param name="list">Output list object data field.</param>
        static private bool _GetAppTypeFieldInfos(Type type,
                                                  string title,
                                                  bool isSourceShape,
                                                  ref List<ObjectDataFieldInfo> list)
        {
            bool result = true;

            if (typeof(OrderCustomProperties) == type)
            {   // specials type: related object OrderCustomProperty
                OrderCustomPropertiesInfo infos = App.Current.Project.OrderCustomPropertiesInfo;
                for (int index = 0; index < infos.Count; ++index)
                {
                    Type propType = (infos[index].Type == OrderCustomPropertyType.Text) ?
                                        typeof(string) : typeof(double);
                    var info = new DataFieldInfo(infos[index].Name, propType);
                    list.Add(new ObjectDataFieldInfo(info, false));
                }
            }

            else if (typeof(Capacities) == type)
            {   // specials type: related object Capacities
                CapacitiesInfo infos = App.Current.Project.CapacitiesInfo;
                for (int index = 0; index < infos.Count; ++index)
                {
                    var info = new DataFieldInfo(infos[index].Name, typeof(double));
                    list.Add(new ObjectDataFieldInfo(info, false));
                }
            }

            else if (typeof(Address) == type)
            {   // specials type: related object Address
                AddressField[] fields = App.Current.Geocoder.AddressFields;
                for (int index = 0; index < fields.Length; ++index)
                {
                    var info = new DataFieldInfo(fields[index].Title, typeof(string));
                    list.Add(new ObjectDataFieldInfo(info, false));
                }
            }

            else if ((typeof(IDataObjectCollection<VehicleSpecialty>) == type) ||
                     (typeof(IDataObjectCollection<DriverSpecialty>) == type) ||
                     (typeof(IDataObjectCollection<Location>) == type) ||
                     (typeof(IDataObjectCollection<Zone>) == type) ||
                     (typeof(FuelType) == type) ||
                     (!string.IsNullOrEmpty(title) &&
                        ((typeof(Location) == type) ||
                         (typeof(MobileDevice) == type) ||
                         (typeof(Vehicle) == type) ||
                         (typeof(Driver) == type))))
            {   // specials types: related objects and objects collection
                var info = new DataFieldInfo(title, typeof(string));
                var fieldInfo = new ObjectDataFieldInfo(info, false);
                list.Add(fieldInfo);
            }

            else
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Gets list of object data field.
        /// </summary>
        /// <param name="type">Propery type.</param>
        /// <param name="title">Property title.</param>
        /// <param name="isSourceShape">Flag is source shape file.</param>
        /// <param name="list">Output list object data field.</param>
        static private void _GetFieldInfos(Type type,
                                           string title,
                                           bool isSourceShape,
                                           ref List<ObjectDataFieldInfo> list)
        {
            //
            // NOTE: function use recursion
            //

            if (!_GetAppTypeFieldInfos(type, title, isSourceShape, ref list))
            {   // other types
                Type attributeType = typeof(DomainPropertyAttribute);

                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (Attribute.IsDefined(property, attributeType))
                    {
                        if (isSourceShape && ("GeoLocation" == property.Name))
                            // NOTE: special case
                            continue; // NOTE: ignore for shape files

                        var importAttribute =
                            Attribute.GetCustomAttribute(property, attributeType)
                                as DomainPropertyAttribute;
                        Debug.Assert(null != importAttribute);

                        Type propertyType = _GetEffectiveType(property.PropertyType);
                        bool isPrefixed = _IsPrefixed(type, title);

                        string fullTitle = _AssemblyPropertyFullName(importAttribute.Title,
                                                                     isPrefixed, title);
                        if (_IsSystemType(propertyType)) // system type
                        {
                            var info = new DataFieldInfo(fullTitle, propertyType);
                            list.Add(new ObjectDataFieldInfo(info, importAttribute.IsMandatory));
                        }
                        else // application internal declaration type
                            _GetFieldInfos(propertyType, fullTitle, isSourceShape, ref list);
                    }
                }
            }
        }

        /// <summary>
        /// Gets dictonary title to name for application type.
        /// </summary>
        /// <param name="type">Propery type.</param>
        /// <param name="title">Property title.</param>
        /// <param name="name">Property name.</param>
        /// <param name="map">Dictonary title to name.</param>
        /// <returns>TRUE if processed successly.</returns>
        static private bool _GetAppTypeTitle2NameMap(Type type,
                                                     string title,
                                                     string name,
                                                     ref StringDictionary map)
        {
            bool result = true;

            if (typeof(OrderCustomProperties) == type)
            {   // specials type: related object OrderCustomProperty
                OrderCustomPropertiesInfo info = App.Current.Project.OrderCustomPropertiesInfo;
                for (int index = 0; index < info.Count; ++index)
                    map.Add(info[index].Name, OrderCustomProperties.GetCustomPropertyName(index));
            }

            else if (typeof(Capacities) == type)
            {   // specials type: related object Capacities
                CapacitiesInfo info = App.Current.Project.CapacitiesInfo;
                for (int index = 0; index < info.Count; ++index)
                    map.Add(info[index].Name, Capacities.GetCapacityPropertyName(index));
            }

            else if (typeof(Address) == type)
            {   // specials type: related object Address
                AddressField[] fields = App.Current.Geocoder.AddressFields;
                for (int index = 0; index < fields.Length; ++index)
                    map.Add(fields[index].Title, fields[index].Type.ToString());
            }

            else if ((typeof(IDataObjectCollection<VehicleSpecialty>) == type) ||
                     (typeof(IDataObjectCollection<DriverSpecialty>) == type) ||
                     (typeof(IDataObjectCollection<Location>) == type) ||
                     (typeof(IDataObjectCollection<Zone>) == type) ||
                     (typeof(FuelType) == type) ||
                     (!string.IsNullOrEmpty(title) &&
                        ((typeof(Location) == type) || (typeof(MobileDevice) == type) ||
                         (typeof(Vehicle) == type) || (typeof(Driver) == type))))
            {   // specials types: related objects and objects collection
                map.Add(title, name);
            }

            else
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Gets dictonary title to name.
        /// </summary>
        /// <param name="type">Propery type.</param>
        /// <param name="title">Property title.</param>
        /// <param name="name">Property name.</param>
        /// <param name="map">Dictonary title to name.</param>
        static private void _GetTitle2NameMap(Type type,
                                              string title,
                                              string name,
                                              ref StringDictionary map)
        {
            //
            // NOTE: function use recursion
            //

            if (!_GetAppTypeTitle2NameMap(type, title, name, ref map))
            {   // other types
                Type attributeType = typeof(DomainPropertyAttribute);

                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (Attribute.IsDefined(property, attributeType))
                    {
                        var importAttribute =
                            Attribute.GetCustomAttribute(property, attributeType)
                                as DomainPropertyAttribute;
                        Debug.Assert(null != importAttribute);

                        Type propertyType = _GetEffectiveType(property.PropertyType);

                        bool isPrefixedName = _IsPrefixed(type, name);
                        bool isPrefixedTitle = _IsPrefixed(type, title);
                        string fullTitle = _AssemblyPropertyFullName(importAttribute.Title,
                                                                     isPrefixedTitle,
                                                                     title);
                        string fullName = _AssemblyPropertyFullName(property.Name,
                                                                    isPrefixedName,
                                                                    name);

                        if (_IsSystemType(propertyType)) // system type
                            map.Add(fullTitle, fullName);
                        else // application internal declaration type
                            _GetTitle2NameMap(propertyType, fullTitle, fullName, ref map);
                    }
                }
            }
        }

        #endregion // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Obligatory indication sign.
        /// </summary>
        private const char OBLIGATORY_INDICATION = '*';
        /// <summary>
        /// "Name" field position in <see cref="DataObject"/>.
        /// </summary>
        private const int NAME_FIELD_POSITION = 0;

        #endregion // Private constants
    }
}

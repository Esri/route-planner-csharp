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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Routing.Json;
using ESRI.ArcLogistics.Tracking.TrackingService.DataModel;
using ESRI.ArcLogistics.Tracking.TrackingService.Json;
using ESRI.ArcLogistics.Utility;
using ESRI.ArcLogistics.Utility.Reflection;

namespace ESRI.ArcLogistics.Tracking.TrackingService
{
    /// <summary>
    /// Converts instances of <see cref="FeatureRecordData"/> into <see cref="DataRecordBase"/>
    /// and vice versa.
    /// </summary>
    /// <typeparam name="TFeatureRecord">The type of the feature record
    /// to be converted.</typeparam>
    internal sealed class FeatureRecordMapper<TFeatureRecord>
        where TFeatureRecord : DataRecordBase, new()
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the FeatureRecordMapper class with the specified layer
        /// description.
        /// </summary>
        /// <param name="description">The reference to the layer description for feature
        /// layer containing feature records to be converted.</param>
        /// <param name="serializer">The reference to the serializer object to be used for
        /// serializing complex data types.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="description"/> or
        /// <paramref name="serializer"/> is a null reference.</exception>
        public FeatureRecordMapper(LayerDescription description, ISerializer serializer)
        {
            if (description == null)
            {
                throw new ArgumentNullException("description");
            }

            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            _layerDescription = description;
            _serializer = serializer;

            _properties = TypeDescriptor.GetProperties(typeof(TFeatureRecord))
                .Cast<PropertyDescriptor>()
                .ToDictionary(property => property.Name);

            _locationProperty = _properties.Values
                .Where(property => _IsGeometryType(property.PropertyType))
                .FirstOrDefault();

            var objectIDPropertyName = _GetPropertyName(o => o.ObjectID);
            _properties.TryGetValue(objectIDPropertyName, out _objectIDProperty);
        }
        #endregion

        #region public methods
        /// <summary>
        /// Maps the specified feature record data object into corresponding feature.
        /// </summary>
        /// <param name="featureData">The reference to the feature record data object
        /// to be mapped.</param>
        /// <returns>A reference to a new feature record with the specified data.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="featureData"/> is a
        /// null reference.</exception>
        public TFeatureRecord MapObject(FeatureRecordData featureData)
        {
            if (featureData == null)
            {
                throw new ArgumentNullException("featureData");
            }

            var feature = new TFeatureRecord();
            var objectID = featureData.Attributes.TryGet<long>(_layerDescription.ObjectIDField);
            if (objectID.HasValue)
            {
                feature.ObjectID = objectID.Value;
            }

            var featureGeometry = featureData.Geometry;
            if (_locationProperty != null &&
                featureGeometry != null )
            {
                var geometry = default(object);
                var gpGeometry = featureGeometry;
                if (gpGeometry is GPPoint)
                {
                    geometry = GPObjectHelper.GPPointToPoint((GPPoint)gpGeometry);
                }
                else if (gpGeometry is GPPolyline)
                {
                    geometry = GPObjectHelper.GPPolylineToPolyline((GPPolyline)gpGeometry);
                }
                else if (gpGeometry is GPPolygon)
                {
                    geometry = GPObjectHelper.GPPolygonToPolygon((GPPolygon)gpGeometry);
                }

                _locationProperty.SetValue(feature, geometry);
            }

            var knownAttributes = featureData.Attributes
                .Where(item => _properties.ContainsKey(item.Key));

            foreach (var item in knownAttributes)
            {
                var property = _properties[item.Key];
                _ExtractValue(item.Value, property.PropertyType)
                    .Do(value => property.SetValue(feature, value));
            }

            return feature;
        }

        /// <summary>
        /// Maps the specified feature record object into corresponding feature record data.
        /// </summary>
        /// <param name="feature">The reference to the feature record object
        /// to be mapped.</param>
        /// <returns>A reference to a new feature record data corresponding to the specified
        /// feature.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="feature"/> is a
        /// null reference.</exception>
        public FeatureRecordData MapObject(TFeatureRecord feature)
        {
            if (feature == null)
            {
                throw new ArgumentNullException("feature");
            }

            var featureData = new FeatureRecordData();
            featureData.Attributes = new AttrDictionary();
            featureData.Attributes.Add(_layerDescription.ObjectIDField, feature.ObjectID);

            if (_locationProperty != null)
            {
                var geometry = _locationProperty.GetValue(feature);
                featureData.Geometry = _ConvertGeometryToGPGeometry(geometry);
            }

            var dataProperties =
                from descriptor in _properties.Values
                where
                    descriptor != _objectIDProperty &&
                    descriptor != _locationProperty
                select KeyValuePair.Create(descriptor.Name, descriptor.GetValue(feature));

            foreach (var item in dataProperties)
            {
                var value = item.Value;
                var propertyType = value != null ? value.GetType() : null;
                if (_IsNullable(propertyType))
                {
                    propertyType = propertyType.GetGenericArguments().First();
                }

                if (typeof(DateTime).IsAssignableFrom(propertyType))
                {
                    value = _SerializeDateTime(value);
                }
                else if (typeof(Enum).IsAssignableFrom(propertyType))
                {
                    value = _SerializeEnum(value);
                }
                else if (typeof(Guid).IsAssignableFrom(propertyType))
                {
                    value = _SerializeGuid(value);
                }
                else if (_IsObject(propertyType))
                {
                    value = _serializer.Serialize(value);
                }

                featureData.Attributes.Add(item.Key, value);
            }

            return featureData;
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Converts the specified geometry object to corresponding GP Geometry object.
        /// </summary>
        /// <param name="geometry">A reference to the geometry object to be converted.</param>
        /// <returns>A new <see cref="GeometryHolder"/> with corresponding GP Geometry object
        /// or null reference if <paramref name="geometry"/> is null.</returns>
        private static GPGeometry _ConvertGeometryToGPGeometry(object geometry)
        {
            if (geometry == null)
            {
                return null;
            }

            var gpGeometry = new GPGeometry();
            if (geometry.GetType().IsAssignableFrom(typeof(Point)))
            {
                gpGeometry = GPObjectHelper.PointToGPPoint((Point)geometry);
            }
            else if (geometry.GetType().IsAssignableFrom(typeof(Polyline)))
            {
                gpGeometry = GPObjectHelper.PolylineToGPPolyline((Polyline)geometry);
            }
            else if (geometry.GetType().IsAssignableFrom(typeof(Polygon)))
            {
                gpGeometry = GPObjectHelper.PolygonToGPPolygon((Polygon)geometry);
            }

            return gpGeometry;
        }

        /// <summary>
        /// Gets name of the property specified with member expression.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="expression">The member expression specifying property to get
        /// name for.</param>
        /// <returns>A name of the property.</returns>
        private static string _GetPropertyName<T>(Expression<Func<TFeatureRecord, T>> expression)
        {
            return TypeInfoProvider<TFeatureRecord>.GetPropertyInfo(o => o.ObjectID).Name;
        }

        /// <summary>
        /// Serializes <see cref="System.Guid"/> values.
        /// </summary>
        /// <param name="value">The <see cref="System.Guid"/> instance
        /// to be serialized.</param>
        /// <returns>Serialized <see cref="System.Guid"/> object.</returns>
        private static object _SerializeGuid(object value)
        {
            if (value == null)
                return null;

            return "{" + value.ToString() + "}";
        }

        /// <summary>
        /// Serializes <see cref="System.DateTime"/> values.
        /// </summary>
        /// <param name="value">The <see cref="System.DateTime"/> instance
        /// to be serialized.</param>
        /// <returns>Serialized <see cref="System.DateTime"/> object.</returns>
        private static object _SerializeDateTime(object value)
        {
            if (value == null)
            {
                return null;
            }
            
            return GPObjectHelper.DateTimeToGPDateTime((DateTime)value);
        }

        /// <summary>
        /// Deserializes <see cref="System.DateTime"/> values.
        /// </summary>
        /// <param name="value">The number of milliseconds to be converted to
        /// <see cref="System.DateTime"/> value.</param>
        /// <returns>Deserialized <see cref="System.DateTime"/> object.</returns>
        private static object _DeserializeDateTime(object value)
        {
            return GPObjectHelper.GPDateTimeToDateTime(Convert.ToInt64(value));
        }

        /// <summary>
        /// Serializes enumeration value.
        /// </summary>
        /// <param name="value">The enumeration value to be serialized.</param>
        /// <returns>Serialized enumeration value.</returns>
        private static object _SerializeEnum(object value)
        {
            if (value == null)
            {
                return null;
            }

            var underlyingType = Enum.GetUnderlyingType(value.GetType());
            var result = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);

            return result;
        }

        /// <summary>
        /// Checks if the specified type is a <see cref="System.Nullable&lt;T&gt;"/>.
        /// </summary>
        /// <param name="type">The reference to the type object to be checked.</param>
        /// <returns>True if and only if the specified type is
        /// <see cref="System.Nullable&lt;T&gt;"/>.</returns>
        private static bool _IsNullable(Type type)
        {
            return
                type != null &&
                type.IsGenericType &&
                type.GetGenericTypeDefinition().IsEquivalentTo(typeof(Nullable<>));
        }

        /// <summary>
        /// Checks if the specified type is a non built-in reference type.
        /// </summary>
        /// <param name="type">The type to be checked.</param>
        /// <returns>True if and only if the specified type is a non built-in reference
        /// type.</returns>
        private static bool _IsObject(Type type)
        {
            return
                Type.GetTypeCode(type) == TypeCode.Object &&
                !type.IsValueType;
        }

        /// <summary>
        /// Checks if the specified type represents geometry.
        /// </summary>
        /// <param name="type">The type object to check.</param>
        /// <returns>true if and only if the specified type represents geometry.</returns>
        private static bool _IsGeometryType(Type type)
        {
            Debug.Assert(type != null);

            return
                type.IsAssignableFrom(typeof(Point)) ||
                type.IsAssignableFrom(typeof(Polyline)) ||
                type.IsAssignableFrom(typeof(Polygon));
        }
        #endregion

        #region private methods
        /// <summary>
        /// Extracts value which can be assigned to the specified target type from the specified
        /// object.
        /// </summary>
        /// <param name="value">The reference to the object to extract value from.</param>
        /// <param name="targetType">The target type the extracted value will be stored at.</param>
        /// <returns>A reference to a collection with a single object which can be assigned
        /// to a value of the <paramref name="targetType"/> or an empty conversion if
        /// <paramref name="value"/> cannot be assigned to the <paramref name="targetType"/>.
        /// </returns>
        private IEnumerable<object> _ExtractValue(object value, Type targetType)
        {
            Debug.Assert(targetType != null);

            return _DoExtractValue(value, targetType);
        }

        /// <summary>
        /// Implements <see cref="_ExtractValue"/> method allowing it to provide eager validation
        /// of input arguments.
        /// </summary>
        /// <param name="value">The reference to the object to extract value from.</param>
        /// <param name="targetType">The target type the extracted value will be stored at.</param>
        /// <returns>A reference to a collection with extracted value.</returns>
        private IEnumerable<object> _DoExtractValue(object value, Type targetType)
        {
            if (value == null)
            {
                if (_IsNullable(targetType) || !targetType.IsValueType)
                {
                    yield return null;
                }

                yield break;
            }

            if (_IsNullable(targetType))
            {
                targetType = targetType.GetGenericArguments().First();
            }

            if (targetType.IsEnum)
            {
                value = Enum.ToObject(targetType, value);
            }
            else if (targetType == typeof(DateTime))
            {
                value = _DeserializeDateTime(value);
            }
            else if (_IsObject(targetType) && value is string)
            {
                value = _serializer.Deserialize(targetType, (string)value);
            }

            yield return value;
        }
        #endregion

        #region private fields
        /// <summary>
        /// The reference to the description of feature layer storing
        /// <typeparamref name="TFeatureRecord"/> objects.
        /// </summary>
        private LayerDescription _layerDescription;

        /// <summary>
        /// The reference to the serializer object.
        /// </summary>
        private ISerializer _serializer;

        /// <summary>
        /// The reference to the <typeparamref name="TFeatureRecord"/> properties.
        /// </summary>
        private IDictionary<string, PropertyDescriptor> _properties;

        /// <summary>
        /// The reference to the <typeparamref name="TFeatureRecord"/> location property.
        /// </summary>
        private PropertyDescriptor _locationProperty;

        /// <summary>
        /// The reference to the <typeparamref name="TFeatureRecord"/> object ID property.
        /// </summary>
        private  PropertyDescriptor _objectIDProperty;
        #endregion
    }
}

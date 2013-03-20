using System;
using System.Collections.Generic;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Tracking.TrackingService
{
    /// <summary>
    /// Implements serializer for the JSON format.
    /// </summary>
    internal sealed class JsonSerializer : ISerializer
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the JsonSerializer class.
        /// </summary>
        public JsonSerializer()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the JsonSerializer class.
        /// </summary>
        /// <param name="knownTypes">The reference to the collection of known types or null.</param>
        public JsonSerializer(IEnumerable<Type> knownTypes)
            : this(knownTypes, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the JsonSerializer class.
        /// </summary>
        /// <param name="knownTypes">The reference to the collection of known types or null.</param>
        /// <param name="doPreprocessing">Specifies if the JSON strings should be preprocessed
        /// before deserialization.</param>
        public JsonSerializer(IEnumerable<Type> knownTypes, bool doPreprocessing)
        {
            _knownTypes = knownTypes;
            _doPreprocessing = doPreprocessing;
        }
        #endregion

        #region ISerializer Members
        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="value">The reference to the object to be serialized.</param>
        /// <returns>String representation of the specified object.</returns>
        public string Serialize(object value)
        {
            return JsonSerializeHelper.Serialize(value);
        }

        /// <summary>
        /// Deserializes object of the specified type serialized with this serializer.
        /// </summary>
        /// <param name="type">The type of the object to be deserialized.</param>
        /// <param name="value">String representation of the object to be deserialized.</param>
        /// <returns>A reference to the deserialized object.</returns>
        public object Deserialize(Type type, string value)
        {
            return JS1.Deserialize(type, value, _knownTypes, _doPreprocessing);
        }
        #endregion

        #region private fields
        /// <summary>
        /// The reference to the collection of known types or null.
        /// </summary>
        private IEnumerable<Type> _knownTypes;

        /// <summary>
        /// Specifies if the JSON strings should be preprocessed before deserialization.
        /// </summary>
        private bool _doPreprocessing;
        #endregion
    }
}

using System;

namespace ESRI.ArcLogistics.Tracking.TrackingService
{
    /// <summary>
    /// Provides serialization facilities for objects of the specified type.
    /// </summary>
    internal interface ISerializer
    {
        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="value">The reference to the object to be serialized.</param>
        /// <returns>String representation of the specified object.</returns>
        string Serialize(object value);

        /// <summary>
        /// Deserializes object of the specified type serialized with this serializer.
        /// </summary>
        /// <param name="type">The type of the object to be deserialized.</param>
        /// <param name="value">String representation of the object to be deserialized.</param>
        /// <returns>A reference to the deserialized object.</returns>
        object Deserialize(Type type, string value);
    }
}

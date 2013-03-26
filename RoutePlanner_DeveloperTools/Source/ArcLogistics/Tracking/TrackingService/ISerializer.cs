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

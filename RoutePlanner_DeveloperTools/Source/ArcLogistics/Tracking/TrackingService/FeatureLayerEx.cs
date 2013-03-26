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
using System.Linq;
using ESRI.ArcLogistics.Tracking.TrackingService.DataModel;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Tracking.TrackingService
{
    /// <summary>
    /// Provides helper extensions for <see cref="IFeatureLayer&lt;T&gt;"/> objects.
    /// </summary>
    internal static class FeatureLayerEx
    {
        #region public static methods
        /// <summary>
        /// Gets objects with the specified IDs.
        /// </summary>
        /// <typeparam name="T">The type of objects in the feature layer.</typeparam>
        /// <param name="featureLayer">The reference to feature layer to get objects from.</param>
        /// <param name="objectIDs">The reference to collection of object IDs to get
        /// objects for.</param>
        /// <returns>Collection of objects with the specified IDs.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="featureLayer"/> or
        /// <paramref name="objectIDs"/> argument is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to retrieve objects from the feature layer.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public static IEnumerable<T> QueryObjectsByIDs<T>(
            this IFeatureLayer<T> featureLayer,
            IEnumerable<long> objectIDs)
            where T : DataRecordBase, new()
        {
            if (featureLayer == null)
            {
                throw new ArgumentNullException("featureLayer");
            }

            if (objectIDs == null)
            {
                throw new ArgumentNullException("objectIDs");
            }

            var remainingIDs = objectIDs.ToArray();
            var totalElements = remainingIDs.Length;
            var maxElements = totalElements;

            var result = new List<T>();
            do
            {
                var queryIDs = new HashSet<long>(remainingIDs.Take(maxElements));

                var data = featureLayer.QueryData(
                    queryIDs,
                    ALL_FIELDS,
                    GeometryReturningPolicy.WithGeometry).ToList();
                result.AddRange(data);

                if (data.Count != 0)
                {
                    var receivedIDs = new HashSet<long>(data.Select(item => item.ObjectID));
                    remainingIDs = _ExcludeItems(remainingIDs, receivedIDs).ToArray();

                    maxElements = data.Count;
                }
                else
                {
                    remainingIDs = _ExcludeItems(remainingIDs, queryIDs).ToArray();
                }
            } while (remainingIDs.Length > 0 && result.Count < totalElements);

            return result;
        }

        /// <summary>
        /// Adds specified objects to the feature layer.
        /// </summary>
        /// <typeparam name="T">The type of objects in the feature layer.</typeparam>
        /// <param name="featureLayer">The reference to feature layer to add objects to.</param>
        /// <param name="newObjects">The reference to the collection of objects to be added to
        /// the feature layer.</param>
        /// <returns>The reference to the collection of object IDs identifying added
        /// objects.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="featureLayer"/> or
        /// <paramref name="newObjects"/> argument or any of it's elements is a null
        /// reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to add objects to the feature layer.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public static IEnumerable<long> AddObjects<T>(
            this IFeatureLayer<T> featureLayer,
            IEnumerable<T> newObjects)
            where T : DataRecordBase, new()
        {
            if (featureLayer == null)
            {
                throw new ArgumentNullException("featureLayer");
            }

            if (newObjects == null || newObjects.Any(obj => obj == null))
            {
                throw new ArgumentNullException("newObjects");
            }

            return featureLayer.ApplyEdits(
                newObjects,
                Enumerable.Empty<T>(),
                Enumerable.Empty<long>());
        }

        /// <summary>
        /// Updates the specified objects at the feature layer.
        /// </summary>
        /// <typeparam name="T">The type of objects in the feature layer.</typeparam>
        /// <param name="featureLayer">The reference to feature layer to update objects
        /// at.</param>
        /// <param name="updatedObjects">The reference to the collection of objects
        /// to be updated.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="featureLayer"/> or
        /// <paramref name="updatedObjects"/> argument or any of it's elements is a null
        /// reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to update objects at the feature layer.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public static void UpdateObjects<T>(
            this IFeatureLayer<T> featureLayer,
            IEnumerable<T> updatedObjects)
            where T : DataRecordBase, new()
        {
            if (featureLayer == null)
            {
                throw new ArgumentNullException("featureLayer");
            }

            if (updatedObjects == null || updatedObjects.Any(obj => obj == null))
            {
                throw new ArgumentNullException("updatedObjects");
            }

            featureLayer.ApplyEdits(
                Enumerable.Empty<T>(),
                updatedObjects,
                Enumerable.Empty<long>());
        }
        #endregion

        #region private methods
        /// <summary>
        /// Filters the specified collection by excluding items from the specified set.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="source">The source collection to be filtered.</param>
        /// <param name="excludedElements">The set of items to be excluded from the
        /// <paramref name="source"/>.</param>
        /// <returns>A filtered source collection.</returns>
        private static IEnumerable<T> _ExcludeItems<T>(
            IEnumerable<T> source,
            HashSet<T> excludedElements)
        {
            return source.Where(item => !excludedElements.Contains(item));
        }
        #endregion

        #region private constants
        /// <summary>
        /// Represents a record fields collection resulting in returning all available fields.
        /// </summary>
        private static readonly IEnumerable<string> ALL_FIELDS = EnumerableEx.Return("*");
        #endregion
    }
}

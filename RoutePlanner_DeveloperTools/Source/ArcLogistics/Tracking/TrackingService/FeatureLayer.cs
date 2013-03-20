using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Routing.Json;
using ESRI.ArcLogistics.Tracking.TrackingService.DataModel;
using ESRI.ArcLogistics.Tracking.TrackingService.Json;
using ESRI.ArcLogistics.Tracking.TrackingService.Requests;

namespace ESRI.ArcLogistics.Tracking.TrackingService
{
    /// <summary>
    /// Implements <see cref="IFeatureLayer&lt;TFeatureRecord&gt;"/> using ArcGIS Server REST API.
    /// </summary>
    /// <typeparam name="TFeatureRecord">The type of records in the feature layer.</typeparam>
    internal sealed class FeatureLayer<TFeatureRecord> : IFeatureLayer<TFeatureRecord>
        where TFeatureRecord : DataRecordBase, new()
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the FeatureLayer class.
        /// </summary>
        /// <param name="featureLayerUri">The URL of the feature layer.</param>
        /// <param name="layerDescription">The reference to the layer description object.</param>
        /// <param name="requestSender">The reference to the request sender object.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="featureLayerUri"/>,
        /// <paramref name="layerDescription"/> or <paramref name="requestSender"/> argument is
        /// a null reference.</exception>
        public FeatureLayer(
            Uri featureLayerUri,
            LayerDescription layerDescription,
            IFeatureServiceRequestSender requestSender)
        {
            if (featureLayerUri == null)
            {
                throw new ArgumentNullException("featureLayerUri");
            }

            if (layerDescription == null)
            {
                throw new ArgumentNullException("layerDescription");
            }

            if (requestSender == null)
            {
                throw new ArgumentNullException("requestSender");
            }

            var serializer = new JsonSerializer(VrpRequestBuilder.JsonTypes, true);

            _mapper = new FeatureRecordMapper<TFeatureRecord>(layerDescription, serializer);
            _requestSender = requestSender;
            _uri = featureLayerUri;
            _layerDescription = layerDescription;
        }
        #endregion

        #region IFeatureLayer<TFeatureRecord> Members
        /// <summary>
        /// Retrieves collection of object IDs for the specified where clause.
        /// </summary>
        /// <param name="whereClause">The where clause specifying feature records to get IDs
        /// for.</param>
        /// <returns>A reference to the collection of object IDs for feature records satisfying
        /// the specified where clause.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="whereClause"/> is a null
        /// reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to apply query ids from the feature layer.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST service.</exception>
        public IEnumerable<long> QueryObjectIDs(string whereClause)
        {
            if (whereClause == null)
            {
                throw new ArgumentNullException("whereClause");
            }

            var request = new QueryRequest
            {
                WhereClause = whereClause,
                ReturnIDsOnly = true,
            };

            var query = RestHelper.BuildQueryString(request, Enumerable.Empty<Type>(), true);
            var url = string.Format(QUERY_URL_FORMAT, _uri.AbsoluteUri);
            var options = new HttpRequestOptions
            {
                Method = HttpMethod.Post,
                UseGZipEncoding = true,
            };

            var response = _requestSender.SendRequest<ObjectIDsResponse>(url, query, options);

            // If result is null - return empty collection.
            var result = response.ObjectIDs;
            if (result == null)
                result = new List<long>();

            return result;
        }

        /// <summary>
        /// Filters collection of object IDs by removing inexistent ones.
        /// </summary>
        /// <param name="objectIDs">The reference to the collection of object IDs
        /// to be filtered.</param>
        /// <returns>A reference to the collection of object IDs for feature records which exists
        /// at the feature service.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="objectIDs"/> is a null
        /// reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST service.</exception>
        public IEnumerable<long> FilterObjectIDs(IEnumerable<long> objectIDs)
        {
            if (objectIDs == null)
            {
                throw new ArgumentNullException("objectIDs");
            }

            var request = new QueryRequest
            {
                ObjectIDs = string.Join(",", objectIDs),
                ReturnIDsOnly = true,
            };

            var query = RestHelper.BuildQueryString(request, Enumerable.Empty<Type>(), true);
            var url = string.Format(QUERY_URL_FORMAT, _uri.AbsoluteUri);
            var options = new HttpRequestOptions
            {
                Method = HttpMethod.Post,
                UseGZipEncoding = true,
            };

            var response = _requestSender.SendRequest<ObjectIDsResponse>(url, query, options);

            return response.ObjectIDs;
        }

        /// <summary>
        /// Retrieves collection of feature records for the specified object IDs.
        /// </summary>
        /// <param name="objectIDs">The reference to the collection of object IDs of feature
        /// records to retrieve.</param>
        /// <param name="returnFields">The reference to the collection of field names to
        /// retrieve values for. A wildcard value '*' could be used to retrieve all fields.</param>
        /// <param name="geometryReturningPolicy">A value indicating whether to retrieve feature
        /// geometry.</param>
        /// <returns>A collection of feature records with the specified object IDs and with
        /// specified fields filled with values retrieved from feature service.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="objectIDs"/>,
        /// <paramref name="returnFields"/> or any element in the <paramref name="returnFields"/>
        /// is a null reference.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="geometryReturningPolicy"/>
        /// does not contain either <see cref="GeometryReturningPolicy.WithGeometry"/>
        /// or <see cref="GeometryReturningPolicy.WithoutGeometry"/>.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to query data from the feature layer.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST service.</exception>
        public IEnumerable<TFeatureRecord> QueryData(
            IEnumerable<long> objectIDs,
            IEnumerable<string> returnFields,
            GeometryReturningPolicy geometryReturningPolicy)
        {
            if (objectIDs == null)
            {
                throw new ArgumentNullException("objectIDs");
            }

            if (returnFields == null || returnFields.Any(field => field == null))
            {
                throw new ArgumentNullException("returnFields");
            }

            if (!objectIDs.Any())
            {
                return Enumerable.Empty<TFeatureRecord>();
            }

            var request = new QueryRequest
            {
                ObjectIDs = string.Join(",", objectIDs),
                ReturnFields = string.Join(",", returnFields),
                ReturnIDsOnly = false,
                ReturnGeometry = geometryReturningPolicy == GeometryReturningPolicy.WithGeometry,
            };

            return _QueryData(request);
        }

        /// <summary>
        /// Retrieves collection of feature records for the specified where clause.
        /// </summary>
        /// <param name="whereClause">The where clause specifying objects to be retrieved.</param>
        /// <param name="returnFields">The reference to the collection of field names to
        /// retrieve values for. A wildcard value '*' could be used to retrieve all fields.</param>
        /// <param name="geometryReturningPolicy">A value indicating whether to retrieve feature
        /// geometry.</param>
        /// <returns>A collection of feature records satisfying the specified where clause and with
        /// specified fields filled with values retrieved from feature service.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="whereClause"/>,
        /// <paramref name="returnFields"/> or any element in the <paramref name="returnFields"/>
        /// is a null reference.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="geometryReturningPolicy"/>
        /// does not contain either <see cref="GeometryReturningPolicy.WithGeometry"/>
        /// or <see cref="GeometryReturningPolicy.WithoutGeometry"/>.</exception>
        /// <exception cref="ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST service.</exception>
        public IEnumerable<TFeatureRecord> QueryData(
            string whereClause,
            IEnumerable<string> returnFields,
            GeometryReturningPolicy geometryReturningPolicy)
        {
            if (whereClause == null)
            {
                throw new ArgumentNullException("whereClause");
            }

            if (returnFields == null || returnFields.Any(field => field == null))
            {
                throw new ArgumentNullException("returnFields");
            }

            var request = new QueryRequest
            {
                WhereClause = whereClause,
                ReturnFields = string.Join(",", returnFields),
                ReturnIDsOnly = false,
                ReturnGeometry = geometryReturningPolicy == GeometryReturningPolicy.WithGeometry,
            };

            return _QueryData(request);
        }

        /// <summary>
        /// Applies specified edits to the feature layer.
        /// </summary>
        /// <param name="newObjects">The reference to the collection of feature records to
        /// be added to the layer. Values of object ID field will be ignored.</param>
        /// <param name="updatedObjects">The reference to the collection of feature records
        /// to be updated.</param>
        /// <param name="deletedObjectIDs">The reference to the collection of object IDs of
        /// feature records to be deleted.</param>
        /// <returns>A reference to the collection of object IDs of added feature
        /// records.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="newObjects"/>,
        /// <paramref name="updatedObjects"/>, <paramref name="deletedObjectIDs"/> or any element
        /// in the <paramref name="newObjects"/> or <paramref name="updatedObjects"/> is a null
        /// reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to apply edits to the feature layer.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST service.</exception>
        public IEnumerable<long> ApplyEdits(
            IEnumerable<TFeatureRecord> newObjects,
            IEnumerable<TFeatureRecord> updatedObjects,
            IEnumerable<long> deletedObjectIDs)
        {
            if (newObjects == null || newObjects.Any(obj => obj == null))
            {
                throw new ArgumentNullException("newObjects");
            }

            if (updatedObjects == null || updatedObjects.Any(obj => obj == null))
            {
                throw new ArgumentNullException("updatedObjects");
            }

            if (deletedObjectIDs == null)
            {
                throw new ArgumentNullException("deletedObjectIDs");
            }

            if (!newObjects.Any() && !updatedObjects.Any() && !deletedObjectIDs.Any())
            {
                return Enumerable.Empty<long>();
            }

            var itemsToDelete = updatedObjects.Where(
                item => item.Deleted == DeletionStatus.Deleted);
            if (typeof(TFeatureRecord) == typeof(Device) && itemsToDelete.Any())
            {
                var itemsMessages = JsonSerializeHelper.Serialize(
                    itemsToDelete.Select(_mapper.MapObject).ToArray());
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendFormat(
                    "Executing deletion for {0} records:\n",
                    typeof(TFeatureRecord));
                messageBuilder.Append(string.Join("\n", itemsMessages));

                Logger.Info(messageBuilder.ToString());
            }

            var request = new ApplyEditsRequest
            {
                Adds = JsonProcHelper.DoPostProcessing(JsonSerializeHelper.Serialize(newObjects
                    .Select(_mapper.MapObject)
                    .ToArray(),
                    VrpRequestBuilder.JsonTypes)),
                Updates = JsonProcHelper.DoPostProcessing(JsonSerializeHelper.Serialize(updatedObjects
                    .Select(_mapper.MapObject)
                    .ToArray(),
                    VrpRequestBuilder.JsonTypes)),
                Deletes = string.Join(",", deletedObjectIDs),
            };

            var query = RestHelper.BuildQueryString(request, Enumerable.Empty<Type>(), true);
            var url = string.Format(APPLY_EDITS_URL_FORMAT, _uri.AbsoluteUri);
            var options = new HttpRequestOptions
            {
                Method = HttpMethod.Post,
                UseGZipEncoding = true,
            };

            var response = _requestSender.SendRequest<ApplyEditsResponse>(url, query, options);

            var failures = response
                .AddResults
                .Concat(response.UpdateResults)
                .Concat(response.DeleteResults)
                .Where(editResult => !editResult.Succeeded)
                .Select(editResult => editResult.Error)
                .Distinct()
                .Select(error => error == null ? null : new TrackingServiceError(
                    error.Code,
                    error.Description));
            if (failures.Any())
            {
                var msg = string.Format(
                    Properties.Messages.Error_FeatureLayerCannotApplyEdits,
                    _layerDescription.Name ?? string.Empty);
                throw new TrackingServiceException(msg, failures);
            }

            var ids = response.AddResults
                .Select(addResult => addResult.ObjectID)
                .ToList();

            return ids;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Retrieves collection of feature records for the specified request.
        /// </summary>
        /// <param name="request">The request specifying objects to be retrieved.</param>
        /// <returns>A collection of feature records satisfying the specified request.</returns>
        /// <exception cref="ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST service.</exception>
        private IEnumerable<TFeatureRecord> _QueryData(QueryRequest request)
        {
            Debug.Assert(request != null);

            var query = RestHelper.BuildQueryString(request, Enumerable.Empty<Type>(), true);
            var url = string.Format(QUERY_URL_FORMAT, _uri.AbsoluteUri);
            var options = new HttpRequestOptions
            {
                Method = HttpMethod.Post,
                UseGZipEncoding = true,
            };

            var response = _requestSender.SendRequest<FeatureRecordSetLayer>(url, query, options);
            var data = response.Features.Select(_mapper.MapObject).ToList();

            return data;
        }
        #endregion

        #region private constants
        /// <summary>
        /// Format of the "Query" operation URLs.
        /// </summary>
        private const string QUERY_URL_FORMAT = "{0}/query";

        /// <summary>
        /// Format of the "ApplyEdits" operation URLs.
        /// </summary>
        private const string APPLY_EDITS_URL_FORMAT = "{0}/applyEdits";
        #endregion

        #region private fields
        /// <summary>
        /// The reference to the instance of feature record mapper.
        /// </summary>
        private FeatureRecordMapper<TFeatureRecord> _mapper;

        /// <summary>
        /// The reference to request sender object.
        /// </summary>
        private IFeatureServiceRequestSender _requestSender;

        /// <summary>
        /// The reference to the feature layer URL object.
        /// </summary>
        private Uri _uri;

        /// <summary>
        /// The reference to the feature layer description object.
        /// </summary>
        private LayerDescription _layerDescription;
        #endregion
    }
}

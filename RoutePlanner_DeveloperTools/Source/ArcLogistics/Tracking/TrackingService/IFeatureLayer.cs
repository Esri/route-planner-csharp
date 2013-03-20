using System.Collections.Generic;
using ESRI.ArcLogistics.Tracking.TrackingService.DataModel;

namespace ESRI.ArcLogistics.Tracking.TrackingService
{
    /// <summary>
    /// Provides access to a single feature layer or table.
    /// </summary>
    /// <typeparam name="TFeatureRecord">The type of records in the feature layer.</typeparam>
    internal interface IFeatureLayer<TFeatureRecord>
        where TFeatureRecord : DataRecordBase, new()
    {
        /// <summary>
        /// Retrieves collection of object IDs for the specified where clause.
        /// </summary>
        /// <param name="whereClause">The where clause specifying feature records to get IDs
        /// for.</param>
        /// <returns>A reference to the collection of object IDs for feature records satisfying
        /// the specified where clause.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="whereClause"/> is a null
        /// reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST service.</exception>
        IEnumerable<long> QueryObjectIDs(string whereClause);

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
        IEnumerable<long> FilterObjectIDs(IEnumerable<long> objectIDs);

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
        IEnumerable<TFeatureRecord> QueryData(
            string whereClause,
            IEnumerable<string> returnFields,
            GeometryReturningPolicy geometryReturningPolicy);

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
        /// <exception cref="ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST service.</exception>
        IEnumerable<TFeatureRecord> QueryData(
            IEnumerable<long> objectIDs,
            IEnumerable<string> returnFields,
            GeometryReturningPolicy geometryReturningPolicy);

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
        /// <exception cref="ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST service.</exception>
        IEnumerable<long> ApplyEdits(
            IEnumerable<TFeatureRecord> newObjects,
            IEnumerable<TFeatureRecord> updatedObjects,
            IEnumerable<long> deletedObjectIDs);
    }
}

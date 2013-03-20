using System;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// GPError class.
    /// </summary>
    [DataContract]
    internal class GPError
    {
        [DataMember(Name = "code")]
        public int Code { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "details")]
        public string[] Details { get; set; }
    }

    /// <summary>
    /// GPFaultResponse class.
    /// </summary>
    [DataContract]
    internal class GPFaultResponse
    {
        [DataMember(Name = "error")]
        public GPError Error { get; set; }
    }

    /// <summary>
    /// JobMessage class.
    /// </summary>
    [DataContract]
    internal class JobMessage
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }
    }

    /// <summary>
    /// GPParameterRef class.
    /// </summary>
    [DataContract]
    internal class GPParameterRef
    {
        [DataMember(Name = "paramUrl")]
        public string ParamUrl { get; set; }
    }

    /// <summary>
    /// GPRouteOutputs class.
    /// </summary>
    [DataContract]
    internal class GPRouteOutputs
    {
        [DataMember(Name = "out_stops")]
        public GPParameterRef Stops { get; set; }

        [DataMember(Name = "out_routes")]
        public GPParameterRef Routes { get; set; }

        /// <summary>
        /// Gets or sets a parameter reference to record-set with information about stops with
        /// violations.
        /// </summary>
        [DataMember(Name = "out_unassigned_stops")]
        public GPParameterRef ViolatedStops { get; set; }

        /// <summary>
        /// Gets or sets a reference to directions feature record-set reference.
        /// </summary>
        [DataMember(Name = "out_directions")]
        public GPParameterRef Directions { get; set; }

        [DataMember(Name = "solve_succeeded")]
        public GPParameterRef SolveSucceeded { get; set; }
    }

    /// <summary>
    /// GPRouteInputs class.
    /// </summary>
    [DataContract]
    internal class GPRouteInputs
    {
        [DataMember(Name = "orders")]
        public GPParameterRef Orders { get; set; }

        [DataMember(Name = "depots")]
        public GPParameterRef Depots { get; set; }

        [DataMember(Name = "routes")]
        public GPParameterRef Routes { get; set; }

        [DataMember(Name = "breaks")]
        public GPParameterRef Breaks { get; set; }

        [DataMember(Name = "route_zones")]
        public GPParameterRef RouteZones { get; set; }

        [DataMember(Name = "seed_points")]
        public GPParameterRef SeedPoints { get; set; }

        [DataMember(Name = "route_renewals")]
        public GPParameterRef Renewals { get; set; }

        [DataMember(Name = "specialties_na")]
        public GPParameterRef Specialties { get; set; }

        [DataMember(Name = "order_pairs")]
        public GPParameterRef OrderPairs { get; set; }

        [DataMember(Name = "point_barriers")]
        public GPParameterRef PointBarriers { get; set; }

        [DataMember(Name = "line_barriers")]
        public GPParameterRef LineBarriers { get; set; }

        [DataMember(Name = "polygon_barriers")]
        public GPParameterRef PolygonBarriers { get; set; }

        [DataMember(Name = "attribute_parameter_values")]
        public GPParameterRef AttributeParameters { get; set; }

        [DataMember(Name = "default_date")]
        public GPParameterRef Date { get; set; }

        [DataMember(Name = "Capacity_Count_na")]
        public GPParameterRef CapacityCount { get; set; }

        [DataMember(Name = "uturn_policy")]
        public GPParameterRef UturnPolicy { get; set; }

        [DataMember(Name = "use_hierarchy_in_analysis")]
        public GPParameterRef UseHierarchyInAnalysis { get; set; }
    }

    /// <summary>
    /// GetJobResultResponse class.
    /// </summary>
    [DataContract]
    internal class GetJobResultResponse : GPResponse
    {
        [DataMember(Name = "jobId")]
        public string JobId { get; set; }

        [DataMember(Name = "jobStatus")]
        public string JobStatus { get; set; }

        [DataMember(Name = "messages")]
        public JobMessage[] Messages { get; set; }
    }

    /// <summary>
    /// GetVrpJobResultResponse class.
    /// </summary>
    [DataContract]
    internal class GetVrpJobResultResponse : GetJobResultResponse
    {
        [DataMember(Name = "results")]
        public GPRouteOutputs Outputs { get; set; }

        [DataMember(Name = "inputs")]
        public GPRouteInputs Inputs { get; set; }
    }

    /// <summary>
    /// GPParamObject class.
    /// </summary>
    [DataContract]
    public class GPParamObject
    {
        [DataMember(Name = "paramName")]
        public string paramName { get; set; }

        [DataMember(Name = "dataType")]
        public string dataType { get; set; }

        [DataMember(Name = "value")]
        public object value { get; set; }
    }

    /// <summary>
    /// SyncVrpResponse class.
    /// </summary>
    [DataContract]
    [KnownType(typeof(GPFeatureRecordSetLayer))]
    [KnownType(typeof(GPRecordSet))]
    [KnownType(typeof(GPBoolean))]
    internal class SyncVrpResponse : GPResponse
    {
        /// <summary>
        /// Gets name of the unassigned orders record-set from VRP Solve response.
        /// </summary>
        public static string ParamUnassignedOrders
        {
            get
            {
                return "out_unassigned_stops";
            }
        }

        /// <summary>
        /// Gets name of the driving directions feature record-set from VRP Solve response.
        /// </summary>
        public static string ParamDirections
        {
            get
            {
                return "out_directions";
            }
        }

        /// <summary>
        /// Gets name of the stops feature record-set from VRP Solve response.
        /// </summary>
        public static string ParamStops
        {
            get
            {
                return "out_stops";
            }
        }

        /// <summary>
        /// Gets name of the routes feature record-set from VRP Solve response.
        /// </summary>
        public static string ParamRoutes
        {
            get
            {
                return "out_routes";
            }
        }

        /// <summary>
        /// Gets name of the Succeeded field from VRP Solve response.
        /// </summary>
        public static string ParamSucceeded
        {
            get
            {
                return "solve_succeeded";
            }
        }

        [DataMember(Name = "results")]
        public GPParamObject[] Objects { get; set; }

        [DataMember(Name = "messages")]
        public JobMessage[] Messages { get; set; }
    }

    /// <summary>
    /// GPSpatialReference class.
    /// </summary>
    [DataContract]
    internal class GPSpatialReference
    {
        public GPSpatialReference()
        {
        }

        public GPSpatialReference(int wkid)
        {
            this.WKID = wkid;
        }

        [DataMember(Name = "wkid")]
        public int WKID { get; set; }
    }

    /// <summary>
    /// GPGeometry class.
    /// </summary>
    [DataContract]
    internal class GPGeometry
    {
        [DataMember(Name = "spatialReference")]
        public GPSpatialReference SpatialReference { get; set; }
    }

    /// <summary>
    /// GPPoint class.
    /// </summary>
    [DataContract]
    internal class GPPoint : GPGeometry
    {
        [DataMember(Name = "x")]
        public double X { get; set; }

        [DataMember(Name = "y")]
        public double Y { get; set; }
    }

    /// <summary>
    /// GPEnvelope class.
    /// </summary>
    [DataContract]
    internal class GPEnvelope : GPGeometry
    {
        [DataMember(Name = "xmin")]
        public double XMin { get; set; }

        [DataMember(Name = "ymin")]
        public double YMin { get; set; }

        [DataMember(Name = "xmax")]
        public double XMax { get; set; }

        [DataMember(Name = "ymax")]
        public double YMax { get; set; }
    }

    /// <summary>
    /// GPPolyline class.
    /// </summary>
    [DataContract]
    internal class GPPolyline : GPGeometry
    {
        [DataMember(Name = "paths")]
        public double[][][] Paths { get; set; }
    }

    /// <summary>
    /// GPPolygon class.
    /// </summary>
    [DataContract]
    internal class GPPolygon : GPGeometry
    {
        [DataMember(Name = "rings")]
        public double[][][] Rings { get; set; }
    }

    /// <summary>
    /// GPFeature class.
    /// </summary>
    [DataContract]
    [KnownType(typeof(GPDate))]
    // Dotfuscator cannot correctly handle double[][][] type declared
    // as known type attribute
    //[KnownType(typeof(double[][][]))]
    internal class GPFeature
    {
        [DataMember(Name = "geometry")]
        public GeometryHolder Geometry { get; set; }

        [DataMember(Name = "attributes")]
        public AttrDictionary Attributes { get; set; }
    }

    /// <summary>
    /// GPCompactGeomFeature class.
    /// </summary>
    [DataContract]
    [KnownType(typeof(GPDate))]
    internal class GPCompactGeomFeature
    {
        [DataMember(Name = "compressedGeometry")]
        public string CompressedGeometry { get; set; }

        [DataMember(Name = "attributes")]
        public AttrDictionary Attributes { get; set; }
    }

    /// <summary>
    /// GPFeatureRecordSetLayer class.
    /// </summary>
    [DataContract]
    internal class GPFeatureRecordSetLayer
    {
        [DataMember(Name = "geometryType")]
        public string GeometryType { get; set; }

        [DataMember(Name = "spatialReference")]
        public GPSpatialReference SpatialReference { get; set; }

        [DataMember(Name = "features")]
        public GPFeature[] Features { get; set; }
    }

    /// <summary>
    /// GPRecordSet class.
    /// </summary>
    [DataContract]
    internal class GPRecordSet
    {
        [DataMember(Name = "features")]
        public GPFeature[] Features { get; set; }
    }

    /// <summary>
    /// Represents Linear Unit data type.
    /// </summary>
    [DataContract]
    internal class GPLinearUnit
    {
        /// <summary>
        /// Gets or sets a distance value.
        /// </summary>
        [DataMember(Name = "distance")]
        public double Distance { get; set; }

        /// <summary>
        /// Gets or sets distance measure units.
        /// </summary>
        [DataMember(Name = "units")]
        public string Units { get; set; }
    }

    /// <summary>
    /// GPBoolean class.
    /// </summary>
    [DataContract]
    public class GPBoolean
    {
        [DataMember(Name = "value")]
        public bool Value { get; set; }
    }

    /// <summary>
    /// GPDataFile class.
    /// </summary>
    [DataContract]
    internal class GPDataFile
    {
        [DataMember(Name = "url")]
        public string Url { get; set; }
    }

    /// <summary>
    /// GPOutParameter class.
    /// </summary>
    [DataContract]
    internal class GPOutParameter : GPResponse
    {
        [DataMember(Name = "paramName")]
        public string ParamName { get; set; }

        [DataMember(Name = "dataType")]
        public string DataType { get; set; }
    }

    /// <summary>
    /// GPFeatureRecordSetLayerParam class.
    /// </summary>
    [DataContract]
    internal class GPFeatureRecordSetLayerParam : GPOutParameter
    {
        [DataMember(Name = "value")]
        public GPFeatureRecordSetLayer Value { get; set; }
    }

    /// <summary>
    /// GPRecordSetParam class.
    /// </summary>
    [DataContract]
    internal class GPRecordSetParam : GPOutParameter
    {
        [DataMember(Name = "value")]
        public GPRecordSet Value { get; set; }
    }

    /// <summary>
    /// GPDataFileParam class.
    /// </summary>
    [DataContract]
    internal class GPDataFileParam : GPOutParameter
    {
        [DataMember(Name = "value")]
        public GPDataFile Value { get; set; }
    }

    /// <summary>
    /// GPLongParam class.
    /// </summary>
    [DataContract]
    internal class GPLongParam : GPOutParameter
    {
        [DataMember(Name = "value")]
        public int Value { get; set; }
    }

    /// <summary>
    /// GPBoolParam class.
    /// </summary>
    [DataContract]
    internal class GPBoolParam : GPOutParameter
    {
        [DataMember(Name = "value")]
        public bool Value { get; set; }
    }

}

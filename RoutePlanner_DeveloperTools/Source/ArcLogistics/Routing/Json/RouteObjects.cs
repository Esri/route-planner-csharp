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
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// RouteStopsRecordSet class.
    /// </summary>
    [DataContract]
    internal class RouteStopsRecordSet : GPRecordSet
    {
        [DataMember(Name = "doNotLocateOnRestrictedElements")]
        public bool DoNotLocateOnRestrictedElements { get; set; }
    }

    /// <summary>
    /// RouteRecordSet class.
    /// </summary>
    [DataContract]
    internal class RouteRecordSet
    {
        [DataMember(Name = "features")]
        public GPFeature[] Features { get; set; }
    }

    /// <summary>
    /// RouteSolveResponse class.
    /// </summary>
    [DataContract]
    internal class RouteSolveResponse : GPResponse
    {
        [DataMember(Name = "directions")]
        public RouteDirections[] Directions { get; set; }

        [DataMember(Name = "routes")]
        public RouteRecordSet Routes { get; set; }

        [DataMember(Name = "stops")]
        public RouteRecordSet Stops { get; set; }

        [DataMember(Name = "barriers")]
        public RouteRecordSet Barriers { get; set; }

        [DataMember(Name = "messages")]
        public RouteMessage[] Messages { get; set; }
    }

    /// <summary>
    /// RouteDirections class.
    /// </summary>
    [DataContract]
    internal class RouteDirections
    {
        [DataMember(Name = "routeId")]
        public int RouteId { get; set; }

        [DataMember(Name = "routeName")]
        public string RouteName { get; set; }

        [DataMember(Name = "summary")]
        public RouteDirectionSummary Summary { get; set; }

        [DataMember(Name = "features")]
        public GPCompactGeomFeature[] Features { get; set; }
    }

    /// <summary>
    /// RouteAttrParameters class.
    /// </summary>
    [DataContract]
    internal class RouteAttrParameters
    {
        [DataMember(Name = "parameters")]
        public RouteAttrParameter[] Parameters { get; set; }
    }

    /// <summary>
    /// RouteAttrParameter class.
    /// </summary>
    [DataContract]
    internal class RouteAttrParameter
    {
        [DataMember(Name = "attributeName")]
        public string AttrName { get; set; }

        [DataMember(Name = "parameterName")]
        public string ParamName { get; set; }

        [DataMember(Name = "value")]
        public object Value { get; set; }
    }

    /// <summary>
    /// RouteDirectionSummary class.
    /// </summary>
    [DataContract]
    internal class RouteDirectionSummary
    {
        [DataMember(Name = "totalLength")]
        public double TotalLength { get; set; }

        [DataMember(Name = "totalTime")]
        public double TotalTime { get; set; }

        [DataMember(Name = "totalDriveTime")]
        public double TotalDriveTime { get; set; }

        [DataMember(Name = "envelope")]
        public GPEnvelope Envelope { get; set; }
    }

    /// <summary>
    /// RouteMessage class.
    /// </summary>
    [DataContract]
    internal class RouteMessage
    {
        [DataMember(Name = "type")]
        public int Type { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }
    }
}

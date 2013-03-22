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
    /// Defines description information returned by dicovery service.
    /// </summary>
    [DataContract]
    internal class DiscoveryDescription
    {
        /// <summary>
        /// Layer Id.
        /// </summary>
        [DataMember(Name = "layerId")]
        public int LayerId
        {
            get;
            set;
        }

        /// <summary>
        /// Layer name.
        /// </summary>
        [DataMember(Name = "layerName")]
        public string LayerName
        {
            get;
            set;
        }

        /// <summary>
        /// Value.
        /// </summary>
        [DataMember(Name = "value")]
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// Display field name.
        /// </summary>
        [DataMember(Name = "displayFieldName")]
        public string DisplayFieldName
        {
            get;
            set;
        }

        /// <summary>
        /// Description attributes.
        /// </summary>
        [DataMember(Name = "attributes")]
        public AttrDictionary Attributes
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Defines object returning as a result of discovert service request.
    /// </summary>
    [DataContract]
    internal class DiscoveryServiceResponse : GPResponse
    {
        /// <summary>
        /// Collection of results objects.
        /// </summary>
        [DataMember(Name = "results")]
        public DiscoveryDescription[] Results
        {
            get;
            set;
        }
    }
}

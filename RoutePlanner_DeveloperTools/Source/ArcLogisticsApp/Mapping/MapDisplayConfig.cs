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
using System.Text;
using System.Xml.Serialization;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Map serialized configuration
    /// </summary>
    [DataContract]
    internal class MapDisplayConfig
    {
        public MapDisplayConfig()
        {
            OrderSelectedProp = new StringCollection();
            StopSelectedProp = new StringCollection();
        }

        [DataMember]
        public StringCollection OrderSelectedProp
        {
            get;
            set;
        }

        [DataMember]
        public StringCollection StopSelectedProp
        {
            get;
            set;
        }

        [DataMember]
        public bool TrueRoute
        {
            get;
            set;
        }

        [DataMember]
        public bool LabelingEnabled
        {
            get;
            set;
        }

        [DataMember]
        public bool ShowBarriers
        {
            get;
            set;
        }

        [DataMember]
        public bool ShowZones
        {
            get;
            set;
        }

        [DataMember]
        public bool ShowLeadingStemTime
        {
            get;
            set;
        }

        [DataMember]
        public bool ShowTrailingStemTime
        {
            get;
            set;
        }

        [DataMember]
        public bool AutoZoom
        {
            get;
            set;
        }
    }
}

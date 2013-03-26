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
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics
{
    public class AgsMapLayer : MapLayer
    {
        internal AgsMapLayer(MapServiceInfoWrap serviceInfo, AgsServer server, Map map)
            :base(serviceInfo, server, map)
        {
            Server = server;
        }

        // APIREV: need to create AgsMapLayer class that inhertis from 
        // MapLayer and implements the following propertues: SoapUrl, RestUrl, Server and Type

        public string SoapUrl
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string RestUrl
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        internal AgsServer Server
        {
            get;
            private set;
        }

        public string Type
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}

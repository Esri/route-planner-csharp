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

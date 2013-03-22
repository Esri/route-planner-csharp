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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcLogistics.App.GraphicObjects;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Clusterer class
    /// </summary>
    class ALClusterer : FlareClusterer
    {
        public const string GRAPHIC_PROPERTY_NAME = "Graphic";
        public const string COUNT_PROPERTY_NAME = "Count";
        public const int CLUSTER_COUNT_TO_EXPAND = 50;

        public event EventHandler OnClusteringComplete;

        public ALClusterer()
        {
            MaximumFlareCount = CLUSTER_COUNT_TO_EXPAND;
        }

        public override void ClusterGraphicsAsync(IEnumerable<Graphic> graphics, double resolution)
        {
            ClusterGraphicObject.DisposeAll();
            base.ClusterGraphicsAsync(graphics, resolution);
        }

        protected override Graphic OnCreateGraphic(GraphicCollection cluster, MapPoint point, int maxClusterCount)
        {
            if (OnClusteringComplete != null)
                OnClusteringComplete(this, EventArgs.Empty);

            if (cluster.Count == 1)
                return cluster[0];

            //if (cluster.Count > MaximumFlareCount)
            //    return base.OnCreateGraphic(cluster, point, maxClusterCount);

            return CreateCustomFlareCluster(cluster, point);
        }

        private static Graphic CreateCustomFlareCluster(GraphicCollection cluster, MapPoint point)
        {
            ClusterGraphicObject clGr = ClusterGraphicObject.Create(point, cluster);
            clGr.Attributes.Add(COUNT_PROPERTY_NAME, cluster.Count);

            for (int index = 0; index < cluster.Count; index++)
            {
                clGr.Attributes.Add(GRAPHIC_PROPERTY_NAME + index.ToString(), cluster[index]);
            }

            return clGr;
        }
    }
}
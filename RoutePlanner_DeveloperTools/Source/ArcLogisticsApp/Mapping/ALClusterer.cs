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
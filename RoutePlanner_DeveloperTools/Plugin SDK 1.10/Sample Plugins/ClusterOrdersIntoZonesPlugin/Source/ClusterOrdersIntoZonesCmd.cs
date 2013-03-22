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
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;

using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.App.Commands;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace ClusterOrdersIntoZonesPlugin
{
    // Add Task to Schedule tab's task pane.
    [CommandPlugIn(new string[1] { "ScheduleTaskWidgetCommands" })]

    public class ClusterOrdersIntoZonesCmd : AppCommands.ICommand
    {

        #region ICommand Members

        public void Execute(params object[] args)
        {
            string statusMessage = "Clustering orders.";
            App.Current.Messenger.AddInfo(statusMessage);
            
            /////////////////////////////////
            // Find Current Schedule, Routes and Orders
            /////////////////////////////////
            ESRI.ArcLogistics.DomainObjects.Schedule schedule = new ESRI.ArcLogistics.DomainObjects.Schedule();

            int numSchedules = App.Current.Project.Schedules.Count;

            foreach (ESRI.ArcLogistics.DomainObjects.Schedule s in App.Current.Project.Schedules.Search(App.Current.CurrentDate))
            {
                if (s.Name == "Current")
                {
                    schedule = s;
                    break;
                }
            }

            //Get number of Routes on current Schedule
            int numRoutes = schedule.Routes.Count;

            //Get number of Orders for current Date
            int numOrders = App.Current.Project.Orders.GetCount(App.Current.CurrentDate);

            if (numRoutes > 0 && numOrders > numRoutes)
            {
                /////////////////////////////////
                // Create Dataset
                /////////////////////////////////
                Point2[] data = new Point2[numOrders];
                Point2[] centres = new Point2[numRoutes];

                List<ESRI.ArcLogistics.DomainObjects.Order> oList = App.Current.Project.Orders.Search(App.Current.CurrentDate).ToList();
                for (int i = 0; i < numOrders; i++)
                {
                    ESRI.ArcLogistics.DomainObjects.Order o = oList.ElementAt(i);
                    Point2 p2 = new Point2(0, o.GeoLocation.Value.X, o.GeoLocation.Value.Y);
                    data[i] = p2;
                }

                /////////////////////////////////
                // Perform Clustering
                /////////////////////////////////

                int iterations = cluster(ref data, ref centres);

                /////////////////////////////////
                // Delete all previous zones from the project
                /////////////////////////////////

                for (int c = App.Current.Project.Zones.Count - 1; c >= 0; c--)
                {
                    ESRI.ArcLogistics.DomainObjects.Zone z = App.Current.Project.Zones.ElementAt(c);
                    App.Current.Project.Zones.Remove(z);
                }

                /////////////////////////////////
                // Delete all previous zones from the current day's routes
                /////////////////////////////////

                for (int j = 0; j < numRoutes; j++)
                {
                    for (int c = schedule.Routes[j].Zones.Count - 1; c >= 0; c--)
                    {
                        ESRI.ArcLogistics.DomainObjects.Zone z = schedule.Routes[j].Zones.ElementAt(c);
                        schedule.Routes[j].Zones.Remove(z);
                    }
                }


                /////////////////////////////////
                // Find Convex Hull, create Soft Zones and assign them to routes
                /////////////////////////////////

                List<Point3>[] DataList = new List<Point3>[numRoutes];
                for (int j = 0; j < numRoutes; j++)
                    DataList[j] = new List<Point3>();

                for (int i = 0; i < numOrders; i++)
                {
                    Point3 p3 = new Point3(data[i].X, data[i].Y);
                    DataList[data[i].cluster].Add(p3);
                }

                for (int j = 0; j < numRoutes; j++)
                {
                    // Dont create zones for less than 3 orders 
                    if (DataList[j].Count <= 2)
                        continue;

                    // Get Convex Hull
                    Point3[] chpts = ConvexPolygon.getConvexPolygon(DataList[j].ToArray());

                    
                    // Convert Point3 array to Geometry Point array 
                    ESRI.ArcLogistics.Geometry.Point[] polyPoints = new ESRI.ArcLogistics.Geometry.Point[chpts.Length];
                    for (int k = 0; k < chpts.Length; k++)
                    {
                        Point3 p3 = chpts[k];
                        
                        ESRI.ArcLogistics.Geometry.Point geoP = new ESRI.ArcLogistics.Geometry.Point();
                        geoP.X = p3.x;
                        geoP.Y = p3.y;

                        polyPoints[k] = geoP;
                    }

                    
                    // Create soft zone, and add to Project and to a Route
                    ESRI.ArcLogistics.Geometry.Polygon p = new ESRI.ArcLogistics.Geometry.Polygon(polyPoints);
                    ESRI.ArcLogistics.DomainObjects.Zone z = new ESRI.ArcLogistics.DomainObjects.Zone();
                    z.CreationTime = DateTime.Now.Ticks;
                    z.Geometry = p;
                    z.Name = String.Format("Zone {0}", j+1);

                    App.Current.Project.Zones.Add(z);
                    schedule.Routes[j].Zones.Add(z);
                    schedule.Routes[j].HardZones = false;

                }
                App.Current.Project.Save();
                statusMessage = "Finished clustering orders.";
                App.Current.Messenger.AddInfo(statusMessage);
            }
            else
            {
                statusMessage="";
                
                if(numOrders==0)
                    statusMessage = "No Orders found.";
                else if(numRoutes==0)
                    statusMessage = "No Routes found.";
                else if (numRoutes > numOrders)
                    statusMessage = "Insufficient number of orders.";
                
                App.Current.Messenger.AddError(statusMessage);
            }
        }

        public void Initialize(App app)
        {

        }

        public bool IsEnabled
        {
            get { return true; }
        }

        public KeyGesture KeyGesture
        {
            get { return null; }
        }

        public string Name
        {
            get { return "ClusterOrdersIntoZonesPlugin.ClusterOrdersIntoZonesCmd"; }
        }

        public string Title
        {
            get { return "Cluster Orders Into Zones"; }
        }

        public string TooltipText
        {
            get { return "Cluster the selected day's orders into soft zones for each route"; }
        }

        #endregion

        private int cluster(ref Point2[] data, ref Point2[] Means)
        {
            Random r = new Random((int)DateTime.Now.TimeOfDay.Ticks);

            int index = r.Next(data.Length);
            Means[0].cluster = 0;
            Means[0].X = data[index].X;
            Means[0].Y = data[index].Y;

            for (int j = 1; j < Means.Length; j++)
            {
                double[] tempDist = new double[data.Length];
                double tempMin = double.MaxValue;

                for (int i = 0; i < data.Length; i++)
                {
                    tempDist[i] = double.MaxValue;
                    for (int k = 0; k < j; k++)
                    {
                        tempMin = Distance(data[i], Means[k]);
                        if (tempMin < tempDist[i])
                            tempDist[i] = tempMin;
                    }
                }

                double randomD = r.NextDouble() * (tempDist.Sum());

                for (int i = 0; i < data.Length; i++)
                {
                    if (randomD < tempDist[i])
                    {
                        Means[j].cluster = j;
                        Means[j].X = data[i].X;
                        Means[j].Y = data[i].Y;
                        break;
                    }

                    randomD = randomD - tempDist[i];
                }
            }

            // Find K Means
            double[] Dist = new double[Means.Length];
            Point2[] sums = new Point2[Means.Length];
            bool reiterate = true;

            int iterations = 0;
            while (reiterate)
            {
                iterations++;
                if (iterations > 100) break;
                reiterate = false;

                for (int j = 0; j < Means.Length; j++)
                {
                    sums[j].X = 0.0;
                    sums[j].Y = 0.0;
                    sums[j].count = 0;
                }

                int i;
                for (i = 0; i < data.Length; i++)
                {
                    for (int j = 0; j < Means.Length; j++)
                    {
                        Dist[j] = Distance(data[i], Means[j]);
                    }

                    for (int j = 0; j < Means.Length; j++)
                    {
                        if (Dist[j].Equals((double)Dist.Min()) && data[i].cluster != j)
                        {
                            reiterate = true;
                            data[i].cluster = j;
                            break;
                        }
                    }

                    sums[data[i].cluster].X += data[i].X;
                    sums[data[i].cluster].Y += data[i].Y;
                    sums[data[i].cluster].count++;
                }


                for (int j = 0; j < Means.Length; j++)
                {
                    if (sums[j].count > 0)
                    {
                        Means[j].X = sums[j].X / sums[j].count;
                        Means[j].Y = sums[j].Y / sums[j].count;
                        Means[j].count = sums[j].count;
                    }
                }
            }
            return iterations;

        }

        private double Distance(Point2 P1, Point2 P2)
        {
            return Math.Sqrt(Math.Pow((P2.Y - P1.Y), 2) + Math.Pow((P2.X - P1.X), 2));
        }

        
    }

    struct Point2
    {
        public int cluster;
        public double X;
        public double Y;
        public int count;

        public Point2(int c, double x, double y)
        {
            cluster = c;
            X = x;
            Y = y;
            count = 0;
        }
        public Point2(Point2 P)
        {
            cluster = P.cluster;
            X = P.X;
            Y = P.Y;
            count = P.count;
        }
    }
}

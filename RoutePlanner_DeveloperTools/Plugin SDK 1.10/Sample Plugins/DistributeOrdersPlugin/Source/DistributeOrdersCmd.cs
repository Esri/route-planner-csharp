using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Data.OleDb;
using System.Windows.Input;
using System.Collections.Generic;


using ESRI.ArcLogistics;
using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.DomainObjects;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace DistributeOrdersPlugin
{
    [CommandPlugIn(new string[1] { "DistributeOrdersTaskWidgetCommands" })]
    public class DistributeOrdersCmd : AppCommands.ICommand
    {

        #region ICommand Members

        public void Execute(params object[] args)
        {
            string importPath = DistributeOrdersPage.importpath;
            int numDays = DistributeOrdersPage.numDays;
            string statusMessage = "Started distributing orders.";
            int numClusters = 0;

            m_application.MainWindow.StatusBar.WorkingStatus = statusMessage;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {

                if (File.Exists(importPath))
                {
                    //Read orders info from file
                    DataTable table = new DataTable();
                    table = ReadCSV(importPath);

                    if (numDays > 0)
                    {
                        statusMessage = "Distributing " + table.Rows.Count + " orders over " + numDays + " days.";
                        m_application.Messenger.AddInfo(statusMessage);

                        

                        /////////////////////////////////
                        // Count total number of routes
                        /////////////////////////////////
                        DateTime dt = new DateTime();
                        dt = m_application.CurrentDate;

                        for (int i = 0; i < numDays; i++)
                        {

                            if (m_application.Project.DefaultRoutes.Count > 0) 
                            {
                                List<Schedule> S = m_application.Project.Schedules.Search(dt.AddDays(i)).ToList();
                                
                                //If the Current Schedule has routes, use those, else use default routes
                                if (S.Count>0 && S[0].Routes.Count > 0)
                                    numClusters = numClusters + S[0].Routes.Count;
                                else
                                    foreach (Route route in m_application.Project.DefaultRoutes)
                                    {
                                        if (route.Days.DoesDaySatisfy(dt.AddDays(i)))
                                        {
                                            numClusters++;
                                        }
                                    }
                            }

                        }

                        statusMessage = "Creating " + numClusters + " clusters.";
                        m_application.Messenger.AddInfo(statusMessage);


                        /////////////////////////////////
                        // Create Dataset
                        /////////////////////////////////

                        List<ESRI.ArcLogistics.DomainObjects.Order> oList = processOrders(table);
                        int numOrders = oList.Count;
                        Point[] data = new Point[numOrders];
                        Point[] centres = new Point[numClusters];

                        for (int i = 0; i < numOrders; i++)
                        {
                            ESRI.ArcLogistics.DomainObjects.Order o = oList.ElementAt(i);
                            Point p = new Point(0, o.GeoLocation.Value.X, o.GeoLocation.Value.Y);
                            data[i] = p;
                        }

                        /////////////////////////////////
                        // Perform Clustering
                        /////////////////////////////////

                        int iterations = cluster(ref data, ref centres);
                        
                        /////////////////////////////////
                        // Add clustered orders to days
                        /////////////////////////////////
                        int numRoutes = 0;
                        int index = 0;
                        int tempNumOrders = 0;
                        for (int k = 0; k < numDays; k++)
                        {

                            if (m_application.Project.DefaultRoutes.Count > 0) 
                            {
                                List<Schedule> S = m_application.Project.Schedules.Search(dt.AddDays(k)).ToList();

                                if (S.Count > 0 && S[0].Routes.Count > 0)
                                    numRoutes =  S[0].Routes.Count;
                                else
                                    foreach (Route route in m_application.Project.DefaultRoutes)
                                        if (route.Days.DoesDaySatisfy(dt.AddDays(k)))
                                         numRoutes++;

                                for (int i = 0; i < numOrders; i++)
                                {
                                    if (data[i].cluster >= index && data[i].cluster < (index + numRoutes))
                                    {
                                        ESRI.ArcLogistics.DomainObjects.Order o = oList.ElementAt(i);
                                        o.PlannedDate = dt.AddDays(k);
                                        m_application.Project.Orders.Add(o);
                                        tempNumOrders++;
                                    }

                                }
                                statusMessage = "Added " + numRoutes + " Clusters (" + tempNumOrders + " orders) to " + dt.AddDays(k);
                                m_application.Messenger.AddInfo(statusMessage);


                                index = index + numRoutes;
                                numRoutes = 0;
                                tempNumOrders = 0;
                            }

                        }

                        m_application.Project.Save();
                        statusMessage = "Finished Distributing Orders";
                        m_application.Messenger.AddInfo(statusMessage);

                    }
                    else
                    {
                        statusMessage = "Please specify number of days.";
                        m_application.Messenger.AddError(statusMessage);
                    }

                }
                else
                {
                    statusMessage = "Import from " + importPath + " failed! Specified file does not exist.";
                    m_application.Messenger.AddError(statusMessage);
                }
            }

            catch (Exception e)
            {
                statusMessage = "Distribute Orders encountered an error: " + e.Message;
                m_application.Messenger.AddError(statusMessage);

            }
            finally
            {
                m_application.MainWindow.StatusBar.WorkingStatus = null;
                Mouse.OverrideCursor = null;
            }

            if (Mouse.OverrideCursor != null)
            {
                m_application.MainWindow.StatusBar.WorkingStatus = null;
                Mouse.OverrideCursor = null;
            }
        }

        public void Initialize(App app)
        {
            m_application = app;
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
            get { return "DistributeOrdersPlugin.DistributeOrdersCmd"; }
        }

        public string Title
        {
            get { return "Distribute Orders"; }
        }

        public string TooltipText
        {
            get { return "Distribute imported orders over the selected number of days, starting with the selected date."; }
        }

        public App m_application = null;
        
        #endregion



        private List<ESRI.ArcLogistics.DomainObjects.Order> processOrders(DataTable table)
        {
            List<ESRI.ArcLogistics.DomainObjects.Order> OrderList = new List<Order>();
            
            foreach (DataRow row in table.Rows)
            {
                
                // Create New empty Order
                CapacitiesInfo capInfo = m_application.Project.CapacitiesInfo;
                OrderCustomPropertiesInfo propInfo = m_application.Project.OrderCustomPropertiesInfo;
                ESRI.ArcLogistics.DomainObjects.Order resultOrder = new ESRI.ArcLogistics.DomainObjects.Order(capInfo, propInfo);

                OrderCustomPropertiesInfo orderPropertiesInfo = resultOrder.CustomPropertiesInfo;
                OrderCustomProperties orderProperties = resultOrder.CustomProperties;
                CapacitiesInfo orderCapacitiesInfo = resultOrder.CapacitiesInfo;
                Capacities orderCapacities = resultOrder.Capacities;
                bool geocodeProvided = false;
                bool geocodeCorrect = false;
                bool geocodedCorrectly = false;
                double tempD;
                DateTime TWdateTime;
                Double tempX = 0.0;
                Double tempY = 0.0;

                // Insert Order Information
                resultOrder.PlannedDate = m_application.CurrentDate;

                for (int i = 0; i < table.Columns.Count; i++)
                {
                    try
                    {
                        switch (table.Columns[i].ColumnName)
                        {
                            #region Case Statements
                            case "Name": resultOrder.Name = row["Name"].ToString(); break;
                            case "Address": resultOrder.Address.AddressLine = row["Address"].ToString(); break;
                            case "City": resultOrder.Address.Locality3 = row["City"].ToString(); break;
                            case "State": resultOrder.Address.StateProvince = row["State"].ToString(); break;
                            case "Zip": resultOrder.Address.PostalCode1 = row["Zip"].ToString(); break;
                            case "Zip4": resultOrder.Address.PostalCode2 = row["Zip4"].ToString(); break;
                            case "Country": resultOrder.Address.Country = row["Country"].ToString(); break;

                            case "PlannedDate":
                                DateTime tempDT = new DateTime();
                                if (System.DateTime.TryParse(row["PlannedDate"].ToString(), out tempDT))
                                    resultOrder.PlannedDate = tempDT;
                                break;

                            case "Priority":
                                if (row["Priority"].ToString() == "High") resultOrder.Priority = OrderPriority.High;
                                else if (row["Priority"].ToString() == "Normal") resultOrder.Priority = OrderPriority.Normal;
                                break;

                            case "OrderType":
                                if (row["OrderType"].ToString() == "Pickup") resultOrder.Type = OrderType.Pickup;
                                else if (row["OrderType"].ToString() == "Delivery") resultOrder.Type = OrderType.Delivery;
                                break;

                            case "ServiceTime":
                                if (Double.TryParse(row["ServiceTime"].ToString(), out tempD))
                                    resultOrder.ServiceTime = tempD;
                                break;

                           
                            case "TimeWindowStart":
                                string tempS = row["TimeWindowStart"].ToString();
                                if (DateTime.TryParse(tempS, out TWdateTime))
                                {
                                    if (TWdateTime.TimeOfDay != TimeSpan.Zero)
                                    {
                                        resultOrder.TimeWindow.From = TWdateTime.TimeOfDay;
                                        resultOrder.TimeWindow.IsWideOpen = false;
                                    }
                                }
                                break;

                            case "TimeWindowFinish":
                                if (DateTime.TryParse(row["TimeWindowFinish"].ToString(), out TWdateTime))
                                {
                                    if (TWdateTime.TimeOfDay != TimeSpan.Zero)
                                    {
                                        resultOrder.TimeWindow.To = TWdateTime.TimeOfDay;
                                        resultOrder.TimeWindow.IsWideOpen = false;
                                    }
                                }
                                break;

                            case "TimeWindow2Start":

                                if (DateTime.TryParse(row["TimeWindow2Start"].ToString(), out TWdateTime))
                                {
                                    if (TWdateTime.TimeOfDay != TimeSpan.Zero)
                                    {
                                        resultOrder.TimeWindow2.From = TWdateTime.TimeOfDay;
                                        resultOrder.TimeWindow2.IsWideOpen = false;
                                    }
                                }
                                break;

                            case "TimeWindow2Finish":

                                if (DateTime.TryParse(row["TimeWindow2Finish"].ToString(), out TWdateTime))
                                {
                                    if (TWdateTime.TimeOfDay != TimeSpan.Zero)
                                    {
                                        resultOrder.TimeWindow2.To = TWdateTime.TimeOfDay;
                                        resultOrder.TimeWindow2.IsWideOpen = false;
                                    }
                                }
                                break;

                            case "MaxViolationTime":
                                if (Double.TryParse(row["MaxViolationTime"].ToString(), out tempD))
                                    resultOrder.MaxViolationTime = tempD;
                                break;

                            case "VehicleSpecialties":
                                if (row["VehicleSpecialties"].ToString() != "")
                                {
                                    string[] stringSeparators = new string[] { ";", "," };
                                    string[] specialties = row["VehicleSpecialties"].ToString().Split(stringSeparators, StringSplitOptions.None);
                                    foreach (string s in specialties)
                                    {
                                        VehicleSpecialty vs = new VehicleSpecialty();
                                        vs.Name = s;
                                        foreach (VehicleSpecialty V in m_application.Project.VehicleSpecialties)
                                        {
                                            if (String.Compare(V.Name, vs.Name, true) == 0)
                                            {
                                                V.CopyTo(vs);
                                                m_application.Project.VehicleSpecialties.Remove(V);
                                                break;
                                            }
                                        }
                                        m_application.Project.VehicleSpecialties.Add(vs);
                                        resultOrder.VehicleSpecialties.Add(vs);
                                    }
                                }
                                break;

                            case "DriverSpecialties":
                                if (row["DriverSpecialties"].ToString() != "")
                                {
                                    string[] stringSeparators2 = new string[] { ";", "," };
                                    string[] specialties2 = row["DriverSpecialties"].ToString().Split(stringSeparators2, StringSplitOptions.None);
                                    foreach (string s in specialties2)
                                    {
                                        DriverSpecialty ds = new DriverSpecialty();
                                        ds.Name = s;

                                        foreach (DriverSpecialty D in m_application.Project.DriverSpecialties)
                                        {
                                            if (String.Compare(D.Name, ds.Name, true) == 0)
                                            {
                                                D.CopyTo(ds);
                                                m_application.Project.DriverSpecialties.Remove(D);
                                                break;
                                            }
                                        }
                                        m_application.Project.DriverSpecialties.Add(ds);
                                        resultOrder.DriverSpecialties.Add(ds);
                                    }
                                }
                                break;

                            case "X":
                                string x = row["X"].ToString();
                                if (x != "" && x != null)
                                    if (Double.TryParse(row["X"].ToString(), out tempX))
                                    {
                                        if (tempX >= -180.0 && tempX <= 180.0 && tempX != 0.0)
                                        {
                                            geocodeProvided = true;
                                            geocodeCorrect = true;
                                        }
                                        else if (tempX == 0.0)
                                            geocodeCorrect = true;
                                    }

                                break;

                            case "Y":
                                string y = row["Y"].ToString();
                                if (y != "" && y != null)
                                    if (Double.TryParse(row["Y"].ToString(), out tempY))
                                    {
                                        if (tempY >= -90.0 && tempY <= 90.0 && tempY != 0)
                                        {
                                            geocodeProvided = true;
                                            geocodeCorrect = true;
                                        }
                                        else if (tempY == 0.0)
                                            geocodeCorrect = true;
                                    }

                                break;
                            #endregion
                        }


                        if (orderProperties.Count > 0)
                        {
                            OrderCustomProperty orderPropertyInfoItem = null;
                            for (int j = 0; j < orderPropertiesInfo.Count; j++)
                            {
                                orderPropertyInfoItem = orderPropertiesInfo.ElementAt(j) as OrderCustomProperty;
                                string tempName = orderPropertyInfoItem.Name.Replace(" ", "");
                                if (tempName == table.Columns[i].ColumnName)
                                {
                                    orderProperties[j] = (row[table.Columns[i].ToString()].ToString());
                                    break;
                                }
                            }
                        }

                        if (orderCapacities.Count > 0)
                        {
                            CapacityInfo orderCapacityInfoItem = null;
                            for (int k = 0; k < orderCapacitiesInfo.Count; k++)
                            {
                                orderCapacityInfoItem = orderCapacitiesInfo.ElementAt(k);
                                string tempName = orderCapacityInfoItem.Name.Replace(" ", "");
                                if (tempName == table.Columns[i].ColumnName)
                                {
                                    if (Double.TryParse(row[table.Columns[i].ToString()].ToString(), out tempD))
                                        orderCapacities[k] = tempD;

                                    break;
                                }
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        string statusMessage = " Distribute Orders encountered a problem: " + e.Message;
                        m_application.Messenger.AddError(statusMessage);
                    }
                }

                resultOrder.CustomProperties = orderProperties;
                resultOrder.Capacities = orderCapacities;

                if (geocodeProvided && geocodeCorrect)
                {
                    AddressCandidate candidate1 = new AddressCandidate();
                    ESRI.ArcLogistics.Geometry.Point p = new ESRI.ArcLogistics.Geometry.Point(tempX, tempY);
                    candidate1.GeoLocation = p;
                    candidate1.Score = 100;
                    candidate1.Address = resultOrder.Address;

                    resultOrder.GeoLocation = candidate1.GeoLocation;
                    geocodedCorrectly = true;

                }
                else
                {
                    try
                    {
                        AddressCandidate candidate = new AddressCandidate();
                        candidate = m_application.Geocoder.Geocode(resultOrder.Address);
                        if (candidate != null)
                        {
                            resultOrder.GeoLocation = candidate.GeoLocation;
                            geocodedCorrectly = true;
                        }
                        else
                        {
                            string statusMessage = "Could not geocode address for: "+resultOrder.Name ;
                            m_application.Messenger.AddError(statusMessage);
                            //TODO: Handle orders which were not geocoded!! 
                        }
                    }
                    catch (Exception e)
                    {
                        string statusMessage = "Distribute Orders encountered a problem while geocoding addresses: " + e.Message;
                        m_application.Messenger.AddError(statusMessage);
                    }
                }

                // Add Order
                if (geocodedCorrectly)
                    OrderList.Add(resultOrder);
                else
                {
                    string statusMessage = "Distribute Orders encountered a problem while adding order: " + resultOrder.Name;
                    m_application.Messenger.AddError(statusMessage);
                }

            }

            return(OrderList);

        }

        public static DataTable ReadCSV(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            string full = Path.GetFullPath(filePath);
            string file = Path.GetFileName(full);
            string dir = Path.GetDirectoryName(full);


            string connectionStr = "Provider=Microsoft.Jet.OLEDB.4.0;"
              + "Data Source=\"" + dir + "\\\";"
              + "Extended Properties=\"text;HDR=Yes;FMT=Delimited;IMEX=1\"";

            string queryStr = "SELECT * FROM [" + file + "]";

            DataTable dataTable = new DataTable();

            OleDbDataAdapter oledbDataAdapter = new OleDbDataAdapter(queryStr, connectionStr);

            try
            {
                oledbDataAdapter.Fill(dataTable);
            }
            catch (InvalidOperationException e)
            {
                string s = e.Message;
            }

            oledbDataAdapter.Dispose();

            return dataTable;
        }



        #region Clustering Methods

        private int cluster(ref Point[] data, ref Point[] Means)
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
            Point[] sums = new Point[Means.Length];
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

        private double Distance(Point P1, Point P2)
        {
            return Math.Sqrt(Math.Pow((P2.Y - P1.Y), 2) + Math.Pow((P2.X - P1.X), 2));
        }

        #endregion
    }

    struct Point
    {
        public int cluster;
        public double X;
        public double Y;
        public int count;

        public Point(int c, double x, double y)
        {
            cluster = c;
            X = x;
            Y = y;
            count = 0;
        }
        public Point(Point P)            
        {
            cluster = P.cluster;
            X = P.X;
            Y = P.Y;
            count = P.count;

        }
    }




}

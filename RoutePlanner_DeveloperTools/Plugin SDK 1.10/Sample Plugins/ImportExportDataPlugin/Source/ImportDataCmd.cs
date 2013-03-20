using System;
using System.IO;
using System.Linq;
using System.Data;
using System.Data.OleDb;
using System.Windows.Input;
using System.ComponentModel;

using ESRI.ArcLogistics;
using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.DomainObjects;
using AppCommands = ESRI.ArcLogistics.App.Commands;
using Params = ImportExportDataPlugin.ImportExportPluginPreferencesPageParams;

namespace ImportExportDataPlugin
{
    [CommandPlugIn(new string[1] { "ScheduleTaskWidgetCommands" })]
    public class ImportDataCmd : AppCommands.ICommand, INotifyPropertyChanged
    {
        App m_application = null;

        public void Execute(params object[] args)
        {

            string importPath = Params.importPath;
            string statusMessage = "Started import from  " + Params.Instance.importName;
            m_application.Messenger.AddInfo(statusMessage);
            m_application.MainWindow.StatusBar.WorkingStatus = statusMessage;
            Mouse.OverrideCursor = Cursors.Wait;

            // If Import program exists then call program
            if (File.Exists(importPath))
            {

                // Create temp output file
                string tempOrdersFile = System.IO.Path.GetTempFileName();

                // Create new Import process
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = importPath;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.Arguments = tempOrdersFile;
                try
                {
                    // Start Process
                    p.Start();
                    p.WaitForExit(600 * 1000);
                    // Abort process if it runs for more than 10 minutes.
                    if (p.HasExited == false)
                        p.Kill();

                    //If exectutable was successful
                    if (p.ExitCode == 0)
                    {
                        processOrders(tempOrdersFile);
                        m_application.Project.Save();
                        statusMessage = "Importing from " + Params.Instance.importName + " completed.";
                        m_application.Messenger.AddInfo(statusMessage);
                    }
                    else
                    {
                        statusMessage = "Import from " + Params.Instance.importName + " failed with Exit Code: " + p.ExitCode.ToString();
                        m_application.Messenger.AddError(statusMessage);
                    }

                    if (File.Exists(tempOrdersFile))
                        File.Delete(tempOrdersFile);

                }
                catch (Exception e)
                {
                    statusMessage = " Import from " + Params.Instance.importName + " failed: " + e.Message;
                    m_application.Messenger.AddError(statusMessage);
                }

                finally
                {
                    if (p != null)
                        p.Close();

                }
            }

            else
            {
                statusMessage = " Import from " + Params.Instance.importName + " failed! Specified file does not exist.";
                m_application.Messenger.AddError(statusMessage);
            }

            m_application.MainWindow.StatusBar.WorkingStatus = null;
            Mouse.OverrideCursor = null;

        }

        private void processOrders(string path)
        {
            DataTable table = new DataTable();
            table = ReadCSV(path);

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
                        }// End Switch


                        if (orderProperties.Count > 0 )
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

                        if( orderCapacities.Count > 0)
                        {   
                            CapacityInfo orderCapacityInfoItem = null;
                            for (int k = 0; k < orderCapacitiesInfo.Count; k++)
                            {
                                orderCapacityInfoItem = orderCapacitiesInfo.ElementAt(k);
                                string tempName = orderCapacityInfoItem.Name.Replace(" ", "");
                                if (tempName == table.Columns[i].ColumnName)
                                {
                                    if(Double.TryParse(row[table.Columns[i].ToString()].ToString(), out tempD))
                                        orderCapacities[k] = tempD;
                                        
                                    break;
                                }
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        string statusMessage = " Import from " + Params.Instance.importName + " encountered a problem: " + e.Message;
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

                }
                else
                {
                    AddressCandidate candidate = m_application.Geocoder.Geocode(resultOrder.Address);
                    if (candidate != null)
                        resultOrder.GeoLocation = candidate.GeoLocation;
                    else
                    {
                        //TODO: Handle orders which were not geocoded!! 
                    }
                }

                // Add Order
                m_application.Project.Orders.Add(resultOrder);
            }

        }

        public static void setEnable(bool val)
        {
            Params.Instance.importButtonEnabled = val;
            if (val) Params.importTooltip = Params.ENABLED_IMPORT_TOOLTIP;
            else Params.importTooltip = Params.DISABLED_IMPORT_TOOLTIP;
        }

        public void Initialize(App app)
        {
            m_application = app;
            IsEnabled = Params.Instance.importButtonEnabled;
            Title = Params.Instance.importName;

            // Subscribe to settings change
            Params.Instance.PropertyChanged += new PropertyChangedEventHandler(_importButtonParamsPropertyChanged);
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            protected set
            {
                _isEnabled = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(IS_ENABLED_PROPERTY_NAME));
            }
        }

        public System.Windows.Input.KeyGesture KeyGesture
        {
            get { return null; }
        }

        public string Name
        {
            get { return "ImportExportDataPlugin.ImportDataCmd"; }
        }

        public string Title
        {
            get { return "Import Data from " + _Title; }
            protected set
            {
                _Title = Params.Instance.importName;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Title"));
            }
        }

        public string TooltipText
        {
            get { return Params.importTooltip; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private readonly string IS_ENABLED_PROPERTY_NAME = "IsEnabled";
        private static bool _isEnabled = true;
        private static string _Title = Params.Instance.importName;

        private void _importButtonParamsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsEnabled = Params.Instance.importButtonEnabled;
            Title = Params.Instance.importName;
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

            string queryStr = "SELECT * FROM [" + file +"]";
                        
            DataTable dataTable = new DataTable();

            OleDbDataAdapter oledbDataAdapter = new OleDbDataAdapter(queryStr, connectionStr);

            try
            {
                //fill the DataTable
                oledbDataAdapter.Fill(dataTable);
            }
            catch (InvalidOperationException e)
            {
                string s = e.Message;
            }

            oledbDataAdapter.Dispose();

            return dataTable;
        }
    }
}

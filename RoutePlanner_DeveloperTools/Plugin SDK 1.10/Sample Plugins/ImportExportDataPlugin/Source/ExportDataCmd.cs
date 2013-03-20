using System;
using System.IO;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;


using ESRI.ArcLogistics;
using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.Export;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.DomainObjects;
using AppCommands = ESRI.ArcLogistics.App.Commands;
using Params = ImportExportDataPlugin.ImportExportPluginPreferencesPageParams;

namespace ImportExportDataPlugin
{
    [CommandPlugIn(new string[1] { "ScheduleTaskWidgetCommands" })]
    public class ExportDataCmd : AppCommands.ICommand, INotifyPropertyChanged
    {

        void AppCommands.ICommand.Execute(params object[] args)
        {
            // Set Cursor to busy and display messages.
            string statusMessage = "Started exporting to " + Params.Instance.exportName;
            App.Current.Messenger.AddInfo(statusMessage);
            App.Current.MainWindow.StatusBar.WorkingStatus = statusMessage;
            Mouse.OverrideCursor = Cursors.Wait;

            // Create Output Data Structure
            ICollection<Schedule> _schedule = null;
            _schedule = App.Current.Project.Schedules.Search(App.Current.CurrentDate);
            ICollection<Schedule> _exportedSchedule = new List<Schedule>();
            foreach (Schedule S in _schedule)
                if (S.Name == "Current")
                {
                    _exportedSchedule.Add(S);
                }

            // Create temp output filenames
            string tempRoutesFile = System.IO.Path.GetTempFileName();
            string tempStopsFile = System.IO.Path.GetTempFileName();
            string tempOrdersFile = System.IO.Path.GetTempFileName();
            

            // Create temp profiles
            Profile RoutesProfile = _exporter.CreateProfile(ExportType.TextRoutes, tempRoutesFile);
            Profile StopsProfile = _exporter.CreateProfile(ExportType.TextStops, tempStopsFile);
            Profile OrdersProfile = _exporter.CreateProfile(ExportType.TextOrders, tempOrdersFile);

            // Call special export function to create temp files
            _DoSpecialExport(RoutesProfile, _exportedSchedule);
            _DoSpecialExport(StopsProfile, _exportedSchedule);
            _DoSpecialExport(OrdersProfile, _exportedSchedule);

            // If export program exists then call program
            if (File.Exists(Params.exportPath))
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = Params.exportPath;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.Arguments = tempRoutesFile + " " + tempStopsFile + " " + tempOrdersFile;

                try
                {
                    p.Start();
                    p.WaitForExit(600*1000);
                    // Abort process if it runs for more than 10 minutes.
                    if (p.HasExited == false)
                        p.Kill();

                    if (p.ExitCode == 0)
                    {
                        statusMessage = "Completed exporting to " + Params.Instance.exportName + " successfully.";
                        App.Current.Messenger.AddInfo(statusMessage);
                    }
                    else
                    {
                        statusMessage = " Export to " + Params.Instance.exportName + " failed with Exit Code: " + p.ExitCode.ToString();
                        App.Current.Messenger.AddError(statusMessage);
                    }

                }
                catch (Exception e)
                {
                    statusMessage = " Export to " + Params.Instance.exportName + " failed: " + e.Message;
                    App.Current.Messenger.AddError(statusMessage);
                }

                finally
                {
                    if (p != null)
                        p.Close();

                    App.Current.MainWindow.StatusBar.WorkingStatus = null;
                    Mouse.OverrideCursor = null;
                }
            }

            else
            {
                statusMessage = " Export to " + Params.Instance.exportName + " failed! Specified file does not exist.";
                App.Current.Messenger.AddError(statusMessage);
            }

            App.Current.MainWindow.StatusBar.WorkingStatus = null;
            Mouse.OverrideCursor = null;

        }
        
        public void Initialize(App app)
        {
            IsEnabled = Params.Instance.exportButtonEnabled;
            Title = Params.Instance.exportName;

            // Subscribe to settings change
            Params.Instance.PropertyChanged += new PropertyChangedEventHandler(_exportButtonParamsPropertyChanged);
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            protected set
            {
                _isEnabled = value;
                // Notify about property change.
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled"));
            }
        }

        public System.Windows.Input.KeyGesture KeyGesture
        {
            get { return null; }
        }

        public string Name
        {
            get { return "ImportExportDataPlugin.ExportDataCmd"; }
        }

        public string Title
        {
            get { return "Export Data to " + _Title; }
            protected set
            {
                _Title = Params.Instance.exportName;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Title"));
            }
        }

        public string TooltipText
        {
            get { return Params.exportTooltip; }
        }

        public ExportDataCmd Current
        {
            get { return this;}
        }
 
        private void _DoSpecialExport(Profile profile, ICollection<Schedule> exportedSchedule)
        {

            MapLayer currentMapLayer = null;
            foreach (MapLayer layer in App.Current.Map.Layers)
            {
                if (layer.IsVisible && layer.IsBaseMap)
                {
                    currentMapLayer = layer;
                    break;
                }
            }

            _exporter.DoExport(profile, exportedSchedule, currentMapLayer);

        }

        public static void setEnable(bool val)
        {
            Params.Instance.exportButtonEnabled = val;
            if (val) Params.exportTooltip = Params.ENABLED_EXPORT_TOOLTIP;
            else Params.exportTooltip = Params.DISABLED_EXPORT_TOOLTIP;
        }

        private void _exportButtonParamsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsEnabled = Params.Instance.exportButtonEnabled;
            Title = Params.Instance.exportName;
        }

        
        Exporter _exporter = new Exporter(App.Current.Project.CapacitiesInfo,
                                     App.Current.Project.OrderCustomPropertiesInfo,
                                     App.Current.Geocoder.AddressFields);

        public event PropertyChangedEventHandler PropertyChanged;        
        private static bool _isEnabled = true;
        private static string _Title = Params.Instance.exportName;
    }
}

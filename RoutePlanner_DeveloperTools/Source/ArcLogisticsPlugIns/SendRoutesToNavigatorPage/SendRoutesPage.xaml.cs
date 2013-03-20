using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcLogistics;
using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Widgets;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing;
using Xceed.Wpf.DataGrid;

namespace ArcLogisticsPlugIns.SendRoutesToNavigatorPage
{
    /// <summary>
    /// Interaction logic for SendRoutesPage.xaml.
    /// </summary>
    [PagePlugInAttribute ("Deployment")]
    public partial class SendRoutesPage : PageBase
    {
        #region constructors

        public SendRoutesPage()
        {
            _helpTopic = new HelpTopic(null, Properties.Resources.SendRoutesPageQuickHelp);

            InitializeComponent();
            IsRequired = true;
            IsAllowed = true;
            this.Loaded += new RoutedEventHandler(SendRoutesPage_Loaded);
        }

        #endregion

        #region Page Overrided Members

        public override string Name
        {
            get { return PAGE_NAME; }
        }

        public override string Title
        {
            get { return Properties.Resources.SendRoutesPageCaption; }
        }

        public override TileBrush Icon
        {
            get 
            {
                return Resources["SendRoutesBrush"] as ImageBrush; 
            }
        }

        #endregion

        #region PageBase overrided members

        /// <summary>
        /// Create page's widgets.
        /// </summary>
        protected override void CreateWidgets()
        {
            base.CreateWidgets();

            // add calendary widget
            var calendarWidget = new CalendarWidget();
            calendarWidget.Initialize(this);
            this.EditableWidgetCollection.Insert(0, calendarWidget);
        }

        /// <summary>
        /// Page help topic.
        /// </summary>
        public override HelpTopic HelpTopic
        {
            get { return _helpTopic; }
        }

        /// <summary>
        /// Page commands category name.
        /// </summary>
        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        /// <summary>
        /// Initialize page.
        /// </summary>
        /// <param name="app">Current application.</param>
        public override void Initialize(App app)
        {
            _app = app;
            _app.CurrentDateChanged += new EventHandler(SendRoutesPage_CurrentDateChanged);
        }

        #endregion

        #region private event handlers

        /// <summary>
        /// React on page loaded.
        /// </summary>
        private void SendRoutesPage_Loaded(object sender, RoutedEventArgs e)
        {
            // set void status bar content
            _app.MainWindow.StatusBar.SetStatus(this, "");
        }

        /// <summary>
        /// React on current day changed.
        /// </summary>
        private void SendRoutesPage_CurrentDateChanged(object sender, EventArgs e)
        {
            if (!this.IsLoaded)
            {
                return;
            }

            _InitRoutesCollection();
        }

        /// <summary>
        /// Inits grid structure.
        /// </summary>
        private void SendRoutesGrid_Initialized(object sender, EventArgs e)
        {
            // NOTE: init data grid structure one time when grid initialized
            _sourceCollection = (DataGridCollectionViewSource)LayoutRoot.FindResource("SendRoutesTable");

            // build collection source
            var itemPropertiesCollection = LayoutRoot.FindResource("itemProperties") as ArrayList;
            foreach (DataGridItemProperty property in itemPropertiesCollection)
                _sourceCollection.ItemProperties.Add(property);

            // build column collection
            var columns = LayoutRoot.FindResource("columns") as ArrayList;
            foreach (Column column in columns)
                SendRoutesGrid.Columns.Add(column);

            ColumnBase columnCkecked = SendRoutesGrid.Columns["IsChecked"];
            columnCkecked.CellEditor = (CellEditor)LayoutRoot.FindResource("CheckBoxCellEditor");
        }

        /// <summary>
        /// Inits grid collection.
        /// </summary>
        private void XceedGridRoutes_Loaded(object sender, RoutedEventArgs e)
        {
            // NOTE : when grid loaded (each time when page opens - update collection source)
            Debug.Assert(_sourceCollection != null);

            _InitRoutesCollection();
            _sourceCollection.Source = _routesConfigs;
        }

        /// <summary>
        /// React on sent routes config check.
        /// </summary>
        private void Checked_Click(object sender, RoutedEventArgs e)
        {
            _SetSendButtonEnabled();
        }

        /// <summary>
        /// React on async solve completed.
        /// </summary>
        private void _Cmd_AsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            if (_operationID.Equals(e.OperationId))
            {
                _app.Solver.AsyncSolveCompleted -= _Cmd_AsyncSolveCompleted;

                if (e.Cancelled)
                {
                    _app.MainWindow.Unlock();
                    WorkingStatusHelper.SetReleased();

                    _app.Messenger.AddInfo((string)_app.FindResource("GenerateDirectionsCancelledText"));
                }
                else if (e.Error != null)
                {
                    _OnSolveError(e.Error);
                }
                else
                {
                    _app.Messenger.AddInfo((string)_app.FindResource("GenerateDirectionsCompletedText"));

                    _app.Project.Save();
                    _app.MainWindow.Unlock();
                    WorkingStatusHelper.SetReleased();

                    _StartSendProcess();
                }
            }
        }

        /// <summary>
        /// React on send button click.
        /// </summary>
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var routesWithoutDirections =
                from config in _routesConfigs
                where ((null != config.Route) && !config.Route.Stops.Any(s => s.Directions != null))
                select config.Route;

            List<Route> routesToBuildDirections = routesWithoutDirections.ToList();
            if (0 < routesToBuildDirections.Count)
                _DoGenerateDirections(routesToBuildDirections);
            else
                _StartSendProcess();
        }

        /// <summary>
        /// React on SendButton is enabled changed.
        /// </summary>
        private void SendButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (SendButton.IsEnabled)
                SendButton.ToolTip = Properties.Resources.SendRoutesCommandEnabledTooltip;
            else
                SendButton.ToolTip = Properties.Resources.SendRoutesCommandDisabledTooltip;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Init sent routes collection.
        /// </summary>
        private void _InitRoutesCollection()
        {
            _routesConfigs.Clear();

            IDataObjectCollection<Schedule> schedules = (IDataObjectCollection<Schedule>)
                _app.Project.Schedules.Search(_app.CurrentDate);

            if (schedules.Count > 0)
            {
                Schedule currentSchedule = schedules[0];
                foreach (Route route in currentSchedule.Routes)
                {
                    if (route.Driver == null || route.Stops.Count == 0)
                        continue; // ignore not builded routes

                    SentRouteConfig sentRouteConfig = _CreateSendedRouteConfig(route);
                    _routesConfigs.Add(sentRouteConfig);
                }
            }
            _SetSendButtonEnabled();
        }

        /// <summary>
        /// Create sent route config from route.
        /// </summary>
        /// <param name="route">Route.</param>
        /// <returns>Sent route config.</returns>
        private SentRouteConfig _CreateSendedRouteConfig(Route route)
        {
            var sendedRouteConfig = new SentRouteConfig(route);

            var mobileDevice = route.Driver.MobileDevice;
            if (mobileDevice == null)
                mobileDevice = route.Vehicle.MobileDevice;

            sendedRouteConfig.RouteName = route.Name;
            if (mobileDevice != null && mobileDevice.SyncType != SyncType.None)
            {
                switch (mobileDevice.SyncType)
                {
                    case SyncType.ActiveSync:
                        sendedRouteConfig.SendMethod = string.Format((string)
                            Properties.Resources.SyncingToFormat, mobileDevice.ActiveSyncProfileName);
                        break;
                    case SyncType.EMail:
                        sendedRouteConfig.SendMethod = string.Format((string)
                            Properties.Resources.MailToFormat, mobileDevice.EmailAddress);
                        break;
                    case SyncType.Folder:
                        sendedRouteConfig.SendMethod = string.Format(
                           Properties.Resources.SaveToFormat, mobileDevice.SyncFolder);
                        break;
                    default:
                        sendedRouteConfig.SendMethod = Properties.Resources.SyncTypeIsNotSupported;;
                        break;
                }

                sendedRouteConfig.IsChecked = true;
            }
            else
            {
                sendedRouteConfig.IsChecked = false;
                if (mobileDevice == null)
                    sendedRouteConfig.SendMethod = Properties.Resources.MobileDeviceEmpty;
                else
                {
                    if (mobileDevice == route.Driver.MobileDevice)
                        sendedRouteConfig.SendMethod = Properties.Resources.SyncTypeNotSelectedForDriverDevice;
                    else
                        sendedRouteConfig.SendMethod = Properties.Resources.SyncTypeNotSelectedForVehicleDevice;
                }
            }

            return sendedRouteConfig;
        }

        /// <summary>
        /// Starts sending process.
        /// </summary>
        private void _StartSendProcess()
        {
            Cursor cursor = _app.MainWindow.Cursor;
            try
            {
                _app.MainWindow.Cursor = Cursors.Wait;
                var sendRoutesHelper = new SendRoutesHelper();
                sendRoutesHelper.Initialize(_app, this);
                sendRoutesHelper.Execute(_routesConfigs);
            }
            finally
            {
                _app.MainWindow.Cursor = cursor;
            }
        }

        /// <summary>
        /// Formats service communication error.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <param name="ex">Exception.</param>
        /// <returns>Communication error string.</returns>
        private string _FormatServiceCommunicationError(string serviceName,
                                                        CommunicationException ex)
        {
            switch (ex.ErrorCode)
            {
                case CommunicationError.ServiceTemporaryUnavailable:
                    {
                        var message = _app.GetString("ServiceTemporaryUnavailable", serviceName);
                        return message;
                    }

                case CommunicationError.ServiceResponseTimeout:
                    {
                        var message = _app.GetString("ServiceResponseTimeout", serviceName);
                        return message;
                    }

                default:
                    {
                        var message = _app.GetString("ServiceConnectionError", serviceName);
                        return SendRoutesHelper.FormatCommunicationError(message, ex);
                    }
            }
        }

        /// <summary>
        /// Reports error occured due to failure of the specified service.
        /// </summary>
        /// <param name="service">The name of the failed service.</param>
        /// <param name="exception">The exception occured during reports building.</param>
        private void _ReportServiceError(string service, Exception exception)
        {
            Debug.Assert(exception != null);
            Debug.Assert(service != null);

            Logger.Error(exception);

            Link link = null;
            string message = string.Empty;
            if (exception is AuthenticationException || exception is CommunicationException)
            {
                message = SendRoutesHelper.AddServiceMessageWithDetail(service, exception);

                if (exception is AuthenticationException)
                {
                    link = new Link((string)_app.FindResource("LicencePanelText"),
                       ESRI.ArcLogistics.App.Pages.PagePaths.LicensePagePath, 
                       ESRI.ArcLogistics.App.LinkType.Page);
                }
            }
            else
                message = Properties.Resources.UnknownError;

            var details = new List<MessageDetail>() { new MessageDetail(MessageType.Error, message, link) };
            _app.Messenger.AddError(Properties.Resources.SendingFailedMessage, details);
        }

        /// <summary>
        /// Handles solve error occured upond directions report generation.
        /// </summary>
        /// <param name="exception">The exception occured upon solving.</param>
        private void _OnSolveError(Exception exception)
        {
            Debug.Assert(exception != null);

            _app.MainWindow.Unlock();
            WorkingStatusHelper.SetReleased();

            var service = (string)_app.FindResource("ServiceNameRouting");
            _ReportServiceError(service, exception);
        }

        /// <summary>
        /// Does generation directions. Start solver operation.
        /// </summary>
        private void _DoGenerateDirections(IList<Route> routes)
        {
            WorkingStatusHelper.SetBusy((string)_app.FindResource("GenerateDirections"));

            var message = string.Format((string)_app.FindResource("GenerateDirectionsStartText"),
                routes.First().Schedule.PlannedDate.Value.ToShortDateString());

            _app.Messenger.AddInfo(message);

            _app.MainWindow.Lock(true);

            _app.Solver.AsyncSolveCompleted += _Cmd_AsyncSolveCompleted;

            try
            {
                _operationID = _app.Solver.GenerateDirectionsAsync(routes);
            }
            catch (Exception ex)
            {
                _app.Solver.AsyncSolveCompleted -= _Cmd_AsyncSolveCompleted;

                _OnSolveError(ex);
            }
        }

        /// <summary>
        /// Sets send button enabled property.
        /// </summary>
        private void _SetSendButtonEnabled()
        {
            SendButton.IsEnabled = false;
            foreach (SentRouteConfig sendedRouteConfig in _routesConfigs)
            {
                if (sendedRouteConfig.IsChecked)
                {
                    SendButton.IsEnabled = true;
                    break; // result founded
                }
            }
        }

        #endregion

        #region constants

        private const string PAGE_NAME = "SendRoutes";

        #endregion

        #region private members

        /// <summary>
        /// Route configs.
        /// </summary>
        private ObservableCollection<SentRouteConfig> _routesConfigs = new ObservableCollection<SentRouteConfig>();
        /// <summary>
        /// Page help topic.
        /// </summary>
        private HelpTopic _helpTopic;
        /// <summary>
        /// Data grid source collection.
        /// </summary>
        private DataGridCollectionViewSource _sourceCollection;

        /// <summary>
        /// Solver.Generate directions async operation ID.
        /// </summary>
        private Guid _operationID;

        #endregion
    }
}

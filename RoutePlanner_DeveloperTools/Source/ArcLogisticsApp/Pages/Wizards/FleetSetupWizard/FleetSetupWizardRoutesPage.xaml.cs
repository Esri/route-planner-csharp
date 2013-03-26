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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.BreaksHelpers;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;
using AppData = ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.App.Validators;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Pages.Wizards
{
    /// <summary>
    /// Interaction logic for FleetSetupWizardRoutesPage.xaml
    /// </summary>
    internal partial class FleetSetupWizardRoutesPage : WizardPageBase,
        ISupportBack, ISupportNext, ISupportCancel
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public FleetSetupWizardRoutesPage()
        {
            InitializeComponent();

            // init handlers
            this.Loaded += new RoutedEventHandler(fleetSetupWizardRoutesPage_Loaded);
            this.Unloaded += new RoutedEventHandler(fleetSetupWizardRoutesPage_Unloaded);
        }

        #endregion // Constructors

        #region ISupportBack members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when "Back" button clicked.
        /// </summary>
        public event EventHandler BackRequired;

        #endregion // ISupportBack members

        #region ISupportNext members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when "Next" button clicked.
        /// </summary>
        public event EventHandler NextRequired;

        #endregion // ISupportNext members

        #region ISupportCancel members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when "Cancel" button clicked.
        /// </summary>
        public event EventHandler CancelRequired;

        #endregion // ISupportCancel members

        #region Private properties

        /// <summary>
        /// Specialized context.
        /// </summary>
        private FleetSetupWizardDataContext DataKeeper
        {
            get
            {
                return DataContext as FleetSetupWizardDataContext;
            }
        }

        #endregion

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Page loaded handler.
        /// </summary>
        private void fleetSetupWizardRoutesPage_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.Assert(0 < DataKeeper.Locations.Count); // locations must present

            // If there are no default breaks - add default break to project configuration.
            if (DataKeeper.Project.BreaksSettings.BreaksType == null)
            {
                DataKeeper.Project.BreaksSettings.BreaksType = BreakType.TimeWindow;
                var timeWindowBreak = new TimeWindowBreak();
                DataKeeper.Project.BreaksSettings.DefaultBreaks.Add(timeWindowBreak);
            }

            // init controls
            if (!_isInited)
                _InitControls();

            _InitDataGridCollection();

            // validate controls
            _UpdatePageState();

            vehicleNumberTextBox.Focus();
            vehicleNumberTextBox.SelectAll();
        }

        /// <summary>
        /// Page unloaded handler.
        /// </summary>
        private void fleetSetupWizardRoutesPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _StoreChanges();
        }

        /// <summary>
        /// Vehicle number text changed handler.
        /// </summary>
        private void vehicleNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInited)
            {
                _UpdatePageState();

                _UpdateRoutes();
            }
        }

        /// <summary>
        /// Text box changed handler.
        /// </summary>
        private void maxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInited)
            {
                _UpdateRoutesValues();

                _UpdatePageState();
            }
        }

        /// <summary>
        /// Time text box changed handler.
        /// </summary>
        private void textBoxTime_TimeChanged(object sender, RoutedEventArgs e)
        {
            if (_isInited)
                _UpdateRoutesValues();
        }

        /// <summary>
        /// "Back" button click handler.
        /// </summary>
        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            if (null != BackRequired)
                BackRequired(this, EventArgs.Empty);
        }

        /// <summary>
        /// "Next" button click handler.
        /// </summary>
        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if (CanBeLeftValidator<Route>.IsValid(DataKeeper.Routes))
            {
                if (null != NextRequired)
                    NextRequired(this, EventArgs.Empty);
            }
            else
                CanBeLeftValidator<Route>.ShowErrorMessagesInMessageWindow(DataKeeper.Routes);
        }

        /// <summary>
        /// "Cancel" button click handler.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (null != CancelRequired)
                CancelRequired(this, EventArgs.Empty);
        }

        /// <summary>
        /// vehicleNumberTextBox previewkeydown handler.
        /// </summary>
        private void vehicleNumberTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key pressedKey = e.Key;
            if (pressedKey == Key.Up)
                _IncrementVehicleNumber();
            else if (pressedKey == Key.Down)
                _DecrementVehicleNumber();
            // else do nothing
        }

        /// <summary>
        /// vehicleNumberTextBox mousewheel handler.
        /// </summary>
        private void vehicleNumberTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int delta = e.Delta;
            if (delta > 0)
                _IncrementVehicleNumber();
            else if (delta < 0)
                _DecrementVehicleNumber();
            // else do nothing
        }

        /// <summary>
        /// incrementButton click handler.
        /// </summary>
        private void incrementButton_Click(object sender, RoutedEventArgs e)
        {
            _IncrementVehicleNumber();
        }

        /// <summary>
        /// decrementButton click handler.
        /// </summary>
        private void decrementButton_Click(object sender, RoutedEventArgs e)
        {
            _DecrementVehicleNumber();
        }

        /// <summary>
        /// Items collection changed handler.
        /// </summary>
        private void _items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _UpdateLayout();
        }

        /// <summary>
        /// xceedGrid item source changed handler.
        /// </summary>
        private void xceedGrid_OnItemSourceChanged(object sender, EventArgs e)
        {
            if (null != _items)
                _items.CollectionChanged -= _items_CollectionChanged;

            _items = xceedGrid.Items;
            _items.CollectionChanged += new NotifyCollectionChangedEventHandler(_items_CollectionChanged);

            _UpdateLayout();
        }

        /// <summary>
        /// If edited item has driver/vehicle, which names are the same as others
        /// routes names - raises property changed event for such routes.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">DataGridItemCancelEventArgs.</param>
        private void _DataGridCollectionViewSourceCommittingEdit(object sender, DataGridItemCancelEventArgs e)
        {
            Route editedRoute = e.Item as Route;
            foreach (Route route in DataKeeper.Routes)
            {
                // If drivers name are the same - raise driver property changed.
                if (route.Driver.Name == editedRoute.Driver.Name)
                    (route as IForceNotifyPropertyChanged).RaisePropertyChangedEvent(Route.PropertyNameDriver);

                // If vehicle name are the same - raise vehicle property changed.
                if (route.Vehicle.Name == editedRoute.Vehicle.Name)
                    (route as IForceNotifyPropertyChanged).RaisePropertyChangedEvent(Route.PropertyNameVehicle);
            }
        }
        #endregion // Event handlers

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Loads grid layout.
        /// </summary>
        private void _InitDataGridLayout()
        {
            xceedGrid.OnItemSourceChanged += new EventHandler(xceedGrid_OnItemSourceChanged);

            DataGridCollectionViewSource collectionSource = (DataGridCollectionViewSource)layoutRoot.FindResource(COLLECTION_SOURCE_KEY);
            GridStructureInitializer structureInitializer = new GridStructureInitializer(GridSettingsProvider.FleetRoutesGridStructure);

            structureInitializer.BuildGridStructure(collectionSource, xceedGrid);
        }

        /// <summary>
        /// Gets row definitions from page root.
        /// </summary>
        /// <returns>Page's root row definitions.</returns>
        private RowDefinitionCollection _GetRowDefinitions()
        {
            var layoutBorder = (Border)layoutRoot.Children[0];
            var rootGrid = (Grid)layoutBorder.Child;
            return rootGrid.RowDefinitions;
        }

        /// <summary>
        /// Gets row definition for routes table.
        /// </summary>
        /// <param name="rowDefinitions">Page's root row definitions.</param>
        /// <returns>Row definition for routes table.</returns>
        private RowDefinition _GetRowDefinitionForRoutesTable(RowDefinitionCollection rowDefinitions)
        {
            Debug.Assert(null != rowDefinitions);
            return rowDefinitions[DATA_GRID_ROW_DEFINITION_INDEX];
        }

        /// <summary>
        /// Gets row definition for routes table.
        /// </summary>
        /// <returns>Row definition for routes table.</returns>
        private RowDefinition _GetRowDefinitionForRoutesTable()
        {
            var definitions = _GetRowDefinitions();
            return _GetRowDefinitionForRoutesTable(definitions);
        }

        /// <summary>
        /// Updates page layout.
        /// </summary>
        private void _UpdateLayout()
        {
            // make rotes table space grow as possible

            var rowDefinitions = _GetRowDefinitions();
            var routesTableRowDefinition = _GetRowDefinitionForRoutesTable(rowDefinitions);
            if (0 == DataKeeper.Routes.Count)
                routesTableRowDefinition.Height = new GridLength(0);
            else
            {
                // it's not maximum - do work
                var rowsToShowCount = Math.Max(MINIMUM_ROW_COUNT, xceedGrid.Items.Count);
                var newHeight = (rowsToShowCount + HEADER_ROWS_COUNT) * DEFAULT_ROW_HEIGHT;

                if (newHeight < (layoutRoot.ActualHeight - LABELS_SPACE))
                {   // if all items accommodate in grid with half size of frame, set size on items
                    routesTableRowDefinition.Height = new GridLength(newHeight);
                }
                else
                {   // otherwise set grid size to all free space of frame
                    rowDefinitions[rowDefinitions.Count - 1].Height = new GridLength(0);
                        // NOTE: last column have free all space - chenge this settings
                    routesTableRowDefinition.Height = new GridLength(1, System.Windows.GridUnitType.Star);
                }
            }
        }

        /// <summary>
        /// Inits page controls.
        /// </summary>
        private void _InitControls()
        {
            Debug.Assert(!_isInited); // only once

            // init page controls
            var defaults = Defaults.Instance;

            // init Max Order from Defaults
            if (null == maxOrderTextBox.Value)
                maxOrderTextBox.Value = (UInt32)defaults.RoutesDefaults.MaxOrder;

            // init Max Work Time from Defaults
            if (null == maxWorkTimeTextBox.Value)
                maxWorkTimeTextBox.Value = (UInt32)UnitConvertor.Convert(defaults.RoutesDefaults.MaxTotalDuration, Unit.Minute, Unit.Hour);

            // init Start Time Window from Defaults
            var startTimeWindow = defaults.RoutesDefaults.StartTimeWindow;
            textBoxStart.Time = startTimeWindow.From;
            textBoxEnd.Time = startTimeWindow.To;
            textBoxStart.IsEnabled = textBoxEnd.IsEnabled = !startTimeWindow.IsWideopen;

            // init table
            _InitDataGridLayout();

            // init vehicle number from project
            int routesCount = DataKeeper.Routes.Count;
            if (0 == (UInt32)vehicleNumberTextBox.Value)
            {
                vehicleNumberTextBox.Value = (UInt32)((0 == routesCount) ? DEFAULT_ROUTE_COUNT : routesCount);
                if (0 == routesCount) // defaults records is shown immediately in table
                    _DoGrowList(DEFAULT_ROUTE_COUNT);
            }

            _isInited = true;
        }

        /// <summary>
        /// Inits collection of route.
        /// </summary>
        private void _InitDataGridCollection()
        {
            var routes = DataKeeper.Routes;

            // init source collection
            var sortedRoutesCollection = new AppData.SortedDataObjectCollection<Route>(routes, new RoutesComparer());

            DataGridCollectionViewSource collectionSource =
                (DataGridCollectionViewSource)layoutRoot.FindResource(COLLECTION_SOURCE_KEY);
            collectionSource.Source = sortedRoutesCollection;

        }

        /// <summary>
        /// Updates page controls state.
        /// </summary>
        private void _UpdatePageState()
        {
            var routesCount = DataKeeper.Routes.Count;
            var isRoutePresent = (0 < routesCount);
            var isVehicleNumberSet = (0 < (UInt32)vehicleNumberTextBox.Value);
            var isFullState = (isVehicleNumberSet && isRoutePresent);

            // init state of controls
            var controlVisibility = isFullState ? Visibility.Visible : Visibility.Collapsed;
            border.Visibility =
                textBoxEditTooltip.Visibility =
                    gridBorder.Visibility =
                        xceedGrid.Visibility =
                            textBoxEndTooltip.Visibility = controlVisibility;

            buttonNext.IsEnabled = isFullState && isRoutePresent;
        }

        /// <summary>
        /// Ends edit table.
        /// </summary>
        private void _EndEditTable()
        {
            try
            {
                xceedGrid.EndEdit();
            }
            catch
            {
                xceedGrid.CancelEdit();
            }
        }

        /// <summary>
        /// Gets Max Order value from GUI.
        /// </summary>
        /// <returns>Max Order value.</returns>
        private int _GetInputedMaxOrder()
        {
            return (null == maxOrderTextBox.Value) ? 0 : (int)((UInt32)maxOrderTextBox.Value);
        }

        /// <summary>
        /// Gets Max Work Time value from GUI.
        /// </summary>
        /// <returns>Max Work Time value.</returns>
        private int _GetInputedMaxWorkTime()
        {
            double inputedValue = 0;
            if (null != maxWorkTimeTextBox.Value)
                inputedValue = (double)((UInt32)maxWorkTimeTextBox.Value);
            return (int)UnitConvertor.Convert(inputedValue, Unit.Hour, Unit.Minute);
        }

        /// <summary>
        /// Gets Max Travel Duration form Defaults with valid correction.
        /// </summary>
        /// <param name="maxWorkTime">Max Work Time to correction Max Travel Duration.</param>
        /// <returns>Valid value for Max Travel Duration.</returns>
        private int _GetCorrectedMaxTravelDuration(int maxWorkTime)
        {
            return Math.Min(maxWorkTime, Defaults.Instance.RoutesDefaults.MaxTravelDuration);
        }

        /// <summary>
        /// Updates values of all created routes from GUI.
        /// </summary>
        private void _UpdateRoutesValues()
        {
            // update routes from GUI values
            int maxOrders = _GetInputedMaxOrder();
            int maxWorkTime = _GetInputedMaxWorkTime();
            int maxTravelDuration = _GetCorrectedMaxTravelDuration(maxWorkTime);

            var routes = DataKeeper.Routes;
            for (int index = 0; index < routes.Count; ++index)
            {
                var route = routes[index];

                TimeWindow startTimeWindow = route.StartTimeWindow;
                startTimeWindow.From = textBoxStart.Time;
                startTimeWindow.To = textBoxEnd.Time;
                route.MaxOrders = maxOrders;
                route.MaxTravelDuration = maxTravelDuration;
                route.MaxTotalDuration = maxWorkTime;
            }
        }

        /// <summary>
        /// Updates routes list.
        /// </summary>
        private void _UpdateRoutes()
        {
            // get user choice
            var newNumber = (int)(UInt32)vehicleNumberTextBox.Value;

            // get presented objects
            var presentedNumber = DataKeeper.Routes.Count;
            if (presentedNumber != newNumber)
            {   // count changed
                if (newNumber < presentedNumber)
                    // need remove needles
                    _DoShrinkList(newNumber);
                else if (presentedNumber < newNumber)
                    // need add new vehicle
                    _DoGrowList(newNumber);
                // else do nothing

                _UpdatePageState();
            }
        }

        /// <summary>
        /// Incrementes vehicle number.
        /// </summary>
        private void _IncrementVehicleNumber()
        {
            var value = (UInt32)vehicleNumberTextBox.Value;
            var maxValue = (UInt32)vehicleNumberTextBox.MaxValue;
            if (value < maxValue)
                vehicleNumberTextBox.Value = value + 1;
        }

        /// <summary>
        /// Decrementes vehicle number.
        /// </summary>
        private void _DecrementVehicleNumber()
        {
            if (vehicleNumberTextBox.HasValidationError)
                return;

            var value = (UInt32)vehicleNumberTextBox.Value;
            var minValue = (UInt32)vehicleNumberTextBox.MinValue;
            if (minValue < value)
                vehicleNumberTextBox.Value = value - 1;
        }

        /// <summary>
        /// Stores changes in project.
        /// </summary>
        private void _StoreChanges()
        {
            DataKeeper.Project.Save();
        }

        /// <summary>
        /// Gets from full collection all object not presented in filtred objects.
        /// </summary>
        /// <param name="ignoredObjects">Ignored object collection.</param>
        /// <param name="allObjects">Full object collection.</param>
        /// <returns>Object colletion from full collection not presented in filtred objects.</returns>
        private IList<T> _GetNotUsedObjects<T>(ICollection<T> ignoredObjects,
                                               AppData.IDataObjectCollection<T> allObjects)
            where T : AppData.DataObject
        {
            var filtredObject = new List<T>();
            for (int index = 0; index < allObjects.Count; ++index)
            {
                T obj = allObjects[index];
                if (null == _GetObjectByName(obj.ToString(), ignoredObjects))
                    filtredObject.Add(obj);
            }

            return filtredObject;
        }

        /// <summary>
        /// Get object by name.
        /// </summary>
        /// <param name="name">Object name.</param>
        /// <param name="collection">Objects collections.</param>
        /// <returns>Object or null if not founded.</returns>
        private T _GetObjectByName<T>(string name, ICollection<T> collection)
            where T : AppData.DataObject
        {
            var objects = from obj in collection
                where obj.ToString() == name
                select obj;

            return objects.FirstOrDefault();
        }

        /// <summary>
        /// Checks is object present in object collection.
        /// </summary>
        /// <param name="obj">Object to check.</param>
        /// <param name="collection">Object's collection.</param>
        /// <returns>TRUE if cheked object present in collection.</returns>
        private bool _IsObjectPresentInCollection<T>(T obj, IList<T> collection)
            where T : AppData.DataObject
        {
            var objName = obj.ToString();
            return (null != _GetObjectByName(objName, collection));
        }

        /// <summary>
        /// Gets object unique name.
        /// </summary>
        /// <param name="baseName">Base object name.</param>
        /// <param name="collection">Objects collections.</param>
        /// <returns>Dublicate unique name.</returns>
        private string _GetNewName<T>(string baseName, ICollection<T> collection)
            where T : AppData.DataObject
        {
            var index = 1;
            var newName = string.Format(NEW_NAME_FORMAT, baseName, index);
            while (null != _GetObjectByName(newName, collection))
                newName = string.Format(NEW_NAME_FORMAT, baseName, ++index);

            return newName;
        }

        /// <summary>
        /// Gets driver to init route.
        /// </summary>
        /// <param name="list">Drivers for first selection.</param>
        /// <param name="index">Driver index.</param>
        /// <returns>Driver object.</returns>
        private Driver _GetNextDriver(IList<Driver> list, int index)
        {
            Driver obj = null;
            if (index < list.Count)
                obj = list[index]; // get presented
            else
            {   // create new object
                AppData.IDataObjectCollection<Driver> drivers = DataKeeper.Project.Drivers;

                obj = new Driver();
                obj.Name = _GetNewName(App.Current.FindString("Driver"), drivers);

                drivers.Add(obj);
            }

            return obj;
        }

        /// <summary>
        /// Gets vehicle to init route.
        /// </summary>
        /// <param name="list">Vehicles for first selection.</param>
        /// <param name="index">Vehicle index.</param>
        /// <returns>Vehicle object.</returns>
        private Vehicle _GetNextVehicle(IList<Vehicle> list, int index)
        {
            Vehicle obj = null;
            if (index < list.Count)
                obj = list[index]; // get presented
            else
            {   // create new object
                AppData.IDataObjectCollection<Vehicle> vehicles = DataKeeper.Project.Vehicles;

                obj = new Vehicle(DataKeeper.Project.CapacitiesInfo);
                obj.Name = _GetNewName(App.Current.FindString("Vehicle"), vehicles);

                // init fuel type - use first presented
                AppData.IDataObjectCollection<FuelType> fuelTypes = DataKeeper.Project.FuelTypes;
                if (0 < fuelTypes.Count)
                    obj.FuelType = fuelTypes[0];

                vehicles.Add(obj);
            }

            return obj;
        }

        /// <summary>
        /// Does grow list of routes.
        /// </summary>
        /// <param name="vehicleNumber">New vehicle number.</param>
        private void _DoGrowList(int vehicleNumber)
        {
            _EndEditTable();

            // select objects in use
            List<Driver> driversInUse = new List<Driver>();
            List<Vehicle> vehiclesInUse = new List<Vehicle>();

            AppData.IDataObjectCollection<Route> routes = DataKeeper.Routes;
            for (int index = 0; index < routes.Count; ++index)
            {
                Route route = routes[index];
                if (!_IsObjectPresentInCollection(route.Driver, driversInUse))
                    driversInUse.Add(route.Driver);
                if (!_IsObjectPresentInCollection(route.Vehicle, vehiclesInUse))
                    vehiclesInUse.Add(route.Vehicle);
            }

            Project project = DataKeeper.Project;

            // filter project objects - select not used objects
            IList<Driver> drivers = _GetNotUsedObjects(driversInUse, project.Drivers);
            IList<Vehicle> vehicles = _GetNotUsedObjects(vehiclesInUse, project.Vehicles);

            // use older as default location
            var sortCollection =
                from loc in project.Locations
                orderby loc.CreationTime
                select loc;
            Location defaultLocation = sortCollection.First();

            // create, init and add new routes
            int maxOrders = _GetInputedMaxOrder();
            int maxWorkTime = _GetInputedMaxWorkTime();
            int maxTravelDuration = _GetCorrectedMaxTravelDuration(maxWorkTime);

            int presentedVehicleNumber = DataKeeper.Routes.Count;

            int needAddCount = vehicleNumber - presentedVehicleNumber;
            for (int index = 0; index < needAddCount; ++index)
            {   // create route
                Route route = project.CreateRoute();
                // init route
                route.Name = _GetNewName(App.Current.FindString("Route"), routes);
                route.Color = RouteColorManager.Instance.NextRouteColor(routes);

                route.StartLocation = defaultLocation;
                route.EndLocation = defaultLocation;
                route.Driver = _GetNextDriver(drivers, index);
                route.Vehicle = _GetNextVehicle(vehicles, index);
                TimeWindow startTimeWindow = route.StartTimeWindow;
                startTimeWindow.From = textBoxStart.Time;
                startTimeWindow.To = textBoxEnd.Time;
                route.MaxOrders = maxOrders;
                route.MaxTravelDuration = maxTravelDuration;
                route.MaxTotalDuration = maxWorkTime;

                routes.Add(route);
            }

            _StoreChanges();
        }

        /// <summary>
        /// Deletes objects from project collection.
        /// </summary>
        /// <param name="deleteingObjects">List of deleting objects.</param>
        /// <param name="objectCollection">Project object collection.</param>
        private void _DeleteObjects<T>(IList<T> deleteingObjects, AppData.IDataObjectCollection<T> objectCollection)
            where T : AppData.DataObject
        {
            for (int index = 0; index < deleteingObjects.Count; ++index)
                objectCollection.Remove(deleteingObjects[index]);
        }

        /// <summary>
        /// Deletes routes from project collection.
        /// </summary>
        /// <param name="deletingRoutes">List of deleting routes.</param>
        private void _DeleteRoutes(IList<Route> deletingRoutes)
        {
            AppData.IDataObjectCollection<Route> routes = DataKeeper.Routes;

            // store route relative objects
            List<Driver> deletingDrivers = new List<Driver>();
            List<Vehicle> deletingVehicles = new List<Vehicle>();
            // remove routes
            for (int index = 0; index < deletingRoutes.Count; ++index)
            {
                Route route = deletingRoutes[index];

                Driver driver = route.Driver;
                if (!_IsObjectPresentInCollection(driver, deletingDrivers))
                    deletingDrivers.Add(driver);

                Vehicle vehicle = route.Vehicle;
                if (!_IsObjectPresentInCollection(vehicle, deletingVehicles))
                    deletingVehicles.Add(vehicle);

                routes.Remove(route);
            }

            // remove drivers
            _DeleteObjects(deletingDrivers, DataKeeper.Project.Drivers);

            // remove vehicles
            _DeleteObjects(deletingVehicles, DataKeeper.Project.Vehicles);
        }

        /// <summary>
        /// Does shrinks list of routes.
        /// </summary>
        /// <param name="vehicleNumber">New vehicle number.</param>
        private void _DoShrinkList(int vehicleNumber)
        {
            _EndEditTable();

            var routes = DataKeeper.Routes;

            // do temporary copy
            var routesTmp = new List<Route>(routes);

            // sort by creation time
            var sortCollection =
                from rt in routesTmp
                    orderby rt.CreationTime
                    select rt;

            // prepare deleting route list
            var sortedRoutes = sortCollection.ToList();

            var deletingRoutes = new List<Route>(sortedRoutes.Count - vehicleNumber);
            for (int index = vehicleNumber; index < sortedRoutes.Count; ++index)
                deletingRoutes.Add(sortedRoutes[index]);

            // do delete
            _DeleteRoutes(deletingRoutes);

            _StoreChanges();
        }

        #endregion // Private methods

        #region Private consts
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string COLLECTION_SOURCE_KEY = "routesCollection";
        private const string NAME_PROPERTY_STRING = "Name";

        private const string NEW_NAME_FORMAT = "{0} {1}";

        private const int DEFAULT_ROUTE_COUNT = 1;

        private const double LABELS_SPACE = 255;
        private const int HEADER_ROWS_COUNT = 2;
        private const int MINIMUM_ROW_COUNT = 3;
        private const int DATA_GRID_ROW_DEFINITION_INDEX = 5;
        private readonly double DEFAULT_ROW_HEIGHT = (double)App.Current.FindResource("XceedRowDefaultHeight");

        #endregion // Private consts

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is inited flag.
        /// </summary>
        private bool _isInited;

        /// <summary>
        /// Items for routes table.
        /// </summary>
        private INotifyCollectionChanged _items;

        #endregion // Private fields

    }
}

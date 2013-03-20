using System;
using System.Diagnostics;
using System.Collections.Generic;

using Xceed.Wpf.DataGrid;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Validators;
using ESRI.ArcLogistics.App.GridHelpers;
using System.ComponentModel;
using System.Reflection;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using System.Collections.ObjectModel;
using System.Windows;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Class for handling routes context.
    /// </summary>
    internal class RoutesListViewContextHandler : IListViewContextHandler
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentPage">Optimize and edit page.</param>
        public RoutesListViewContextHandler(OptimizeAndEditPage parentPage)
        {
            Debug.Assert(null != parentPage);
            Debug.Assert(null != parentPage.CurrentSchedule);

            _parentPage = parentPage;
            _routes = parentPage.CurrentSchedule.Routes;
            _dataGridControl = parentPage.RoutesView.RoutesGrid;
            _dataGridControl.InitializingInsertionRow += new EventHandler<InitializingInsertionRowEventArgs>(xceedGrid_InitializingInsertionRow);
            _mapControl = parentPage.MapView.mapCtrl;
            _mapControl.EndEditRouteCallback = _EditEnded;
        }

        #endregion

        #region IContextHandler Members

        /// <summary>
        /// Creates new object.
        /// </summary>
        public void CreateNewItem(DataGridCreatingNewItemEventArgs e)
        {
            Debug.Assert(e.NewItem is Route);

            if (_mapControl.IsInEditedMode)
                e.Cancel = true;
            else
            {
                e.NewItem = _CreateRouteWithDefaultValues();
                _addedRoute = (Route)e.NewItem;
                e.Handled = true;
                _mapControl.StartEdit(e.NewItem);
                _statusBuilder.FillCreatingStatus((string)App.Current.FindResource("Route"), _parentPage);
            }
        }

        /// <summary>
        /// React on new item committed.
        /// </summary>
        public void CancellingNewItem(DataGridItemHandledEventArgs e)
        {
            Debug.Assert(_mapControl.EditedObject is Route);

            _mapControl.EditEnded();

            _addedRoute = null;

            string status = string.Format((string)App.Current.FindResource(OptimizeAndEditPage.NoSelectionStatusFormat),
                                          _parentPage.CurrentSchedule.Routes.Count,
                                          _parentPage.CurrentSchedule.UnassignedOrders.Count);
            App.Current.MainWindow.StatusBar.SetStatus(_parentPage, status);
        }

        /// <summary>
        /// Adds created object to source collection.
        /// </summary>
        public void CommitNewItem(DataGridCommittingNewItemEventArgs e)
        {
            Debug.Assert(e.Item is Route);

            _mapControl.EditEnded();
            ICollection<Route> source = e.CollectionView.SourceCollection as ICollection<Route>;

            source.Add(_addedRoute);

            e.Index = source.Count - 1;
            e.NewCount = source.Count;

            App.Current.Project.Save();

            _addedRoute = null;
            e.Handled = true;

            _statusBuilder.FillSelectionStatusWithoutCollectionSize(((ICollection<Route>)e.CollectionView.SourceCollection).Count, (string)App.Current.FindResource("Route"), 0, _parentPage);
        }

        /// <summary>
        /// React on new item committed.
        /// </summary>
        public void CommittedNewItem(DataGridItemEventArgs e)
        {
        }

        /// <summary>
        /// React on begin edit item.
        /// </summary>
        public void BeginEditItem(DataGridItemCancelEventArgs e)
        {
            Route route = e.Item as Route;

            if (null == route)
            {   // stop selected
                IEnumerable<DataGridContext> childContexts = _dataGridControl.GetChildContexts();
                foreach (DataGridContext dataGridContext in childContexts)
                {
                    if (dataGridContext.CurrentItem != null)
                    {
                        if (!dataGridContext.CurrentColumn.Equals(dataGridContext.Columns["IsLocked"]))
                        {
                            e.Cancel = true;
                            dataGridContext.CancelEdit();
                        }

                        break;
                    }
                }
            }

            if (!e.Cancel)
            {
                if (null != route)
                {
                    // if route already has stops we should watch it's properties changes because some of them can have influence to route's validity
                    if (route.Stops != null && route.Stops.Count > 0)
                    {
                        // create list of affect routing properties
                        _CreateListOfInitialRouteAffectRoutingProperties((Route)e.Item);
                        ((INotifyPropertyChanged)e.Item).PropertyChanged += new PropertyChangedEventHandler(RouteViewContextHandler_PropertyChanged);
                    }
                }

                e.Handled = true;
                if (_mapControl.IsInEditedMode)
                {
                    e.Cancel = true;
                }
                else
                {
                    _mapControl.StartEdit(e.Item);

                    if (null != route)
                        _statusBuilder.FillEditingStatus(e.Item.ToString(), (string)App.Current.FindResource("Route"), _parentPage);
                }
            }
            // else do nothing
        }

        /// <summary>
        /// React on cancel edit item.
        /// </summary>
        public void CancelEditItem(DataGridItemEventArgs e)
        {
            _mapControl.EditEnded();
            _statusBuilder.FillSelectionStatus(e.CollectionView.Count, (string)App.Current.FindResource("Route"), _dataGridControl.SelectedItems.Count, _parentPage);

            if (e.Item is Route)
                _ClearListsOfRouteProperties();
        }

        /// <summary>
        /// React on commit item.
        /// </summary>
        public void CommitItem(DataGridItemCancelEventArgs e)
        {
            _mapControl.EditEnded();
            e.Handled = true;
        }

        /// <summary>
        /// React on EditCommited.
        /// </summary>
        public void CommitedEdit(DataGridItemEventArgs e)
        {
            if (e.Item is Route)
            {
                _statusBuilder.FillSelectionStatusWithoutCollectionSize(e.CollectionView.Count, (string)App.Current.FindResource("Route"), _dataGridControl.SelectedItems.Count, _parentPage);

                if (_changedRouteProperties.Count == 0)
                    return;

                Route editedRoute = e.Item as Route;
                PropertyInfo[] properties = (typeof(Route)).GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    // if property was changed and it's AffectRoutingProperty
                    if (_changedRouteProperties.Contains(property.Name) && _initialRouteProperties.ContainsKey(property.Name))
                    {
                        // if at least one affects routing property value was changed - show warning message
                        if (_WasPropetyValueChanged(property.Name, editedRoute))
                        {
                            App.Current.Messenger.AddWarning(string.Format((string)App.Current.FindResource("RouteAffectsRoutingPropertyChangedMessage"), editedRoute.Name));
                            break;
                        }
                    }
                }

                _ClearListsOfRouteProperties();
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// End edit handler. Called from map control.
        /// </summary>
        /// <param name="commit">Is item need to be committed.</param>
        private void _EditEnded(bool commit)
        {
            if (_dataGridControl.IsBeingEdited)
            {
                if (commit)
                {
                    try
                    {
                        _dataGridControl.EndEdit();
                    }
                    catch
                    {
                        _dataGridControl.CancelEdit();
                    }
                }
                else
                {
                    _dataGridControl.CancelEdit();
                }
            }

            Debug.Assert(!_mapControl.IsInEditedMode);
            if (_mapControl.IsInEditedMode)
                _mapControl.EditEnded();
        }

        /// <summary>
        /// Method clears temp collections of route properties
        /// </summary>
        private void _ClearListsOfRouteProperties()
        {
            _changedRouteProperties.Clear();
            _initialRouteProperties.Clear();
        }

        /// <summary>
        /// Create list of all affect routing properties of initial route.
        /// </summary>
        /// <param name="route"></param>
        private void _CreateListOfInitialRouteAffectRoutingProperties(Route route)
        {
            _initialRouteProperties.Clear();

            // create copy of route before it will be changed
            PropertyInfo[] propertes = typeof(Route).GetProperties();
            foreach (PropertyInfo info in propertes)
            {
                if (Attribute.IsDefined(info, typeof(AffectsRoutingPropertyAttribute)))
                {
                    object value = info.GetValue(route, null);
                    if (value is Breaks)
                        // NOTE: workaround to cancel change Breaks value in _initialRouteProperties
                        //       when it's changes in object
                        _initialRouteProperties.Add(info.Name, route.Breaks.Clone());

                    // create zones collection
                    else if (value is IDataObjectCollection<Zone>)
                    {
                        Collection<Zone> valuesCollection = new Collection<Zone>();
                        foreach (Zone zone in (IDataObjectCollection<Zone>)value)
                            valuesCollection.Add((Zone)zone);

                        _initialRouteProperties.Add(info.Name, valuesCollection);
                    }

                    // create locations collection 
                    else if (value is IDataObjectCollection<Location>)
                    {
                        Collection<Location> valuesCollection = new Collection<Location>();
                        foreach (Location location in (IDataObjectCollection<Location>)value)
                            valuesCollection.Add(location);

                        _initialRouteProperties.Add(info.Name, valuesCollection);
                    }
                    else
                        _initialRouteProperties.Add(info.Name, value);
                }
            }
        }

        /// <summary>
        /// Method check is property value was really changed.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="editedRoute"></param>
        /// <returns></returns>
        private bool _WasPropetyValueChanged(string propertyName, Route editedRoute)
        {
            PropertyInfo newInfo = editedRoute.GetType().GetProperty(propertyName);

            object newValue = newInfo.GetValue(editedRoute, null);

            // check Zones value 
            if (newValue is IDataObjectCollection<Zone>)
            {
                IDataObjectCollection<Zone> newCollection = (IDataObjectCollection<Zone>)newValue;
                ICollection<Zone> oldCollection = (ICollection<Zone>)_initialRouteProperties[propertyName];

                if (newCollection.Count != oldCollection.Count)
                    return true;
                foreach (Zone newLocation in newCollection)
                    if (!oldCollection.Contains(newLocation))
                        return true;
                return false;
            }

            // check Locations value
            if (newValue is IDataObjectCollection<Location>)
            {
                IDataObjectCollection<Location> newCollection = (IDataObjectCollection<Location>)newValue;
                ICollection<Location> oldCollection = (ICollection<Location>)_initialRouteProperties[propertyName];

                if (newCollection.Count != oldCollection.Count)
                    return true;
                foreach (Location newLocation in newCollection)
                    if (!oldCollection.Contains(newLocation))
                        return true;
                return false;
            }

            object newObjValue = newInfo.GetValue(editedRoute, null);

            if (newObjValue is Boolean)
                return (!_initialRouteProperties[propertyName].Equals(newObjValue));

            return (_initialRouteProperties[propertyName] != newObjValue);
        }

        /// <summary>
        /// Creates route and initializes route's fields by default values if possible.
        /// </summary>
        /// <returns>Created route.</returns>
        private Route _CreateRouteWithDefaultValues()
        {
            Route route = App.Current.Project.CreateRoute();

            // set default driver
            Collection<ESRI.ArcLogistics.Data.DataObject> usedDrivers = RoutesHelper.CreateUsedDriversCollection();
            Collection<Driver> freeDrivers = new Collection<Driver>();
            foreach (Driver driver in App.Current.Project.Drivers)
            {
                if (!usedDrivers.Contains(driver))
                    freeDrivers.Add(driver);
            }

            if (freeDrivers.Count == 1)
                route.Driver = freeDrivers[0];

            // set default vehicle
            Collection<ESRI.ArcLogistics.Data.DataObject> usedVehicles = RoutesHelper.CreateUsedVehiclesCollection();
            Collection<Vehicle> freeVehicles = new Collection<Vehicle>();
            foreach (Vehicle vehicle in App.Current.Project.Vehicles)
            {
                if (!usedVehicles.Contains(vehicle))
                    freeVehicles.Add(vehicle);
            }

            if (freeVehicles.Count == 1)
                route.Vehicle = freeVehicles[0];

            // set default locations
            if (App.Current.Project.Locations.Count == 1)
            {
                route.StartLocation = App.Current.Project.Locations[0];
                route.EndLocation = App.Current.Project.Locations[0];
            }

            // set default color
            route.Color = RouteColorManager.Instance.NextRouteColor(_routes);

            return route;
        }

        /// <summary>
        /// Method returns route if it's name contain in App.Project.DefaultRoutes
        /// otherwise returns null.
        /// </summary>
        /// <param name="routeName"></param>
        /// <returns></returns>
        private Route _FindRoute(string routeName)
        {
            Route foundRoute = null;
            foreach (Route route in App.Current.Project.DefaultRoutes)
            {
                if (route.Name.Equals(routeName))
                {
                    foundRoute = route;
                    break; // NOTE: result founded. Exit.
                }
            }

            if (foundRoute != null)
            {
                foreach (Route route in _routes)
                {
                    if (route.Name == foundRoute.Name)
                    {
                        foundRoute = null;
                        break; // NOTE: result founded. Exit.
                    }
                }
            }

            return foundRoute;
        }

        #endregion

        #region Event handlers
        /// <summary>
        /// Xceed grid initializing insertion row handler.
        /// </summary>
        private void xceedGrid_InitializingInsertionRow(object sender, InitializingInsertionRowEventArgs e)
        {
            Cell cell = ((InsertionRow)e.InsertionRow).Cells["Name"];
            cell.EditEnded -= cell_EditEnded;
            cell.EditEnded += new RoutedEventHandler(cell_EditEnded);
        }

        /// <summary>
        /// Route view contex property changed handler.
        /// </summary>
        private void RouteViewContextHandler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // when any route property changed - add it's name into collection
            if (!_changedRouteProperties.Contains(e.PropertyName))
                _changedRouteProperties.Add(e.PropertyName);
        }

        /// <summary>
        /// Cell edit ended handler.
        /// </summary>
        private void cell_EditEnded(object sender, RoutedEventArgs e)
        {
            if (_addedRoute == null)
                return;

            if (((Cell)sender).Content != null)
            {
                string newRouteName = ((Cell)sender).Content.ToString();
                Route defaultRoute = _FindRoute(newRouteName);
                if (defaultRoute != null)
                    defaultRoute.CopyTo(_addedRoute);
            }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Handler parent page.
        /// </summary>
        private OptimizeAndEditPage _parentPage;

        /// <summary>
        /// Map control.
        /// </summary>
        private MapControl _mapControl;

        /// <summary>
        /// Last added route.
        /// </summary>
        private Route _addedRoute;

        /// <summary>
        /// Initial route properties.
        /// </summary>
        private Dictionary<string, object> _initialRouteProperties = new Dictionary<string, object>();

        /// <summary>
        /// Changed route properties.
        /// </summary>
        private Collection<string> _changedRouteProperties = new Collection<string>();

        /// <summary>
        /// Context routes collection.
        /// </summary>
        private ICollection<Route> _routes;

        /// <summary>
        /// Status builder.
        /// </summary>
        private StatusBuilder _statusBuilder = new StatusBuilder();

        /// <summary>
        /// Data grid control.
        /// </summary>
        private DataGridControl _dataGridControl;

        #endregion
    }
}

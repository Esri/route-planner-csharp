using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Class presented edit handler halpers for Orders view.
    /// </summary>
    internal class OrdersViewContextHandler : IListViewContextHandler
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parentPage">Parent page.</param>
        public OrdersViewContextHandler(OptimizeAndEditPage parentPage)
        {
            Debug.Assert(null != parentPage);

            _parentPage = parentPage;
            _currentSchedule = parentPage.CurrentSchedule;
            _geocodablePage = parentPage.GeocodablePage;
        }

        #endregion

        #region IContextHandler Members
        /// <summary>
        /// React on EditCommited.
        /// </summary>
        public void CommitedEdit(DataGridItemEventArgs e)
        {
        }

        /// <summary>
        /// Creates new object.
        /// </summary>
        public void CreateNewItem(DataGridCreatingNewItemEventArgs e)
        {
            Project project = App.Current.Project;

            if (!_geocodablePage.IsGeocodingInProcess)
            {
                e.NewItem = new Order(project.CapacitiesInfo, project.OrderCustomPropertiesInfo);
                _geocodablePage.OnCreatingNewItem(e);
            }
            else
            {
                e.Cancel = true;
            }
            e.Handled = true;

            _statusBuilder.FillCreatingStatus((string)App.Current.FindResource("Order"), _parentPage);
        }

        /// <summary>
        /// Cancel creating new item.
        /// </summary>
        public void CancellingNewItem(DataGridItemHandledEventArgs e)
        {
            _geocodablePage.OnNewItemCancelling();

            string status = string.Format((string)App.Current.FindResource(OptimizeAndEditPage.NoSelectionStatusFormat),
                                          _currentSchedule.Routes.Count, _currentSchedule.UnassignedOrders.Count);
            App.Current.MainWindow.StatusBar.SetStatus(_parentPage, status);
        }

        /// <summary>
        /// Adds created object to source collection.
        /// </summary>
        public void CommitNewItem(DataGridCommittingNewItemEventArgs e)
        {
            if (!e.Cancel && _geocodablePage.OnCommittingNewItem(e))
            {
                ICollection<Order> source = (ICollection<Order>)e.CollectionView.SourceCollection;

                Order order = e.Item as Order;
                order.PlannedDate = _currentSchedule.PlannedDate;

                Project project = App.Current.Project;
                project.Orders.Add(order);
                project.Save();

                e.Index = source.Count;
                e.NewCount = source.Count + 1;
            }
        }

        /// <summary>
        /// React on new item committed.
        /// </summary>
        public void CommittedNewItem(DataGridItemEventArgs e)
        {
            _geocodablePage.OnNewItemCommitted();
        }

        /// <summary>
        /// React on begin edit item.
        /// </summary>
        public void BeginEditItem(DataGridItemCancelEventArgs e)
        {
            if (!e.Cancel)
            {
                _geocodablePage.OnBeginningEdit(e);
                _statusBuilder.FillEditingStatus(e.Item.ToString(),
                                                 (string)App.Current.FindResource("Order"), _parentPage);
            }
        }

        /// <summary>
        /// React on cancel edit item.
        /// </summary>
        public void CancelEditItem(DataGridItemEventArgs e)
        {
            _geocodablePage.OnEditCanceled(e);
            _statusBuilder.FillSelectionStatus(e.CollectionView.Count,
                (string)App.Current.FindResource("Order"), _parentPage.OrdersView.OrdersGrid.SelectedItems.Count, _parentPage);
        }

        /// <summary>
        /// React on commit item.
        /// </summary>
        public void CommitItem(DataGridItemCancelEventArgs e)
        {
            if (!e.Cancel)
            {
                _geocodablePage.OnCommittingEdit(e, true);

                Project project = App.Current.Project;
                if (project != null)
                    project.Save();

                DataGridControlEx dataGrid = _parentPage.OrdersView.OrdersGrid;
                if (dataGrid.SelectedItems == null)
                {
                    string status = string.Format((string)App.Current.FindResource(OptimizeAndEditPage.NoSelectionStatusFormat),
                                                  _currentSchedule.Routes.Count, _currentSchedule.UnassignedOrders.Count);
                    App.Current.MainWindow.StatusBar.SetStatus(_parentPage, status);
                    return;
                }

                _statusBuilder.FillSelectionStatus(e.CollectionView.Count,
                    (string)App.Current.FindResource("Order"), dataGrid.SelectedItems.Count, _parentPage);
            }
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Current schedule.
        /// </summary>
        private Schedule _currentSchedule;
        /// <summary>
        /// Geocodablr page.
        /// </summary>
        private GeocodablePage _geocodablePage;
        /// <summary>
        /// Status builder.
        /// </summary>
        private StatusBuilder _statusBuilder = new StatusBuilder();
        /// <summary>
        /// Parent page.
        /// </summary>
        private OptimizeAndEditPage _parentPage;

        #endregion
    }
}

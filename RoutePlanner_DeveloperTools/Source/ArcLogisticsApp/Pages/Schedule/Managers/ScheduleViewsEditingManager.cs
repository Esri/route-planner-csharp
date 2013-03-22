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
using System.Diagnostics;
using ESRI.ArcLogistics.App.Controls;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Class for views editing managment in OptimizeAndEdit page. 
    /// Catches editing events from views, handles they and raises same editing events which will be caught in OptimizeAndEditPage.
    /// </summary>
    internal class ScheduleViewsEditingManager : ISupportDataObjectEditing
    {
        #region Constructors

        /// <summary>
        /// Creates instance of ScheduleViewsEditingManager, initializes local field _multipleListViewManager
        /// and adds handler to ListViewsCollectionChanged event. 
        /// </summary>
        /// <param name="optimizeAndEditPage">OptimizeAndEditPage.</param>
        public ScheduleViewsEditingManager(OptimizeAndEditPage optimizeAndEditPage)
        {
            _ordersView = optimizeAndEditPage.OrdersView;
            _AddEditingEventHandlers(_ordersView);

            _routesView = optimizeAndEditPage.RoutesView;
            _AddEditingEventHandlers(_routesView);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Object is creating or editing at the moment.
        /// </summary>
        public object EditedObject
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Cancels editing.
        /// </summary>
        public void CancelObjectEditing()
        {
            _ordersView.CancelNewObject(false);
            _ordersView.SaveEditedItem();

            _routesView.CancelNewObject();
            _routesView.SaveEditedItem();

            GeocodablePage geocodablePage = _ordersView.ParentPage.GeocodablePage;
            if (geocodablePage.IsInEditedMode)
            {
                geocodablePage.EditEnded(true);
            }
        }

        #endregion

        #region ISupportDataObjectEditing Members

        /// <summary>
        /// Returns bool value to define whether editing in progress. Not supported there. 
        /// </summary>
        public bool IsEditingInProgress
        {
            get;
            private set;
        }

        /// <summary>
        /// Occurs when editing is starting. 
        /// </summary>
        public event DataObjectCanceledEventHandler BeginningEdit;

        /// <summary>
        /// Occurs when editing was started.
        /// </summary>
        public event DataObjectEventHandler EditBegun;

        /// <summary>
        /// Occurs when editing is commiting.
        /// </summary>
        public event DataObjectCanceledEventHandler CommittingEdit;

        /// <summary>
        /// Occurs when editing was commited.
        /// </summary>
        public event DataObjectEventHandler EditCommitted;

        /// <summary>
        /// Occurs when editing was cancelled.
        /// </summary>
        public event DataObjectEventHandler EditCanceled;

        /// <summary>
        /// Occurs when new object is creating.
        /// </summary>
        public event DataObjectCanceledEventHandler CreatingNewObject;

        /// <summary>
        /// Occurs when new object was created.
        /// </summary>
        public event DataObjectEventHandler NewObjectCreated;

        /// <summary>
        /// Occurs when new object is commiting.
        /// </summary>
        public event DataObjectCanceledEventHandler CommittingNewObject;

        /// <summary>
        /// Occurs when new object was commited.
        /// </summary>
        public event DataObjectEventHandler NewObjectCommitted;

        /// <summary>
        /// Occurs when new object was cancelled.
        /// </summary>
        public event DataObjectEventHandler NewObjectCanceled;

        #endregion

        #region Private Methods

        /// <summary>
        /// Method cancels editing in all List views except current. 
        /// </summary>
        /// <param name="view">View where editing should not be cancelled.</param>
        private void _CancelEditingInOtherViews(object currentView)
        {
            Debug.Assert(null != currentView);

            if (currentView is OrdersView)
            {
                _routesView.CancelNewObject();
                _routesView.SaveEditedItem();
            }
            else if (currentView is RoutesView)
            {
                _ordersView.CancelNewObject(false);
                _ordersView.SaveEditedItem();
            }
            else
            {
                Debug.Assert(false);
            }
        }

        /// <summary>
        /// Method adds handlers for all editing events.
        /// </summary>
        /// <param name="view">Object implements ISupportDataObjectEditing - List view or Map view.</param>
        private void _AddEditingEventHandlers(ISupportDataObjectEditing view)
        {
            // Add handlers to create events.
            view.CreatingNewObject += new DataObjectCanceledEventHandler(_ViewCreatingNewObject);
            view.NewObjectCreated += new DataObjectEventHandler(_ViewNewObjectCreated);

            // Add handlers to commit events.
            view.CommittingNewObject += new DataObjectCanceledEventHandler(_ViewCommittingNewObject);
            view.NewObjectCommitted += new DataObjectEventHandler(_ViewNewObjectCommitted);

            // Add handlers to cancel new events.
            view.NewObjectCanceled += new DataObjectEventHandler(_ViewNewObjectCanceled);

            // Add handlers to begin edit events.
            view.BeginningEdit += new DataObjectCanceledEventHandler(_ViewBeginningEdit);
            view.EditBegun += new DataObjectEventHandler(_ViewEditBegun);

            // Add handlers to cancel edit events.
            view.EditCanceled += new DataObjectEventHandler(_ViewEditCanceled);

            // Add handlers to commit edit events.
            view.CommittingEdit += new DataObjectCanceledEventHandler(_ViewCommittingEdit);
            view.EditCommitted += new DataObjectEventHandler(_ViewEditCommitted);
        }

        /// <summary>
        /// Method removes handlers for all editing events.
        /// </summary>
        /// <param name="view">Object implements ISupportDataObjectEditing - List view or Map view.</param>
        private void _RemoveEditingEventHandlers(ISupportDataObjectEditing view)
        {
            // Remove handlers to create events.
            view.CreatingNewObject -= _ViewCreatingNewObject;
            view.NewObjectCreated -= _ViewNewObjectCreated;

            // Remove handlers to commit events.
            view.CommittingNewObject -= _ViewCommittingNewObject;
            view.NewObjectCommitted -= _ViewNewObjectCommitted;

            // Remove handlers to cancel new events.
            view.NewObjectCanceled -= _ViewNewObjectCanceled;

            // Remove handlers to begin edit events.
            view.BeginningEdit -= _ViewBeginningEdit;
            view.EditBegun -= _ViewEditBegun;

            // Remove handlers to cancel edit events.
            view.EditCanceled -= _ViewEditCanceled;

            // Remove handlers to commit edit events.
            view.CommittingEdit -= _ViewCommittingEdit;
            view.EditCommitted -= _ViewEditCommitted;
        }

        #endregion

        #region Private Editing Event Handlers

        /// <summary>
        /// Handler raises event about new object was cancelled.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ViewNewObjectCanceled(object sender, DataObjectEventArgs e)
        {
            // Raise event to OptimizeAndEdit page about new object was cancelled. 
            if (NewObjectCanceled != null)
                NewObjectCanceled(this, e);
        }

        /// <summary>
        /// Handler raises event about edit was cancelled.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ViewEditCanceled(object sender, DataObjectEventArgs e)
        {
            // Raise event to OptimizeAndEdit page about edit was cancelled.
            if (EditCanceled != null)
                EditCanceled(this, e);
        }

        /// <summary>
        /// Handler raises event about new object is commiting.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ViewCommittingNewObject(object sender, DataObjectCanceledEventArgs e)
        {
            // Raise event to OptimizeAndEdit page about new object is commiting.
            if (CommittingNewObject != null)
                CommittingNewObject(this, e);
        }

        /// <summary>
        /// Handler raises event about new object was commited.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ViewNewObjectCommitted(object sender, DataObjectEventArgs e)
        {
            // Raise event to OptimizeAndEdit page about new object was commited.
            if (NewObjectCommitted != null)
                NewObjectCommitted(this, e);
        }

        /// <summary>
        /// Handler cancels editing in all views, starts creating new in current view and raises event about new object is creating.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ViewCreatingNewObject(object sender, DataObjectCanceledEventArgs e)
        {
            _CancelEditingInOtherViews(sender);

            EditedObject = e.Object;

            // Raise event to OptimizeAndEdit page about creating new is starting.
            if (CreatingNewObject != null)
                CreatingNewObject(this, e);
        }

        /// <summary>
        /// Handler raises event about new object was created.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ViewNewObjectCreated(object sender, DataObjectEventArgs e)
        {
            // Raise event to OptimizeAndEdit page about new object was created.
            if (NewObjectCreated != null)
                NewObjectCreated(this, e);
        }

        /// <summary>
        /// Handler cancels editing in all views, starts editing in current and raises event about editing is beginning.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ViewBeginningEdit(object sender, DataObjectCanceledEventArgs e)
        {
            _CancelEditingInOtherViews(sender);

            EditedObject = e.Object;

            // Raise event to OptimizeAndEdit page about editing is starting.
            if (BeginningEdit != null)
                BeginningEdit(this, e);
        }

        /// <summary>
        /// Handler raises event about edit begun.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ViewEditBegun(object sender, DataObjectEventArgs e)
        {
            // Raise event to OptimizeAndEdit page about editing was started.
            if (EditBegun != null)
                EditBegun(this, e);
        }

        /// <summary>
        /// Handler raises event about edit is commiting.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args e.</param>
        private void _ViewCommittingEdit(object sender, DataObjectCanceledEventArgs e)
        {
            // Raise event to OptimizeAndEdit page about commiting is starting.
            if (CommittingEdit != null)
                CommittingEdit(this, e);
        }

        /// <summary>
        /// Hndler raises event about editing was commited.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ViewEditCommitted(object sender, DataObjectEventArgs e)
        {
            // Raise event to OptimizeAndEdit page about editing was commited.
            if (EditCommitted != null)
                EditCommitted(this, e);
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Orders list view.
        /// </summary>
        private OrdersView _ordersView;

        /// <summary>
        /// Routes list view.
        /// </summary>
        private RoutesView _routesView;

        #endregion
    }
}

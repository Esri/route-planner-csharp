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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.DataGrid;
using ESRI.ArcLogistics.App.GridHelpers;
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.App.Validators;
using Xceed.Wpf.DataGrid.Settings;
using System.Collections.Specialized;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for SpecialtiesSubPage.xaml
    /// </summary>
    internal abstract partial class SpecialtiesPanelBase : Grid, ISupportSelection, 
        ISupportDataObjectEditing, ICancelDataObjectEditing, ISupportSelectionChanged
    {
        #region Constructors

        public SpecialtiesPanelBase()
        {
            InitializeComponent();
            _InitEventHandlers();
        }

        #endregion

        #region ICancelDataObjectEditing Members

        public void CancelObjectEditing()
        {
            XceedGrid.CancelEdit();
        }

        #endregion

        #region ISupportSelection Members

        public System.Collections.IList SelectedItems
        {
            get { return XceedGrid.SelectedItems; }
        }

        /// <summary>
        /// Not implemented there
        /// </summary>
        /// <returns></returns>
        public bool SaveEditedItem()
        {
            return false;
        }

        public void Select(System.Collections.IEnumerable items)
        {
            _Select(items);
        }

        #endregion

        #region ISupportDataObjectEditing Members

        public bool IsEditingInProgress
        {
            get;
            protected set;
        }

        public event DataObjectCanceledEventHandler BeginningEdit;

        public event DataObjectEventHandler EditBegun;

        public event DataObjectCanceledEventHandler CommittingEdit;

        public event DataObjectEventHandler EditCommitted;

        public event DataObjectEventHandler EditCanceled;

        public event DataObjectCanceledEventHandler CreatingNewObject;

        public event DataObjectEventHandler NewObjectCreated;

        public event DataObjectCanceledEventHandler CommittingNewObject;

        public event DataObjectEventHandler NewObjectCommitted;

        public event DataObjectEventHandler NewObjectCanceled;

        #endregion

        #region ISupportSelectionChanged Members

        public event EventHandler SelectionChanged;

        #endregion

        #region Public Methods

        /// <summary>
        /// Method cancels new object and clear insertion row
        /// TODO : ? move to ICancelDataObjectEditing
        /// </summary>
        public void CancelNewObject()
        {
            if (_insertionRow.IsDirty)
                _insertionRow.CancelEdit();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Inits data grid layout
        /// </summary>
        protected void _InitDataGridLayout()
        {
            DataGridCollectionViewSource collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);

            Debug.Assert(!string.IsNullOrEmpty(GridStructureName));
            GridStructureInitializer gridStructureInitializer = new GridStructureInitializer(GridStructureName);

            gridStructureInitializer.BuildGridStructure(collectionSource, XceedGrid);

            // load grid layout
            Debug.Assert(!string.IsNullOrEmpty(SettingsRepositoryName));
            GridLayoutLoader vehicleLayoutLoader = new GridLayoutLoader(SettingsRepositoryName, collectionSource.ItemProperties);
            vehicleLayoutLoader.LoadLayout(XceedGrid);
            _isDataGridLayoutLoaded = true;
        }

        /// <summary>
        /// Method init event handlers
        /// </summary>
        protected void _InitEventHandlers()
        {
            App.Current.ProjectLoaded += new EventHandler(AppCurrent_ProjectLoaded);
            App.Current.ProjectClosed += new EventHandler(Current_ProjectClosed);
            this.Loaded += new RoutedEventHandler(SpecialtiesPanelBase_Loaded);
            this.Unloaded += new RoutedEventHandler(SpecialtiesPanelBase_Unloaded);
            App.Current.Exit += new ExitEventHandler(Current_Exit);
        }

        #endregion

        #region Abstract Protected Methods

        /// <summary>
        /// Inits grid items collection 
        /// </summary>
        protected abstract void _InitDataGridCollection();

        /// <summary>
        /// Clears collection in grid
        /// </summary>
        protected abstract void _ClearDataGridCollection();

        /// <summary>
        /// Creates new item 
        /// </summary>
        protected abstract void _CreateNewItem(DataGridCreatingNewItemEventArgs e);

        /// <summary>
        /// Commits new item
        /// </summary>
        /// <param name="e"></param>
        protected abstract void _CommitNewItem(DataGridCommittingNewItemEventArgs e);

        /// <summary>
        /// Sets selection status (should be overrided in each child class)
        /// </summary>
        protected abstract void _SetSelectionStatus();
        
        /// <summary>
        /// Change item's name.
        /// </summary>
        /// <param name="e">DataGridItemEventArgs.</param>
        protected abstract void _ChangeName(Xceed.Wpf.DataGrid.DataGridItemEventArgs e,
            InsertionRow insertionRow);

        /// <summary>
        /// Sets editing status (should be overrided in each child class)
        /// </summary>
        protected abstract void _SetEditingStatus(string name);

        /// <summary>
        /// Sets creating status (should be overrided in each child class)
        /// </summary>
        protected abstract void _SetCreatingStatus();

        /// <summary>
        /// Saves Layout
        /// </summary>
        protected abstract void SaveLayout();

        /// <summary>
        /// Selects items
        /// </summary>
        /// <param name="items"></param>
        protected abstract void _Select(System.Collections.IEnumerable items);

        #endregion

        #region Protected fields

        /// <summary>
        /// Grid structure name
        /// </summary>
        protected abstract string GridStructureName
        {
            get;
            set;
        }

        /// <summary>
        /// Settings repository name
        /// </summary>
        protected abstract string SettingsRepositoryName
        {
            get;
            set;
        }

        /// <summary>
        /// Command category name
        /// </summary>
        protected abstract string CommandCategoryName
        {
            get;
            set;
        }

        /// <summary>
        /// Panel's header
        /// </summary>
        protected string PanelHeader
        {
            get
            {
                return headerLabel.Content.ToString();
            }
            set
            {
                headerLabel.Content = value;
            }
        }

        #endregion

        #region Event Handlers

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            SaveLayout();
        }

        private void SpecialtiesPanelBase_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveLayout();
        }

        private void SpecialtiesPanelBase_Loaded(object sender, RoutedEventArgs e)
        {
         
            XceedGrid.SelectionChanged -= XceedGrid_SelectionChanged;
            ((INotifyCollectionChanged)XceedGrid.Items).CollectionChanged -= SpecialtiesPanelBase_CollectionChanged;

            // create commands
            Debug.Assert(!string.IsNullOrEmpty(CommandCategoryName));
            commandButtonsGroup.Initialize(CommandCategoryName, XceedGrid);

            if (!_isDataGridLayoutLoaded)
                _InitDataGridLayout();

            if (!_isCollectionLoaded)
            {
                _InitDataGridCollection();
                _isCollectionLoaded = true;
            }

            XceedGrid.SelectionChanged += new DataGridSelectionChangedEventHandler(XceedGrid_SelectionChanged);

            if (XceedGrid.Items != null)
                ((INotifyCollectionChanged)XceedGrid.Items).CollectionChanged += new NotifyCollectionChangedEventHandler(SpecialtiesPanelBase_CollectionChanged);
        }

        private void SpecialtiesPanelBase_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _SetSelectionStatus();
        }

        private void AppCurrent_ProjectLoaded(object sender, EventArgs e)
        {
            _InitDataGridLayout();
            _InitDataGridCollection();
        }

        private void Current_ProjectClosed(object sender, EventArgs e)
        {
            _ClearDataGridCollection();
        }

        private void XceedGrid_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);

            _SetSelectionStatus();
        }

        private void InsertionRow_Loaded(object sender, RoutedEventArgs e)
        {
            _insertionRow = (InsertionRow)sender;
        }

        #endregion

        #region Data Object Editing Event Handlers

        private void DataGridCollectionViewSource_BeginningEdit(object sender, DataGridItemCancelEventArgs e)
        {
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
             if (BeginningEdit != null)
                BeginningEdit(this, args);
             
            e.Handled = true;

            if (!args.Cancel)
            {
                IsEditingInProgress = true;
                _SetEditingStatus(e.Item.ToString());
            }
            else
                e.Cancel = true;
        }

        private void DataGridCollectionViewSource_EditBegun(object sender, DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (EditBegun != null)
                EditBegun(this, args);
        }

        private void DataGridCollectionViewSource_CommittingEdit(object sender, DataGridItemCancelEventArgs e)
        {
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (CommittingEdit != null)
                CommittingEdit(this, args);
            
            e.Handled = true;

            if (!args.Cancel)
            {
                App.Current.Project.Save();
                IsEditingInProgress = false;
                _SetSelectionStatus();
            }
            else
                e.Cancel = true;
        }

        private void DataGridCollectionViewSource_EditCommitted(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (EditCommitted != null)
                EditCommitted(this, args);
        }

        private void DataGridCollectionViewSource_EditCanceled(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (EditCanceled != null)
                EditCanceled(this, args);
            _SetSelectionStatus();
        }

        private void DataGridCollectionViewSource_CreatingNewItem(object sender, Xceed.Wpf.DataGrid.DataGridCreatingNewItemEventArgs e)
        {
            e.Handled = true;
            _CreateNewItem(e);
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.NewItem);
            if (CreatingNewObject != null)
                CreatingNewObject(this, args); ;

            if (!args.Cancel)
            {
                _isNewItemCreated = true; // set flag to true because new object was created
                IsEditingInProgress = false;
                _SetCreatingStatus();
            }
            else
            {
                e.Cancel = true;
                _isNewItemCreated = false; // set flag to false because new object wasn't created
            }
        }

        private delegate void ParamsDelegate(Xceed.Wpf.DataGrid.DataGridItemEventArgs item,
            InsertionRow insertionRow);

        private void DataGridCollectionViewSource_NewItemCreated(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            // Invoking changing of the item's name. Those method must be invoked, otherwise 
            // grid will not understand that item in insertion ro was changed and grid wouldnt allow
            // to commit this item.
            Dispatcher.BeginInvoke(new ParamsDelegate(_ChangeName),
                System.Windows.Threading.DispatcherPriority.Render, e, _insertionRow);

            IsEditingInProgress = true;
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (NewObjectCreated != null)
                NewObjectCreated(this, args);
        }

        private void DataGridCollectionViewSource_CommittingNewItem(object sender, Xceed.Wpf.DataGrid.DataGridCommittingNewItemEventArgs e)
        {
            e.Handled = true;
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (CommittingNewObject != null)
                CommittingNewObject(this, args);

            if (!args.Cancel)
            {
                _CommitNewItem(e);
                IsEditingInProgress = false;
                _SetSelectionStatus();
            }
            else
                e.Cancel = true;
        }

        private void DataGridCollectionViewSource_NewItemCommitted(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            IsEditingInProgress = false;

            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (NewObjectCommitted != null)
                NewObjectCommitted(this, args);
        }

        private void DataGridCollectionViewSource_NewItemCanceled(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            IsEditingInProgress = false;
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (NewObjectCanceled != null)
                NewObjectCanceled(this, args);
            _SetSelectionStatus();
        }

        private void DataGridCollectionViewSource_CancelingNewItem(object sender, DataGridItemHandledEventArgs e)
        {
            // set property to true if new item was created or to false if new item wasn't created
            // otherwise an InvalidOperationException will be thrown (see http://doc.xceedsoft.com/products/XceedWpfDataGrid/Inserting_Data.html)
            e.Handled = _isNewItemCreated;
            IsEditingInProgress = false;
            _SetSelectionStatus();
        }

        private void DataGridCollectionViewSource_CancelingEdit(object sender, DataGridItemHandledEventArgs e)
        {
            e.Handled = true;
            IsEditingInProgress = false;
            _SetSelectionStatus();
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Collection source key
        /// </summary>
        protected const string COLLECTION_SOURCE_KEY = "specialtiesSource";
        protected const string NAME_PROPERTY_STRING = "Name";

        // Flag shows is new item was created or not to set correct value of Handled property in DataGridCollectionViewSource_CancelingNewItem 
        private bool _isNewItemCreated = false;
        private bool _isDataGridLayoutLoaded = false;
        private bool _isCollectionLoaded = false;

        private InsertionRow _insertionRow;

        #endregion
    }
}

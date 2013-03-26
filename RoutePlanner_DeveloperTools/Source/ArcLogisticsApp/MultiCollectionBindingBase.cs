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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.App.Controls;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Event args for changing in multicollection binding.
    /// </summary>
    internal class NotifyMultiCollectionChangedEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="e">Notify collection changed event args from changed collection.</param>
        /// <param name="initiator">Initiator collection.</param>
        public NotifyMultiCollectionChangedEventArgs(NotifyCollectionChangedEventArgs e, IList initiator)
        {
            EventArgs = e;
            Initiator = initiator;
        }

        #endregion

        #region Public members

        /// <summary>
        /// Notify collection changed event args from changed collection.
        /// </summary>
        public NotifyCollectionChangedEventArgs EventArgs
        {
            get;
            private set;
        }

        /// <summary>
        /// Initiator collection.
        /// </summary>
        public IList Initiator
        {
            get;
            private set;
        }

        #endregion
    }

    /// <summary>
    /// Delegate for notifying about collection changes.
    /// </summary>
    /// <param name="sender">Multi collection binding.</param>
    /// <param name="e">Event args.</param>
    internal delegate void NotifyMultiCollectionChangedEventHandler(
        Object sender,
        NotifyMultiCollectionChangedEventArgs e
    );

    /// <summary>
    /// Base class for multi collection binding.
    /// </summary>
    internal abstract class MultiCollectionBindingBase
    {
        #region Public events

        /// <summary>
        /// Event for notifying about collection changes.
        /// </summary>
        public event NotifyMultiCollectionChangedEventHandler NotifyMultiCollectionChanged;

        #endregion

        #region Public methods

        /// <summary>
        /// Unregisters collection.
        /// </summary>
        /// <param name="collection">Collection to unregister.</param>
        public virtual void UnregisterCollection(IList collection)
        {
            Debug.Assert(collection != null);
            Debug.Assert(_collections != null);
            Debug.Assert(_grids != null);

            int index = _collections.IndexOf(collection);

            // Remove collection.
            _collections.Remove(collection);

            if (_grids[index] == null)
            {
                // Unsubscribe from events.
                INotifyCollectionChanged notificator = collection as INotifyCollectionChanged;
                Debug.Assert(notificator != null);
                notificator.CollectionChanged -= new NotifyCollectionChangedEventHandler(_NotifiedCollectionChanged);
            }
            else
            {
                _grids[index].SelectionChanged -= new DataGridSelectionChangedEventHandler(_GridSelectionChanged);
            }

            _grids.RemoveAt(index);
        }

        /// <summary>
        /// Unregisters all collection.
        /// </summary>
        public void UnregisterAllCollections()
        {
            Debug.Assert(_collections != null);

            IList colls = _collections.ToArray();

            foreach (IList coll in colls)
            {
                UnregisterCollection(coll);
            }
        }

        #endregion

        #region Protected members

        /// <summary>
        /// Registered collections.
        /// </summary>
        protected List<IList> Collections
        {
            get
            {
                return _collections;
            }
        }

        /// <summary>
        /// Registered datagrids.
        /// </summary>
        protected List<DataGridControlEx> DataGridList
        {
            get
            {
                return _grids;
            }
        }

        /// <summary>
        /// Flag, which indicates changes was made internally.
        /// </summary>
        protected bool IsRecursive
        {
            get
            {
                return _isRecursive;
            }
            set
            {
                _isRecursive = value;
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Filters out invalid additions.
        /// </summary>
        /// <param name="collSender">Collection - changes initiator.</param>
        /// <param name="itemsToAdd">Items to add.</param>
        protected abstract void _FilterInvalidChanges(IList collSender, IList itemsToAdd);

        /// <summary>
        /// Copy items to collection.
        /// </summary>
        /// <param name="collection">Collection to copy.</param>
        /// <param name="newItems">Items to copy</param>
        protected abstract void _CopyItems(IList collection, IList newItems);

        /// <summary>
        /// Add items to collection.
        /// </summary>
        /// <param name="collection">Collection, from to items will added.</param>
        /// <param name="addedItems">Items to add.</param>
        /// <param name="newStartingIndex">New starting index.</param>
        protected abstract void _AddItems(IList collection, IList addedItems, int newStartingIndex);

        /// <summary>
        /// Register collection, which implements INotifyPropertyChanged.
        /// </summary>
        /// <param name="collection">Collection to register.</param>
        protected void _RegisterCollection(IList collection)
        {
            Debug.Assert(collection != null);
            Debug.Assert(_grids != null);

            // Subscribe on events from collection.
            INotifyCollectionChanged notificator = collection as INotifyCollectionChanged;
            if (notificator != null)
            {
                notificator.CollectionChanged += new NotifyCollectionChangedEventHandler(_NotifiedCollectionChanged);
            }
            else
            {
                throw new ArgumentException("Collection doesn't support INotifyCollectionChanged interface and Grid not used.");
            }

            _grids.Add(null);
            _AddCollection(collection);
        }

        /// <summary>
        /// Register grid collection.
        /// </summary>
        /// <param name="grid">Grid to register.</param>
        protected void _RegisterCollection(DataGridControlEx grid)
        {
            Debug.Assert(grid != null);
            Debug.Assert(_grids != null);

            grid.SelectionChanged += new DataGridSelectionChangedEventHandler(_GridSelectionChanged);
            _grids.Add(grid);
            _AddCollection(grid.SelectedItems);
        }

        /// <summary>
        /// Add collection to list and synchronize it.
        /// </summary>
        /// <param name="collection">Collection to add.</param>
        protected void _AddCollection(IList collection)
        {
            Debug.Assert(collection != null);
            Debug.Assert(_collections != null);

            // Add collection.
            _collections.Add(collection);

            // Syncronize new collection with others.
            if (_collections.Count > 1)
            {
                _SuspendCollectionEventsHandling(collection);

                IList sourceCollection = _GetSourceCollection();
                _CopyItems(collection, sourceCollection);

                _ResumeCollectionEventsHandling();
            }
        }

        /// <summary>
        /// React on selection changed in grid.
        /// </summary>
        /// <param name="sender">Grid with changed selection.</param>
        /// <param name="e">Changed event args.</param>
        protected void _GridSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            Debug.Assert(sender != null);
            Debug.Assert(e != null);

            DataGridControlEx grid = (DataGridControlEx)sender;
            if (grid.SelectedItems == _suspendedCollection)
                return; // skip events from suspended collection

            var addedItems = e.SelectionInfos
                .SelectMany(info => info.AddedItems)
                .ToList();
            DataGridControlEx datagrid = (DataGridControlEx)e.Source;
            _FilterInvalidChanges(datagrid.SelectedItems, addedItems);

            var removedItems = e.SelectionInfos
                .SelectMany(info => info.RemovedItems)
                .ToList();
            var args = CommonHelpers.GetSelectionChangedArgs(addedItems, removedItems);
            if (args != null)
            {
                _CollectionChanged(grid.SelectedItems, args);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// React on some registered collection changed.
        /// </summary>
        /// <param name="sender">Changes initiator collection.</param>
        /// <param name="e">Changes args.</param>
        private void _CollectionChanged(IList collSender, NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(collSender != null);
            Debug.Assert(_collections != null);

            if (_isRecursive)
                return;

            foreach (IList item in Collections)
            {
                if (item.Equals(collSender))
                    continue; // skip sender

                _SuspendCollectionEventsHandling(item);

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            _AddItems(item, e.NewItems, e.NewStartingIndex);
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            _RemoveItems(item, e.OldItems);
                            break;
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            item.Clear();
                            break;
                        }
                    case NotifyCollectionChangedAction.Replace:
                        {
                            _RemoveItems(item, e.OldItems);
                            _AddItems(item, e.NewItems, 0);
                            break;
                        }
                    default:
                        Debug.Assert(false);
                        break;
                }

                _ResumeCollectionEventsHandling();
            }

            // Notify about changes.
            _isRecursive = true;
            if (NotifyMultiCollectionChanged != null)
                NotifyMultiCollectionChanged(this,
                    new NotifyMultiCollectionChangedEventArgs(e, collSender));
            _isRecursive = false;
        }

        /// <summary>
        /// Suspend collection changing.
        /// </summary>
        /// <param name="collection">Collection to suspend.</param>
        private void _SuspendCollectionEventsHandling(IList collection)
        {
            Debug.Assert(collection != null);

            _suspendedCollection = collection;
        }

        /// <summary>
        /// Resume collection changing
        /// </summary>
        private void _ResumeCollectionEventsHandling()
        {
            Debug.Assert(_suspendedCollection != null);

            _suspendedCollection = null;
        }

        /// <summary>
        /// Remove items from collection.
        /// </summary>
        /// <param name="collection">Collection, from which items will removed.</param>
        /// <param name="removedItems">Items to remove.</param>
        private void _RemoveItems(IList collection, IList removedItems)
        {
            Debug.Assert(collection != null);
            Debug.Assert(removedItems != null);

            int collectionIndex = _collections.IndexOf(collection);
            DataGridControlEx datagridControl = _grids[collectionIndex];

            foreach (object remItem in removedItems)
            {
                if (datagridControl != null)
                {
                    // Check selected item is in main context.
                    if (collection.Contains(remItem))
                    {
                        collection.Remove(remItem);
                    }
                    else
                    {
                        // Remove from child context.
                        datagridControl.RemoveFromChildContextSelection(remItem);
                    }
                }
                else
                {
                    collection.Remove(remItem);
                }
            }
        }

        /// <summary>
        /// React on changes in collection, which implement INotifyCollectionChanged.
        /// </summary>
        /// <param name="sender">Changed collection.</param>
        /// <param name="e">Changed event args.</param>
        private void _NotifiedCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(sender != null);
            Debug.Assert(e != null);

            if (sender == _suspendedCollection)
                return; // skip events from suspended collection

            _CollectionChanged((IList)sender, e);
        }

        /// <summary>
        /// Get source collection for copy to just registered collection.
        /// </summary>
        /// <returns>Source collection for copy to just registered collection.</returns>
        private IList _GetSourceCollection()
        {
            IList sourceCollection = _collections[0];

            // NOTE: _collections may contains XceedGrid internal selected items collections or
            // observable collections from MapView and TimeView.
            // Source selection have to contain all items.
            // To implement this - find observable collection in case of collections count > 2.
            for (int index = 1; index < _collections.Count - 1; index++)
            {
                ObservableCollection<object> observableCollection = _collections[index] as ObservableCollection<object>;

                if (observableCollection != null)
                {
                    sourceCollection = observableCollection;
                    break;
                }
            }

            // If first collection is grid collection and observable collection not exists - get collection
            // of selected items from all contexts of grid.
            int collectionIndex = _collections.IndexOf(sourceCollection);
            if (_grids[collectionIndex] != null)
            {
                sourceCollection = _grids[collectionIndex].SelectedItemsFromAllContexts;
            }

            return sourceCollection;
        }

        #endregion

        #region Private members

        /// <summary>
        /// Suspended collection.
        /// </summary>
        private IList _suspendedCollection;

        /// <summary>
        /// Registered collection list.
        /// </summary>
        private List<IList> _collections = new List<IList>();

        /// <summary>
        /// Grids list.
        /// </summary>
        private List<DataGridControlEx> _grids = new List<DataGridControlEx>();

        /// <summary>
        /// Flag, which indicates changes was made internally.
        /// </summary>
        private bool _isRecursive;

        #endregion
    }
}
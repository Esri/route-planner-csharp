using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Threading;
using ESRI.ArcLogistics.App.Controls;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App
{
    internal delegate bool MultiCollectionFilter(object item);

    /// <summary>
    /// Class that allows to keep several registered collection synchronized. 
    /// Collections must implement interfaces IList and INotifyCollectionChanged
    /// or collection must be xceed datagrid selected items collection.
    /// Collections support filtration on add.
    /// </summary>
    internal class MultiCollectionBindingEx : MultiCollectionBindingBase
    {
        #region Constructors

        /// <summary>
        /// Constuctor.
        /// </summary>
        /// <param name="canSelect">Delegate for checking newly added items.</param>
        public MultiCollectionBindingEx(MultiCollectionFilter canSelect)
        {
            Debug.Assert(canSelect != null);

            _canSelect = canSelect;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Register grid collection.
        /// </summary>
        /// <param name="grid">Grid to register.</param>
        /// <param name="multiCollectionFilter">Collection filter.</param>
        public void RegisterCollection(DataGridControlEx grid, MultiCollectionFilter multiCollectionFilter)
        {
            _multiCollectionFilters.Add(multiCollectionFilter);
            _RegisterCollection(grid);
        }

        /// <summary>
        /// Register grid collection
        /// </summary>
        /// <param name="grid">Grid to register</param>
        /// <param name="multiCollectionFilter">Collection filter.</param>
        public void RegisterCollection(IList collection, MultiCollectionFilter multiCollectionFilter)
        {
            _multiCollectionFilters.Add(multiCollectionFilter);
            _RegisterCollection(collection);
        }

        /// <summary>
        /// Unregisters collection.
        /// </summary>
        /// <param name="collection">Collection to unregister.</param>
        public override void UnregisterCollection(IList collection)
        {
            int index = Collections.IndexOf(collection);
            if (index != -1)
            {
                _multiCollectionFilters.RemoveAt(index);
                base.UnregisterCollection(collection);
            }
            else
                Debug.Assert(false);
        }

        /// <summary>
        /// Is collection registered.
        /// </summary>
        /// <param name="collection">Collection to check.</param>
        /// <returns>Is collection registered.</returns>
        public bool IsCollectionRegistered(IList collection)
        {
            int index = Collections.IndexOf(collection);
            return index > -1;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Filters out invalid additions.
        /// </summary>
        /// <param name="collSender">Collection - changes initiator.</param>
        /// <param name="itemsToAdd">Items to add.</param>
        protected override void _FilterInvalidChanges(IList collSender, IList itemsToAdd)
        {
            Debug.Assert(collSender != null);

            // If changes was made by internal logic - they dont need to be apply to other collections.
            if (IsRecursive)
                return;

            // Changes, without adding items and not with key modifiers - valid always.
            if (itemsToAdd == null || itemsToAdd.Count == 0 ||
                (Keyboard.Modifiers != ModifierKeys.Shift && Keyboard.Modifiers != ModifierKeys.Control))
                return;

            // Get items, which must not be added.
            List<object> itemsToClearFromSelection = new List<object>();
            foreach (object obj in itemsToAdd)
            {
                if (!_canSelect(obj))
                {
                    itemsToClearFromSelection.Add(obj);
                }
            }
            
            // Remove them from itemsToAdd collection.
            foreach (object obj in itemsToClearFromSelection)
            {
                itemsToAdd.Remove(obj);
            }

            // If not valid items to add exists - do suspended removing.
            if (itemsToClearFromSelection.Count > 0)
            {
                App.Current.MainWindow.Dispatcher.BeginInvoke(new CollectionClearDelegate(_RemoveFromCollection),
                DispatcherPriority.Send, itemsToClearFromSelection, collSender);
            }
        }

        /// <summary>
        /// Remove items from collection.
        /// </summary>
        /// <param name="newItemsList">Items to remove.</param>
        /// <param name="collSender">Collection to remove items.</param>
        private void _RemoveFromCollection(IList newItemsList, IList collSender)
        {
            Debug.Assert(newItemsList != null);
            Debug.Assert(collSender != null);
            Debug.Assert(DataGridList != null);

            IsRecursive = true;

            // Find grid from which needs to remove items.
            int dataGridIndex = Collections.IndexOf(collSender);
            DataGridControlEx parentGrid = DataGridList[dataGridIndex];

            Debug.Assert(parentGrid != null);

            // Go through items to remove collection and remove them from initiator collection.
            foreach (object item in newItemsList)
            {
                // If item in main collection - remove it.
                if (collSender.Contains(item))
                {
                    collSender.Remove(item);
                }
                else
                {
                    // Find child context, that contain this item.
                    DataGridContext parentDataGridContext = null;
                    IEnumerable<DataGridContext> childContexts = parentGrid.GetChildContexts();
                    foreach (DataGridContext dataGridContext in childContexts)
                    {
                        if (dataGridContext.SelectedItems.Contains(item))
                        {
                            parentDataGridContext = dataGridContext;
                            break;
                        }
                    }

                    // Remove item from selection.
                    parentDataGridContext.SelectedItems.Remove(item);
                }

            }

            IsRecursive = false;
        }

        /// <summary>
        /// Add items to collection.
        /// </summary>
        /// <param name="collection">Collection, from to items will added.</param>
        /// <param name="addedItems">Items to add.</param>
        /// <param name="newStartingIndex">New starting index.</param>
        protected override void _AddItems(IList collection, IList addedItems, int newStartingIndex)
        {
            // Get filter for collection.
            int collectionIndex = Collections.IndexOf(collection);
            MultiCollectionFilter filter = _multiCollectionFilters[collectionIndex];

            int index = newStartingIndex;
            foreach (object addItem in addedItems)
            {
                Debug.Assert(!collection.Contains(addItem));
                if (!collection.Contains(addItem))
                {
                    if (filter(addItem))
                    {
                        if (collection.Count >= index)
                        {
                            collection.Insert(index++, addItem);
                        }
                        else
                        {
                            collection.Add(addItem);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copy items from source collection to target collection.
        /// </summary>
        /// <param name="collection">Target collection.</param>
        /// <param name="newItems">Source collection.</param>
        protected override void _CopyItems(IList collection, IList newItems)
        {
            // Get filter for collection.
            int index = Collections.IndexOf(collection);
            MultiCollectionFilter filter = _multiCollectionFilters[index];

            collection.Clear();
            foreach (object item in newItems)
            {
                Debug.Assert(!collection.Contains(item));
                
                // Add item, if item can be add.
                if (filter(item))
                {
                    collection.Add(item);

                    // If item not selected - select in child context.
                    if (!collection.Contains(item))
                    {
                        int dataGridIndex = Collections.IndexOf(collection);

                        DataGridList[dataGridIndex].SelectInChildContext(item);
                    }
                }
            }
        }

        #endregion

        #region private members

        /// <summary>
        /// Filters for collections.
        /// </summary>
        private List<MultiCollectionFilter> _multiCollectionFilters = new List<MultiCollectionFilter>();

        /// <summary>
        /// Delegate for checking newly added items.
        /// </summary>
        private MultiCollectionFilter _canSelect;

        /// <summary>
        /// Delegate for removing items from collection.
        /// </summary>
        /// <param name="newItemsList">Items to remove.</param>
        /// <param name="collSender">Collection to change.</param>
        private delegate void CollectionClearDelegate(IList newItemsList, IList collSender);

        #endregion
    }
}

using System.Collections;
using System.Diagnostics;
using ESRI.ArcLogistics.App.Controls;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Class that allows to keep several registered collection synchronized. 
    /// Collections must implement interfaces IList and INotifyCollectionChanged or it needs to be DataGridControlEx.
    /// </summary>
    internal class MultiCollectionBinding : MultiCollectionBindingBase
    {
        /// <summary>
        /// Registers collection.
        /// </summary>
        /// <param name="collection">Collection to register.</param>
        public void RegisterCollection(IList collection)
        {
            Debug.Assert(collection != null);

            _RegisterCollection(collection);
        }

        /// <summary>
        /// Register grid collection.
        /// </summary>
        /// <param name="grid">Grid to register.</param>
        public void RegisterCollection(DataGridControlEx grid)
        {
            Debug.Assert(grid != null);

            _RegisterCollection(grid);
        }

        #region private methods

        /// <summary>
        /// Filters out invalid additions.
        /// </summary>
        /// <param name="collSender">Collection - changes initiator.</param>
        /// <param name="itemsToAdd">Items to add.</param>
        protected override void _FilterInvalidChanges(IList collSender, IList itemsToAdd)
        {
        }

        /// <summary>
        /// Add items to collection.
        /// </summary>
        /// <param name="collection">Collection, from to items will added.</param>
        /// <param name="addedItems">Items to add.</param>
        /// <param name="newStartingIndex">New starting index.</param>
        protected override void _AddItems(IList collection, IList addedItems, int newStartingIndex)
        {
            Debug.Assert(collection != null);
            Debug.Assert(addedItems != null);

            int index = newStartingIndex;
            foreach (object remItem in addedItems)
            {
                if (collection.Count >= index)
                {
                    collection.Insert(index++, remItem);
                }
                else
                {
                    collection.Add(remItem);
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
            Debug.Assert(collection != null);
            Debug.Assert(newItems != null);

            collection.Clear();
            foreach (object item in newItems)
            {
                collection.Add(item);
            }
        }

        #endregion
    }
}

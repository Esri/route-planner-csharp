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
using System.Data.Objects.DataClasses;
using System.Diagnostics;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Class that wraps data object collection and represents it sorted. It listens to the base collection for 
    /// changes and updates itself accordingly.
    /// </summary>
    public class SortedDataObjectCollection<T> : IDataObjectCollection<T>
        where T : DataObject 
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>SortedDataObjectCollection</c> class.
        /// </summary>
        public SortedDataObjectCollection(IDataObjectCollection<T> coll,
            IComparer<T> comparer)
        {
            Debug.Assert(coll != null);
            Debug.Assert(comparer != null);

            _Init(coll, comparer);

            coll.CollectionChanged += new NotifyCollectionChangedEventHandler(
                _coll_CollectionChanged);

            _coll = coll;
            _comparer = comparer;
        }

        #endregion constructors

        #region INotifyCollectionChanged members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion INotifyCollectionChanged members

        #region IDisposable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Stops listening to the base collection for changes.
        /// </summary>
        public void Dispose()
        {
            if (_coll != null)
            {
                _coll.CollectionChanged -= new NotifyCollectionChangedEventHandler(
                    _coll_CollectionChanged);
            }
        }

        #endregion IDisposable interface members

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            return CollectionHelper.ToString(this as IList<T>);
        }

        /// <summary>
        /// Returns wrapped not sorted collection.
        /// </summary>
        public IDataObjectCollection<T> InternalCollection
        {
            get
            {
                return _coll;
            }
        }

        #endregion public methods

        #region IList<T> interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public T this[int index]
        {
            get
            {
                return (T)_sortedList[index];
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public int Count
        {
            get
            {
                return _sortedList.Count;
            }
        }

        public void Add(T dataObject)
        {
            _coll.Add(dataObject);
        }

        public bool Remove(T dataObject)
        {
            return _coll.Remove(dataObject);
        }

        public void Clear()
        {
            _coll.Clear();
        }

        public bool Contains(T dataObject)
        {
            if (dataObject == null)
                throw new ArgumentNullException();

            return _sortedList.Contains(dataObject);
        }

        public void CopyTo(T[] dataObjectArray, int index)
        {
            _sortedList.CopyTo(dataObjectArray, index);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _sortedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _sortedList.GetEnumerator();
        }

        public bool IsReadOnly
        {
            get { return _coll.IsReadOnly; }
        }

        public int IndexOf(T item)
        {
            return _sortedList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            throw new InvalidOperationException();
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _sortedList.Count)
                throw new ArgumentException();

            _coll.Remove(_sortedList[index]);
        }

        #endregion IList interface members

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _Init(IDataObjectCollection<T> coll,
            IComparer<T> comparer)
        {
            // fill collection
            _sortedList.AddRange(coll);

            // sort
            _sortedList.Sort(comparer);
        }

        private void _Reset()
        {
            _sortedList.Clear();
            _Init(_coll, _comparer);
        }

        private void _coll_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _OnAddItems(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _OnRemoveItems(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _OnResetItems();
                    break;
            }
        }

        private void _NotifyCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, e);
        }

        private void _OnAddItems(IList items)
        {
            foreach (object item in items)
            {
                int index = _InsertItem(item as T);

                _NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    item,
                    index));
            }
        }

        private void _OnRemoveItems(IList items)
        {
            foreach (object item in items)
            {
                //int index = _sortedList.BinarySearch(item as T, _comparer);
                int index = _sortedList.IndexOf(item as T);
                if (index >= 0)
                {
                    Object obj = _sortedList[index];
                    _sortedList.RemoveAt(index);

                    _NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        obj,
                        index));
                }
            }
        }

        private void _OnResetItems()
        {
            _Reset();

            _NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Reset));
        }

        private int _InsertItem(T item)
        {
            int index = _sortedList.BinarySearch(item, _comparer);

            if (index < 0)
                index = ~index;

            _sortedList.Insert(index, item);

            return index;
        }

        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private List<T> _sortedList = new List<T>();
        private IDataObjectCollection<T> _coll;
        private IComparer<T> _comparer;

        #endregion private fields
    }
}

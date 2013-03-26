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
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Data.Objects.DataClasses;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// RelationObjectCollection class.
    /// Represents a collection of data objects on the "many" end of a relationship.
    /// </summary>
    internal class RelationObjectCollection<T, TEntity> : IDataObjectCollection<T>
        where T : DataObject
        where TEntity : EntityObject
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of RelationObjectCollection class.
        /// </summary>
        public RelationObjectCollection(EntityCollection<TEntity> entities,
            DataObject owner,
            bool isReadOnly)
        {
            Debug.Assert(entities != null);
            Debug.Assert(owner != null);

            if (owner.IsStored)
            {
                // load related objects
                if (!entities.IsLoaded)
                    entities.Load();
            }

            _entities = entities;
            _owner = owner;
            _isReadOnly = isReadOnly;

            // init data objects collection
            _FillCollection();

            // attach to collection events
            _entities.AssociationChanged += new CollectionChangeEventHandler(
                _OnAssociationChanged);

            _dataObjects.CollectionChanged += new NotifyCollectionChangedEventHandler(
                _dataObjects_CollectionChanged);
        }

        #endregion constructors

        #region INotifyCollectionChanged members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion INotifyCollectionChanged members

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            return CollectionHelper.ToString(this as IList<T>);
        }

        #endregion public methods

        #region IDisposable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public void Dispose()
        {
            if (_entities != null)
            {
                _entities.AssociationChanged -= new CollectionChangeEventHandler(
                    _OnAssociationChanged);
            }

            if (_dataObjects != null)
            {
                _dataObjects.CollectionChanged -= new NotifyCollectionChangedEventHandler(
                    _dataObjects_CollectionChanged);
            }
        }

        #endregion IDisposable interface members

        #region IList<T> interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Default accessor for the collection.
        /// </summary>
        public T this[int index]
        {
            get
            {
                return (T)_dataObjects[index];
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Number of elements in the collection.
        /// </summary>
        public int Count
        {
            get { return _dataObjects.Count; }
        }

        /// <summary>
        /// Adds data object to the collection.
        /// </summary>
        public virtual void Add(T dataObject)
        {
            if (dataObject == null)
                throw new ArgumentNullException();

            _CheckReadOnlyFlag();

            _entities.Add(DataObjectHelper.GetEntityObject(
                dataObject) as TEntity);
        }

        /// <summary>
        /// Removes data object from the collection.
        /// </summary>
        public virtual bool Remove(T dataObject)
        {
            if (dataObject == null)
                throw new ArgumentNullException();

            _CheckReadOnlyFlag();

            return _entities.Remove(DataObjectHelper.GetEntityObject(
                dataObject) as TEntity);
        }

        /// <summary>
        /// Clear the collection of all it's elements.
        /// </summary>
        public virtual void Clear()
        {
            _CheckReadOnlyFlag();
            _entities.Clear();

            // explicitly clear collection
            _dataObjects.Clear();
        }

        /// <summary>
        /// Returns boolean value based on whether or not it finds the
        /// requested object in the collection.
        /// </summary>
        public bool Contains(T dataObject)
        {
            if (dataObject == null)
                throw new ArgumentNullException();

            return _dataObjects.Contains(dataObject);
        }

        /// <summary>
        /// Copies objects from this collection into another array.
        /// </summary>
        public void CopyTo(T[] dataObjectArray, int index)
        {
            _dataObjects.CopyTo(dataObjectArray, index);
        }

        /// <summary>
        /// Returns custom generic enumerator for this collection.
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _dataObjects.GetEnumerator();
        }

        /// <summary>
        /// Explicit non-generic interface implementation for IEnumerable
        /// extended and required by ICollection (implemented by ICollection<T>).
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dataObjects.GetEnumerator();
        }

        /// <summary>
        /// Returns a boolean value indicating whether the collection is
        /// read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        public int IndexOf(T item)
        {
            return _dataObjects.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            throw new InvalidOperationException();
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _dataObjects.Count)
                throw new ArgumentException();

            Remove(_dataObjects[index]);
        }

        #endregion IList<T> interface members

        #region protected properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected ObservableCollection<T> DataObjects
        {
            get { return _dataObjects; }
        }

        #endregion protected properties

        #region protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected DataObject _Owner
        {
            get
            {
                return _owner;
            }
        }

        protected bool GetObjectContext(out DataObjectContext context)
        {
            context = null;

            bool res = false;
            if (_owner.IsStored)
            {
                context = ContextHelper.GetObjectContext(_entities);
                res = true;
            }

            return res;
        }

        #endregion protected methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _dataObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, e);
        }

        private void _OnAssociationChanged(object sender, CollectionChangeEventArgs e)
        {
            EntityObject entity = e.Element as EntityObject;
            if (entity != null)
            {
                if (e.Action == CollectionChangeAction.Add)
                {
                    // add item
                    // TODO: check if sender is underlying collection
                    T obj = _FindObject(entity);
                    if (obj == null)
                        _dataObjects.Add(_GetDataObject(entity));
                }
                else if (e.Action == CollectionChangeAction.Remove)
                {
                    // remove item
                    T obj = _FindObject(entity);
                    if (obj != null)
                        _dataObjects.Remove(obj);
                }
                else if (e.Action == CollectionChangeAction.Refresh)
                {
                    // TODO: cannot reset collection here since underlying collection is not yet updated
                }
            }
        }

        private void _FillCollection()
        {
            foreach (TEntity entity in _entities)
                _dataObjects.Add(_GetDataObject(entity));
        }

        private void _CheckReadOnlyFlag()
        {
            if (_isReadOnly)
            {
                throw new InvalidOperationException(
                    Properties.Messages.Error_ReadOnlyCollectionChange);
            }
        }

        private T _FindObject(EntityObject entity)
        {
            T res = null;
            foreach (T obj in _dataObjects)
            {
                IRawDataAccess dataAccess = obj as IRawDataAccess;
                if (dataAccess != null)
                {
                    if (dataAccess.RawEntity.Equals(entity))
                    {
                        res = obj;
                        break;
                    }
                }
            }

            return res;
        }

        private T _GetDataObject(EntityObject entity)
        {
            T obj = null;
            if (_owner.IsStored)
            {
                // Try to find data object through IWrapDataAccess.
                obj = DataObjectHelper.GetDataObject(entity) as T;

                if (obj == null)
                {
                    DataObjectContext ctx = ContextHelper.GetObjectContext(
                        _entities);

                    obj = DataObjectHelper.GetOrCreateDataObject<T>(ctx,
                        entity);
                }
            }
            else
            {
                obj = DataObjectHelper.GetDataObject(entity) as T;
                if (obj == null)
                    throw new DataException(Properties.Messages.Error_InvalidDataObjectInstance);
            }

            return obj;
        }

        #endregion private methods

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ObservableCollection<T> _dataObjects = new ObservableCollection<T>();
        private EntityCollection<TEntity> _entities;
        private DataObject _owner;
        private bool _isReadOnly = false;

        #endregion private members
    }
}

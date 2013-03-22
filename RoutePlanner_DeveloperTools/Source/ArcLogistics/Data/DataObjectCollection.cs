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
using System.ComponentModel;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Class represents a collection of data objects.
    /// </summary>
    internal class DataObjectCollection<TDataObject, TEntity> :
        IDataObjectCollection<TDataObject>
        where TDataObject : DataObject 
        where TEntity : EntityObject
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of DataObjectCollection class.
        /// </summary>
        public DataObjectCollection(DataObjectContext context,
            string entitySetName, bool isReadOnly)
        {
            _dataService = new DataService<TDataObject>(context, entitySetName);
            _isReadOnly = isReadOnly;
        }

        /// <summary>
        /// Creates a new instance of DataObjectCollection class.
        /// </summary>
        public DataObjectCollection(DataObjectContext context,
            string entitySetName,
            SpecFields specFields, bool isReadOnly)
        {
            _dataService = new DataService<TDataObject>(context, entitySetName,
                specFields);
            _isReadOnly = isReadOnly;
        }

        /// <summary>
        /// Creates a new instance of DataObjectCollection class.
        /// </summary>
        public DataObjectCollection(DataService<TDataObject> dataService,
            bool isReadOnly)
        {
            Debug.Assert(dataService != null);

            _dataService = dataService;
            _isReadOnly = isReadOnly;
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

        /// <summary>
        /// Second-level initialization.
        /// Loads collection data.
        /// <param name="skipDeletedObjects">
        /// A boolean value indicating whether objects marked as deleted must
        /// be skipped when loading data.
        /// </param>
        /// </summary>
        public void Initialize(bool skipDeletedObjects, bool syncWithContext)
        {
            Debug.Assert(!_isInited); // init once

            // load data
            IEnumerable<TDataObject> objects = null;
            if (skipDeletedObjects)
                objects = _dataService.FindNotDeletedObjects<TEntity>();
            else
                objects = _dataService.FindAllObjects<TEntity>();

            _FillCollection(objects);

            // observable collection events
            _dataObjects.CollectionChanged += new NotifyCollectionChangedEventHandler(
                _dataObjects_CollectionChanged);

            if (syncWithContext)
            {
                // context events
                _dataService.ContextCollectionChanged += new CollectionChangeEventHandler(
                    _dataService_ContextCollectionChanged);
            }

            _isInited = true;

            this.OnCollectionInitialized();
        }

        public void Initialize(string initialQuery,
            ObjectParameter[] queryParams,
            Expression<Func<TEntity, bool>> filterClause)
        {
            Debug.Assert(!_isInited); // init once

            // load data
            var objects = _dataService.FindObjects<TEntity>(initialQuery, queryParams);

            this.Initialize(objects, filterClause);
        }

        public void Initialize(Expression<Func<TEntity, bool>> initialClause,
            Expression<Func<TEntity, bool>> filterClause)
        {
            Debug.Assert(!_isInited); // init once

            // load data
            var objects = _dataService.FindObjects(initialClause);

            this.Initialize(objects, filterClause);
        }

        // TODO: workaround
        public void Initialize(IEnumerable<TDataObject> objects,
            Expression<Func<TEntity, bool>> filterClause)
        {
            Debug.Assert(!_isInited); // init once

            // load data
            _FillCollection(objects);

            // observable collection events
            _dataObjects.CollectionChanged += _dataObjects_CollectionChanged;

            if (filterClause != null)
            {
                // context events
                _dataService.ContextCollectionChanged += _dataService_ContextCollectionChanged;
                _filter = filterClause.Compile();
            }

            _isInited = true;

            this.OnCollectionInitialized();
        }

        public override string ToString()
        {
            return CollectionHelper.ToString(this as IList<TDataObject>);
        }

        #endregion public methods

        #region IDisposable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public void Dispose()
        {
            if (_filter != null && _dataService != null)
            {
                _dataService.ContextCollectionChanged -= new CollectionChangeEventHandler(
                    _dataService_ContextCollectionChanged);
            }

            if (_dataObjects != null)
            {
                _dataObjects.CollectionChanged -= new NotifyCollectionChangedEventHandler(
                    _dataObjects_CollectionChanged);
            }
        }

        #endregion IDisposable interface members

        #region IList<TDataObject> interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Default accessor for the collection.
        /// </summary>
        public TDataObject this[int index]
        {
            get
            {
                return (TDataObject)_dataObjects[index];
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
        public virtual void Add(TDataObject dataObject)
        {
            if (dataObject == null)
                throw new ArgumentNullException();

            // check if collecton can be modified
            _CheckReadOnlyFlag();

            // check if object already exists
            if (_dataObjects.Contains(dataObject))
                throw new DataException(Properties.Messages.Error_DataObjectExistsInCollection);

            // check if object is applicable to collection filter
            if (_CanAddObject(dataObject))
            {
                // set CanSave flag
                dataObject.CanSave = true;

                // add object to database
                _dataService.AddObject(dataObject);
            }
            else
            {
                // object does not conform to filtration clause
                throw new DataException(Properties.Messages.Error_DataObjectRejectedByFilter);
            }
        }

        /// <summary>
        /// Removes data object from the collection.
        /// </summary>
        public virtual bool Remove(TDataObject dataObject)
        {
            if (dataObject == null)
                throw new ArgumentNullException();

            // check if collection can be modified
            _CheckReadOnlyFlag();

            // remove object from database
            _dataService.RemoveObject(dataObject);

            return true;
        }

        /// <summary>
        /// Clear the collection of all it's elements.
        /// </summary>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns boolean value based on whether or not it finds the
        /// requested object in the collection.
        /// </summary>
        public bool Contains(TDataObject dataObject)
        {
            if (dataObject == null)
                throw new ArgumentNullException();

            return _dataObjects.Contains(dataObject);
        }

        /// <summary>
        /// Copies objects from this collection into another array.
        /// </summary>
        public void CopyTo(TDataObject[] dataObjectArray, int index)
        {
            _dataObjects.CopyTo(dataObjectArray, index);
        }

        /// <summary>
        /// Returns custom generic enumerator for this collection.
        /// </summary>
        IEnumerator<TDataObject> IEnumerable<TDataObject>.GetEnumerator()
        {
            return _dataObjects.GetEnumerator();
        }

        /// <summary>
        /// Explicit non-generic interface implementation for IEnumerable
        /// extended and required by ICollection (implemented by ICollection<TDataObject>).
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

        public int IndexOf(TDataObject item)
        {
            return _dataObjects.IndexOf(item);
        }

        public void Insert(int index, TDataObject item)
        {
            throw new InvalidOperationException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        #endregion IList interface members

        #region protected properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected ObservableCollection<TDataObject> DataObjects
        {
            get { return _dataObjects; }
        }

        protected DataService<TDataObject> DataService
        {
            get { return _dataService; }
        }

        #endregion protected properties

        #region protected methods
        /// <summary>
        /// Called when this data object collection becomes initialized.
        /// </summary>
        protected virtual void OnCollectionInitialized()
        {
        }
        #endregion

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _dataObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, e);
        }

        private void _dataService_ContextCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            TDataObject obj = e.Element as TDataObject;
            if (obj != null)
            {
                if (e.Action == CollectionChangeAction.Add)
                {
                    if (obj.CanSave &&
                        !_dataObjects.Contains(obj) &&
                        _CanAddObject(obj))
                    {
                        _AddToInternalCollection(obj);
                    }
                }
                else if (e.Action == CollectionChangeAction.Remove)
                {
                    _RemoveFromInternalCollection(obj);
                }
            }
        }

        private void _FillCollection(IEnumerable<TDataObject> objects)
        {
            foreach (TDataObject obj in objects)
                _AddToInternalCollection(obj);
        }

        /// <summary>
        /// Add object to _dataObjects.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        protected virtual void _AddToInternalCollection(TDataObject obj)
        {
            _dataObjects.Add(obj);
        }

        /// <summary>
        /// Remove object from _dataObjects.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        protected virtual void _RemoveFromInternalCollection(TDataObject obj)
        {
            _dataObjects.Remove(obj);
        }

        private bool _CanAddObject(TDataObject dataObject)
        {
            if (_filter == null)
            {
                return true;
            }

            var entity = (TEntity)DataObjectHelper.GetEntityObject(dataObject);

            return _filter(entity);
        }

        private void _CheckReadOnlyFlag()
        {
            if (_isReadOnly)
            {
                throw new InvalidOperationException(
                    Properties.Messages.Error_ReadOnlyCollectionChange);
            }
        }

        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ObservableCollection<TDataObject> _dataObjects = new ObservableCollection<TDataObject>();
        private DataService<TDataObject> _dataService;
        private bool _isReadOnly = false;
        private bool _isInited = false;

        /// <summary>
        /// Filters entity objects before adding them to the collection.
        /// </summary>
        private Func<TEntity, bool> _filter;
        #endregion private fields
    }
}

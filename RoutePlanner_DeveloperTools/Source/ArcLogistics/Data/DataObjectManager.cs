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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ESRI.ArcLogistics;
using ESRI.ArcLogistics.DomainObjects;
using DataModel = ESRI.ArcLogistics.Data.DataModel;
using System.Data.Objects.DataClasses;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Class that manages specified data objects inside of the project. This class is abstract and used as a base for concrete data object managers.
    /// </summary>
    /// <typeparam name="T">Type of managed data objects.</typeparam>
    public abstract class DataObjectManager<T>
        where T : DataObject
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal DataObjectManager(DataObjectContext context, string entityName, SpecFields specFields)
        {
            _context = context;

            _dataService = new DataService<T>(context, entityName, specFields);
            _dataService.ContextCollectionChanged += new CollectionChangeEventHandler(
                _dataService_ContextCollectionChanged);
        }

        internal DataObjectManager(DataService<T> dataService)
        {
            Debug.Assert(dataService != null);

            _dataService = dataService;
            _dataService.ContextCollectionChanged += new CollectionChangeEventHandler(
                _dataService_ContextCollectionChanged);
        }

        #endregion // Constructors

        #region internal events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal event CollectionChangeEventHandler ContextCollectionChanged;

        #endregion

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds data object to the collection.
        /// </summary>
        public virtual void Add(T dataObject)
        {
            System.Diagnostics.Debug.Assert(null != dataObject);

            dataObject.CanSave = true; // object is added explicitly, so it should be saved
            _dataService.AddObject(dataObject);
        }

        /// <summary>
        /// Removes data object from the collection.
        /// </summary>
        public virtual void Remove(T dataObject)
        {
            if (CanRemove(dataObject))
            {
                _Remove(dataObject);
            }
            else
            {
                throw new DataException(this._RemoveErrorMessage,
                    DataError.ObjectRemovalRestricted);
            }
        }

        /// <summary>
        /// Returns a boolean value indicating whether specified object can be removed.
        /// </summary>
        public virtual bool CanRemove(T dataObject)
        {
            return true;
        }

        /// <summary>
        /// Returns object by specified id or null if object cannot be found.
        /// </summary>
        public T SearchById(Guid id)
        {
            return _dataService.FindObjectById(id);
        }

        #endregion // Public methods

        #region Protected properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Data object context.
        /// </summary>
        internal DataObjectContext _Context
        {
            get { return _context; }
        }

        /// <summary>
        /// Data service.
        /// </summary>
        internal DataService<T> _DataService
        {
            get { return _dataService; }
        }

        // APIREV: we don't want to expose these methods to public API

        /// <summary>
        /// Data object entity name.
        /// </summary>
        protected string _EntitySetName
        {
            get { return _dataService.EntitySetName; }
        }

        /// <summary>
        /// Gets removal error message.
        /// </summary>
        protected virtual string _RemoveErrorMessage
        {
            get { return Properties.Messages.Error_DataObjectRemovalFailed; }
        }

        /// <summary>
        /// Removes data object from the collection.
        /// </summary>
        protected virtual void _Remove(T dataObject)
        {
            _dataService.RemoveObject(dataObject);
        }

        /// <summary>
        /// Retrieves data from the specified source.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to be returned.</typeparam>
        /// <param name="source">The source of data to be retrieved.</param>
        /// <param name="filterClause">Filters new data objects for synchronized data object
        /// collections.</param>
        /// <returns>A read-only collection of domain objects for the specified data
        /// source.</returns>
        protected IDataObjectCollection<T> Query<TEntity>(
            IQueryable<TEntity> source,
            Expression<Func<TEntity, bool>> filterClause)
            where TEntity : EntityObject
        {
            var objects = _dataService.FindObjects<TEntity>(source);
            var result = new DataObjectCollection<T, TEntity>(_dataService, true);
            result.Initialize(objects, filterClause);

            return result;
        }

        /// <summary>
        /// Searches the table and returns read-only collection of found domain objects.
        /// </summary>
        /// <typeparam name="TEntity">Type of entities.</typeparam>
        /// <param name="whereClause">Where clause used to search objects.</param>
        /// <param name="filterClause">Filter clause used for syncronized data object collections.</param>
        /// <returns>Collection of found domain objects. It is syncronized in <c>filterClause</c> isn't null.</returns>
        protected IDataObjectCollection<T> _Search<TEntity>(
            Expression<Func<TEntity, bool>> whereClause,
            Expression<Func<TEntity, bool>> filterClause)
            where TEntity : EntityObject
        {
            DataObjectCollection<T, TEntity> resultCol = new DataObjectCollection<T, TEntity>(_dataService, true);
            resultCol.Initialize(whereClause, filterClause);

            return resultCol;
        }

        /// <summary>
        /// Returns syncronized read-only collection of all found domain objects.
        /// </summary>
        /// <typeparam name="TEntity">Type of entities.</typeparam>
        /// <returns>Syncronized read-only collection of all found domain objects.</returns>
        protected IDataObjectCollection<T> _SearchAll<TEntity>(bool asSynchronized)
            where TEntity : EntityObject
        {
            DataObjectCollection<T, TEntity> resultCol = new DataObjectCollection<T, TEntity>(_dataService, true);
            resultCol.Initialize(false, asSynchronized);

            return resultCol;
        }

        /// <summary>
        /// Searches the table and returns read-only collection of found domain objects.
        /// </summary>
        /// <typeparam name="TEntity">Type of entities.</typeparam>
        /// <param name="initialQuery">Intial query to the database.</param>
        /// <param name="queryParams">Parameters for the query.</param>
        /// <param name="filterClause">Filter clause used for syncronized data object collections.</param>
        /// <returns>Collection of found domain objects. It is syncronized in <c>filterClause</c> isn't null.</returns>
        protected IDataObjectCollection<T> _Search<TEntity>(string initialQuery,
            ObjectParameter[] queryParams,
            Expression<Func<TEntity, bool>> filterClause)
            where TEntity : EntityObject
        {
            DataObjectCollection<T, TEntity> resultCol = new DataObjectCollection<T, TEntity>(_dataService, true);
            resultCol.Initialize(initialQuery, queryParams, filterClause);

            return resultCol;
        }

        #endregion // Protected properties

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _dataService_ContextCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            if (ContextCollectionChanged != null)
                ContextCollectionChanged(this, e);
        }

        #endregion // Private methods

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DataObjectContext _context = null;
        private DataService<T> _dataService = null;

        #endregion // Private fields
    }
}

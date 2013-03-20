using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Data.Objects;
using System.Data;
using System.Linq.Expressions;
using System.Linq;
using System.Data.SqlServerCe;
using System.Data.EntityClient;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// DataService class.
    /// Base class for data services. Provides base data service functionality
    /// like adding, removing and retrieving data objects.
    /// </summary>
    internal abstract class BaseDataService
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of DataService class.
        /// </summary>
        protected BaseDataService(DataObjectContext context, string entitySetName)
        {
            Debug.Assert(context != null);
            Debug.Assert(entitySetName != null);

            _fullEntitySet = ContextHelper.GetFullEntitySetName(context,
                entitySetName);

            _context = context;
            _entitySet = entitySetName;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets fully qualified entity set name.
        /// </summary>
        public string FullEntitySetName
        {
            get { return _fullEntitySet; }
        }

        /// <summary>
        /// Gets entity set name.
        /// </summary>
        public string EntitySetName
        {
            get { return _entitySet; }
        }

        public SqlCeConnection StoreConnection
        {
            get
            {
                EntityConnection entityConn = _context.Connection as EntityConnection;
                Debug.Assert(entityConn != null);

                return entityConn.StoreConnection as SqlCeConnection;
            }
        }

        #endregion public properties

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ObjectQuery<TEntity> CreateDefaultQuery<TEntity>()
        {
            return _context.CreateQuery<TEntity>(
                SqlFormatHelper.FormatObjName(_entitySet));
        }

        #endregion public methods

        #region protected properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets object context.
        /// </summary>
        protected DataObjectContext ObjectContext
        {
            get { return _context; }
        }

        #endregion protected properties

        #region protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds data object to object context.
        /// </summary>
        /// <param name="dataObject">
        /// DataObject object to add.
        /// </param>
        protected void AddObject(DataObject dataObject)
        {
            Debug.Assert(dataObject != null);

            // add entity object to context
            _context.AddObject(_fullEntitySet, DataObjectHelper.GetEntityObject(
                dataObject));
        }

        /// <summary>
        /// Removes data object from object context.
        /// </summary>
        /// <param name="dataObject">
        /// DataObject object to remove.
        /// </param>
        protected void RemoveObject(DataObject dataObject)
        {
            Debug.Assert(dataObject != null);

            EntityObject entity = DataObjectHelper.GetEntityObject(
                dataObject);

            // remove data object from cache
            // TODO: entity stays in context until SaveChanges is called,
            // so alternatively we can sync. cache with context handling
            // SavingChanges event.
            //_context.Cache.RemoveObject(entity);

            // remove entity object from context
            _context.DeleteObject(entity);
        }

        /// <summary>
        /// Removes all data objects from object context.
        /// </summary>
        protected void RemoveAllObjects<TEntity>()
            where TEntity : EntityObject
        {
            // remove data object from cache
            //_context.Cache.Clear();

            // remove entity objects from context
            // EF 1.0 does not support RemoveAll functionality
            // TODO: use entity SQL
            ObjectQuery<TEntity> query = _context.CreateQuery<TEntity>(
                SqlFormatHelper.FormatObjName(_entitySet));

            foreach (TEntity entity in query)
                _context.DeleteObject(entity);
        }

        /// <summary>
        /// Retrieves data object by specified key name and object id.
        /// </summary>
        /// <param name="key">
        /// EntityKey object that represents entity id.
        /// </param>
        /// <returns>
        /// Generic data object. Null value is returned if the data object
        /// cannot be found.
        /// </returns>
        protected T FindObjectById<T>(EntityKey key)
            where T : DataObject
        {
            Debug.Assert(key != null);

            T obj = default(T);

            object entity = null;
            if (_context.TryGetObjectByKey(key, out entity))
            {
                obj = DataObjectHelper.GetOrCreateDataObject<T>(_context,
                    entity as EntityObject);
            }

            return obj;
        }

        /// <summary>
        /// Retrieves data object by specified key name and object id.
        /// </summary>
        /// <param name="keyName">
        /// Name of identity field in the table.
        /// </param>
        /// <param name="id">
        /// Data object id.
        /// </param>
        /// <returns>
        /// Generic data object. Null value is returned if the data object
        /// cannot be found.
        /// </returns>
        protected T FindObjectById<T>(string keyName, object id)
            where T : DataObject
        {
            Debug.Assert(keyName != null);

            return FindObjectById<T>(new EntityKey(_fullEntitySet, keyName, id));
        }

        /// <summary>
        /// Retrieves collection of data objects by specified query.
        /// </summary>
        /// <param name="queryStr">
        /// Query string.
        /// </param>
        /// <param name="parameters">
        /// Query parameters.
        /// </param>
        /// <returns>
        /// Collection of generic data objects. Empty collection is returned
        /// if no data objects found.
        /// </returns>
        protected IEnumerable<T> FindObjects<T, TEntity>(string queryStr,
            params ObjectParameter[] parameters)
            where T : DataObject
            where TEntity : EntityObject
        {
            Debug.Assert(queryStr != null);

            ObjectQuery<TEntity> query = _context.CreateQuery<TEntity>(queryStr,
                parameters);

            List<T> list = new List<T>(_FindObjects<T, TEntity>(query));

            return list.AsReadOnly();
        }

        protected IEnumerable<T> FindObjects<T, TEntity>(Expression<Func<TEntity, bool>> whereClause)
            where T : DataObject
            where TEntity : EntityObject
        {
            Debug.Assert(whereClause != null);

            // get objects query
            ObjectQuery<TEntity> query = _context.CreateQuery<TEntity>(SqlFormatHelper.FormatObjName(_entitySet));

            // find objects
            IQueryable<TEntity> resultQuery = query.Where(whereClause);

            List<T> list = new List<T>(_FindObjects<T, TEntity>(resultQuery));

            return list.AsReadOnly();
        }

        /// <summary>
        /// Retrieves data objects from the specified source.
        /// </summary>
        /// <typeparam name="T">The type of data objects to be retrieved.</typeparam>
        /// <typeparam name="TEntity">The type of source entities.</typeparam>
        /// <param name="source">The source of data objects to be retrieved.</param>
        /// <returns>A collection of data objects from the specified source.</returns>
        protected IEnumerable<T> FindObjects<T, TEntity>(IQueryable<TEntity> source)
            where T : DataObject
            where TEntity : EntityObject
        {
            Debug.Assert(source != null);

            return _FindObjects<T, TEntity>(source);
        }

        /// <summary>
        /// Retrieves collection of data objects.
        /// </summary>
        /// <returns>
        /// Collection of generic data objects. Empty collection is returned
        /// if no data objects found.
        /// </returns>
        protected IEnumerable<T> FindAllObjects<T, TEntity>()
            where T : DataObject
            where TEntity : EntityObject
        {
            return FindObjects<T, TEntity>(
                SqlFormatHelper.FormatObjName(_entitySet));
        }
        #endregion protected methods

        #region private methods
        /// <summary>
        /// Retrieves data objects from the specified source.
        /// </summary>
        /// <typeparam name="T">The type of data objects to be retrieved.</typeparam>
        /// <typeparam name="TEntity">The type of source entities.</typeparam>
        /// <param name="source">The source of data objects to be retrieved.</param>
        /// <returns>A collection of data objects from the specified source.</returns>
        private IEnumerable<T> _FindObjects<T, TEntity>(IQueryable<TEntity> source)
            where T : DataObject
            where TEntity : EntityObject
        {
            foreach (var entity in source)
            {
                yield return DataObjectHelper.GetOrCreateDataObject<T>(_context, entity);
            }
        }
        #endregion

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DataObjectContext _context;
        private string _entitySet;
        private string _fullEntitySet;

        #endregion private fields
    }
}

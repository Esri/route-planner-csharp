using System;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Diagnostics;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// EntityCollWrapper class.
    /// Wraps entity collection and provides access to data objects collection.
    /// </summary>
    internal class EntityCollWrapper<TDataObj, TEntity>
        where TDataObj : DataObject
        where TEntity : EntityObject
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of EntityCollWrapper class.
        /// </summary>
        public EntityCollWrapper(
            EntityCollection<TEntity> entities,
            DataObject owner,
            bool isReadOnly)
            : this(entities, owner, isReadOnly, delegate { })
        {
        }

        /// <summary>
        /// Initializes a new instance of EntityCollWrapper class.
        /// </summary>
        /// <param name="entities">Entities collection to be wrapped.</param>
        /// <param name="owner">The owner of the entities collection.</param>
        /// <param name="isReadOnly">Indicates if the collection should be read-only.</param>
        /// <param name="collectionInitializedCallback">A callback to be called after collection
        /// initialization.</param>
        public EntityCollWrapper(
            EntityCollection<TEntity> entities,
            DataObject owner,
            bool isReadOnly,
            Action collectionInitializedCallback)
        {
            Debug.Assert(collectionInitializedCallback != null);

            _entities = entities;
            _owner = owner;
            _isReadOnly = isReadOnly;
            _collectionInitializedCallback = collectionInitializedCallback;
        }
        #endregion constructors

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets data objects collection that represents "many" end of a
        /// relationship.
        /// </summary>
        public IDataObjectCollection<TDataObj> DataObjects
        {
            get
            {
                _Init();
                return _dataObjects;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                _Init();
                if (value != _dataObjects)
                    _Reset(value);
            }
        }

        #endregion public methods

        #region protected overridable methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected virtual IDataObjectCollection<TDataObj> CreateCollection(
            EntityCollection<TEntity> entities,
            DataObject owner,
            bool isReadOnly)
        {
            return new RelationObjectCollection<TDataObj, TEntity>(
                _entities,
                _owner,
                _isReadOnly);
        }
        
        #endregion protected overridable methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _Init()
        {
            if (_dataObjects == null)
            {
                _dataObjects = CreateCollection(_entities, _owner, _isReadOnly);
                _collectionInitializedCallback();
            }
        }

        private void _Reset(IDataObjectCollection<TDataObj> dataObjects)
        {
            _dataObjects.Clear();

            List<TDataObj> coll = new List<TDataObj>(dataObjects);
            foreach (TDataObj obj in coll)
                _dataObjects.Add(obj);
        }

        #endregion private methods

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private EntityCollection<TEntity> _entities;
        private DataObject _owner;
        private IDataObjectCollection<TDataObj> _dataObjects;
        private bool _isReadOnly;

        /// <summary>
        /// Stores a callback to be called after collection initialization.
        /// </summary>
        private Action _collectionInitializedCallback;
        #endregion private members
    }
}

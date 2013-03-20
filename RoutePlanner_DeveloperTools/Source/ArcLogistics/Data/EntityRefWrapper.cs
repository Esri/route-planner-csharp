using System;
using System.Diagnostics;
using System.Data.Objects.DataClasses;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// EntityRefWrapper class.
    /// Wraps entity reference.
    /// </summary>
    internal class EntityRefWrapper<TDataObj, TEntity>
        where TDataObj : DataObject
        where TEntity : EntityObject
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public EntityRefWrapper(EntityReference<TEntity> entityRef,
            DataObject owner)
        {
            Debug.Assert(entityRef != null);

            _entityRef = entityRef;
            _owner = owner;
        }

        #endregion constructors

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets data object that repesents "one" end of a relationship.
        /// </summary>
        public TDataObj Value
        {
            get
            {
                return _GetValue();
            }
            set
            {
                _SetValue(value);
            }
        }

        #endregion public methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private TDataObj _GetValue()
        {
            _Load();

            TDataObj dataObject = null;
            TEntity entity = _entityRef.Value;
            if (entity != null)
                dataObject = _GetDataObject(entity);

            return dataObject;
        }

        private void _SetValue(TDataObj dataObject)
        {
            TEntity entity = null;
            if (dataObject != null)
            {
                entity = DataObjectHelper.GetEntityObject(dataObject) as TEntity;
                if (entity == null)
                    throw new DataException(Properties.Messages.Error_InvalidDataObjectInstance);
            }

            _entityRef.Value = entity;
        }

        private void _Load()
        {
            if (_entityRef.Value == null)
            {
                if (_owner.IsStored && !_entityRef.IsLoaded)
                    _entityRef.Load();
            }
        }

        private TDataObj _GetDataObject(EntityObject entity)
        {
            TDataObj obj = null;
            if (_owner.IsStored)
            {
                // Try to find data object through IWrapDataAccess.
                obj = DataObjectHelper.GetDataObject(entity) as TDataObj;

                if (obj == null)
                {
                    DataObjectContext ctx = ContextHelper.GetObjectContext(
                        _entityRef);

                    obj = DataObjectHelper.GetOrCreateDataObject<TDataObj>(ctx,
                        entity);
                }
            }
            else
            {
                obj = DataObjectHelper.GetDataObject(entity) as TDataObj;
                if (obj == null)
                    throw new DataException(Properties.Messages.Error_InvalidDataObjectInstance);
            }

            return obj;
        }

        #endregion private methods

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private EntityReference<TEntity> _entityRef;
        private DataObject _owner;

        #endregion private members
    }

}

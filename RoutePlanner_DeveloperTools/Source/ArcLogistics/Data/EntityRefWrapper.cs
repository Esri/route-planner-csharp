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

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// DataService class.
    /// </summary>
    internal class DataService<T> : BaseDataService
        where T : DataObject
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public DataService(DataObjectContext context, string entitySetName)
            : base(context, entitySetName)
        {
            context.ObjectStateManager.ObjectStateManagerChanged += new CollectionChangeEventHandler(
                ObjectStateManager_Changed);
        }

        public DataService(DataObjectContext context, string entitySetName,
            SpecFields specFields)
            : base(context, entitySetName)
        {
            Debug.Assert(specFields != null);
            _specFields = specFields;

            context.ObjectStateManager.ObjectStateManagerChanged += new CollectionChangeEventHandler(
                ObjectStateManager_Changed);
        }

        #endregion constructors

        #region public events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raises when objects are added or removed from context.
        /// </summary>
        public event CollectionChangeEventHandler ContextCollectionChanged;

        #endregion public events

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets and sets special fields.
        /// </summary>
        public SpecFields SpecFields
        {
            get { return _specFields; }
            set
            {
                Debug.Assert(value != null);
                _specFields = value;
            }
        }

        #endregion public properties

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public void AddObject(T dataObject)
        {
            EntityObject entity = DataObjectHelper.GetEntityObject(dataObject);

            // check if entity already added
            // entity could be implicitly added by setting a relation object
            if (!_ContainsEntity(entity))
            {
                base.AddObject(dataObject);
            }
            else
            {
                // emulate Add event
                _NotifyContextCollectionChanged(CollectionChangeAction.Add,
                    dataObject);
            }
        }

        public void RemoveObject(T dataObject)
        {
            IMarkableAsDeleted delMark = dataObject as IMarkableAsDeleted;
            if (delMark != null)
            {
                // mark object as deleted
                delMark.IsMarkedAsDeleted = true;

                // emulate Remove event
                _NotifyContextCollectionChanged(CollectionChangeAction.Remove,
                    dataObject);
            }
            else
            {
                // delete object from context
                base.RemoveObject(dataObject);
            }
        }

        public T FindObjectById(Guid id)
        {
            if (_specFields == null || _specFields.KeyFieldName == null)
                throw new InvalidOperationException(Properties.Messages.Error_NoEntityKeyName);

            return base.FindObjectById<T>(_specFields.KeyFieldName, id);
        }

        public T FindObjectById(EntityKey key)
        {
            return base.FindObjectById<T>(key);
        }

        public IEnumerable<T> FindObjects<TEntity>(string query,
            params ObjectParameter[] parameters)
            where TEntity : EntityObject
        {
            return base.FindObjects<T, TEntity>(query, parameters);
        }

        public IEnumerable<T> FindObjects<TEntity>(Expression<Func<TEntity, bool>> whereClause)
            where TEntity : EntityObject
        {
            return base.FindObjects<T, TEntity>(whereClause);
        }

        /// <summary>
        /// Retrieves data objects from the specified source.
        /// </summary>
        /// <typeparam name="TEntity">The type of source entities.</typeparam>
        /// <param name="source">The source of data objects to be retrieved.</param>
        /// <returns>A collection of data objects from the specified source.</returns>
        public IEnumerable<T> FindObjects<TEntity>(IQueryable<TEntity> source)
            where TEntity : EntityObject
        {
            Debug.Assert(source != null);

            return base.FindObjects<T, TEntity>(source);
        }

        public IEnumerable<T> FindAllObjects<TEntity>()
            where TEntity : EntityObject
        {
            return base.FindAllObjects<T, TEntity>();
        }

        public IEnumerable<T> FindNotDeletedObjects<TEntity>()
            where TEntity : EntityObject
        {
            if (_specFields == null || _specFields.DeletionFieldName == null)
                throw new InvalidOperationException(Properties.Messages.Error_NoDeletionFieldName);

            string query = String.Format(QUERY_OBJECTS_BY_DELETED_VALUES,
                this.EntitySetName,
                _specFields.DeletionFieldName);

            return base.FindObjects<T, TEntity>(query,
                new ObjectParameter("deleted_value", false));
        }

        #endregion public methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void ObjectStateManager_Changed(object sender, CollectionChangeEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Refresh)
            {
                _NotifyContextCollectionChanged(e);
            }
            else
            {
                if (e.Element != null && e.Element is EntityObject)
                {
                    EntityObject entity = e.Element as EntityObject;
                    if (entity.EntityState != EntityState.Detached)
                    {
                        object obj = DataObjectHelper.GetDataObject(entity);
                        if (obj != null &&
                            obj.GetType() == typeof(T)) // NOTE: use exact types comparison, derived types are not accepted
                        {
                            _NotifyContextCollectionChanged(e, obj as T);
                        }
                    }
                }
            }
        }

        private void _NotifyContextCollectionChanged(CollectionChangeEventArgs e, T obj)
        {
            if (e.Action == CollectionChangeAction.Add)
            {
                _NotifyContextCollectionChanged(CollectionChangeAction.Add,
                    obj);
            }
            else if (e.Action == CollectionChangeAction.Remove)
            {
                _NotifyContextCollectionChanged(CollectionChangeAction.Remove,
                    obj);
            }
        }

        private void _NotifyContextCollectionChanged(CollectionChangeAction action, T obj)
        {
            _NotifyContextCollectionChanged(new CollectionChangeEventArgs(
                action,
                obj));
        }

        private void _NotifyContextCollectionChanged(CollectionChangeEventArgs e)
        {
            if (ContextCollectionChanged != null)
                ContextCollectionChanged(this, e);
        }

        private bool _ContainsEntity(EntityObject entity)
        {
            Debug.Assert(entity != null);

            IEnumerable<ObjectStateEntry> entries = ObjectContext.ObjectStateManager.GetObjectStateEntries(
                EntityState.Added);

            bool res = false;
            foreach (ObjectStateEntry entry in entries)
            {
                EntityObject addedEntity = (EntityObject)entry.Entity;
                if (addedEntity != null && addedEntity.Equals(entity))
                {
                    res = true;
                    break;
                }
            }

            return res;
        }

        #endregion private methods
        #region private constants

        /// <summary>
        /// Query to DataBase.
        /// </summary>
        private const string QUERY_OBJECTS_BY_DELETED_VALUES = @"select value object from {0} as object where object.{1} is null or object.{1} = @deleted_value";

        #endregion

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private SpecFields _specFields;

        #endregion private members
    }
}

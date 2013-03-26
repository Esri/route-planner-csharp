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
using System.Collections.Generic;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// DataObjectHelper class.
    /// </summary>
    internal class DataObjectHelper
    {
        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets entity object by specified data object.
        /// </summary>
        /// <param name="dataObject">
        /// DataObject object.
        /// </param>
        /// <returns>
        /// EntityObject object.
        /// </returns>
        public static EntityObject GetEntityObject(DataObject dataObject)
        {
            Debug.Assert(dataObject != null);

            IRawDataAccess dataAccess = dataObject as IRawDataAccess;
            if (dataAccess == null)
                throw new DataException(Properties.Messages.Error_InvalidDataObjectInstance);

            return dataAccess.RawEntity;
        }

        /// <summary>
        /// Gets id of specified entity object.
        /// </summary>
        /// <param name="entity">
        /// EntityObject object.
        /// </param>
        /// <returns>
        /// Guid value.
        /// </returns>
        public static Guid GetEntityObjectId(EntityObject entity)
        {
            Debug.Assert(entity != null);

            if (entity.EntityKey == null ||
                entity.EntityKey.EntityKeyValues == null ||
                entity.EntityKey.EntityKeyValues.Length < 1 ||
                entity.EntityKey.EntityKeyValues[0].Value.GetType() != typeof(Guid))
            {
                throw new DataException(Properties.Messages.Error_GetKeyFailed);
            }

            return (Guid)entity.EntityKey.EntityKeyValues[0].Value;
        }

        /// <summary>
        /// Entity set name of specified entity object.
        /// </summary>
        /// <param name="entity">
        /// EntityObject object.
        /// </param>
        /// <returns>
        /// Entity set name value.
        /// </returns>
        public static string GetEntitySetName(EntityObject entity)
        {
            Debug.Assert(entity != null);

            string entitySetName = null;
            if (entity.EntityKey != null)
                entitySetName = entity.EntityKey.EntitySetName;

            return entitySetName;
        }

        /// <summary>
        /// Gets data object associated with specified entity object.
        /// </summary>
        /// <param name="entity">
        /// Entity object.
        /// </param>
        /// <returns>
        /// Data object or null if there is no data object associated
        /// with specified entity.
        /// </returns>
        public static DataObject GetDataObject(EntityObject entity)
        {
            Debug.Assert(entity != null);

            DataObject dataObject = null;

            IWrapDataAccess dataAccess = entity as IWrapDataAccess;

            dataObject = (dataAccess != null) ? dataAccess.DataObject : null;

            return dataObject;
        }

        /// <summary>
        /// Gets or creates data object depending on whether entity object
        /// contains associated data object.
        /// </summary>
        /// <param name="context">DataObjectContext object.</param>
        /// <param name="entity">Entity object.</param>
        /// <returns>Generic data object.</returns>
        public static T GetOrCreateDataObject<T>(DataObjectContext context,
            EntityObject entity)
            where T : DataObject
        {
            Debug.Assert(entity != null);

            // get associated data object
            IWrapDataAccess dataAccess = entity as IWrapDataAccess;
            if (dataAccess == null)
                throw new DataException(Properties.Messages.Error_GetWrapDataObjectFailed);

            T obj = GetDataObject(entity) as T;
            if (obj == null)
            {
                // associated object does not exist, create it
                obj = context.CreateObject<T>(entity);

                // associate object with entity
                dataAccess.DataObject = obj;
            }

            return obj;
        }

        /// <summary>
        /// Finds data object by specified objectId.
        /// </summary>
        /// <param name="objectId">Object id to search.</param>
        /// <param name="objects">Objects collection.</param>
        /// <returns>
        /// Found object or null if object is not found.
        /// </returns>
        public static T FindObjectById<T>(Guid objectId, ICollection<T> objects)
            where T : DataObject
        {
            T res = null;
            foreach (T obj in objects)
            {
                if (obj.Id == objectId)
                {
                    res = obj;
                    break;
                }
            }
            return res;
        }

        #endregion public methods
    }
}

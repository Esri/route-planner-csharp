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
using System.Reflection;
using System.Collections.Generic;
using System.Data.Objects;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Objects.DataClasses;
using System.Data;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// DataObjectContext class.
    /// </summary>
    internal class DataObjectContext : DataModel.Entities
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // EDM type names
        private const string EDM_TYPE_String = "String";

        // EDM facets
        private const string FACET_MaxLength = "MaxLength";

        // configuration schemes
        private static readonly Guid SCHEME_ID_CAPACITIES = new Guid("101E2289-6E5B-4a4e-884D-78A6EB8AA26D");
        private static readonly Guid SCHEME_ID_ORDERPROPERTIES = new Guid("5C7EBA4A-0415-481a-9E59-AED986277E1E");
        private const string SCHEME_NAME_CAPACITIES = "Capacities";
        private const string SCHEME_NAME_ORDERPROPERTIES = "OrderCustomProperties";

        private const string SCHEMES_ENTITY_SET = "ConfigSchemes";
        private const string SCHEMES_KEY = "Id";

        #endregion constants

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new DataObjectContext instance.
        /// </summary>
        public DataObjectContext(string connectionString) : 
                base(connectionString)
        {
            _Init();
        }

        /// <summary>
        /// Initializes a new DataObjectContext instance.
        /// </summary>
        public DataObjectContext(EntityConnection connection) : 
                base(connection)
        {
            _Init();
        }

        #endregion constructors

        #region public events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raises when SaveChanges operation completed.
        /// </summary>
        public event SaveChangesCompletedEventHandler SaveChangesCompleted;

        /// <summary>
        /// Raises on saving changes.
        /// </summary>
        public event SavingChangesEventHandler PostSavingChanges;

        #endregion public events

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Explicit initialization. Must be called on creating new project.
        /// </summary>
        public void PostInit(CapacitiesInfo capacitiesInfo,
            OrderCustomPropertiesInfo orderCustomPropertiesInfo)
        {
            Debug.Assert(!_isInited); // init once

            // add scheme objects
            DataModel.ConfigSchemes scheme = DataModel.ConfigSchemes.CreateConfigSchemes(SCHEME_ID_CAPACITIES);
            scheme.Name = SCHEME_NAME_CAPACITIES;
            scheme.Value = ConfigDataSerializer.SerializeCapacitiesInfo(capacitiesInfo);
            AddObject(SCHEMES_ENTITY_SET, scheme);

            scheme = DataModel.ConfigSchemes.CreateConfigSchemes(SCHEME_ID_ORDERPROPERTIES);
            scheme.Name = SCHEME_NAME_ORDERPROPERTIES;
            scheme.Value = ConfigDataSerializer.SerializeOrderCustomPropertiesInfo(orderCustomPropertiesInfo);
            AddObject(SCHEMES_ENTITY_SET, scheme);

            base.SaveChanges();

            // create object factory
            ObjectInitData initData = new ObjectInitData();
            initData.CapacitiesInfo = capacitiesInfo;
            initData.OrderCustomPropertiesInfo = orderCustomPropertiesInfo;

            _fact = new DataObjectFactory(initData);
            _objInitData = initData;

            // Attach handler for SavingChanges event.
            this.SavingChanges += new EventHandler(DataObjectContext_SavingChanges);
            _isInited = true;
        }

        /// <summary>
        /// Creates data object by specified entity object.
        /// </summary>
        /// <param name="entity">Entity object.</param>
        /// <returns>Generic data object.</returns>
        public T CreateObject<T>(EntityObject entity)
            where T : DataObject
        {
            if (!_isInited)
                throw new InvalidOperationException();

            return _fact.CreateObject<T>(entity);
        }

        public new int SaveChanges()
        {
            if (!_isInited)
                throw new InvalidOperationException();

            int result;
            try
            {
                result = base.SaveChanges();
                _NotifySaveChangesCompleted(true);
            }
            catch
            {
                _NotifySaveChangesCompleted(false);
                throw;
            }

            return result;
        }

        /// <summary>
        /// CapacitiesInfo.
        /// </summary>
        public CapacitiesInfo CapacitiesInfo
        {
            get
            {
                if (!_isInited)
                    throw new InvalidOperationException();

                return _objInitData.CapacitiesInfo;
            }
        }

        /// <summary>
        /// OrderCustomPropertiesInfo.
        /// </summary>
        public OrderCustomPropertiesInfo OrderCustomPropertiesInfo
        {
            get
            {
                if (!_isInited)
                    throw new InvalidOperationException();

                return _objInitData.OrderCustomPropertiesInfo;
            }
        }

        /// <summary>
        /// Updates order custom properties info in database.
        /// </summary>
        /// <param name="propertiesInfo">Order custom properrties info.</param>
        public void UpdateCustomOrderPropertiesInfo(OrderCustomPropertiesInfo propertiesInfo)
        {
            Debug.Assert(propertiesInfo != null);

            // Get custom order properties config scheme.
            DataModel.ConfigSchemes customOrderPropertiesScheme = _GetConfigSchemeObject(SCHEME_ID_ORDERPROPERTIES);

            // Update value of custom order properties database field.
            customOrderPropertiesScheme.Value =
                ConfigDataSerializer.SerializeOrderCustomPropertiesInfo(propertiesInfo);

            // Update init data.
            _objInitData.OrderCustomPropertiesInfo = propertiesInfo;

            // Save changes to the database.
            base.SaveChanges();
        }

        #endregion

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _Init()
        {
            Debug.Assert(!_isInited); // init once

            // check if project DB contains config schemes
            if (_IsConfigSchemeExist(SCHEME_ID_CAPACITIES) &&
                _IsConfigSchemeExist(SCHEME_ID_ORDERPROPERTIES))
            {
                // load schemes
                ObjectInitData initData = new ObjectInitData();
                initData.CapacitiesInfo = _LoadCapacitiesInfo();
                initData.OrderCustomPropertiesInfo = _LoadOrderPropertiesInfo();

                // create object factory
                _fact = new DataObjectFactory(initData);
                _objInitData = initData;

                // attach events
                this.SavingChanges += new EventHandler(DataObjectContext_SavingChanges);
                _isInited = true;
            }
        }

        private void DataObjectContext_SavingChanges(object sender, EventArgs e)
        {
            IEnumerable<ObjectStateEntry> entries = this.ObjectStateManager.GetObjectStateEntries(
                EntityState.Added |
                EntityState.Modified |
                EntityState.Deleted);

            List<DataObject> addedItems = new List<DataObject>();
            List<DataObject> modifiedItems = new List<DataObject>();
            List<DataObject> deletedItems = new List<DataObject>();

            foreach (ObjectStateEntry entry in entries)
            {
                if (!entry.IsRelationship &&
                    entry.EntitySet.ElementType is EntityType)
                {
                    if (entry.State == EntityState.Added ||
                        entry.State == EntityState.Modified)
                    {
                        // apply database constraints
                        _ApplyConstraints(entry);

                        // check whether object can be saved
                        _CheckCanSave(entry);

                        // fill event collections
                        if (entry.State != EntityState.Detached)
                        {
                            DataObject item = DataObjectHelper.GetDataObject(
                                entry.Entity as EntityObject);

                            if (item != null)
                            {
                                if (entry.State == EntityState.Added)
                                    addedItems.Add(item);
                                else if (entry.State == EntityState.Modified)
                                {
                                    if (_IsMarkedAsDeleted(item))
                                        deletedItems.Add(item);
                                    else
                                        modifiedItems.Add(item);
                                }
                            }
                        }
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        DataObject item = DataObjectHelper.GetDataObject(
                            entry.Entity as EntityObject);

                        if (item != null)
                            deletedItems.Add(item);
                    }
                }
            }

            _NotifySavingChanges(addedItems.AsReadOnly(),
                modifiedItems.AsReadOnly(),
                deletedItems.AsReadOnly());
        }

        private void _CheckCanSave(ObjectStateEntry entry)
        {
            Debug.Assert(entry != null);

            if (entry.State == EntityState.Added)
            {
                DataObject obj = DataObjectHelper.GetDataObject(entry.Entity as EntityObject);
                if (obj != null)
                {
                    if (!obj.CanSave)
                        this.Detach(entry.Entity);
                }
            }
        }

        private void _ApplyConstraints(ObjectStateEntry entry)
        {
            Debug.Assert(entry != null);

            EntityType entityType = (EntityType)entry.EntitySet.ElementType;
            foreach (EdmProperty prop in entityType.Properties)
            {
                if (prop.TypeUsage.EdmType.Name.Equals(EDM_TYPE_String))
                    _ApplyStringConstraints(entry.Entity as EntityObject, prop);
            }
        }

        private void _ApplyStringConstraints(EntityObject entity, EdmProperty prop)
        {
            _FixStrLength(entity, prop);
        }

        private void _FixStrLength(EntityObject entity, EdmProperty prop)
        {
            Debug.Assert(entity != null);
            Debug.Assert(prop != null);

            Facet facet = null;
            if (prop.TypeUsage.Facets.TryGetValue(FACET_MaxLength, true, out facet) &&
                facet.Value is int)
            {
                int maxLength = (int)facet.Value;
                if (maxLength > 0)
                {
                    PropertyInfo pi = entity.GetType().GetProperty(prop.Name);
                    if (pi != null)
                    {
                        object value = pi.GetValue(entity, null);
                        if (value != null && value is String)
                        {
                            string strValue = (string)value;
                            if (strValue.Length > maxLength)
                                pi.SetValue(entity, strValue.Remove(maxLength), null);
                        }
                    }
                }
            }
        }

        private void _NotifySaveChangesCompleted(bool isSucceeded)
        {
            if (SaveChangesCompleted != null)
                SaveChangesCompleted(this, new SaveChangesCompletedEventArgs(isSucceeded));
        }

        private void _NotifySavingChanges(IList<DataObject> addedItems,
            IList<DataObject> modifiedItems,
            IList<DataObject> deletedItems)
        {
            if (PostSavingChanges != null)
            {
                PostSavingChanges(this, new SavingChangesEventArgs(addedItems,
                    modifiedItems,
                    deletedItems));
            }
        }

        private CapacitiesInfo _LoadCapacitiesInfo()
        {
            CapacitiesInfo info = null;
            try
            {
                string xml = _GetConfigScheme(SCHEME_ID_CAPACITIES);
                info = ConfigDataSerializer.ParseCapacitiesInfo(xml);
            }
            catch (Exception e)
            {
                throw new DataException(String.Format(
                    Properties.Messages.Error_ConfigSchemeLoadFailed, SCHEME_ID_CAPACITIES), e);
            }

            return info;
        }

        private OrderCustomPropertiesInfo _LoadOrderPropertiesInfo()
        {
            OrderCustomPropertiesInfo info = null;
            try
            {
                string xml = _GetConfigScheme(SCHEME_ID_ORDERPROPERTIES);
                info = ConfigDataSerializer.ParseOrderCustomPropertiesInfo(xml);
            }
            catch (Exception e)
            {
                throw new DataException(String.Format(
                    Properties.Messages.Error_ConfigSchemeLoadFailed, SCHEME_ID_ORDERPROPERTIES), e);
            }

            return info;
        }

        private bool _IsConfigSchemeExist(Guid schemeId)
        {
            string entitySet = ContextHelper.GetFullEntitySetName(this,
                SCHEMES_ENTITY_SET);

            EntityKey key = new EntityKey(entitySet, SCHEMES_KEY,
                schemeId);
            
            object entity = null;
            return TryGetObjectByKey(key, out entity);
        }

        private string _GetConfigScheme(Guid schemeId)
        {
            string entitySet = ContextHelper.GetFullEntitySetName(this,
                SCHEMES_ENTITY_SET);

            EntityKey key = new EntityKey(entitySet, SCHEMES_KEY,
                schemeId);
            
            string schemeXML = null;
            object entity = null;
            if (TryGetObjectByKey(key, out entity))
            {
                DataModel.ConfigSchemes scheme = entity as DataModel.ConfigSchemes;
                if (scheme != null)
                    schemeXML = scheme.Value;
            }

            if (schemeXML == null)
                throw new DataException();

            return schemeXML;
        }

        /// <summary>
        /// Gets config schemes object with given ID.
        /// </summary>
        /// <param name="schemeId">ID of scheme.</param>
        /// <returns>ConfigSchemes object.</returns>
        private DataModel.ConfigSchemes _GetConfigSchemeObject(Guid schemeId)
        {
            // Get full entity scheme name.
            string entitySet = ContextHelper.GetFullEntitySetName(this,
                SCHEMES_ENTITY_SET);

            // Create entity key.
            EntityKey key = new EntityKey(entitySet, SCHEMES_KEY,
                schemeId);

            DataModel.ConfigSchemes scheme = null;
            object entity = null;

            // Try to get object by key.
            if (TryGetObjectByKey(key, out entity))
            {
                scheme = entity as DataModel.ConfigSchemes;
            }

            if (scheme == null)
                throw new DataException();

            return scheme;
        }

        private static bool _IsMarkedAsDeleted(DataObject obj)
        {
            bool isMarked = false;

            IMarkableAsDeleted mark = obj as IMarkableAsDeleted;
            if (mark != null)
                isMarked = mark.IsMarkedAsDeleted;

            return isMarked;
        }

        #endregion private methods

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ObjectInitData _objInitData;
        private DataObjectFactory _fact;
        private bool _isInited = false;

        #endregion private members
    }
}

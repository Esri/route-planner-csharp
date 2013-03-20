using System.Data.Objects.DataClasses;
using System.Collections.ObjectModel;
using System.Collections;

namespace ESRI.ArcLogistics.Data
{
    internal class DataObjectOwnerCollection<TDataObject, TEntity> : 
        DataObjectCollection<TDataObject, TEntity>
        where TDataObject : DataObject
        where TEntity : EntityObject
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of DataObjectOwnerCollection class.
        /// </summary>
        public DataObjectOwnerCollection(DataObjectContext context,
            string entitySetName, bool isReadOnly):base(context,entitySetName,isReadOnly)
        {
        }

        /// <summary>
        /// Creates a new instance of DataObjectOwnerCollection class.
        /// </summary>
        public DataObjectOwnerCollection(DataObjectContext context,
            string entitySetName,
            SpecFields specFields, bool isReadOnly):base(context,entitySetName,specFields,isReadOnly)
        {
        }

        /// <summary>
        /// Creates a new instance of DataObjectOwnerCollection class.
        /// </summary>
        public DataObjectOwnerCollection(DataService<TDataObject> dataService,
            bool isReadOnly):base(dataService,isReadOnly)
        {
        }
        #endregion constructors

        /// <summary>
        /// Add object to _dataObjects. If this obj has ParentCollectionProperty - link it to that
        /// collection and raise property changed event for all items with the same name.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        protected override void _AddToInternalCollection(TDataObject obj)
        {
            base.DataObjects.Add(obj);

            /// If this object has owner collection then link it to this collection.
            _SetOwnerCollectionRaiseNamePropertyChanged(base.DataObjects, obj);
        }

        /// <summary>
        /// Remove object from _dataObjects.If this obj has ParentCollectionProperty - clear it and
        /// raise property changed event for all items with the same name.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        protected override void _RemoveFromInternalCollection(TDataObject obj)
        {
            base.DataObjects.Remove(obj);

            /// If this object has owner collection then clear it.
            _SetOwnerCollectionRaiseNamePropertyChanged(null, obj);
        }

        /// <summary>
        /// Set object's onwercollection to collection and raise name property changed for all
        /// items in this collection which name is the same with object's name.
        /// </summary>
        /// <param name="collection">Owner collection.</param>
        /// <param name="obj">TDataObject which owner collection is set.</param>
        private void _SetOwnerCollectionRaiseNamePropertyChanged
            (ObservableCollection<TDataObject> collection, TDataObject obj)
        {
            if (obj is ISupportOwnerCollection)
            {
                (obj as ISupportOwnerCollection).OwnerCollection = collection;

                // If object has name - do validation for all object with the same name.
                if (obj is ISupportName && base.DataObjects != null)
                    DataObjectValidationHelper.RaisePropertyChangedForDuplicate(base.DataObjects,
                        (obj as ISupportName).Name);
            }
        }

    }
}

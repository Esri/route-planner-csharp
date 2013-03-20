using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Collection of DataObject, which have owners. When new item is added to collection,
    /// its OwnerCollection property is set to this collection and notify property changed raises
    /// for all collection member with the same name , when item is deleted, its
    /// OwnerCollection set to null and notify property changed raises as well.
    /// </summary>
    /// <typeparam name="TDataObject">Type of Data Object.</typeparam>
    internal class OwnerCollection<TDataObject> : ObservableCollection<TDataObject>
        where TDataObject : DataObject
    {
        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public OwnerCollection()
            : base()
        {
            CollectionChanged += new NotifyCollectionChangedEventHandler(_OwnerCollectionCollectionChanged);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// When collection change - set owner collection and raise name property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">NotifyCollectionChangedEventArgs.</param>
        private void _OwnerCollectionCollectionChanged(object sender, 
            NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach(TDataObject dataObject in e.NewItems)
                // If this object has OwnerCollectionProperty - link it to this collection
                // and raise property changed event for all items with the same name.
                    _SetOwnerCollectionRaiseNamePropertyChanged(this, dataObject);
            else if (e.Action == NotifyCollectionChangedAction.Remove)
                foreach (TDataObject dataObject in e.OldItems)
                //If this obj has OwnerCollectionProperty - clear it and
                // raise property changed event for all items with the same name.
                    _SetOwnerCollectionRaiseNamePropertyChanged(null, dataObject);
        } 
        #endregion

        #region Private methods
        /// <summary>
        /// Set object's owner collection to collection and raise name property changed for all
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
                if (obj is ISupportName)
                    DataObjectValidationHelper.RaisePropertyChangedForDuplicate(this,
                        (obj as ISupportName).Name);
            }
        } 
        #endregion
    }
}

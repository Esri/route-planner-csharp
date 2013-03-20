using System;
using System.Diagnostics;
using System.Collections.Generic;
//using System.Data.Objects.DataClasses;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Provides data for <c>SaveChangesCompleted</c> event.
    /// </summary>
    public class SaveChangesCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <c>SaveChangesCompletedEventArgs</c> class.
        /// </summary>
        public SaveChangesCompletedEventArgs(bool isSucceeded)
        {
            _isSucceeded = isSucceeded;
        }

        /// <summary>
        /// Indicates whether changes were successfully saved.
        /// </summary>
        public bool IsSucceeded
        {
            get { return _isSucceeded; }
        }

        private bool _isSucceeded;
    }

    /// <summary>
    /// Provides data for <c>SavingChanges</c> event.
    /// </summary>
    public class SavingChangesEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <c>SavingChangesEventArgs</c> class.
        /// </summary>
        public SavingChangesEventArgs(IList<DataObject> addedItems,
            IList<DataObject> modifiedItems,
            IList<DataObject> deletedItems)
        {
            _addedItems = addedItems;
            _modifiedItems = modifiedItems;
            _deletedItems = deletedItems;
        }

        /// <summary>
        /// Items added since last changes were saved.
        /// </summary>
        public IList<DataObject> AddedItems
        {
            get { return _addedItems; }
        }

        /// <summary>
        /// Items modified since last changes were saved.
        /// </summary>
        public IList<DataObject> ModifiedItems
        {
            get { return _modifiedItems; }
        }

        /// <summary>
        /// Items deleted since last changes were saved.
        /// </summary>
        public IList<DataObject> DeletedItems
        {
            get { return _deletedItems; }
        }

        private IList<DataObject> _addedItems;
        private IList<DataObject> _modifiedItems;
        private IList<DataObject> _deletedItems;
    }

    /// <summary>
    /// Represents the method that handles <c>SaveChangesCompleted</c> event. 
    /// </summary>
    public delegate void SaveChangesCompletedEventHandler(
        Object sender,
        SaveChangesCompletedEventArgs e
    );

    /// <summary>
    /// Represents the method that handles <c>SavingChanges</c> event. 
    /// </summary>
    public delegate void SavingChangesEventHandler(
        Object sender,
        SavingChangesEventArgs e
    );
}

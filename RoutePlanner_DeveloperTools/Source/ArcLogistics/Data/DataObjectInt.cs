using System.Collections;
using System.Data.Objects.DataClasses;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Supports indication either object is modified or not since last changes were saved.
    /// </summary>
    public interface IModifyState
    {
        #region properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a value indicating whether the object is modified since last changes were saved.
        /// </summary>
        bool IsModified
        {
            get;
        }

        #endregion properties
    }

    /// <summary>
    /// Provides the functionality to offer custom error information. 
    /// </summary>
    public interface IValidatable
    {
        #region properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a boolean value indicating whether the object is valid.
        /// </summary>
        bool IsValid
        {
            get;
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        string FullError
        {
            get;
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object. It returns errors only for those properties that affect routing.
        /// </summary>
        string PrimaryError
        {
            get;
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with specified property of object.
        /// </summary>
        /// <remarks>Default is an empty string.</remarks>
        string GetPropertyError(string propName);

        #endregion properties
    }

    /// <summary>
    /// IRawDataAccess interface.
    /// Provides access to internal data structures.
    /// </summary>
    internal interface IRawDataAccess
    {
        #region properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets underlying entity object of DataObject instance.
        /// </summary>
        EntityObject RawEntity
        {
            get;
        }

        #endregion properties
    }

    /// <summary>
    /// ICapacitiesInit interface.
    /// </summary>
    internal interface ICapacitiesInit
    {
        #region properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets CapacitiesInfo.
        /// </summary>
        CapacitiesInfo CapacitiesInfo
        {
            set;
        }

        #endregion properties
    }

    /// <summary>
    /// IOrderPropertiesInit interface.
    /// </summary>
    internal interface IOrderPropertiesInit
    {
        #region properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets OrderCustomPropertiesInfo.
        /// </summary>
        OrderCustomPropertiesInfo OrderCustomPropertiesInfo
        {
            set;
        }

        #endregion properties
    }

    /// <summary>
    /// IMarkableAsDeleted interface.
    /// </summary>
    internal interface IMarkableAsDeleted
    {
        #region properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets a value indicating whether data object is marked as deleted.
        /// </summary>
        bool IsMarkedAsDeleted
        {
            get;
            set;
        }

        #endregion properties
    }

    /// <summary>
    /// IWrapDataAccess interface.
    /// Provides access to entity's wrapping data object.
    /// </summary>
    internal interface IWrapDataAccess
    {
        #region properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets and sets entity's wrapping data object.
        /// </summary>
        DataObject DataObject
        {
            get;
            set;
        }

        #endregion properties
    }

    /// <summary>
    /// Supports copying of data object properties to another data object.
    /// </summary>
    public interface ICopyable
    {
        #region properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Copies all the object's data to the target data object.
        /// </summary>
        /// <param name="obj">Target data object.</param>
        void CopyTo(DataObject obj);

        #endregion properties
    }

    /// <summary>
    /// Supports raising of property changed event for selected property of DataObject.
    /// </summary>
    internal interface IForceNotifyPropertyChanged 
    {
        #region Methods

        /// <summary>
        /// Call property changed event for selected property.
        /// </summary>
        /// <param name="propertyName">Property for which event is called.</param>
        void RaisePropertyChangedEvent(string propertyName);

        #endregion
    }

    /// <summary>
    /// Support internal collection in which this item is placed.
    /// </summary>
    internal interface ISupportOwnerCollection 
    {
        #region Properties

        /// <summary>
        /// Collection in which this DataObject is placed.
        /// </summary>
        IEnumerable OwnerCollection
        {
            get;
            set;
        }

        #endregion
    }

    /// <summary>
    /// Interface for Property 'Name' with get accessor.
    /// </summary>
    public interface ISupportName 
    {
        #region Properties

        /// <summary>
        /// 'Name' property.
        /// </summary>
        string Name 
        { get; }
        #endregion
    }
}

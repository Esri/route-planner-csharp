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

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using ESRI.ArcLogistics.DomainObjects.Validation;
using DataModel = ESRI.ArcLogistics.Data.DataModel;
using System.Collections;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents a mobile device.
    /// </summary>
    /// <remarks>
    /// It can be a personal navigation device carried by driver or navigation system integrated directly into vehicle.
    /// </remarks>
    public class MobileDevice : DataObject, IMarkableAsDeleted, ISupportOwnerCollection
    {
        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the Name property.
        /// </summary>
        public static string PropertyNameName
        {
            get { return PROP_NAME_Name; }
        }

        /// <summary>
        /// Name of the ActiveSyncProfileName property.
        /// </summary>
        public static string PropertyNameActiveSyncProfileName
        {
            get { return PROP_NAME_ActiveSyncProfileName; }
        }

        /// <summary>
        /// Name of the SyncFolder property.
        /// </summary>
        public static string PropertyNameSyncFolder
        {
            get { return PROP_NAME_SyncFolder; }
        }

        /// <summary>
        /// Name of the SyncType property.
        /// </summary>
        public static string PropertyNameSyncType
        {
            get { return PROP_NAME_SyncType; }
        }

        /// <summary>
        /// Name of the EmailAddress property
        /// </summary>
        public static string PropertyNameEmailAddress
        {
            get { return PROP_NAME_EmailAddress; }
        }

        // APIREV: make private
        public const string PARTNERS_KEY_PATH = "Software\\Microsoft\\Windows CE Services\\Partners";
        public const string PARTNERS_DISPLAYNAME = "DisplayName";
        public const string PARTNER_SYNC_PATH = "Services\\Synchronization";
        public const string BRIEFCASEPATH = "Briefcase Path";

        #endregion // Constants

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>MobileDevice</c> class.
        /// </summary>
        public MobileDevice()
            : base(DataModel.MobileDevices.CreateMobileDevices(Guid.NewGuid()))
        {
            base.SetCreationTime();
        }

        internal MobileDevice(DataModel.MobileDevices entity)
            : base(entity)
        {
            Debug.Assert(0 < entity.CreationTime); // NOTE: must be inited
        }

        #endregion constructors

        #region public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the object's type title.
        /// </summary>
        public override string TypeTitle
        {
            get { return Properties.Resources.MobileDevice; }
        }
        
        /// <summary>
        /// Gets the object's globally unique identifier.
        /// </summary>
        public override Guid Id
        {
            get { return _Entity.Id; }
        }

        /// <summary>
        /// Gets\sets object creation time.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException">Although property can get null value
        /// (for backward compatibility with existent plug-ins) it is not actually supported.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Although property can get 0 or less value
        /// (for backward compatibility with existent plug-ins) it is not actually supported.</exception>
        public override long? CreationTime
        {
            get
            {
                Debug.Assert(0 < _Entity.CreationTime); // NOTE: must be inited
                return _Entity.CreationTime;
            }
            set
            {
                if (!value.HasValue)
                    throw new ArgumentNullException(); // exception
                if (value.Value <= 0)
                    throw new ArgumentOutOfRangeException(); // exception

                _Entity.CreationTime = value.Value;
            }
        }

        /// <summary>
        /// Mobile device name.
        /// </summary>
        [DomainProperty("DomainPropertyNameName", true)]
        [DuplicateNameValidator]
        [NameNotNullValidator]
        public override string Name
        {
            get { return _Entity.Name; }
            set
            {
                // Save current name.
                var name = _Entity.Name;

                // Set new name.
                _Entity.Name = value;

                // Raise Property changed event for all items which 
                // has the same name, as item's old name.
                if ((this as ISupportOwnerCollection).OwnerCollection != null)
                    DataObjectValidationHelper.RaisePropertyChangedForDuplicate
                        ((this as ISupportOwnerCollection).OwnerCollection, name);

                NotifyPropertyChanged(PROP_NAME_Name);
            }
        }

        /// <summary>
        /// Active sync profile name used for syncronization with the mobile device.
        /// </summary>
        [MobileDevicePropertyValidator]
        [DomainProperty("DomainPropertyNameActiveSyncProfileName")]
        public string ActiveSyncProfileName
        {
            get { return _Entity.ActiveSyncProfileName; }
            set
            {
                _Entity.ActiveSyncProfileName = value;
                NotifyPropertyChanged(PROP_NAME_ActiveSyncProfileName);
            }
        }

        /// <summary>
        /// E-mail address of the mobile device.
        /// </summary>
        [MobileDevicePropertyValidator]
        [DomainProperty("DomainPropertyNameEmailAddress")]
        public string EmailAddress
        {
            get { return _Entity.EmailAddress; }
            set
            {
                _Entity.EmailAddress = value;
                NotifyPropertyChanged(PROP_NAME_EmailAddress);
            }
        }

        /// <summary>
        /// Folder that is used for syncronization with the mobile device.
        /// </summary>
        [MobileDevicePropertyValidator]
        [DomainProperty("DomainPropertyNameSyncFolder")]
        public string SyncFolder
        {
            get { return _Entity.SyncFolder; }
            set
            {
                _Entity.SyncFolder = value;
                NotifyPropertyChanged(PROP_NAME_SyncFolder);
            }
        }

        /// <summary>
        /// Tracking Id associated with the mobile device.
        /// </summary>
        public string TrackingId
        {
            get { return Name; }
            set
            {
                Name = value;
            }
        }

        /// <summary>
        /// Mobile device syncronization type.
        /// </summary>
        [DomainProperty("DomainPropertyNameSyncType")]
        public SyncType SyncType
        {
            get { return (SyncType)_Entity.SyncType; }
            set
            {
                _Entity.SyncType = (int)value;
                NotifyPropertyChanged(PROP_NAME_SyncType);
            }
        }

        #endregion public members

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Returns the name of the mobile device.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion public methods

        #region ICloneable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            MobileDevice obj = new MobileDevice();
            this.CopyTo(obj);

            return obj;
        }

        #endregion ICloneable interface members

        #region ICopyable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Copies all the object's data to the target data object.
        /// </summary>
        /// <param name="obj">Target data object.</param>
        public override void CopyTo(DataObject obj)
        {
            MobileDevice mobileDevice = obj as MobileDevice;
            mobileDevice.Name = this.Name;
            mobileDevice.ActiveSyncProfileName = this.ActiveSyncProfileName;
            mobileDevice.EmailAddress = this.EmailAddress;
            mobileDevice.SyncFolder = this.SyncFolder;
            mobileDevice.SyncType = this.SyncType;
        }

        #endregion ICopyable interface members

        #region IMarkableAsDeleted interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets a value indicating whether data object is marked as deleted.
        /// </summary>
        bool IMarkableAsDeleted.IsMarkedAsDeleted
        {
            get { return _Entity.Deleted; }
            set { _Entity.Deleted = value; }
        }

        #endregion IMarkableAsDeleted interface members

        #region ISupportOwnerCollection Members

        /// <summary>
        /// Collection in which this DataObject is placed.
        /// </summary>
        IEnumerable ISupportOwnerCollection.OwnerCollection
        {
            get;
            set;
        }

        #endregion

        #region private constants

        /// <summary>
        /// Name of the Name property.
        /// </summary>
        private const string PROP_NAME_Name = "Name";

        /// <summary>
        /// Name of the ActiveSyncProfileName property.
        /// </summary>
        private const string PROP_NAME_ActiveSyncProfileName = "ActiveSyncProfileName";

        /// <summary>
        /// Name of the EmailAddress property.
        /// </summary>
        private const string PROP_NAME_EmailAddress = "EmailAddress";

        /// <summary>
        /// Name of the SyncFolder property.
        /// </summary>
        private const string PROP_NAME_SyncFolder = "SyncFolder";

        /// <summary>
        /// Name of the SyncType property.
        /// </summary>
        private const string PROP_NAME_SyncType = "SyncType";

        #endregion

        #region private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DataModel.MobileDevices _Entity
        {
            get
            {
                return (DataModel.MobileDevices)base.RawEntity;
            }
        }

        #endregion private properties
    }
}

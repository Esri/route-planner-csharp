using System;
using System.Diagnostics;
using System.Data.Objects.DataClasses;
using ESRI.ArcLogistics.Data;
using DataModel = ESRI.ArcLogistics.Data.DataModel;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using ESRI.ArcLogistics.DomainObjects.Validation;
using System.Collections;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents vehicle specialty.
    /// </summary>
    /// <remarks>
    /// Typically vehicle specialty means some vehicle's capability. For example, vehicle specialty may be that it has a refrigerated trailer.
    /// </remarks>
    public class VehicleSpecialty : DataObject, IMarkableAsDeleted,
        ISupportOwnerCollection
    {
        #region public static properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the Name property.
        /// </summary>
        public static string PropertyNameName
        {
            get { return PROP_NAME_Name;}
        }

        /// <summary>
        /// Name of the Comment property.
        /// </summary>
        public static string PropertyNameComment
        {
            get { return PROP_NAME_Comment;}
        }


        #endregion // Constants

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>VehicleSpecialty</c> class.
        /// </summary>
        public VehicleSpecialty()
            : base(DataModel.VehicleSpecialties.CreateVehicleSpecialties(Guid.NewGuid()))
        {
            base.SetCreationTime();
        }

        internal VehicleSpecialty(DataModel.VehicleSpecialties entity)
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
            get { return Properties.Resources.VehicleSpecialty; }
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
        /// Vehicle specialty name.
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
                    DataObjectValidationHelper.RaisePropertyChangedForDuplicate((this as ISupportOwnerCollection).OwnerCollection, name);

                NotifyPropertyChanged(PROP_NAME_Name);
            }
        }

        /// <summary>
        /// Arbitrary text about the vehicle specialty.
        /// </summary>
        [DomainProperty("DomainPropertyNameComment")]
        public string Comment
        {
            get { return _Entity.Comment; }
            set
            {
                _Entity.Comment = value;
                NotifyPropertyChanged(PROP_NAME_Comment);
            }
        }

        #endregion public members

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Returns the name of the vehicle specialty.
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
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            VehicleSpecialty obj = new VehicleSpecialty();
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
            Debug.Assert(obj is VehicleSpecialty);

            VehicleSpecialty spec = obj as VehicleSpecialty;
            spec.Name = this.Name;
            spec.Comment = this.Comment;
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

        #region private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DataModel.VehicleSpecialties _Entity
        {
            get
            {
                return (DataModel.VehicleSpecialties)base.RawEntity;
            }
        }

        #endregion private properties

        #region private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the Name property.
        /// </summary>
        private const string PROP_NAME_Name = "Name";

        /// <summary>
        /// Name of the Comment property.
        /// </summary>
        private const string PROP_NAME_Comment = "Comment";

        #endregion // Constants
    }
}

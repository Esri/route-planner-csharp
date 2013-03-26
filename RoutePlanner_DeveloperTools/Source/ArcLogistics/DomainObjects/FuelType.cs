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

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using DataModel = ESRI.ArcLogistics.Data.DataModel;
using System.Collections;
using ESRI.ArcLogistics.DomainObjects.Validation;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents a fuel type.
    /// </summary>
    public class FuelType : DataObject, IMarkableAsDeleted, ISupportOwnerCollection
    {
        #region Constants

        /// <summary>
        /// Name of the Name property.
        /// </summary>
        public static string PropertyNameName
        {
            get { return PROP_NAME_Name; }
        }

        /// <summary>
        /// Name of Price property.
        /// </summary>
        public static string PropertyNamePrice
        {
            get { return PROP_NAME_Price; }
        }

        /// <summary>
        /// Name of Co2Emission property.
        /// </summary>
        public static string PropertyNameCo2Emission
        {
            get { return PROP_NAME_Co2Emission; }
        }


        #endregion // Constants

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>FuelType</c> class.
        /// </summary>
        public FuelType()
            : base(DataModel.FuelTypes.CreateFuelTypes(Guid.NewGuid()))
        {
            base.SetCreationTime();
        }

        internal FuelType(DataModel.FuelTypes entity)
            : base(entity)
        {
            Debug.Assert(0 < entity.CreationTime); // NOTE: must be inited
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            FuelType obj = new FuelType();

            obj.Name = this.Name;
            obj.Price = this.Price;
            obj.Co2Emission = this.Co2Emission;

            return obj;
        }

        #endregion

        #region ISupportParentCollection Members

        /// <summary>
        /// Collection in which this DataObject is placed.
        /// </summary>
        IEnumerable ISupportOwnerCollection.OwnerCollection
        {
            get;
            set;
        }

        #endregion

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Returns the name of the FuelType.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion public methods

        #region public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the object's type title.
        /// </summary>
        public override string TypeTitle
        {
            get { return Properties.Resources.FuelType; }
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

        #endregion

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

        #region private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DataModel.FuelTypes _Entity
        {
            get
            {
                return (DataModel.FuelTypes)base.RawEntity;
            }
        }

        #endregion private properties

        #region public properties

        /// <summary>
        /// Fuel type name.
        /// </summary>
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
        /// Fuel price.
        /// </summary>
        [RangeValidator(0.0, RangeBoundaryType.Inclusive, 0.0, RangeBoundaryType.Ignore,
            MessageTemplateResourceName = "Error_InvalidFuelPrice",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [UnitPropertyAttribute(Unit.CurrencyPerGallon, Unit.CurrencyPerGallon, Unit.CurrencyPerLiter)]
        [AffectsRoutingProperty]
        public double Price
        {
            get { return _Entity.Price; }
            set
            {
                _Entity.Price = value;
                NotifyPropertyChanged(PROP_NAME_Price);
            }
        }

        /// <summary>
        /// CO2 emission in pounds per gallon.
        /// </summary>
        [RangeValidator(0.0, RangeBoundaryType.Inclusive, 0.0, RangeBoundaryType.Ignore,
            MessageTemplateResourceName = "Error_InvalidCo2Emission",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages))]
        [UnitPropertyAttribute(Unit.PoundPerGallon, Unit.PoundPerGallon, Unit.KilogramPerLiter)]
        public double Co2Emission
        {
            get { return _Entity.Co2Emission; }
            set
            {
                _Entity.Co2Emission = value;
                NotifyPropertyChanged(PROP_NAME_Co2Emission);
            }
        }

        #endregion

        #region private constants

        /// <summary>
        /// Name of Name property.
        /// </summary>
        private const string PROP_NAME_Name = "Name";

        /// <summary>
        /// Name of Price property.
        /// </summary>
        private const string PROP_NAME_Price = "Price";

        /// <summary>
        /// Name of Co2Emission property.
        /// </summary>
        private const string PROP_NAME_Co2Emission = "Co2Emission";

        #endregion
    }
}

using System;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using DataModel = ESRI.ArcLogistics.Data.DataModel;
using System.Collections;
using ESRI.ArcLogistics.DomainObjects.Validation;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents a zone.
    /// </summary>
    public class Zone : DataObject, IMarkableAsDeleted, ISupportOwnerCollection
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>Zone</c> class.
        /// </summary>
        public Zone()
            : base(DataModel.Zones.CreateZones(Guid.NewGuid()))
        {
            base.SetCreationTime();
        }

        internal Zone(DataModel.Zones entity) : base(entity)
        {
        }
        #endregion // Constructors

        #region Public static properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets name of the Name property.
        /// </summary>
        public static string PropertyNameName
        {
            get { return PROP_NAME_NAME; }
        }

        /// <summary>
        /// Gets name of the Comment property.
        /// </summary>
        public static string PropertyNameComment
        {
            get { return PROP_NAME_COMMENT; }
        }

        /// <summary>
        /// Gets name of the Geometry property.
        /// </summary>
        public static string PropertyNameGeometry
        {
            get { return PROP_NAME_GEOMETRY; }
        }

        #endregion // Public static properties

        #region Public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the object's type title.
        /// </summary>
        public override string TypeTitle
        {
            get { return Properties.Resources.Zone; }
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
        /// Zone name.
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

                NotifyPropertyChanged(PROP_NAME_NAME);
            }
        }

        /// <summary>
        /// Arbitrary text about zone.
        /// </summary>
        /// <remarks>Validation not need</remarks>
        [DomainProperty("DomainPropertyNameComment")]
        public string Comment
        {
            get { return _Entity.Comment; }
            set
            {
                _Entity.Comment = value;
                NotifyPropertyChanged(PROP_NAME_COMMENT);
            }
        }

        /// <summary>
        /// Zone shape.
        /// </summary>
        [NotNullValidator(MessageTemplateResourceName = "Error_InvalidGeometryZone",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [AffectsRoutingProperty]
        public object Geometry
        {
            get
            {
                if ((null == _geometry) && (null != _Entity.Geometry))
                {
                    Polygon polygon = new Polygon(_Entity.Geometry);
                    if (1 == polygon.TotalPointCount)
                    {
                        ESRI.ArcLogistics.Geometry.Point[] points = polygon.GetPoints(0, 1);
                        _geometry = new Point(points[0].X, points[0].Y);
                    }
                    else
                        _geometry = polygon;
                }

                return _geometry;
            }

            set
            {
                if (null == value)
                {
                    _Entity.Geometry = null;
                    _geometry = null;
                }
                else
                {
                    if (value is Polygon)
                    {
                        _Entity.Geometry = (value as Polygon).ToByteArray();
                        if (null != _Entity.Geometry)
                            _geometry = new Polygon(_Entity.Geometry);
                    }
                    else if (value is Point)
                    {
                        Point? pt = value as Point?;

                        Polygon polygon = new Polygon(new Point[] { pt.Value });
                        _Entity.Geometry = polygon.ToByteArray();

                        _geometry = new Point(pt.Value.X, pt.Value.Y);
                    }
                    else
                        throw new NotSupportedException();
                }

                NotifyPropertyChanged(PROP_NAME_GEOMETRY);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the name of the zone.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion // Public members

        #region ICloneable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            Zone obj = new Zone();
            this.CopyTo(obj);

            return obj;
        }
        #endregion // ICloneable members

        #region ICopyable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Copies all the object's data to the target data object.
        /// </summary>
        /// <param name="obj">Target data object.</param>
        public override void CopyTo(DataObject obj)
        {
            System.Diagnostics.Debug.Assert(obj is Zone);

            Zone zone = obj as Zone;
            zone.Name = this.Name;
            zone.Comment = this.Comment;

            if (null != this.Geometry)
            {
                if (this.Geometry is Polygon)
                    zone.Geometry = (this.Geometry as Polygon).Clone();
                else if (this.Geometry is Point)
                {
                    Point? pt = this.Geometry as Point?;
                    zone.Geometry = new Point(pt.Value.X, pt.Value.Y);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false); // NOTE: not supported
                }
            }
        }

        #endregion ICopyable interface members

        #region IMarkableAsDeleted interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        bool IMarkableAsDeleted.IsMarkedAsDeleted
        {
            get { return _Entity.Deleted; }
            set { _Entity.Deleted = value; }
        }

        #endregion // IMarkableAsDeleted interface members

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

        #region Private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DataModel.Zones _Entity
        {
            get { return (base.RawEntity as DataModel.Zones); }
        }
        #endregion // Private properties

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the Name property.
        /// </summary>
        private const string PROP_NAME_NAME = "Name";

        /// <summary>
        /// Name of the Comment property.
        /// </summary>
        private const string PROP_NAME_COMMENT = "Comment";

        /// <summary>
        /// Name of the Geometry property.
        /// </summary>
        private const string PROP_NAME_GEOMETRY = "Geometry";

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private object _geometry = null;
        #endregion // Private members
    }
}

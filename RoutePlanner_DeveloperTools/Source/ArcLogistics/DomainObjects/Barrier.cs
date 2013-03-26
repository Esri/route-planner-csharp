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
using System.ComponentModel;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using ESRI.ArcLogistics.Data;
using AppGeometry = ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.DomainObjects.Validation;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using DataModel = ESRI.ArcLogistics.Data.DataModel;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents a barrier.
    /// </summary>
    /// <remarks>
    /// Barriers are blockages to street segments that prohibit vehicles from traveling on those
    /// streets. Barriers can represent many types of blockages and can have a date range assigned
    /// to them.
    /// </remarks>
    public class Barrier : DataObject
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new instance of the <c>Barrier</c> class.
        /// </summary>
        public Barrier(DateTime startDate, DateTime endTime)
            : base(DataModel.Barriers.CreateBarriers(Guid.NewGuid(), startDate, endTime))
        {
            base.SetCreationTime();

            _barrierEffect = new BarrierEffect();
            _UpdateBarrierEffectEntityData();

            _barrierEffect.PropertyChanged +=
                new PropertyChangedEventHandler(_BarrierEffect_PropertyChanged);
        }

        /// <summary>
        /// Create and initializes a instance of the <c>Barrier</c> class.
        /// </summary>
        /// <param name="entity">Entity for initialization.</param>
        internal Barrier(DataModel.Barriers entity)
            : base(entity)
        {
            Debug.Assert(0 < entity.CreationTime); // NOTE: must be inited

            _barrierEffect = BarrierEffect.CreateFromDBString(entity.BarrierType);

            _barrierEffect.PropertyChanged +=
                new PropertyChangedEventHandler(_BarrierEffect_PropertyChanged);
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
        /// Gets name of the Name property.
        /// </summary>
        public static string PropertyNameStartDate
        {
            get { return PROP_NAME_STARTDATE; }
        }

        /// <summary>
        /// Gets name of the Name property.
        /// </summary>
        public static string PropertyNameFinishDate
        {
            get { return PROP_NAME_FINISHDATE; }
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

        /// <summary>
        /// Gets name of the BarrierEffect property.
        /// </summary>
        public static string PropertyNameBarrierEffect
        {
            get { return PROP_NAME_BarrierEffect; }
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
            get { return Properties.Resources.Barrier; }
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
        /// Barrier's name.
        /// </summary>
        [DomainProperty("DomainPropertyNameName", true)]
        public override string Name
        {
            get { return _Entity.Name; }
            set
            {
                _Entity.Name = value;
                NotifyPropertyChanged(PROP_NAME_NAME);
            }
        }

        /// <summary>
        /// Start date for the period when this barrier is active.
        /// </summary>
        [DataTimeNullableValidator(Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameStartDate")]
        [AffectsRoutingProperty]
        public DateTime StartDate
        {
            get { return _Entity.StartDate; }
            set
            {
                _Entity.StartDate = value;
                NotifyPropertyChanged(PROP_NAME_STARTDATE);
            }
        }

        /// <summary>
        /// Finish date for the period when this barrier is active.
        /// </summary>
        [BarrierFinishDateValidator]
        [DomainProperty("DomainPropertyNameFinishDate")]
        [AffectsRoutingProperty]
        public DateTime FinishDate
        {
            get { return _Entity.FinishDate; }
            set
            {
                _Entity.FinishDate = value;
                NotifyPropertyChanged(PROP_NAME_FINISHDATE);
            }
        }

        /// <summary>
        /// Arbitrary text about the barrier.
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
        /// Barrier's shape.
        /// </summary>
        [NotNullValidator(MessageTemplateResourceName = "Error_InvalidGeometryBarrier",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [AffectsRoutingProperty]
        public object Geometry
        {
            get
            {
                if ((null == _geometry) && (null != _Entity.Geometry))
                {
                    var polyline = new AppGeometry.Polyline(_Entity.Geometry);
                    AppGeometry.Point[] points = polyline.GetPoints(0, polyline.TotalPointCount);

                    if (1 == points.Length)
                        // convert to point
                        _geometry = points[0];

                    else
                    {
                        AppGeometry.Point ptFirst = points[0];
                        AppGeometry.Point ptLast = points[points.Length - 1];
                        if (ptFirst == ptLast)
                            // convert to polygon
                            _geometry = new AppGeometry.Polygon(_Entity.Geometry);
                        else
                            // it is polyline
                            _geometry = polyline;
                    }
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
                    var polygon = value as AppGeometry.Polygon;
                    if (null != polygon)
                    {
                        _Entity.Geometry = polygon.ToByteArray();
                        if (null != _Entity.Geometry)
                            _geometry = new AppGeometry.Polygon(_Entity.Geometry);
                    }
                    else
                    {
                        var polyline = value as AppGeometry.Polyline;
                        if (null != polyline)
                        {
                            _Entity.Geometry = polyline.ToByteArray();
                            if (null != _Entity.Geometry)
                                _geometry = new AppGeometry.Polyline(_Entity.Geometry);
                        }
                        else
                        {
                            var point = value as AppGeometry.Point?;
                            if (null != point)
                            {
                                var points = new AppGeometry.Point[] { point.Value };
                                var plgn = new AppGeometry.Polygon(points);
                                _Entity.Geometry = plgn.ToByteArray();

                                _geometry = point.Value;
                            }
                            else
                                throw new NotSupportedException(); // exception
                        }
                    }
                }

                NotifyPropertyChanged(PROP_NAME_GEOMETRY);
            }
        }

        /// <summary>
        /// Barrier's type.
        /// </summary>
        [NotNullValidator(MessageTemplateResourceName = "Error_InvalidBarrierEffect",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [BarrierEffectValidator]
        [AffectsRoutingProperty]
        [DomainProperty("DomainPropertyNameBarrierEffect")]
        public BarrierEffect BarrierEffect
        {
            get { return _barrierEffect; }
            set
            {
                if (_barrierEffect != null)
                {
                    _barrierEffect.PropertyChanged -= _BarrierEffect_PropertyChanged;
                }

                _barrierEffect = value;
                if (null == value)
                    _Entity.BarrierType = null;
                else
                {
                    _UpdateBarrierEffectEntityData();

                    _barrierEffect.PropertyChanged +=
                        new PropertyChangedEventHandler(_BarrierEffect_PropertyChanged);
                }

                NotifyPropertyChanged(PROP_NAME_BarrierEffect);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the barrier's name.
        /// </summary>
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
        public override object Clone()
        {
            Barrier obj = new Barrier(this.StartDate, this.FinishDate);
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
            Debug.Assert(obj is Barrier);

            var barrier = obj as Barrier;
            barrier.Name = this.Name;
            barrier.Comment = this.Comment;

            barrier.Name = this.Name;
            barrier.StartDate = this.StartDate;
            barrier.FinishDate = this.FinishDate;
            barrier.Comment = this.Comment;

            if (null != this.Geometry)
            {
                var polygon = this.Geometry as AppGeometry.Polygon;
                if (null != polygon)
                    barrier.Geometry = polygon.Clone();
                else
                {
                    var polyline = this.Geometry as AppGeometry.Polyline;
                    if (null != polyline)
                        barrier.Geometry = polyline.Clone();
                    else
                    {
                        var point = this.Geometry as AppGeometry.Point?;
                        if (null != point)
                            barrier.Geometry = point.Value;
                        else
                        {
                            Debug.Assert(false); // NOTE: not supported
                        }
                    }
                }
            }

            if (null != this.BarrierEffect)
                barrier.BarrierEffect = this.BarrierEffect.Clone() as BarrierEffect;
        }

        #endregion // ICopyable interface members

        #region Private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Related entity.
        /// </summary>
        private DataModel.Barriers _Entity
        {
            get { return (base.RawEntity as DataModel.Barriers); }
        }

        #endregion // Private properties

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Notifies about change of the barrier type property.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event arguments.</param>
        private void _BarrierEffect_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _UpdateBarrierEffectEntityData();
            NotifySubPropertyChanged(PROP_NAME_BarrierEffect, e.PropertyName);
        }

        /// <summary>
        /// Updates barrier type to enty data.
        /// </summary>
        private void _UpdateBarrierEffectEntityData()
        {
            _Entity.BarrierType = BarrierEffect.AssemblyDBString(_barrierEffect);
        }

        #endregion Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the Name property.
        /// </summary>
        private const string PROP_NAME_NAME = "Name";

        /// <summary>
        /// Name of the StartDate property.
        /// </summary>
        private const string PROP_NAME_STARTDATE = "StartDate";

        /// <summary>
        /// Name of the FinishDate property.
        /// </summary>
        private const string PROP_NAME_FINISHDATE = "FinishDate";

        /// <summary>
        /// Name of the Comment property.
        /// </summary>
        private const string PROP_NAME_COMMENT = "Comment";

        /// <summary>
        /// Name of the Geometry property.
        /// </summary>
        private const string PROP_NAME_GEOMETRY = "Geometry";

        /// <summary>
        /// Name of the BarrierEffect property.
        /// </summary>
        private const string PROP_NAME_BarrierEffect = "BarrierEffect";

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Barrier's geometry.
        /// </summary>
        private object _geometry;
        /// <summary>
        /// Barrier's type.
        /// </summary>
        private BarrierEffect _barrierEffect;

        #endregion // Private members
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.App.OrderSymbology
{
    /// <summary>
    /// Order quantity symbology record
    /// </summary>
    [DataContract]
    internal class OrderQuantity: SymbologyRecord
    {
        #region Constants

        public const string PROP_NAME_MinValue = "MinValue";
        public const string PROP_NAME_MaxValue = "MaxValue";

        #endregion

        #region constructors

        public OrderQuantity()
            : base()
        {
        }

        public OrderQuantity(bool defaultValue)
            :base()
        {
            DefaultValue = defaultValue;
        }

        #endregion

        #region public members

        /// <summary>
        /// Min value of quantity
        /// </summary>
        [DataMember]
        public double MinValue
        {
            get { return _minValue; }
            set
            {
                _minValue = value;
                _NotifyPropertyChanged(PROP_NAME_MinValue);
            }
        }

        /// <summary>
        /// Max value of quantity
        /// </summary>
        [DataMember]
        public double MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                _NotifyPropertyChanged(PROP_NAME_MaxValue);
            }
        }

        #endregion

        #region private members

        private double _minValue;
        private double _maxValue;

        #endregion
    }
}

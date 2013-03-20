using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.IO;
using System.Windows;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.App.OrderSymbology
{
    /// <summary>
    /// Order category symbology record
    /// </summary>
    [DataContract]
    internal class OrderCategory : SymbologyRecord
    {
        #region Constants

        public const string PROP_NAME_Value = "Value";

        #endregion

        #region constructors

        public OrderCategory()
            :base()
        {
            Value = "";
        }

        public OrderCategory(bool defaultValue)
            :base()
        {
            DefaultValue = defaultValue;
            Value = "";
        }

        #endregion

        #region public members

        /// <summary>
        /// Category value
        /// </summary>
        [DataMember]
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _NotifyPropertyChanged(PROP_NAME_Value);
            }
        }

        #endregion

        #region private members

        private string _value;

        #endregion
    }
}

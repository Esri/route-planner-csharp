using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.App.OrderSymbology
{
    /// <summary>
    /// SymbologyConfig class.
    /// </summary>
    [DataContract]
    internal class SymbologyConfig 
    {
        [DataMember]
        public SymbologyType OrderSymbologyType
        {
            get;
            set;
        }

        [DataMember]
        public string CategoryOrderField
        {
            get;
            set;
        }

        [DataMember]
        public string QuantityOrderField
        {
            get;
            set;
        }

        [DataMember]
        public ObservableCollection<OrderCategory> OrderCategories
        {
            get;
            set;
        }

        [DataMember]
        public ObservableCollection<OrderQuantity> OrderQuantities
        {
            get;
            set;
        }
    }
}

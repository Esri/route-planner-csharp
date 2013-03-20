using System;
using System.Collections;
using System.Collections.Generic;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Type of a custom order property.
    /// </summary>
    public enum OrderCustomPropertyType
    {
        Text,
        Numeric
    }

    /// <summary>
    /// OrderCustomProperiy class contains information about a custom order property used in the current project.
    /// </summary>
    /// <remarks>Read only class.</remarks>
    public class OrderCustomProperty : ICloneable
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new instance of the <c>OrderCustomProperty</c> class.
        /// </summary>
        public OrderCustomProperty(string name, OrderCustomPropertyType type, int length,
                                   string description, Boolean orderPairKey)
        {
            _name = name;
            _type = type;
            _length = ((OrderCustomPropertyType.Numeric == type) && (length <= 0)) ?
                        NUMERIC_DEFAULT_LENGTH : length;
            _description = description;
            _orderPairKey = orderPairKey;
        }

        #endregion // Constructors

        #region Public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the custom order property.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Type of the custom order property.
        /// </summary>
        public OrderCustomPropertyType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Max length of the custom order property.
        /// </summary>
        /// <remarks>Useful only for a custom order property of type Text.</remarks>
        public int Length
        {
            get { return _length; }
        }

        /// <summary>
        /// Description of the custom order property.
        /// </summary>
        public string Description
        {
            get { return _description; }
        }

        /// <summary>
        /// Custom order property is used as the key for pairing orders.
        /// </summary>
        public bool OrderPairKey
        {
            get { return _orderPairKey; }
        }

        #endregion // Public members

        #region ICloneable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clones the custom order property.
        /// </summary>
        public object Clone()
        {
            return new OrderCustomProperty(this.Name, this.Type, this.Length, this.Description, this.OrderPairKey);
        }

        #endregion // ICloneable members

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private readonly OrderCustomPropertyType _type = OrderCustomPropertyType.Text;
        private readonly string _description;
        private readonly string _name;
        private readonly int _length;
        private readonly bool _orderPairKey;

        private const int NUMERIC_DEFAULT_LENGTH = 10;

        #endregion // Private members
    }
}

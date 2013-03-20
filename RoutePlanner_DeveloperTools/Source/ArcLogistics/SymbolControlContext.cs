using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Symbol control context class
    /// </summary>
    internal class SymbolControlContext
    {
        #region constants

        public const string FILL_ATTRIBUTE_NAME = "Fill";
        public const string SEQUENCE_NUMBER_ATTRIBUTE_NAME = "SequenceNumber";
        public const string GEOMETRY_ATTRIBUTE_NAME = "Geometry";

        #endregion

        #region constructors

        public SymbolControlContext()
        {
            _attributes = new Dictionary<string, object>();
            Attributes.Add(FILL_ATTRIBUTE_NAME, null);
            Attributes.Add(SEQUENCE_NUMBER_ATTRIBUTE_NAME, null);
            Attributes.Add(GEOMETRY_ATTRIBUTE_NAME, null);
        }

        #endregion

        #region public members

        /// <summary>
        /// Attributes for binding
        /// </summary>
        public Dictionary<string, object> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        #endregion

        #region private members

        private Dictionary<string, object> _attributes;

        #endregion
    }
}

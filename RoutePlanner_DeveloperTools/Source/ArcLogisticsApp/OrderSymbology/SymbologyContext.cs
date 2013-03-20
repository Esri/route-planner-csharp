using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ESRI.ArcLogistics.App.OrderSymbology
{
    /// <summary>
    /// Symbology datacontext class
    /// </summary>
    class SymbologyContext
    {
        #region constants

        public const string FILL_ATTRIBUTE_NAME = "Fill";
        public const string SIZE_ATTRIBUTE_NAME = "Size";
        public const string FULLSIZE_ATTRIBUTE_NAME = "FullSize";
        public const string IS_VIOLATED_ATTRIBUTE_NAME = "IsViolated";
        public const string IS_LOCKED_ATTRIBUTE_NAME = "IsLocked";
        public const string OFFSETX_ATTRIBUTE_NAME = "OffsetX";
        public const string OFFSETY_ATTRIBUTE_NAME = "OffsetY";
        public const string SEQUENCE_NUMBER_ATTRIBUTE_NAME = "SequenceNumber";

        #endregion

        #region constructors

        public SymbologyContext()
        {
            _attributes = new Dictionary<string, object>();
            Attributes.Add(FILL_ATTRIBUTE_NAME, null);
            Attributes.Add(SIZE_ATTRIBUTE_NAME, null);
            Attributes.Add(FULLSIZE_ATTRIBUTE_NAME, null);
            Attributes.Add(IS_VIOLATED_ATTRIBUTE_NAME, false);
            Attributes.Add(IS_LOCKED_ATTRIBUTE_NAME, false);
            Attributes.Add(OFFSETX_ATTRIBUTE_NAME, 0);
            Attributes.Add(OFFSETY_ATTRIBUTE_NAME, 0);
        }

        #endregion

        #region public members

        /// <summary>
        /// Attributes for binding
        /// </summary>
        public IDictionary<string, object> Attributes
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

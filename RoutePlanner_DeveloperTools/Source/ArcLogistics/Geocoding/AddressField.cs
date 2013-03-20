using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Class that represents address field.
    /// </summary>
    public class AddressField
    {
        #region constructors

        internal AddressField(string title, AddressPart type, bool visible, string decsription)
        {
            _title = title;
            _type = type;
            _visible = visible;
            _decsription = decsription;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Address field title.
        /// </summary>
        public string Title
        {
            get { return _title; }
        }

        /// <summary>
        /// Address field type.
        /// </summary>
        public AddressPart Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Indicates whether this address field should be shown in the UI.
        /// </summary>
        public bool Visible
        {
            get { return _visible; }
        }

        /// <summary>
        /// Address field description.
        /// </summary>
        public string Description
        {
            get { return _decsription; }
        }

        #endregion

        #region private members

        private string _title;
        private string _decsription;
        private AddressPart _type;
        private bool _visible;

        #endregion
    }
}

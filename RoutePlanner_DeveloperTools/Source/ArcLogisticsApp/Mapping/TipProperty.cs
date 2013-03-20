using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// TipProperty class
    /// </summary>
    internal class TipProperty
    {
        #region constructors

        public TipProperty(string name, string title, Unit? valueUnits, Unit? displayUnits)
        {
            Name = name;
            Title = title;
            ValueUnits = valueUnits;
            DisplayUnits = displayUnits;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Tip property string
        /// </summary>
        /// <returns>Property title</returns>
        public override string ToString()
        {
            return Title;
        }

        #endregion

        #region Public members

        /// <summary>
        /// Property title
        /// </summary>
        public string Title
        {
            get;
            private set;
        }

        /// <summary>
        /// Property name
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Prefix path to bind the property.
        /// </summary>
        public string PrefixPath
        {
            get;
            set;
        }

        /// <summary>
        /// Property value units
        /// </summary>
        public Unit? ValueUnits
        {
            get;
            private set;
        }

        /// <summary>
        /// Property Display units
        /// </summary>
        public Unit? DisplayUnits
        {
            get;
            private set;
        }

        #endregion
    }
}

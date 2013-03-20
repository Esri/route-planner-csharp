using System;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// SelectPropertiesWrapper class
    /// </summary>
    internal class SelectPropertiesWrapper : IDescripted
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates object.
        /// </summary>
        /// <param name="fieldName">Face name.</param>
        /// <param name="description">Description text (can be null).</param>
        /// <param name="isCheked">Is cheked flag.</param>
        public SelectPropertiesWrapper(string name, string description, bool isCheked)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(name));

            _name = name;
            _isChecked = isCheked;
            _description = description;
        }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Is checked flag.
        /// </summary>
        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; }
        }

        /// <summary>
        /// Converts object to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (string.IsNullOrEmpty(_description)) ? _name : string.Format(FORMAT, _name, _description);
        }

        #endregion // Public methods

        #region IDescripted
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Description.
        /// </summary>
        public string Description
        {
            get { return _description; }
        }

        #endregion // IDescripted

        #region Private consts
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string FORMAT = "{0}: {1}";

        #endregion // Private consts

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _name = null;
        private string _description = null;
        private bool _isChecked = false;

        #endregion // Private members
    }
}

using System;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// ImportProfileDataWrapper class
    /// </summary>
    internal class ImportProfileDataWrapper
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ImportProfileDataWrapper(Nullable<bool> IsDefault, string Name, string ImportType, string Description)
        {
            _isDefault = IsDefault;
            _name = Name;
            _type = ImportType;
            _description = Description;
        }
        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public Nullable<bool> IsDefault
        {
            get { return _isDefault; }
            set { _isDefault = value; }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Import name is required.", _name);
                _name = value;
            }
        }

        public string Type
        {
            get { return _type; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Import type is required.", _type);
                _type = value;
            }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        #endregion // Public methods

        #region Override methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            return _name;
        }
        #endregion // Override methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _name;
        private string _type;
        private string _description;
        private Nullable<bool> _isDefault;
        #endregion // Private members
    }
}

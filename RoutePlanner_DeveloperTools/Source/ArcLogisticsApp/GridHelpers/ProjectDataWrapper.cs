using System;
using System.ComponentModel;
using ESRI.ArcLogistics.Utility.ComponentModel;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    internal class ProjectDataWrapper : NotifyPropertyChangedBase, INotifyPropertyChanged
    {
        #region Constructor

        public ProjectDataWrapper(bool IsCurrent, string Name, string Description)
        {
            _isCurrent = IsCurrent;
            _name = Name;
            _description = Description;
        }

        #endregion

        #region Public Properties
        
        public bool IsCurrent
        {
            get
            {
                return _isCurrent;
            }
            set
            {
                _isCurrent = value;
                NotifyPropertyChanged(PROP_NAME_ISCURRENT);
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                // Thrown when new project canceled.
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Project name is required.", _name);
                _name = value;
                NotifyPropertyChanged(PROP_NAME_NAME);
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                NotifyPropertyChanged(PROP_NAME_DESCRIPTION);
            }
        }

        #endregion

        #region Public methods

        public void SetCurrent(bool value)
        {
            _isCurrent = value;
        }

        #endregion

        #region Private constants

         /// <summary>
        /// Name of the Name property.
        /// </summary>
        private const string PROP_NAME_NAME = "Name";

        /// <summary>
        /// Name of the StartDate property.
        /// </summary>
        private const string PROP_NAME_DESCRIPTION = "Description";

        /// <summary>
        /// Name of the FinishDate property.
        /// </summary>
        private const string PROP_NAME_ISCURRENT = "IsCurrent";

        #endregion

        #region Private Fields

        private string _name;
        private string _description;
        private bool _isCurrent; 
        
        #endregion
    }
}

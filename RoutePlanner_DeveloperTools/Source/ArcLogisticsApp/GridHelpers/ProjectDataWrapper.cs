/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

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

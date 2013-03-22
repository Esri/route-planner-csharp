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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;
using System.Collections;
using System.Collections.ObjectModel;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command duplicates mobile devices
    /// </summary>
    class DuplicateMobileDevices : DuplicateCommandBase
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DuplicateMobileDevices";

        public override string Name
        {
            get
            {
                return COMMAND_NAME;
            }
        }

        #endregion

        #region DuplicateCommandBase Protected Methods

        /// <summary>
        /// Duplicates mobile devices
        /// </summary>
        protected override void _Duplicate()
        {
            Project project = App.Current.Project;

            List<MobileDevice> selectedMobileDevices = new List<MobileDevice>();

            foreach (MobileDevice md in ((ISupportSelection)ParentPage).SelectedItems)
                selectedMobileDevices.Add(md);

            foreach (MobileDevice mobileDevice in selectedMobileDevices)
            {
                MobileDevice md = mobileDevice.Clone() as MobileDevice;
                md.Name = DataObjectNamesConstructor.GetDuplicateName(mobileDevice.Name, project.MobileDevices);
                project.MobileDevices.Add(md);
            }            

            App.Current.Project.Save();
        }

        #endregion DuplicateCommandBase Protected Methods

        #region DuplicateCommandBase Protected Properties

        protected override ISupportDataObjectEditing ParentPage
        {
            get 
            {
                if (_parentPage == null)
                {
                    MobileDevicesPage page = (MobileDevicesPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.MobileDevicesPagePath);
                    _parentPage = page;
                }

                return _parentPage;
            }
        }

        #endregion DuplicateCommandBase Protected Properties

        #region Private Members

        private ISupportDataObjectEditing _parentPage;

        #endregion Private Members
    }
}

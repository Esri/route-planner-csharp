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

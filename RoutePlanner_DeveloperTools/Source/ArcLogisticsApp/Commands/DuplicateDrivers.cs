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
    /// Command duplicates drivers
    /// </summary>
    class DuplicateDrivers : DuplicateCommandBase
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DuplicateDrivers";

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
        /// Duplicates drivers
        /// </summary>
        protected override void _Duplicate()
        {
            Project project = App.Current.Project;

            List<Driver> selectedDrivers = new List<Driver>();

            foreach (Driver driver in ((ISupportSelection)ParentPage).SelectedItems)
                selectedDrivers.Add(driver);

            foreach (Driver driver in selectedDrivers)
            {
                Driver dr = driver.Clone() as Driver;
                dr.Name = DataObjectNamesConstructor.GetDuplicateName(driver.Name, project.Drivers);
                project.Drivers.Add(dr);
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
                    DriversPage page = (DriversPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.DriversPagePath);
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

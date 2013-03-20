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
    /// Command duplicates driver specialties
    /// </summary>
    class DuplicateDriverSpecialties : DuplicateCommandBase
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DuplicateDriverSpecialties";

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
        /// Duplicates driver specialties
        /// </summary>
        protected override void _Duplicate()
        {
            Project project = App.Current.Project;

            List<DriverSpecialty> selectedSpecialties = new List<DriverSpecialty>();

            foreach (DriverSpecialty specialty in ((ISupportSelection)ParentPage).SelectedItems)
                selectedSpecialties.Add(specialty);

            foreach (DriverSpecialty driverSpecialty in selectedSpecialties)
            {
                DriverSpecialty ds = driverSpecialty.Clone() as DriverSpecialty;
                ds.Name = DataObjectNamesConstructor.GetDuplicateName(driverSpecialty.Name, project.DriverSpecialties);
                project.DriverSpecialties.Add(ds);
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
                    SpecialtiesPage page = (SpecialtiesPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.SpecialtiesPagePath);

                    // get driver specialties panel
                    _parentPage = page.driverSpecialties;
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

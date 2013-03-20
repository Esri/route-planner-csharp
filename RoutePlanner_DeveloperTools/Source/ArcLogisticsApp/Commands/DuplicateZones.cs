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
    /// Command duplicates zones
    /// </summary>
    class DuplicateZones : DuplicateCommandBase
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DuplicateZones";

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
        /// Duplicates zones
        /// </summary>
        protected override void _Duplicate()
        {
            Project project = App.Current.Project;

            List<Zone> selectedZones = new List<Zone>();

            foreach (Zone zone in ((ISupportSelection)ParentPage).SelectedItems)
                selectedZones.Add(zone);

            foreach (Zone zone in selectedZones)
            {
                Zone zn = zone.Clone() as Zone;
                zn.Name = DataObjectNamesConstructor.GetDuplicateName(zone.Name, project.Zones);
                project.Zones.Add(zn);
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
                    ZonesPage page = (ZonesPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.ZonesPagePath);
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

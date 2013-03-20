using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.App.PageCategories
{
    internal class ScheduleCategory : PageCategoryItem
    {
        #region Constructors

        public ScheduleCategory()
        {
            App.Current.ProjectLoaded += new EventHandler(OnProjectLoaded);
            App.Current.ProjectClosed += new EventHandler(OnProjectClosed);
            Project project = App.Current.Project;
            if (project != null)
            {
                project.Locations.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Locations_CollectionChanged);
                project.Vehicles.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Vehicles_CollectionChanged);
                project.Drivers.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Drivers_CollectionChanged);
                project.DefaultRoutes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(DefaultRoutes_CollectionChanged);
            }

            _CheckCategoryAllowed();
        }

        #endregion

        #region Public Properties

        public override bool IsEnabled
        {
            get
            {
                return base.IsEnabled;
            }
            set
            {
                base.IsEnabled = value;

                if (value)
                    TooltipText = null;
                else
                    TooltipText = (string)App.Current.FindResource("ScheduleTabDisabledTooltip");
            }
        }

        #endregion

        #region Protected methods

        protected void _CheckCategoryAllowed()
        {
            Project project = App.Current.Project;
            IsEnabled = (project != null && project.Locations.Count > 0 &&
                project.Vehicles.Count > 0 && project.Drivers.Count > 0 &&
                project.DefaultRoutes.Count > 0);
        }

        #endregion

        #region Event handlers

        private void DefaultRoutes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _CheckCategoryAllowed();
        }

        private void Drivers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _CheckCategoryAllowed();
        }

        private void Vehicles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _CheckCategoryAllowed();
        }

        private void OnProjectClosed(object sender, EventArgs e)
        {
            _CheckCategoryAllowed();
        }

        private void Locations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _CheckCategoryAllowed();
        }

        private void OnProjectLoaded(object sender, EventArgs e)
        {
            Project project = App.Current.Project;
            project.Locations.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Locations_CollectionChanged);
            project.Vehicles.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Vehicles_CollectionChanged);
            project.Drivers.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Drivers_CollectionChanged);
            project.DefaultRoutes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(DefaultRoutes_CollectionChanged);
            _CheckCategoryAllowed();
        }

        #endregion
    }
}

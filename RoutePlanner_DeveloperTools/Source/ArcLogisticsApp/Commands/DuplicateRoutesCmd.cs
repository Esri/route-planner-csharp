using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Pages;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Commands
{
    class DuplicateRoutesCmd: RoutesCommandBase
    {
        public const string COMMAND_NAME = "ArcLogistics.Commands.DuplicateRoutes";

        #region Public Override Members

        public override bool IsEnabled
        {
            get
            {
                return base.IsEnabled;
            }
            protected set
            {
                base.IsEnabled = value;

                if (value)
                    TooltipText = (string)App.Current.FindResource("DuplicateCommandEnabledTooltip");
                else
                    TooltipText = (string)App.Current.FindResource("DuplicateCommandDisabledTooltip"); 
            }
        }

        public override string TooltipText
        {
            get
            {
                return _tooltipText;
            }
            protected set
            {
                _tooltipText = value;
                _NotifyPropertyChanged(TOOLTIP_PROPERTY_NAME);
            }
        }

        public override string Name
        {
            get
            {
                return COMMAND_NAME;
            }
        }

        public override string Title
        {
            get
            {
                return (string)App.Current.FindResource("DuplicateRoutesCommandTitle");
            }
        }
 
        #endregion

        #region Protected Override Members

        protected override void _Execute(params object[] args)
        {
            Collection<Route> selected = new Collection<Route>();

            foreach (Route item in _GetRoutesFromSelection())
            {
                Route route = item.CloneNoResults() as Route;
                route.Vehicle = null;
                route.Driver = null;
                route.DefaultRouteID = null;
                selected.Add(route);
            }

            foreach (Route item in selected)
            {
                item.Name = DataObjectNamesConstructor.GetDuplicateName(item.Name, CurrentSchedule.Routes);
                CurrentSchedule.Routes.Add(item);
            }

            App.Current.Project.Save();
            StatusBuilder statusBuilder = new StatusBuilder();
            statusBuilder.FillSelectionStatusWithoutCollectionSize(CurrentSchedule.Routes.Count, (string)App.Current.FindResource("Route"), 0, OptimizePage);
        }

        protected override void _CheckEnabled()
        {
            Schedule schedule = CurrentSchedule;
            IsEnabled = ((null != schedule) && (null != schedule.Routes) && (0 < schedule.Routes.Count)
                        && !OptimizePage.IsEditingInProgress && (_GetRoutesFromSelection().Count == 1));
        }

        #endregion

        #region Private Members

        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";

        private string _tooltipText = null;

        #endregion
    }
}

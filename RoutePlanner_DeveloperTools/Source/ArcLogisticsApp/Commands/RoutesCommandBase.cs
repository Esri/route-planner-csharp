using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Pages;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace ESRI.ArcLogistics.App.Commands
{
    class RoutesCommandBase : CommandBase
    {
        #region Override Members

        public override void Initialize(App app)
        {
            base.Initialize(app);
            IsEnabled = false;
            App.Current.ApplicationInitialized += new EventHandler(Current_ApplicationInitialized);
        }

        public override bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            protected set
            {
                _isEnabled = value;
                _NotifyPropertyChanged("IsEnabled");
            }
        }

        protected override void _Execute(params object[] args)
        {
            //throw new NotImplementedException();
        }

        public override string Name
        {
            get { return null; }
        }

        public override string Title
        {
            get { return null; }
        }

        public override string TooltipText
        {
            get { return null; }
            protected set { }
        }

        #endregion

        #region Event Handlers

        protected virtual void Current_ApplicationInitialized(object sender, EventArgs e)
        {
            _optimizePage = (OptimizeAndEditPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.SchedulePagePath);
            
            _optimizePage.CurrentScheduleChanged += new EventHandler(optimizePage_CurrentScheduleChanged);
            _optimizePage.SelectionChanged += new EventHandler(_schedulePage_SelectionChanged);
            _optimizePage.EditBegun += new DataObjectEventHandler(_optimizePage_EditBegun);
            _optimizePage.EditCommitted += new DataObjectEventHandler(_optimizePage_EditCommitted);
            _optimizePage.EditCanceled += new ESRI.ArcLogistics.App.Pages.DataObjectEventHandler(_optimizePage_EditCanceled);
        }

        private void _optimizePage_EditCommitted(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _optimizePage_EditBegun(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _schedulePage_SelectionChanged(object sender, EventArgs e)
        {
            _CheckEnabled();
        }

        private void _optimizePage_EditCanceled(object sender, ESRI.ArcLogistics.App.Pages.DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void optimizePage_CurrentScheduleChanged(object sender, EventArgs e)
        {
            _currentSchedule = _optimizePage.CurrentSchedule;
            _CheckEnabled();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///  Method checks is command enabled
        /// </summary>
        protected virtual void _CheckEnabled()
        {
            IsEnabled = (_GetRoutesFromSelection().Count > 0
               && CurrentSchedule.Routes != null
               && CurrentSchedule.Routes.Count > 0
               && !OptimizePage.IsEditingInProgress);
        }


        /// <summary>
        /// Methods returns collection of selected routes 
        /// </summary>
        /// <returns></returns>
        protected Collection<Route> _GetRoutesFromSelection()
        {
            Collection<Route> routes = new Collection<Route>();

            foreach (Object obj in OptimizePage.SelectedItems)
            {
                if (obj is Route)
                {
                    routes.Add((Route)obj);
                }
            }
            return routes;
        }

        protected Schedule CurrentSchedule
        {
            get { return _currentSchedule; }
        }

        protected OptimizeAndEditPage OptimizePage
        {
            get { return _optimizePage; }
        }

        #endregion

        #region Private Fields

        Schedule _currentSchedule;
        bool _isEnabled;
        OptimizeAndEditPage _optimizePage;

        #endregion
    }
}

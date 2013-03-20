using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.App.Pages;
using System.Windows.Forms;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Cancel current routing operation
    /// </summary>
    internal class CancelRoutingOperationCmd : CommandBase
    {
        #region Constants

        public const string COMMAND_NAME = "ArcLogistics.Commands.CancelRoutingOperationCmd";

        #endregion

        #region Command Base Public Porperties

        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("CancelRoutingOperationCmdTitle"); }
        }

        public override string TooltipText
        {
            get { return (string)App.Current.FindResource("CancelRoutingOperationCmdToolTip"); }
            protected set { }
        }

        #endregion

        #region Command Base Public Methods

        public override void Initialize(App app)
        {
            base.Initialize(app);
            IsEnabled = true;
        }

        #endregion

        #region Command Base Protected  Methods

        protected override void _Execute(params object[] args)
        {
            OptimizeAndEditPage schedulePage = (OptimizeAndEditPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.SchedulePagePath);
            IVrpSolver solver = App.Current.Solver;
            Debug.Assert(solver != null);

            // get operations for current date
            IList<AsyncOperationInfo> currentOperations = solver.GetAsyncOperations(App.Current.CurrentDate);
            Debug.Assert(currentOperations != null && currentOperations.Count == 1);

            // cancel current operation
            bool isEnabled = !solver.CancelAsyncOperation(currentOperations[0].Id);
            Debug.Assert(!isEnabled);

            // Change status string for current date to "Canceling..." and hide "Cancel" button
            schedulePage.SetCancellingStatus(App.Current.CurrentDate);
        }

        #endregion
    }
}

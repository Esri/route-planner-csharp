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
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Class for generation routes shapes in case of "Follow streets" option is turned on, but directions are absent.
    /// </summary>
    class RouteShapeGenerationCmd : CommandBase
    {
        #region Override Members

        /// <summary>
        /// Command name.
        /// </summary>
        public override string Name
        {
            get
            {
                return COMMAND_NAME;
            }
        }

        /// <summary>
        /// Command title.
        /// </summary>
        public override string Title
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Command tooltip.
        /// </summary>
        public override string TooltipText
        {
            get { return null; }
            protected set { }
        }

        /// <summary>
        /// Execute command.
        /// </summary>
        /// <param name="args">
        /// Command args.
        /// Routes to generate shapes.
        /// </param>
        protected override void _Execute(params object[] args)
        {
            try
            {
                List<Route> routesWithoutGeometry = args[0] as List<Route>;

                Debug.Assert(routesWithoutGeometry != null);

                _DoGenerateRoutesShapes(routesWithoutGeometry);
            }
            catch (RouteException e)
            {
                if (e.InvalidObjects != null) // if exception throw because any Routes or Orders are invalid
                    _ShowSolveValidationResult(e.InvalidObjects);
                else
                    App.Current.Messenger.AddError(RoutingCmdHelpers.FormatRoutingExceptionMsg(e));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                if ((e is LicenseException) || (e is AuthenticationException) || (e is CommunicationException))
                    CommonHelpers.AddRoutingErrorMessage(e);
                else
                    throw;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Do routes shapes generation.
        /// </summary>
        /// <param name="routesWithoutGeometry">Route to generate geometry.</param>
        private void _DoGenerateRoutesShapes(List<Route> routesWithoutGeometry)
        {
            Debug.Assert(routesWithoutGeometry != null);
            Debug.Assert(routesWithoutGeometry.Count > 0);

            // Set routing status.
            Route route = routesWithoutGeometry[0];
            DateTime date = route.Schedule.PlannedDate.Value;
            OptimizeAndEditPage optimizeAndEditPage = (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);
            optimizeAndEditPage.SetRoutingStatus((string)App.Current.FindResource("GenerateRouteShapes"), date);

            // Add routing message to messenger.
            string message = string.Format((string)App.Current.FindResource("GenerateRouteShapesStartText"), date.ToShortDateString());
            App.Current.Messenger.AddInfo(message);

            // Lock all application UI.
            App.Current.UIManager.Lock(true);

            // Subscribe to routing command completed.
            App.Current.Solver.AsyncSolveCompleted += _AsyncSolveCompleted;

            try
            {
                // Save operation ID.
                _operationID = App.Current.Solver.GenerateDirectionsAsync(routesWithoutGeometry);
            }
            catch (Exception e)
            {
                App.Current.Solver.AsyncSolveCompleted -= _AsyncSolveCompleted;

                Logger.Error(e);
                CommonHelpers.AddRoutingErrorMessage(e);

                _ReturnUIToDefaultState();
            }
        }

        /// <summary>
        /// Unlock UI and set default status.
        /// </summary>
        private void _ReturnUIToDefaultState()
        {
            // Unlock application UI.
            App.Current.UIManager.Unlock();

            // Remove routing status.
            WorkingStatusHelper.SetReleased();
        }

        /// <summary>
        /// React on solve operation finished.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Solve operation event args.</param>
        private void _AsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            // Check that finished operation ID equals to current.
            if (!_operationID.Equals(e.OperationId))
                return;

            App.Current.Solver.AsyncSolveCompleted -= _AsyncSolveCompleted;

            if (e.Cancelled)
            {
                App.Current.Messenger.AddInfo(
                    string.Format((string)App.Current.FindResource("GenerateRouteShapesCancelledText"), App.Current.CurrentDate.ToShortDateString()));

                App.Current.MapDisplay.TrueRoute = false;
            }
            else if (e.Error != null)
            {
                Debug.Assert(e.Error != null);
                Logger.Error(e.Error);
                CommonHelpers.AddRoutingErrorMessage(e.Error);

                App.Current.MapDisplay.TrueRoute = false;
            }
            else
            {
                App.Current.Messenger.AddInfo(
                    string.Format((string)App.Current.FindResource("GenerateRouteShapesCompletedText"), App.Current.CurrentDate.ToShortDateString()));

                App.Current.Project.Save();

                // Set "Follow streets option". Set it to false first for notifying all route graphics.
                App.Current.MapDisplay.TrueRoute = false;
                App.Current.MapDisplay.TrueRoute = true;
            }

            _ReturnUIToDefaultState();
        }

        /// <summary>
        /// Shows solve validation dialog
        /// </summary>
        /// <param name="invalidObjects">Not valid objects.</param>
        private void _ShowSolveValidationResult(ESRI.ArcLogistics.Data.DataObject[] invalidObjects)
        {
            RoutingSolveValidator validator = new RoutingSolveValidator();
            validator.Validate(invalidObjects);
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Command name.
        /// </summary>
        private const string COMMAND_NAME = "ArcLogistics.Commands.RouteShapeGeneration";

        #endregion

        #region Private fields

        /// <summary>
        /// Shape generation ID.
        /// </summary>
        private Guid _operationID = Guid.Empty;

        #endregion
    }
}

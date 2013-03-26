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
using System.Diagnostics;
using ESRI.ArcLogistics.App.Dialogs;

namespace ESRI.ArcLogistics.App.Pages.Wizards
{
    /// <summary>
    /// Breaks setup wizard.
    /// </summary>
    internal partial class BreaksSetupWizard : WizardBase
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public BreaksSetupWizard(DefaultBreaksController breaksController)
            : base(_pageTypes, new BreaksSetupWizardDataContext())
        {
            _defaultBreaksContoller = breaksController;
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Starts wizard.
        /// </summary>
        public override void Start()
        {
            // Block breaks controller check method.
            _defaultBreaksContoller.IsCheckEnabled = false;

            base.Start();

            // Store application state.
            MainWindow mainWindow = App.Current.MainWindow;
            DataKeeper[FleetSetupWizardDataContext.ParentPageFieldName] = mainWindow.CurrentPage;
            DataKeeper[FleetSetupWizardDataContext.ProjectFieldName] = App.Current.Project;

            // If we have selected breaks type before - lock UI, so user cannot navigate to 
            // other Action Panels.
            if (App.Current.Project.BreaksSettings.BreaksType != null)
                App.Current.UIManager.Lock(false);
            
            // Start wizard.
            _NavigateToPage(START_PAGE_INDEX);
        }

        #endregion // Public methods

        #region Private properties

        /// <summary>
        /// Specialized context.
        /// </summary>
        private BreaksSetupWizardDataContext DataKeeper
        {
            get
            {
                return DataContext as BreaksSetupWizardDataContext;
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// "Cancel" button cliked in page.
        /// </summary>
        /// <param name="sender">Page when button clicked.</param>
        /// <param name="e">Ignored.</param>
        protected override void _OnCancelRequired(object sender, EventArgs e)
        {
            // Close wizard.
            _Close(PagePaths.ProjectsPagePath);
        }

        /// <summary>
        /// "Ok" or "Cancel" button cliked in page.
        /// </summary>
        /// <param name="sender">Page when button clicked.</param>
        /// <param name="e">Ignored.</param>
        protected override void _OnFinishRequired(object sender, EventArgs e)
        {
            // Close wizard.
            _Close(PagePaths.BreaksPagePath);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Close wizard routine.
        /// </summary>
        /// <param name="showPagePath">Page to showing after close wizard.</param>
        private void _Close(string showPagePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(showPagePath));

            // Unlock UI.
            App.Current.UIManager.Unlock();

            // Unlock breaks controller.
            _defaultBreaksContoller.IsCheckEnabled = true;

            App.Current.MainWindow.Navigate(showPagePath);
        }

        #endregion // Private methods
        
        #region Private consts

        /// <summary>
        /// Start page index.
        /// </summary>
        private const int START_PAGE_INDEX = 0;

        /// <summary>
        /// Predifined wizards pages.
        /// </summary>
        /// <remarks>Wizard show pages in same order.</remarks>
        private static Type[] _pageTypes = new Type[]
        {
            typeof(BreaksSetupWizardPage),
        };

        /// <summary>
        /// When default breaks changing - updates default routes breaks if necessary.
        /// </summary>
        private DefaultBreaksController _defaultBreaksContoller =
            new DefaultBreaksController();

        #endregion
    }
}

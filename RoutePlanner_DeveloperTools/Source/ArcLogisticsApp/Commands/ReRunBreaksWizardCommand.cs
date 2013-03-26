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

using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Pages.Wizards;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// StartBreaksSetupWizardCommand class // REV: correct comment.
    /// </summary>
    internal class ReRunBreaksWizardCommand : CommandBase
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ReRunBreaksWizardCommand()
        {
        }

        #endregion // Constructors

        #region Overridden properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the command. Must be unique and unchanging.
        /// </summary>
        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        /// <summary>
        /// Title of the command that can be shown in UI.
        /// </summary>
        public override string Title
        {
            get { return (string)_Application.FindResource("ReRunBreaksWizardCommandTtile"); }
        }

        /// <summary>
        /// Tooltip text.
        /// </summary>
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

        /// <summary>
        /// Is command enabled property.
        /// </summary>
        public override bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            protected set
            {
                _isEnabled = value;
                _NotifyPropertyChanged(ENABLED_PROPERTY_NAME);

                if (value)
                    TooltipText = (string)App.Current.
                        FindResource("ReRunBreaksWizardCommandEnabledTooltip");
                else
                    TooltipText = (string)App.Current.
                        FindResource("ReRunBreaksWizardCommandDisabledTooltip");
            }
        }

        #endregion // Overridden properties

        #region Overridden methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Inits procedure.
        /// </summary>
        /// <param name="app">Application.</param>
        public override void Initialize(App app)
        {
            base.Initialize(app);
            IsEnabled = true;
            App.Current.ApplicationInitialized += new EventHandler(_CurrentApplicationInitialized);
        }

        /// <summary>
        /// Command execute code.
        /// </summary>
        /// <param name="args">Command arguments.</param>
        protected override void _Execute(params object[] args)
        {
            // Start breaks setup wizard.
            var breaksPage = (App.Current.MainWindow.GetPage(PagePaths.BreaksPagePath)
                as BreaksPage);
            breaksPage.StartBreaksSetupWizard();
        }

        #endregion // Overridden methods

        #region Private Property

        /// <summary>
        /// Page, which inits command.
        /// </summary>
        private BreaksPage ParentPage
        {
            get
            {
                if (_parentPage == null)
                {
                    BreaksPage page = (BreaksPage)App.Current.MainWindow.GetPage(PagePaths.BreaksPagePath);
                    _parentPage = page;
                }

                return _parentPage;
            }
        }

        #endregion

        #region Private EventHandlers

        /// <summary>
        /// Creates parent page event handlers.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CurrentApplicationInitialized(object sender, EventArgs e)
        {
            if (ParentPage != null)
            {
                // Subscribe to page edit events.
                ParentPage.EditBegun += new EventHandler(_CheckEnabled);
                ParentPage.EditFinished += new EventHandler(_CheckEnabled);
            }
            else
                // Breaks page must be initialized.
                Debug.Assert(false);
        }

        /// <summary>
        /// Check is command enabled.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CheckEnabled(object sender, EventArgs e)
        {
            if (ParentPage != null)
                IsEnabled = !_parentPage.IsEditingInProgress;
        }

        #endregion

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Names of command and properties.
        /// </summary>
        private const string COMMAND_NAME = "ArcLogistics.Commands.ReRunBreaksWizardCommand";
        private const string ENABLED_PROPERTY_NAME = "IsEnabled";
        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enabled flag.
        /// </summary>
        private bool _isEnabled = true;

        /// <summary>
        /// Page, which launched the command.
        /// </summary>
        private BreaksPage _parentPage;

        /// <summary>
        /// Tooltip for control with this command.
        /// </summary>
        private string _tooltipText = null;

        #endregion // Private members
    }
}

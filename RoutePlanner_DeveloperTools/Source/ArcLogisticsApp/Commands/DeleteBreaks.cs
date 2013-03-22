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
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ESRI.ArcLogistics.App.Dialogs;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command deletes breaks.
    /// </summary>
    class DeleteBreaks : CommandBase
    {
        #region Public Command Base Members

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
                    TooltipText = (string)App.Current.FindResource("DeleteCommandEnabledTooltip");
                else
                    TooltipText = (string)App.Current.FindResource("DeleteCommandDisabledTooltip");
            }
        }

        /// <summary>
        /// Command name.
        /// </summary>
        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        /// <summary>
        /// Comand title.
        /// </summary>
        public override string Title
        {
            get { return (string)App.Current.FindResource("DeleteCommandTitle"); }
        }

        /// <summary>
        /// Command tooltip.
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

        public override void Initialize(App app)
        {
            base.Initialize(app);
            IsEnabled = false;
            App.Current.ApplicationInitialized += new EventHandler(Current_ApplicationInitialized);

            KeyGesture = new KeyGesture(INVOKE_KEY);
        }

        #endregion Public Command Base Members

        #region Protected Command Base Methods

        protected override void _Execute(params object[] args)
        {
            // If editing is process in ParentPage - cancel editing.
            if (ParentPage.IsEditingInProgress)
                ((ICancelDataObjectEditing)ParentPage).CancelObjectEditing();

            IList selectedObjects = ((ISupportSelection)ParentPage).SelectedItems;
            if (0 < selectedObjects.Count)
            {
                bool doProcess = true;
                if (Settings.Default.IsAllwaysAskBeforeDeletingEnabled)
                    // Show warning dialog.
                    doProcess = DeletingWarningHelper.Execute(selectedObjects);

                // Do process.
                if (doProcess)
                    _Delete(selectedObjects);
            }
        }

        #endregion Protected Command Base Methods
        
        #region Protected Event Handlers

        protected void Current_ApplicationInitialized(object sender, EventArgs e)
        {
            _InitParentPageEventHandlers();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Deletes Breaks.
        /// </summary>
        private void _Delete(IList selectedObjects)
        {
            Collection<Break> removingBreaks = new Collection<Break>();

            foreach (Break dr in selectedObjects)
                removingBreaks.Add(dr);

            foreach (Break dr in removingBreaks)
                App.Current.Project.BreaksSettings.DefaultBreaks.Remove(dr);

            App.Current.Project.Save();
        }

        /// <summary>
        /// Checks is command enabled and sets property IsEnabled to false or true.
        /// </summary>
        private void _CheckEnabled()
        {
            // According new requirements we should enable "Delete" command when selection contains 
            // at list one item including insertion row, but Xceed data grid not consider item in 
            // insertion row like one of item in selection  and the collection of selected items is 
            // empty when user create new item therefore we set command "enabled" when selection 
            // isn't empty or editing is in progress (case when user adding new item in insertion row)
            IsEnabled = (((ISupportSelection)ParentPage).SelectedItems.Count > 0 || ParentPage.IsEditingInProgress);
        }

        /// <summary>
        /// Creates parent page event handlers.
        /// </summary>
        private void _InitParentPageEventHandlers()
        {
            if (null != ParentPage)
            {
                ParentPage.EditBegun += (e, args) => _CheckEnabled();
                ParentPage.EditFinished += (e, args) => _CheckEnabled();
                if (ParentPage is ISupportSelectionChanged)
                    ((ISupportSelectionChanged)ParentPage).SelectionChanged += 
                        (e, args) => _CheckEnabled();
            }
        }

        #endregion DeleteCommandBase Protected Methods

        #region Private Properties

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

        #endregion DeleteCommandBase Protected Properties
        
        #region Private constants

        /// <summary>
        /// Key to invoke command.
        /// </summary>
        private const Key INVOKE_KEY = Key.Delete;

        /// <summary>
        /// Names of command and properties.
        /// </summary>
        private const string ENABLED_PROPERTY_NAME = "IsEnabled";
        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";
        private const string COMMAND_NAME = "ArcLogistics.Commands.DeleteBreaks";

        #endregion

        #region Private Members

        /// <summary>
        /// Page, which launched the command.
        /// </summary>
        private BreaksPage _parentPage;

        /// <summary>
        /// Tooltip for control with this command.
        /// </summary>
        private string _tooltipText = null;

        /// <summary>
        /// Flag - is command enabled.
        /// </summary>
        private bool _isEnabled = false;

        #endregion
    }
}

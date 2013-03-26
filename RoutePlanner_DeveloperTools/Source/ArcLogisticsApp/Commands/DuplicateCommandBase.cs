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
using ESRI.ArcLogistics.App.Pages;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Base class for delete command
    /// </summary>
    abstract class DuplicateCommandBase : CommandBase
    {
        #region Public Command Base Members

        public override string Name
        {
            get { return null; }
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("DuplicateCommandTitle"); }
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
                _NotifyPropertyChanged(ENABLED_PROPERTY_NAME);

                if (value)
                    TooltipText = (string)App.Current.FindResource("DuplicateCommandEnabledTooltip");
                else
                    TooltipText = (string)App.Current.FindResource("DuplicateCommandDisabledTooltip"); 
            }
        }

        #endregion Public Command Base Members

        #region Protected Command Base Methods

        protected override void _Execute(params object[] args)
        {
            _Duplicate();
        }

        #endregion Protected Command Base Methods

        #region Protected Properties

        /// <summary>
        /// Page/panel which contains command 
        /// </summary>
        protected abstract ISupportDataObjectEditing ParentPage
        {
            get;
        }

        #endregion Protected Porperties

        #region Protected Event Handlers

        protected void Current_ApplicationInitialized(object sender, EventArgs e)
        {
            _InitParentPageEventHandlers();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Duplicates selected items (should be override in each child class)
        /// </summary>
        /// <param name="parentPage"></param>
        protected abstract void _Duplicate();

        /// <summary>
        /// Checks is command enabled and sets property IsEnabled to false or true
        /// </summary>
        protected void _CheckEnabled()
        {
            IsEnabled = ((!ParentPage.IsEditingInProgress) &&
                (((ISupportSelection)ParentPage).SelectedItems.Count == 1));
        }

        /// <summary>
        /// Creates parent page event handlers
        /// </summary>
        protected void _InitParentPageEventHandlers()
        {
            if (null != ParentPage)
            {
                ParentPage.EditBegun += new DataObjectEventHandler(ParentPage_EditBegun);
                ParentPage.EditCommitted += new DataObjectEventHandler(ParentPage_EditCommitted);
                ParentPage.EditCanceled += new DataObjectEventHandler(ParentPage_EditCanceled);
            
                if (ParentPage is ISupportSelectionChanged)
                    ((ISupportSelectionChanged)ParentPage).SelectionChanged += new EventHandler(DuplicateCommandBase_SelectionChanged);
            }
        }

        #endregion

        #region Private Event Handlers

        private void ParentPage_EditCanceled(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void ParentPage_EditCommitted(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void ParentPage_EditBegun(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void DuplicateCommandBase_SelectionChanged(object sender, EventArgs e)
        {
            _CheckEnabled();
        }

        #endregion Event Handlers

        #region Private Fields

        private const string ENABLED_PROPERTY_NAME = "IsEnabled";
        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";

        private string _tooltipText = null;
        private bool _isEnabled = false;

        #endregion
    }
}

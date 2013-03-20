using System;
using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command duplicates breaks.
    /// </summary>
    class DuplicateBreaks : CommandBase
    {
        #region Public Command Base Members

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
        /// Comand title.
        /// </summary>
        public override string Title
        {
            get { return (string)App.Current.FindResource("DuplicateCommandTitle"); }
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
            App.Current.ApplicationInitialized += new EventHandler(_CurrentApplicationInitialized);
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
                    TooltipText = (string)App.Current.FindResource("DuplicateCommandEnabledTooltip");
                else
                    TooltipText = (string)App.Current.FindResource("DuplicateCommandDisabledTooltip");
            }
        }

        #endregion Public Command Base Members

        #region Protected Command Base Methods

        /// <summary>
        /// Starts executing command.
        /// </summary>
        /// <param name="args">Ignored.</param>
        protected override void _Execute(params object[] args)
        {
            _Duplicate();
        }

        #endregion Protected Command Base Methods

        #region Private Methods

        /// <summary>
        /// Checks is command enabled and sets property IsEnabled to false or true.
        /// </summary>
        private void _CheckEnabled()
        {
            IsEnabled = ((!ParentPage.IsEditingInProgress) &&
                (((ISupportSelection)ParentPage).SelectedItems.Count == 1) &&
                App.Current.Project.BreaksSettings.DefaultBreaks.Count < Breaks.MaximumBreakCount);
        }

        /// <summary>
        /// Duplicates breaks.
        /// </summary>
        private void _Duplicate()
        {
            Project project = App.Current.Project;

            List<Break> selectedBreaks = new List<Break>();

            foreach (Break breakObj in ((ISupportSelection)ParentPage).SelectedItems)
                selectedBreaks.Add(breakObj);

            foreach (Break breakObj in selectedBreaks)
            {
                Break br = breakObj.Clone() as Break;
                project.BreaksSettings.DefaultBreaks.Add(br);
            }            

            App.Current.Project.Save();
            _CheckEnabled();
        }

        #endregion

        #region  Protected Properties

        /// <summary>
        /// Page, which inits command.
        /// </summary>
        protected BreaksPage ParentPage
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
        
        #region Private Event Handlers
        
        /// <summary>
        /// Check is command enabled.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CheckEnabled(object sender, EventArgs e)
        {
            _CheckEnabled();
        }

        /// <summary>
        /// Creates parent page event handlers.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CurrentApplicationInitialized(object sender, EventArgs e)
        {
            if (null != ParentPage)
            {
                ParentPage.EditBegun += new EventHandler(_CheckEnabled);
                ParentPage.EditFinished += new EventHandler(_CheckEnabled);
                if (ParentPage is ISupportSelectionChanged)
                    ((ISupportSelectionChanged)ParentPage).SelectionChanged += new EventHandler(_CheckEnabled);
            }
            else
                // Breaks page must be initialized.
                Debug.Assert(false);
        }
        #endregion

        #region Private constants

        /// <summary>
        /// Names of command and properties.
        /// </summary>
        private const string COMMAND_NAME = "ArcLogistics.Commands.DuplicateBreaks";
        private const string ENABLED_PROPERTY_NAME = "IsEnabled";
        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";

        #endregion

        #region Private members

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

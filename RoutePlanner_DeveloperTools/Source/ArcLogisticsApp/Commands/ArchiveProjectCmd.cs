using System;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;

using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Archiving;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Commands
{
    class ArchiveProjectCmd : CommandBase
    {
        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string COMMAND_NAME = "ArcLogistics.Commands.ArchivingProject";

        #endregion // Constants

        #region Overrided properties
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
            get { return (string)App.Current.FindResource("ArchiveProjectCommandTitle"); }
        }

        /// <summary>
        /// Tooltip text.
        /// </summary>
        public override string TooltipText
        {
            get { return _tooltipText; }
            protected set
            {
                if (_tooltipText != value)
                {
                    _tooltipText = value;
                    _NotifyPropertyChanged(PROPERTY_NAME_TOOLTIP);
                }
            }
        }

        /// <summary>
        /// Is enabled flag.
        /// </summary>
        public override bool IsEnabled
        {
            get { return _isEnabled; }
            protected set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    _NotifyPropertyChanged(PROPERTY_NAME_ISENABLED);
                }
            }
        }

        #endregion // Overrided properties

        #region Overrided methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override void Initialize(App app)
        {
            base.Initialize(app);
            App.Current.ApplicationInitialized += new EventHandler(App_ApplicationInitialized);
        }

        protected override void _Execute(params object[] args)
        {
            Debug.Assert(null != _projectsPage);

            string filePath = null;
			bool needClearBusyState = false;
            string projectName = null;
            bool isAutoArchive = false;
            try
            {
                // select project to archiving
                isAutoArchive = ((1 == args.Length) && (null != args[0]));
                projectName = (isAutoArchive) ? args[0] as string : _projectsPage.SelectedProjectName;
                    // prescribed for command or selected project

                Debug.Assert(null != projectName);

                // find checked project configuration
                ProjectConfiguration config = _FindConfigByName(projectName);
                Debug.Assert(null != config);

                // set status
                string statusMessage = string.Format((string)_Application.FindResource("ArchiveMessageProcessStatusFormat"), projectName);
                WorkingStatusHelper.SetBusy(statusMessage);
				needClearBusyState = true;

                bool routingOperationsInProgress = false;
                if (projectName.Equals(_projectsPage.CurrentProjectName, StringComparison.InvariantCultureIgnoreCase))
                {   // check some routing operation is on progress
                    if (_Application.Solver.HasPendingOperations)
                    {
                        _Application.Messenger.AddWarning((string)_Application.FindResource("ArchiveMessageRoutingOperationsInProgress"));
                        routingOperationsInProgress = true;
                    }
                    else
                    {
                        // since archiving requires project to be closed, the command must close the project at first
                        _Application.CloseCurProject();
                        filePath = config.FilePath;
                    }
                }

                if (!routingOperationsInProgress)
                {   // archive it
                    ProjectArchivingSettings archivingSettings = config.ProjectArchivingSettings;
                    Debug.Assert(!archivingSettings.IsArchive);

                    DateTime date = DateTime.Now.Date.AddMonths(-archivingSettings.TimeDomain);
                    ArchiveResult result = ProjectFactory.ArchiveProject(config, date);
                    if (result.IsArchiveCreated)
                    {   // project was successfully archived
                        string message = string.Format((string)_Application.FindResource("ArchiveMessageProcessDoneFromat"),
                                                       projectName, Path.GetFileNameWithoutExtension(result.ArchivePath));
                        _Application.Messenger.AddInfo(message);
                        _projectsPage.UpdateView();
                    }
                    else
                    {   // command run for a project and there is nothing to archive actually
                        string message = string.Format((string)_Application.FindResource("ArchiveMessageNothingArchiveFormat"), projectName);
                        _Application.Messenger.AddWarning(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Critical(ex);

                Collection<MessageDetail> details = new Collection<MessageDetail>();
                details.Add(new MessageDetail(MessageType.Error, ex.Message));
                string message = string.Format((string)_Application.FindResource("ArchiveMessageProcessFailedFromat"), projectName);
                MessageType type = (isAutoArchive)? MessageType.Warning : MessageType.Error;
                _Application.Messenger.AddMessage(type, message, details);
            }
            if (needClearBusyState)
                WorkingStatusHelper.SetReleased();

            // open it again
            if (!string.IsNullOrEmpty(filePath))
                _Application.OpenProject(filePath, false);
        }

        #endregion // Overrided methods

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void App_ApplicationInitialized(object sender, EventArgs e)
        {
            App.Current.ProjectLoaded += new EventHandler(_ProjectLoaded);
            App.Current.ProjectClosing += new EventHandler(_ProjectClosing);

            _projectsPage = (ProjectsPage)_Application.MainWindow.GetPage(PagePaths.ProjectsPagePath);
            _projectsPage.XceedGrid.SelectionChanged += new Xceed.Wpf.DataGrid.DataGridSelectionChangedEventHandler(XceedGrid_SelectionChanged);
            _UpdateState();
        }

        private void _ProjectLoaded(object sender, EventArgs e)
        {
            _isProjectChanged = false;
            _UpdateState();
        }

        private void _ProjectClosing(object sender, EventArgs e)
        {
            _isProjectChanged = true;
            _UpdateState();
        }

        void XceedGrid_SelectionChanged(object sender, Xceed.Wpf.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            _UpdateState();
        }

        #endregion // Event handlers

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ProjectConfiguration _FindConfigByName(string name)
        {
            ProjectConfiguration config = null;
            foreach (ProjectConfiguration prj in _Application.ProjectCatalog.Projects)
            {
                if (prj.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    config = prj;
                    break; // NOTE: exit - founded
                }
            }

            return config;
        }

        private void _UpdateState()
        {
            bool isEnabled = false;
            string tooltipTextResourceName = "ArchiveProjectCommandDisabledTooltipProjectAbsent";
            if (!string.IsNullOrEmpty(_projectsPage.SelectedProjectName) && !_isProjectChanged)
            {
                ProjectConfiguration config = _FindConfigByName(_projectsPage.SelectedProjectName);
                    // NOTE: can be null when project deleted
                if (null != config)
                {
                    if (config.ProjectArchivingSettings.IsArchive)
                        tooltipTextResourceName = "ArchiveProjectCommandDisabledTooltipArchived";
                    else
                    {
                        tooltipTextResourceName = "ArchiveProjectCommandEnabledTooltip";
                        isEnabled = true;
                    }
                }
            }

            TooltipText = (string)App.Current.FindResource(tooltipTextResourceName);
            IsEnabled = isEnabled;
        }

        #endregion // Private helpers

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ProjectsPage _projectsPage = null;
        private bool _isProjectChanged = false;
        
        private bool _isEnabled = false;
        private string _tooltipText = null;

        private const string PROPERTY_NAME_TOOLTIP = "TooltipText";
        private const string PROPERTY_NAME_ISENABLED = "IsEnabled";
        
        #endregion // Private members
    }
}

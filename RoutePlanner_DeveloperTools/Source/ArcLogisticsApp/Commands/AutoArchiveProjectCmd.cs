using System;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.Archiving;
using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Commands
{
    class AutoArchiveProjectCmd : CommandBase
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string COMMAND_NAME = "ArcLogistics.Commands.AutoArchiveProject";
        private const string PROPERTY_NAME_TOOLTIP = "TooltipText";

        #endregion constants

        #region overridden properties
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
                if (value != _tooltipText)
                {
                    _tooltipText = value;
                    _NotifyPropertyChanged(PROPERTY_NAME_TOOLTIP);
                }
            }
        }

        #endregion overridden properties

        #region overridden methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected override void _Execute(params object[] args)
        {
            bool needClearBusyState = false;
            string projectName = null;
            try
            {
                Debug.Assert(args.Length > 0);

                string projectPath = args[0] as string;
                Debug.Assert(projectPath != null);

                // get project name
                projectName = Path.GetFileNameWithoutExtension(projectPath);

                // get project configuration
                ProjectConfiguration config = _FindConfigByName(projectName);
                if (config != null)
                {
                    // check if we need to auto-archive project
                    if (_NeedToAutoArchive(config))
                    {
                        // set status
                        string statusMessage = string.Format((string)_Application.FindResource("ArchiveMessageProcessStatusFormat"), projectName);
                        WorkingStatusHelper.SetBusy(statusMessage);
                        needClearBusyState = true;

                        ProjectArchivingSettings arSet = config.ProjectArchivingSettings;
                        DateTime date = DateTime.Now.Date.AddMonths(-arSet.TimeDomain);

                        ArchiveResult result = ProjectFactory.ArchiveProject(config, date);
                        _ShowResult(projectName, result);

                        if (result.IsArchiveCreated)
                        {   // Update project page
                            ProjectsPage projectsPage = (ProjectsPage)_Application.MainWindow.GetPage(PagePaths.ProjectsPagePath);
                            projectsPage.UpdateView();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                Collection<MessageDetail> details = new Collection<MessageDetail>();
                details.Add(new MessageDetail(MessageType.Error, ex.Message));

                string message = string.Format((string)_Application.FindResource("ArchiveMessageProcessFailedFromat"), projectName);
                _Application.Messenger.AddMessage(MessageType.Warning, message, details);
            }

            if (needClearBusyState)
                WorkingStatusHelper.SetReleased();
        }

        #endregion overridden methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private bool _NeedToAutoArchive(ProjectConfiguration config)
        {
            Debug.Assert(config != null);

            bool needToArchive = false;

            ProjectArchivingSettings arSet = config.ProjectArchivingSettings;
            if (arSet.IsAutoArchivingEnabled &&
                !arSet.IsArchive)
            {
                DateTime lastArchiveDate = arSet.LastArchivingDate != null ?
                    (DateTime)arSet.LastArchivingDate : config.CreationTime;

                DateTime mustArchiveDate = DateTime.Now.AddMonths(-arSet.AutoArchivingPeriod);
                if (mustArchiveDate.Date >= lastArchiveDate.Date)
                    needToArchive = true;
            }

            return needToArchive;
        }

        private ProjectConfiguration _FindConfigByName(string projectName)
        {
            Debug.Assert(projectName != null);

            ProjectConfiguration projectConfig = null;
            foreach (ProjectConfiguration config in _Application.ProjectCatalog.Projects)
            {
                if (config.Name.Equals(projectName, StringComparison.InvariantCultureIgnoreCase))
                {
                    projectConfig = config;
                    break;
                }
            }

            return projectConfig;
        }

        private void _ShowResult(string projectName, ArchiveResult result)
        {
            if (result.IsArchiveCreated)
            {
                string message = string.Format((string)_Application.FindResource("ArchiveMessageProcessDoneFromat"),
                                                projectName, Path.GetFileNameWithoutExtension(result.ArchivePath));
                _Application.Messenger.AddInfo(message);
            }
            else
            {   // project database does not contain data to archive
                string message = string.Format((string)_Application.FindResource("ArchiveMessageNothingArchiveFormat"), projectName);
                _Application.Messenger.AddWarning(message);
            }
        }

        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Tooltip text
        /// </summary>
        private string _tooltipText = null;

        #endregion private fields
    }
}

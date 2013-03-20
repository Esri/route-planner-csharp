using ESRI.ArcLogistics.BreaksHelpers;

namespace ESRI.ArcLogistics.App.Pages.Wizards
{
    /// <summary>
    /// Breaks setup wizard data context class.
    /// </summary>
    internal class BreaksSetupWizardDataContext : WizardDataContext
    {
        #region Public static members

        /// <summary>
        /// Project field name.
        /// </summary>
        public static string ProjectFieldName
        {
            get
            {
                return PROJECT_FIELD_NAME;
            }
        }

        /// <summary>
        /// Parent page field name.
        /// </summary>
        public static string ParentPageFieldName
        {
            get
            {
                return PARENTPAGE_FIELD_NAME;
            }
        }

        #endregion

        #region Public members

        /// <summary>
        /// Edited project.
        /// </summary>
        public Project Project
        {
            get
            {
                return this[PROJECT_FIELD_NAME] as Project;
            }
        }

        /// <summary>
        /// Project default breaks type.
        /// </summary>
        public BreakType? Breaks
        {
            get
            {
                return Project.BreaksSettings.BreaksType;
            }
        }

        /// <summary>
        /// Wizard parent page.
        /// </summary>
        public Page ParentPage
        {
            get
            {
                return this[PARENTPAGE_FIELD_NAME] as Page;
            }
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Project field name.
        /// </summary>
        private const string PROJECT_FIELD_NAME = "Project";

        /// <summary>
        /// Parent page field name.
        /// </summary>
        private const string PARENTPAGE_FIELD_NAME = "ParentPage";

        #endregion
    }
}
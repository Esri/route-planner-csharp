using System.Collections.Generic;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Pages.Wizards
{
    /// <summary>
    /// Fleet setup wizard data context class.
    /// </summary>
    internal class FleetSetupWizardDataContext : WizardDataContext
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
        /// Added orders field name.
        /// </summary>
        public static string AddedOrdersFieldName
        {
            get
            {
                return ADDED_ORDERS_FIELD_NAME;
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
        /// Added orders.
        /// </summary>
        public IList<Order> AddedOrders
        {
            get
            {
                return this[ADDED_ORDERS_FIELD_NAME] as IList<Order>;
            }
            set
            {
                this[ADDED_ORDERS_FIELD_NAME] = value;
            }
        }

        /// <summary>
        /// Project routes.
        /// </summary>
        public IDataObjectCollection<Route> Routes
        {
            get
            {
                return Project.DefaultRoutes;
            }
        }

        /// <summary>
        /// Project locations.
        /// </summary>
        public IDataObjectCollection<Location> Locations
        {
            get
            {
                return Project.Locations;
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
        /// Added orders field name.
        /// </summary>
        private const string ADDED_ORDERS_FIELD_NAME = "AddedOrders";

        /// <summary>
        /// Parent page field name.
        /// </summary>
        private const string PARENTPAGE_FIELD_NAME = "ParentPage";

        #endregion
    }
}

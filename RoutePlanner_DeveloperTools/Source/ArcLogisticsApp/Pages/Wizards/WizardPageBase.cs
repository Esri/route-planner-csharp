using System.Diagnostics;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.App.Pages.Wizards
{
    /// <summary>
    /// Base page class provide basic login on all wizard pages.
    /// </summary>
    internal class WizardPageBase : Grid
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public WizardPageBase()
        {
        }

        #endregion // Constructors

        #region Public virtual methods

        /// <summary>
        /// Inits state.
        /// </summary>
        /// <param name="dataContext">Fleet setup wizard data keeper.</param>
        public virtual void Initialize(WizardDataContext dataContext)
        {
            Debug.Assert(null != dataContext);

            _dataContext = dataContext;
        }

        #endregion // Public virtual methods

        #region Protected members

        /// <summary>
        /// Data keeper.
        /// </summary>
        protected WizardDataContext DataContext
        {
            get
            {
                return _dataContext;
            }
        }

        #endregion // Protected members

        #region Private fields

        /// <summary>
        /// Data keeper.
        /// </summary>
        private WizardDataContext _dataContext;

        #endregion // Private fields
    }
}

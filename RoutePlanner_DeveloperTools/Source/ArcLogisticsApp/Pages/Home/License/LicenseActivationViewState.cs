namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Represents state of the license activation panel.
    /// </summary>
    internal sealed class LicenseActivationViewState
    {
        /// <summary>
        /// Gets or sets a reference to the login view state.
        /// </summary>
        public object LoginViewState
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a reference to the information view state.
        /// </summary>
        public object InformationViewState
        {
            get;
            set;
        }
    }
}

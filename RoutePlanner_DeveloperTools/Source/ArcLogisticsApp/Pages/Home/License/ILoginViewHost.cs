namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Provides access to the object hosting login views.
    /// </summary>
    internal interface ILoginViewHost
    {
        /// <summary>
        /// Gets or sets a value indicating if focus was set on a control for entering username.
        /// </summary>
        bool IsUsernameControlFocused
        {
            get;
            set;
        }
    }
}

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Provides uri navigating facilities.
    /// </summary>
    internal interface IUriNavigator
    {
        /// <summary>
        /// Navigates to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to navigate to.</param>
        void NavigateToUri(string uri);
    }
}

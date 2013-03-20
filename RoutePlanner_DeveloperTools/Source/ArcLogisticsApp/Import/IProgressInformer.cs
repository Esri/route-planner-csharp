using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Interface import progress informer.
    /// </summary>
    internal interface IProgressInformer
    {
        /// <summary>
        /// Import object face name.
        /// </summary>
        string ObjectName { get; }

        /// <summary>
        /// Import objects face name.
        /// </summary>
        string ObjectsName { get; }

        /// <summary>
        /// Parent page for status panel.
        /// </summary>
        Page ParentPage { get; }

        /// <summary>
        /// Sets status message. Hide progress bar and button.
        /// </summary>
        /// <param name="statusNameRsc">Name of resource for status text.</param>
        void SetStatus(string statusNameRsc);
    }
}

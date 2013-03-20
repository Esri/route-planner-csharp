using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interface provides helpers for handling view context.
    /// </summary>
    internal interface IListViewContextHandler
    {
        /// <summary>
        /// Creates new object.
        /// </summary>
        void CreateNewItem(DataGridCreatingNewItemEventArgs e);

        /// <summary>
        /// Cancel creating new item.
        /// </summary>
        void CancellingNewItem(DataGridItemHandledEventArgs e);

        /// <summary>
        /// Adds created object to source collection.
        /// </summary>
        void CommitNewItem(DataGridCommittingNewItemEventArgs e);

        /// <summary>
        /// React on new item committed.
        /// </summary>
        void CommittedNewItem(DataGridItemEventArgs e);

        /// <summary>
        /// React on cancel edit item.
        /// </summary>
        void CancelEditItem(DataGridItemEventArgs e);

        /// <summary>
        /// React on commit item.
        /// </summary>
        void CommitItem(DataGridItemCancelEventArgs e);

        /// <summary>
        /// React on begin edit item.
        /// </summary>
        /// <param name="e"></param>
        void BeginEditItem(DataGridItemCancelEventArgs e);

        /// <summary>
        /// React on EditCommited.
        /// </summary>
        void CommitedEdit(DataGridItemEventArgs e);
    }
}

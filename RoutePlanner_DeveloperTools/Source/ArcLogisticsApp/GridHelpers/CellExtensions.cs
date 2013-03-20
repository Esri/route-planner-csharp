using ESRI.ArcLogistics.Utility.CoreEx;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// Helper extension methods for Xceed DataGrid cells.
    /// </summary>
    internal static class CellExtensions
    {
        /// <summary>
        /// Marks the specified cell as dirty.
        /// </summary>
        /// <param name="cell">The cell object to be marked dirty.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="cell"/> is a null
        /// reference.</exception>
        public static void MarkDirty(this Cell cell)
        {
            CodeContract.RequiresNotNull("cell", cell);

            var content = cell.Content;
            cell.Content = new object();
            cell.Content = content;
        }
    }
}

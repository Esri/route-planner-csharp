using System.Diagnostics;
using System.Windows.Documents;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Provides helper methods for flow documents.
    /// </summary>
    internal static class FlowDocumentHelpers
    {
        /// <summary>
        /// Creates flow document containing first block of the specified document.
        /// </summary>
        /// <param name="source">The source document to extract first block from.</param>
        /// <returns>A new flow document instance.</returns>
        public static FlowDocument ExtractFirstBlock(FlowDocument source)
        {
            Debug.Assert(source != null);

            if (source.Blocks.Count == 0)
            {
                return null;
            }

            var paragraphDocument = new FlowDocument()
            {
                Style = source.Style,
            };

            var block = source.Blocks.FirstBlock;
            source.Blocks.Remove(block);

            paragraphDocument.Blocks.Add(block);

            return paragraphDocument;
        }
    }
}

using System.ServiceModel.Channels;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Specifies format to which incoming messages from an ArcGIS servers should be mapped.
    /// </summary>
    internal sealed class ArcGisWebContentTypeMapper : WebContentTypeMapper
    {
        /// <summary>
        /// Gets correct message format for the specified content type.
        /// </summary>
        /// <param name="contentType">The MIME data type of the incoming message.</param>
        /// <returns>The web content format to map incoming message to.</returns>
        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            return WebContentFormat.Json;
        }
    }
}

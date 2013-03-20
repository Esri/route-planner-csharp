using System.Net;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Provides authentication facilities for ArcGIS servers.
    /// </summary>
    internal interface IAgsServerAuthenticator
    {
        /// <summary>
        /// Gets a value indicating whether ArcGIS server requests require
        /// tokens.
        /// </summary>
        bool RequiresTokens
        {
            get;
        }

        /// <summary>
        /// Gets last token instance.
        /// </summary>
        string LastToken
        {
            get;
        }

        /// <summary>
        /// Generates new token to be used for ArcGIS server requests.
        /// </summary>
        /// <param name="credential">Credential to be used for authenticating
        /// within ArcGIS server.</param>
        /// <returns>New token which can be used for ArcGIS server requests.</returns>
        string GenerateToken(NetworkCredential credential);
    }
}

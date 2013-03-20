using System.Diagnostics;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides access to REST service connection context.
    /// </summary>
    internal sealed class RestServiceContext
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the RestServiceContext class.
        /// </summary>
        /// <param name="context">The reference to the request context
        /// object for the REST service.</param>
        /// <param name="url">The url to be used for REST requests.</param>
        public RestServiceContext(RequestContext context, string url)
        {
            Debug.Assert(context != null);
            Debug.Assert(!string.IsNullOrEmpty(url));

            this.Context = context;
            this.Url = url;
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets reference to the REST servce request context.
        /// </summary>
        public RequestContext Context
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets url for REST requests.
        /// </summary>
        public string Url
        {
            get;
            private set;
        }
        #endregion
    }
}

using System;

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Class that represents geocoding exception.
    /// </summary>
    public class GeocodeException : Exception
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>GeocodeException</c> class.
        /// </summary>
        public GeocodeException()
            : base(Properties.Resources.GeocodeOperationFailed)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>GeocodeException</c> class.
        /// </summary>
        /// <param name="message">Exception description.</param>
        public GeocodeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>GeocodeException</c> class.
        /// </summary>
        /// <param name="message">Exception description.</param>
        /// <param name="inner">Inner exception.</param>
        public GeocodeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion constructors
    }

}
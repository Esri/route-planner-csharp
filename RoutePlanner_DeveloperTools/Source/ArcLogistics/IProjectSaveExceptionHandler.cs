using System;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Interface represents project's Save method exception handler.
    /// </summary>
    public interface IProjectSaveExceptionHandler
    {
        /// <summary>
        /// Exception handler.
        /// </summary>
        /// <param name="e">Exception.</param>
        /// <returns>Returns 'false' if this exeption must be thrown, 'true' otherwise</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="e"/>
        /// argument is a null reference.</exception>
        bool HandleException(Exception e);
    }
}

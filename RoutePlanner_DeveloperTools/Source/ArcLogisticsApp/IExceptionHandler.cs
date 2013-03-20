using System;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Provides a way to abstract exception handling policy.
    /// </summary>
    internal interface IExceptionHandler
    {
        /// <summary>
        /// Handles the specified exception in an implementation-specific way.
        /// </summary>
        /// <param name="exceptionToHandle">The reference to the exception
        /// object to be handled.</param>
        /// <returns>True if and only if the exception was handled and need not
        /// be rethrown.</returns>
        bool HandleException(Exception exceptionToHandle);
    }
}

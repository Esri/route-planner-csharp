using System;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Encapsulates application working status managing.
    /// </summary>
    internal interface IWorkingStatusController
    {
        /// <summary>
        /// Sets application busy state with the specified status.
        /// </summary>
        /// <param name="status">The string describing busy state reason.</param>
        /// <returns><see cref="System.IDisposable"/> instance which should
        /// be disposed upon exiting from the busy state.</returns>
        /// <remarks><see cref="M:WorkingStatusHelper.EnterBusyState"/> method
        /// description for more details.
        /// </remarks>
        IDisposable EnterBusyState(string status);
    }
}

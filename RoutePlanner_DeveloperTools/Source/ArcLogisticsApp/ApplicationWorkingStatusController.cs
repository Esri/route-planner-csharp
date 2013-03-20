using System;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Implements <see cref="T:ESRI.ArcLogistics.App.IWorkingStatusController"/>
    /// interface by changing application status with
    /// <see cref="T:ESRI.ArcLogistics.App.WorkingStatusHelper"/> class.
    /// </summary>
    internal sealed class ApplicationWorkingStatusController : IWorkingStatusController
    {
        #region IWorkingStatusController Members
        /// <summary>
        /// Sets application busy state with the specified status.
        /// </summary>
        /// <param name="status">The string describing busy state reason.</param>
        /// <returns><see cref="System.IDisposable"/> instance which should
        /// be disposed upon exiting from the busy state.</returns>
        /// <remarks><see cref="M:WorkingStatusHelper.EnterBusyState"/> method
        /// description for more details.
        /// </remarks>
        public IDisposable EnterBusyState(string status)
        {
            return WorkingStatusHelper.EnterBusyState(status);
        }
        #endregion
    }

}

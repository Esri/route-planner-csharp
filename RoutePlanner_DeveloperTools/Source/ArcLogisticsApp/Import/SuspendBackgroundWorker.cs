using System.Threading;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Class provide functions to execute an operation on a separate thread
    /// with suspend functionality.
    /// </summary>
    internal sealed class SuspendBackgroundWorker : BackgroundWorker
    {
        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Blocks the current thread until the receives resume signal.
        /// Call this function for stop process if need.
        /// </summary>
        public void WaitResume()
        {
            _event.WaitOne();
        }

        /// <summary>
        /// Resumes one or more waiting threads to proceed.
        /// </summary>
        public void Resume()
        {
            _event.Set();
        }

        /// <summary>
        /// Suspends one or more threads to block.
        /// </summary>
        public void Suspend()
        {
            _event.Reset();
        }

        #endregion // Public methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Event for suspend worker.
        /// </summary>
        private ManualResetEvent _event = new ManualResetEvent(true);

        #endregion // Private members
    }
}

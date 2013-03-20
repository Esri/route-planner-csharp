using System.Diagnostics;
using System.ComponentModel;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// BackgroundWorkCancelTracker class.
    /// ICancelTracker implementation of background worker.
    /// </summary>
    internal class BackgroundWorkCancelTracker : ICancelTracker
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public BackgroundWorkCancelTracker(BackgroundWorker worker)
        {
            Debug.Assert(worker != null);
            _worker = worker;
        }

        #endregion constructors

        #region ICancelTracker interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public bool IsCancelled
        {
            get { return _worker.CancellationPending; }
        }

        #endregion ICancelTracker interface members

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private BackgroundWorker _worker;

        #endregion private fields
    }
}

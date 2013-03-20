using System.Diagnostics;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Class that implements cancelation process tracker for import.
    /// </summary>
    internal sealed class CancellationTracker : ICancellationChecker
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>CancellationTracker</c> class.
        /// </summary>
        /// <param name="tracker">Cancel tracker.</param>
        public CancellationTracker(ImportCancelTracker tracker)
        {
            Debug.Assert(null != tracker); // created
            _tracker = tracker;
        }

        #endregion // Constructors

        #region ICancellationChecker interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This property indicates whether cancellation has been requested for this token source.
        /// </summary>
        public bool IsCancellationRequested
        {
            get
            {
                Debug.Assert(null != _tracker);
                return _tracker.IsCancelled;
            }
        }

        /// <summary>
        /// Checks cancel state if operation was canceled throw excemtion.
        /// </summary>
        /// <exception cref="ESRI.ArcLogistics.UserBreakException">
        /// Operation was cancelled.
        /// </exception>
        public void ThrowIfCancellationRequested()
        {
            if (null != _tracker)
            {
                if (IsCancellationRequested)
                {
                    throw new UserBreakException(); // exception
                }
            }
        }

        #endregion // IStateChecker interface members

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cancel tracker.
        /// </summary>
        private ImportCancelTracker _tracker;

        #endregion // Private methods
    }
}

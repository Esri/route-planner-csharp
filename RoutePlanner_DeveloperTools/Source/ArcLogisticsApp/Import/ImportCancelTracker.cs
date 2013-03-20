using System.Windows;
using System.Diagnostics;

using ESRI.ArcLogistics.App.Dialogs;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Import cancel tracker class.
    /// Special implementation <c>ICancelTracker</c> for import process by BackgroundWorker.
    /// </summary>
    internal sealed class ImportCancelTracker : ICanceler, ICancelTracker
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>ImportCancelTracker</c> class.
        /// </summary>
        /// <param name="worker">Background worker.</param>
        public ImportCancelTracker(SuspendBackgroundWorker worker)
        {
            Debug.Assert(null != worker); // created
            _worker = worker;
        }

        #endregion // Constructors

        #region ICanceler interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Does cancel operation.
        /// Block operation. Show real stop question.
        /// </summary>
        public void Cancel()
        {
            _worker.Suspend();

            // show real stop question
            App currentApp = App.Current;
            string text = currentApp.FindString("ImportProcessCanceledQuestion");
            string caption = currentApp.FindString("ApplicationTitle");

            MessageBoxExButtonType result =
                MessageBoxEx.Show(currentApp.MainWindow,
                                  text, caption,
                                  System.Windows.Forms.MessageBoxButtons.YesNo,
                                  MessageBoxImage.Question);

            // user choice "Yes" do cancel
            if (MessageBoxExButtonType.Yes == result)
            {
                if (_worker.IsBusy)
                    _worker.CancelAsync();
            }

            _worker.Resume();
        }

        #endregion // ICanceler interface members

        #region ICancelTracker interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns a boolean value indicating whether an operation was canceled.
        /// </summary>
        public bool IsCancelled
        {
            get
            {
                bool cancelled = false;
                if (null != _worker)
                {
                    _worker.WaitResume();

                    cancelled = _worker.CancellationPending;
                }

                return cancelled;
            }
        }

        #endregion // ICancelTracker interface members

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Background worker.
        /// </summary>
        private SuspendBackgroundWorker _worker;

        #endregion // Private fields
    }
}

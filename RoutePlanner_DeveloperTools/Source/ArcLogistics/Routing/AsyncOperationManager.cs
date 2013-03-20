using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// AsyncOperationManager class.
    /// </summary>
    internal class AsyncOperationManager
    {
        #region public events
        /// <summary>
        /// Raises when async. solve task completed.
        /// </summary>
        public event AsyncSolveCompletedEventHandler AsyncSolveCompleted;
        #endregion public events

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a boolean value indicating if there are pending operations.
        /// </summary>
        public bool HasPendingOperations
        {
            get { return _workers.Count > 0; }
        }

        #endregion public properties

        #region public methods
        /// <summary>
        /// Runs the specified solve task asynchronously.
        /// </summary>
        /// <param name="task">Task for the solve operation to be run asynchronously.</param>
        /// <returns>The operation identifier.</returns>
        public Guid RunAsync(IAsyncSolveTask task)
        {
            Debug.Assert(task != null);

            var id = Guid.NewGuid();
            _RunAsync(task, id);

            return id;
        }

        /// <summary>
        /// Cancels asynchronous operation.
        /// </summary>
        /// <param name="operationId">Operation id.</param>
        /// <returns>
        /// true if operation is successfully cancelled, false if operation
        /// with specified id was not found.
        /// </returns>
        /// <remarks>
        /// Method raises SolveCompleted event and then makes operation info
        /// inaccessible.
        /// </remarks>
        public bool CancelAsync(Guid operationId)
        {
            bool res = false;

            BackgroundWorker bw = _FindWorker(operationId);
            if (bw != null)
            {
                bw.CancelAsync();
                try
                {
                    // raise operation cancelled event
                    // WARNING: we don't wait when worker thread will be actually completed
                    _NotifyAsyncSolveCompleted(null, true, operationId);
                }
                finally
                {
                    _workers.Remove(bw);
                }

                res = true;
            }

            return res;
        }

        public void ShutdownWorkers()
        {
            foreach (var entry in _workers)
            {
                BackgroundWorker bw = entry.Key;

                // detach events
                bw.DoWork -= _BackgroundWorkerDoWork;
                bw.RunWorkerCompleted -= _BackgroundWorkerRunWorkerCompleted;

                // request cancellation 
                bw.CancelAsync();
            }

            _workers.Clear();
        }

        #endregion public methods

        #region private data types
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Stores context for an asynchronous operation.
        /// </summary>
        private class AsyncSolveContext
        {
            /// <summary>
            /// Gets or sets a reference to the task for running solve operation.
            /// </summary>
            public IAsyncSolveTask Task
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets operation identifier.
            /// </summary>
            public Guid OperationID
            {
                get;
                set;
            }
        }
        #endregion private data types

        #region private methods
        private void _RunAsync(IAsyncSolveTask task, Guid operationID)
        {
            Debug.Assert(task != null);

            var context = new AsyncSolveContext
            {
                Task = task,
                OperationID = operationID,
            };

            var worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += _BackgroundWorkerDoWork;
            worker.RunWorkerCompleted += _BackgroundWorkerRunWorkerCompleted;

            _workers.Add(worker, context);

            worker.RunWorkerAsync(task);
        }

        private void _BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            Debug.Assert(sender is BackgroundWorker);
            Debug.Assert(e.Argument is IAsyncSolveTask);

            BackgroundWorker bw = sender as BackgroundWorker;
            try
            {
                BackgroundWorkCancelTracker tracker = new BackgroundWorkCancelTracker(bw);

                var task = (IAsyncSolveTask)e.Argument;
                var result = task.Run(tracker);

                if (tracker.IsCancelled)
                    e.Cancel = true;
                else
                    e.Result = result;
            }
            catch (UserBreakException)
            {
                e.Cancel = true;
            }
        }

        private void _BackgroundWorkerRunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;

            try
            {
                AsyncSolveContext ctx = null;
                if (_workers.TryGetValue(bw, out ctx))
                {
                    _HandleWorkerCompleted(e, ctx);
                }
                else
                {
                    // normally haveWorker = false only if operation was cancelled
                    Debug.Assert(e.Cancelled);
                }
            }
            finally
            {
                bw.Dispose(); // actually does nothing
                _workers.Remove(bw);
            }
        }

        private void _HandleWorkerCompleted(
            RunWorkerCompletedEventArgs e,
            AsyncSolveContext ctx)
        {
            Guid id = ctx.OperationID;

            // NOTE: don't need to handle Cancelled status here, we raise SolveCompleted
            // event immediately after setting worker to cancellation state
            if (e.Cancelled)
            {
                return;
            }

            if (e.Error != null)
            {
                // operation failed
                _NotifyAsyncSolveCompleted(e.Error, false, id);

                return;
            }

            var operation = ctx.Task;
            var resultProvider = (Func<SolveTaskResult>)e.Result;

            bool isConverted = false;
            Exception error = null;
            var result = _ConvertResult(resultProvider, out error);
            if (result != null)
            {
                // check if we need to run next task.
                var nextTask = result.NextTask;
                if (nextTask != null)
                {
                    // run next task with the same operation id.
                    isConverted = _TryRunAsync(nextTask, id, out error);
                }
                else
                {
                    // converted successfully, provide the result
                    _NotifyAsyncSolveCompleted(null, false, id, result.SolveResult);
                    isConverted = true;
                }
            }

            if (!isConverted)
            {
                // conversion failed, set error
                _NotifyAsyncSolveCompleted(error, false, id);
            }
        }

        private SolveTaskResult _ConvertResult(
            Func<SolveTaskResult> resultProvider,
            out Exception error)
        {
            Debug.Assert(resultProvider != null);

            SolveTaskResult result = null;
            error = null;
            try
            {
                result = resultProvider();
            }
            catch (Exception e)
            {
                error = e;
            }

            return result;
        }

        private bool _TryRunAsync(IAsyncSolveTask operation, Guid operationId,
            out Exception error)
        {
            error = null;

            bool res = false;
            try
            {
                _RunAsync(operation, operationId);
                res = true;
            }
            catch (Exception e)
            {
                error = e;
            }

            return res;
        }

        private BackgroundWorker _FindWorker(Guid operationId)
        {
            BackgroundWorker worker = null;
            foreach (var entry in _workers)
            {
                if (entry.Value.OperationID == operationId)
                {
                    worker = entry.Key;
                    break;
                }
            }

            return worker;
        }

        private void _NotifyAsyncSolveCompleted(Exception error,
            bool cancelled,
            Guid operationId)
        {
            if (AsyncSolveCompleted != null)
            {
                AsyncSolveCompleted(this,
                    new AsyncSolveCompletedEventArgs(error, cancelled, operationId));
            }
        }

        private void _NotifyAsyncSolveCompleted(Exception error,
            bool cancelled,
            Guid operationId,
            SolveResult result)
        {
            if (AsyncSolveCompleted != null)
            {
                AsyncSolveCompleted(this,
                    new AsyncSolveCompletedEventArgs(error, cancelled, operationId, result));
            }
        }

        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Dictionary<BackgroundWorker, AsyncSolveContext> _workers = new Dictionary<
            BackgroundWorker, AsyncSolveContext>();

        #endregion private fields
    }
    
}

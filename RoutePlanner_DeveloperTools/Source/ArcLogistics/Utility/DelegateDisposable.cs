using System;

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Implements <see cref="System.IDisposable"/> by running the provided delegate.
    /// </summary>
    internal class DelegateDisposable : IDisposable
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the DelegateDisposable class with
        /// the specified action.
        /// </summary>
        /// <param name="disposeAction">The action to be performed upon
        /// disposal.</param>
        public DelegateDisposable(Action disposeAction)
        {
            if (disposeAction == null)
            {
                throw new ArgumentNullException("disposeAction");
            }

            _disposeAction = disposeAction;
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Implements disposing by calling associated action.
        /// </summary>
        public void Dispose()
        {
            if (_disposeAction != null)
            {
                _disposeAction();
                _disposeAction = null;
            }
        }
        #endregion

        #region private fields
        /// <summary>
        /// The action to be performed upon disposal.
        /// </summary>
        private Action _disposeAction;
        #endregion
    }
}

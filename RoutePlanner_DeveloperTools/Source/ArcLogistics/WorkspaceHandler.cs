using System;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// SaveExceptionHandler implementation for ProjectWorkspaceCe.
    /// Wraps another IProjectSaveExceptionHandler implementation and throw exception if this 
    /// implementation unhandled or if property Handled set to 'false'.
    /// </summary>
    internal class WorkspaceHandler : IProjectSaveExceptionHandler
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="handler">Implementation of IProjectSaveExceptionHandler.</param>
        internal WorkspaceHandler(IProjectSaveExceptionHandler handler)
        {
            _handler = handler;
            Handled = false;
        }

        #endregion

        #region IProjectSaveExceptionHandler methods

        /// <summary>
        /// Exception handler.
        /// </summary>
        /// <param name="e">Exception.</param>
        /// <returns>Returns 'false' if this exeption must be thrown, 'true' otherwise</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="e"/>
        /// argument is a null reference.</exception>
        public bool HandleException(Exception e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            // If it was unhadled in inner handler then return false.
            if (!_handler.HandleException(e))
            {
                return false;
            }

            return Handled;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Flag shows was exception handled or not. False by default.
        /// </summary>
        public bool Handled
        {
            get;
            set;
        }

        #endregion

        #region Private members

        /// <summary>
        /// Some implementation of IProjectSaveExceptionHandler.
        /// </summary>
        private IProjectSaveExceptionHandler _handler;

        #endregion
    }
}

using System;
using System.IO;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// App.xaml.cs implementation of SaveExceptionHandler.
    /// If there was exceptions with DB or project config access - show message in Message Window.
    /// </summary>
    internal class AppSaveExceptionHandler : ESRI.ArcLogistics.IProjectSaveExceptionHandler
    {
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

            bool handled = false;

            // If we have problem with configuration file - show the message.
            if (e is IOException || e is UnauthorizedAccessException)
            {
                App.Current.Messenger
                    .AddError((string)App.Current.FindResource("CantWriteToProjectConfiguration"));
                handled = true;
            }
            // If we have problem with database file - show the message.
            if (e is System.Data.UpdateException || e is System.Data.EntityException)
            {
                App.Current.Messenger.AddError((string)App.Current.FindResource("CantWriteToDB"));
                handled = true;
            }

            Logger.Error(e);
            return handled;
        }

        #endregion
    }
}

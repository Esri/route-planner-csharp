using System;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// UI Manager interface
    /// </summary>
    internal interface IUIManager
    {
        /// <summary>
        /// Lock MainWindow UI.
        /// </summary>
        /// <param name="lockPageFrame">Lock page frame flag.</param>
        void Lock(bool lockPageFrame);

        /// <summary>
        /// Unlock MainWindow UI.
        /// </summary>
        void Unlock();

        /// <summary>
        /// MainWindow UI is loked flag.
        /// </summary>
        bool IsLocked { get; }

        /// <summary>
        /// Lock message window UI.
        /// </summary>
        void LockMessageWindow();

        /// <summary>
        /// Unlock message window UI.
        /// </summary>
        void UnlockMessageWindow();

        /// <summary>
        /// Fires when MainWindow UI locked.
        /// </summary>
        event EventHandler Locked;

        /// <summary>
        /// Fires when MainWindow UI unlocked.
        /// </summary>
        event EventHandler UnLocked;
    }
}

using System;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Provides a way to commit/rollback changes to data objects.
    /// </summary>
    internal interface IDataObjectContext : IDisposable
    {
        /// <summary>
        /// Commits all changes to the current data object context.
        /// </summary>
        void Commit();
    }
}

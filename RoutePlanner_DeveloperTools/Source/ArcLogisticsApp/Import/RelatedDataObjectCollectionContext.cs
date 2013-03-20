using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Implements <see cref="T:ESRI.ArcLogistics.App.Import.IDataObjectContext"/> for a collection
    /// of data objects.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the collection.</typeparam>
    internal sealed class RelatedDataObjectCollectionContext<T> : IDataObjectContext
        where T : DataObject
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the RelatedDataObjectCollectionContext class.
        /// </summary>
        /// <param name="source">Source collection of data objects to track changes for.</param>
        public RelatedDataObjectCollectionContext(IDataObjectCollection<T> source)
        {
            Debug.Assert(source != null);
            Debug.Assert(source.All(item => item != null));

            _source = source;
            _Reset();
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets a reference to the collection of related data objects.
        /// </summary>
        public IDataObjectContainer<T> DataObjects
        {
            get;
            private set;
        }
        #endregion


        #region IDataObjectContext Members
        /// <summary>
        /// Commits all changes to the <see cref="P:DataObjects"/> collection.
        /// </summary>
        public void Commit()
        {
            foreach (var item in this.DataObjects.GetAddedObjects())
            {
                _source.Add(item);
            }

            _Reset();
        }

        #endregion

        #region IDisposable Members
        /// <summary>
        /// Rollbacks changes made to the <see cref="P:DataObjects"/> collection.
        /// </summary>
        public void Dispose()
        {
            _Reset();
        }
        #endregion

        #region private members
        /// <summary>
        /// Re-initializes <see cref="P:DataObjects"/> property.
        /// </summary>
        private void _Reset()
        {
            this.DataObjects = new DataObjectContainer<T>(_source);
        }
        #endregion

        #region private fields
        /// <summary>
        /// Stores reference to the source data objects collection.
        /// </summary>
        private IDataObjectCollection<T> _source;
        #endregion
    }
}

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Stores collection element along with its index in the collection.
    /// </summary>
    /// <typeparam name="T">The type of an element to be stored.</typeparam>
    internal sealed class Indexed<T>
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the Indexed class.
        /// </summary>
        /// <param name="item">The element of the collection to add index for.</param>
        /// <param name="index">The index of an element.</param>
        public Indexed(T item, int index)
        {
            this.Value = item;
            this.Index = index;
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets collection element.
        /// </summary>
        public T Value
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets collection element index.
        /// </summary>
        public int Index
        {
            get;
            private set;
        }
        #endregion
    }
}

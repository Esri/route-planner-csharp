namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Provides access to loading/saving objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects to be loaded/saved by the storage.</typeparam>
    internal interface IGenericStorage<T>
        where T : new()
    {
        /// <summary>
        /// Loads object of type <typeparamref name="T"/> from implementation specific storage.
        /// </summary>
        /// <returns>Reference to the loaded object.</returns>
        T Load();

        /// <summary>
        /// Saves object of type <typeparamref name="T"/> to the implementation specific storage.
        /// </summary>
        /// <param name="obj">The reference to the object to be saved.</param>
        /// <exception cref="System.ArgumentException"><paramref name="obj"/> is a null
        /// reference.</exception>
        void Save(T obj);
    }
}

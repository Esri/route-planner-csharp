using System.Collections.Generic;

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Provides helper methods for <see cref="KeyValuePair&lt;TKey, TValue&gt;"/>.
    /// </summary>
    internal static class KeyValuePair
    {
        /// <summary>
        /// Creates a new <see cref="KeyValuePair&lt;TKey, TValue&gt;"/> object.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key component of the pair.</param>
        /// <param name="value">The value component of the pair.</param>
        /// <returns>A new <see cref="KeyValuePair&lt;TKey, TValue&gt;"/> instance with
        /// the specified key and value.</returns>
        public static KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }
}

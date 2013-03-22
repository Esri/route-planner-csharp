/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Provides additional extension methods for sequences.
    /// </summary>
    internal static class EnumerableEx
    {
        /// <summary>
        /// Converts the specified class reference to the enumerable that contains
        /// a single value.
        /// </summary>
        /// <typeparam name="TSource">The type of the value to be wrapped with
        /// enumerable.</typeparam>
        /// <param name="value">The reference to be wrapped with enumerable.</param>
        /// <returns>Enumerable that contains the specified value or empty
        /// enumerable if the value is null.</returns>
        /// <remarks>This method provides a way to treat object references as
        /// one-element sequences.</remarks>
        public static IEnumerable<TSource> ToEnumerable<TSource>(TSource value)
            where TSource : class
        {
            if (value != null)
            {
                yield return value;
            }
        }

        /// <summary>
        /// Returns enumerable that contains a single value.
        /// </summary>
        /// <typeparam name="TSource">The type of the value to be wrapped with
        /// enumerable.</typeparam>
        /// <param name="value">The value to be wrapped with enumerable.</param>
        /// <returns>Enumerable that contains the specified value.</returns>
        public static IEnumerable<TSource> Return<TSource>(TSource value)
        {
            yield return value;
        }

        /// <summary>
        /// Converts the specified collection to an indexed collection.
        /// </summary>
        /// <typeparam name="T">The type of collection elements.</typeparam>
        /// <param name="source">The collection to be indexed.</param>
        /// <returns>A reference to the collection of elements from the <paramref name="source"/>
        /// collection along with their indices.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> argument
        /// is a null reference.</exception>
        public static IEnumerable<Indexed<T>> ToIndexed<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return source.Select((item, index) => new Indexed<T>(item, index));
        }

        /// <summary>
        /// Executes the specified action for each element in the specified collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="source">The reference to a collection which elements will be
        /// processed by <paramref name="action"/>.</param>
        /// <param name="action">The <see cref="System.Action&lt;T&gt;"/> to be invoked
        /// for <paramref name="source"/> elements.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or
        /// <paramref name="action"/> argument is a null reference.</exception>
        public static void Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            foreach (var item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// Splits the specified collection into the collection of chunks of
        /// the specified size.
        /// </summary>
        /// <param name="source">The reference to the collection to be split.</param>
        /// <param name="maxChunkSize">The maximum size of the chunks to be created.</param>
        /// <returns>The collection of chunks of the specified size. The last chunk
        /// in the collection could be of smaller size if there were not enough
        /// elements in the <paramref name="source"/> collection.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> argument
        /// is a null reference.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxChunkSize"/>
        /// argument is less than or equal to zero.</exception>
        public static IEnumerable<IList<T>> SplitToChunks<T>(
            this IEnumerable<T> source,
            int maxChunkSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (maxChunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException("maxChunkSize");
            }

            return _SplitToChunks(source, maxChunkSize);
        }

        /// <summary>
        /// Converts the specified collection to the <see cref="HashSet&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T">The type of collection elements.</typeparam>
        /// <param name="source">The collection to be converted.</param>
        /// <returns>A new HashSet object with elements from <paramref name="source"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> argument
        /// is a null reference.</exception>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return new HashSet<T>(source);
        }

        #region private static methods
        /// <summary>
        /// Implements <see cref="SplitToChunks&lt;T&gt;"/> method using iterator block.
        /// </summary>
        /// <param name="source">The reference to the collection to be split.</param>
        /// <param name="maxChunkSize">The maximum size of the chunks to be created.</param>
        /// <returns>The collection of chunks of the specified size. The last chunk
        /// in the collection could be of smaller size if there were not enough
        /// elements in the <paramref name="source"/> collection.</returns>
        private static IEnumerable<IList<T>> _SplitToChunks<T>(
            IEnumerable<T> source,
            int maxChunkSize)
        {
            var result = new List<T>(maxChunkSize);
            var index = 0;
            foreach (var item in source)
            {
                result.Add(item);
                ++index;

                if (index == maxChunkSize)
                {
                    yield return result.ToList();
                    result.Clear();
                    index = 0;
                }
            }

            if (index > 0)
            {
                yield return result.ToList();
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using ESRI.ArcLogistics.Utility.CoreEx;

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Provides access to generic algorithms implementations for <see cref="IList&lt;T&gt;"/>
    /// objects.
    /// </summary>
    internal static class ListAlgorithms
    {
        /// <summary>
        /// Removes all elements in the specified collection for which the specified predicate
        /// returns true.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The collection to remove elements from.</param>
        /// <param name="predicate">The predicate to be used for finding elements
        /// to be removed.</param>
        /// <returns>The index in the <paramref name="source"/> collection such that all elements
        /// in range [0, index) do not satisfy the <paramref name="predicate"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or
        /// <paramref name="predicate"/> is a null reference.</exception>
        /// <remarks>The relative order of element that are not removed is the same as their
        /// relative order before removal of other elements.</remarks>
        public static int RemoveIf<T>(
            this IList<T> source,
            Func<T, bool> predicate)
        {
            CodeContract.RequiresNotNull("source", source);
            CodeContract.RequiresNotNull("predicate", predicate);

            return source.RemoveIf(0, source.Count, predicate);
        }

        /// <summary>
        /// Removes elements in the specified collection for which the specified predicate
        /// returns true in the range [first, last).
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The collection to remove elements from.</param>
        /// <param name="first">The index in the <paramref name="source"/> collection specifying
        /// the beginning of the elements range to perform removing at.</param>
        /// <param name="last">The index in the <paramref name="source"/> collection specifying
        /// the end of the elements range to perform removing at.</param>
        /// <param name="predicate">The predicate to be used for finding elements
        /// to be removed.</param>
        /// <returns>The index in the <paramref name="source"/> collection such that all elements
        /// in range [0, index) do not satisfy the <paramref name="predicate"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or
        /// <paramref name="predicate"/> is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="first"/> or
        /// <paramref name="last"/> is not in [0, <paramref name="source"/>.Count] range or
        /// <paramref name="first"/> is greater than <paramref name="last"/>.</exception>
        /// <remarks>The relative order of element that are not removed is the same as their
        /// relative order before removal of other elements.</remarks>
        public static int RemoveIf<T>(
            this IList<T> source,
            int first,
            int last,
            Func<T, bool> predicate)
        {
            CodeContract.RequiresNotNull("source", source);
            CodeContract.RequiresValueInRange("first", first, 0, source.Count);
            CodeContract.RequiresValueInRange("last", last, 0, source.Count);
            CodeContract.RequiresLessThanOrEqual("first", first, last);
            CodeContract.RequiresNotNull("predicate", predicate);

            var next = first;
            for (; first != last; ++first)
            {
                if (!predicate(source[first]))
                {
                    var tmp = source[next];
                    source[next] = source[first];
                    source[first] = tmp;

                    ++next;
                }
            }

            return next;
        }
    }
}

using System;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Provides set of extension methods for implementing query operators on
    /// objects of <see cref="T:System.Nullable`1"/> type treating them as enumerables
    /// with only one element at most.
    /// </summary>
    internal static class NullableEx
    {
        /// <summary>
        /// Converts <paramref name="source"/> to the collection containing no elements if the
        /// source is null and a signle element with value wrapped in source
        /// otherwise.
        /// </summary>
        /// <typeparam name="TSource">Type of the value wrapped in the
        /// <paramref name="source"/>.</typeparam>
        /// <param name="source">The nullable instance to be converted into the collection.</param>
        /// <returns><see cref="T:System.Collections.Generic.IEnumerable&lt;TSource&gt;"/>
        /// instance containing no elements if <paramref name="source"/> is null or
        /// value wrapped in the <paramref name="source"/> otherwise.</returns>
        public static IEnumerable<TSource> AsEnumerable<TSource>(
            this Nullable<TSource> source)
            where TSource : struct
        {
            if (source.HasValue)
            {
                yield return source.Value;
            }
        }

        /// <summary>
        /// Converts <paramref name="source"/> to the nullable containing its first
        /// element.
        /// </summary>
        /// <typeparam name="TSource">Type of values in the collection.</typeparam>
        /// <param name="source">The reference to the collection to be converted
        /// to the nullable.</param>
        /// <returns><see cref="T:System.Nullable&lt;TSource&gt;"/> containing
        /// the first value of the <paramref name="source"/> or null if there 
        /// are no values in the <paramref name="source"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/>
        /// is a null reference.</exception>
        public static Nullable<TSource> ToNullable<TSource>(
            this IEnumerable<TSource> source)
            where TSource : struct
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            foreach (var item in source)
            {
                return item;
            }

            return null;
        }

        /// <summary>
        /// Projects value wrapped in the nullable into a new one.
        /// </summary>
        /// <typeparam name="TSource">Type of the value wrapped in the
        /// <paramref name="source"/>.</typeparam>
        /// <typeparam name="TResult">Type of the value returned by
        /// <paramref name="selector"/>.</typeparam>
        /// <param name="source">Nullable wrapping a value to apply
        /// <paramref name="selector"/> to.</param>
        /// <param name="selector">A reference to the transform function to be
        /// applied to the value wrapped with <paramref name="source"/>.</param>
        /// <returns>A <see cref="T:System.Nullable&lt;TResult&gt;"/> containing
        /// the result of applying <paramref name="selector"/> to the value
        /// wrapped with <paramref name="source"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="selector"/>
        /// is a null reference.</exception>
        /// <example>
        /// The function can be used to simplify handling of nullable values.
        /// Consider you need to extract Date portion from a nullable wrapping an
        /// instance of the <see cref="T:System.DateTime"/>.
        /// <![CDATA[
        /// DateTime? currentDateTime = DateTime.Now;
        /// ]]>
        /// With explicit checking for <see cref="P:System.Nullable`1.HasValue"/>
        /// the code will look like this:
        /// <![CDATA[
        /// DateTime? currentDate = null;
        /// if (currentDateTime.HasValue)
        /// {
        ///     currentDate = currentDateTime.Value.Date;
        /// }
        /// ]]>
        /// Using this extension method the code could be simplified to this:
        /// <![CDATA[
        /// DateTime? currentDate = currentDateTime.Select(value => value.Date);
        /// ]]>
        /// or using LINQ query syntax:
        /// <![CDATA[
        /// DateTime? currentDate =
        ///     from value in currentDateTime
        ///     select value.Date;
        /// ]]>
        /// </example>
        public static Nullable<TResult> Select<TSource, TResult>(
            this Nullable<TSource> source,
            Func<TSource, TResult> selector)
            where TSource : struct
            where TResult : struct
        {
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            return source.SelectMany(value => (TResult?)selector(value));
        }

        /// <summary>
        /// Projects value wrapped in the nullable into another nullable.
        /// </summary>
        /// <typeparam name="TSource">Type of the source value.</typeparam>
        /// <typeparam name="TResult">Type of the resulting nullable.</typeparam>
        /// <param name="source">The source nullable wrapping value to be passed
        /// to the <paramref name="selector"/>.</param>
        /// <param name="selector">The function to be applied to the value wrapped
        /// in the <paramref name="source"/>.</param>
        /// <returns>null reference if the <paramref name="source"/> is null or
        /// result of applying <paramref name="selector"/> to the value wrapped with
        /// <paramref name="source"/> otherwise</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="selector"/>
        /// is a null reference.</exception>
        public static Nullable<TResult> SelectMany<TSource, TResult>(
            this Nullable<TSource> source,
            Func<TSource, Nullable<TResult>> selector)
            where TSource : struct
            where TResult : struct
        {
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            if (source.HasValue)
            {
                return selector(source.Value);
            }

            return null;
        }

        /// <summary>
        /// Projects value wrapped in the nullable into another nullable and
        /// invokes <paramref name="resultSelector"/> on values of both nullables.
        /// </summary>
        /// <typeparam name="TFirst">Type of the source value.</typeparam>
        /// <typeparam name="TSecond">Type of the nullable returned by
        /// the <paramref name="selector"/>.</typeparam>
        /// <typeparam name="TResult">Type of the resulting nullable.</typeparam>
        /// <param name="source">The source nullable wrapping value to be passed
        /// to the <paramref name="selector"/>.</param>
        /// <param name="selector">The function to be applied to the value wrapped
        /// in the <paramref name="source"/>.</param>
        /// <param name="resultSelector">The function to be applied to the value
        /// wrapped in the <paramref name="source"/> and result of the
        /// <paramref name="selector"/>.</param>
        /// <returns>null reference if the <paramref name="source"/> is null or
        /// result of applying <paramref name="selector"/> is null otherwise
        /// returns result of applying <paramref name="resultSelector"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="selector"/>
        /// or <paramref name="resultSelector"/> is a null reference.</exception>
        /// <remarks>This method provides ability to use multiple from clauses
        /// for nullable values.
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use SelectMany to compose
        /// values of two nullable date/time values.
        /// <![CDATA[
        /// DateTime? first = DateTime.Now;
        /// DateTime? second = DateTime.Now;
        /// DateTime? result =
        ///     from firstValue in first
        ///     from secondValue in second
        ///     select firstValue.Date + secondValue.TimeOfDay;
        /// ]]>
        /// </example>
        public static Nullable<TResult> SelectMany<TFirst, TSecond, TResult>(
            this Nullable<TFirst> source,
            Func<TFirst, Nullable<TSecond>> selector,
            Func<TFirst, TSecond, Nullable<TResult>> resultSelector)
            where TFirst : struct
            where TSecond : struct
            where TResult : struct
        {
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }

            return source.SelectMany(
                firstValue => selector(firstValue).SelectMany(
                    secondValue => resultSelector(firstValue, secondValue)));
        }
    }
}

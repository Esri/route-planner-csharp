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
using System.Diagnostics;

namespace ESRI.ArcLogistics.Utility.CoreEx
{
    /// <summary>
    /// Provides helper methods for checking various code contracts.
    /// </summary>
    internal static class CodeContract
    {
        /// <summary>
        /// Checks that the specified argument is not null.
        /// </summary>
        /// <typeparam name="T">The type of the argument to be checked.</typeparam>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="argumentValue">The value of the argument.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argumentValue"/> is a null
        /// reference.</exception>
        public static void RequiresNotNull<T>(string argumentName, T argumentValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(argumentName));

            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Checks that the specified argument belongs to the specified range.
        /// </summary>
        /// <typeparam name="T">The type of the argument to be checked.</typeparam>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="argumentValue">The value of the argument.</param>
        /// <param name="lowerBound">The lower bound of the range the element must
        /// belong to.</param>
        /// <param name="upperBound">The upper bound of the range the element must
        /// belong to.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="argumentValue"/> does
        /// not belong to range [<paramref name="lowerBound"/>, <paramref name="upperBound"/>].
        /// </exception>
        public static void RequiresValueInRange<T>(
            string argumentName,
            T argumentValue,
            T lowerBound,
            T upperBound)
        {
            Debug.Assert(!string.IsNullOrEmpty(argumentName));

            if (_Compare(argumentValue, lowerBound) < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }

            if (_Compare(argumentValue, upperBound) > 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Checks that the specified argument is less than or equal to the specified value.
        /// </summary>
        /// <typeparam name="T">The type of the argument to be checked.</typeparam>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="argumentValue">The value of the argument.</param>
        /// <param name="bound">The value against which to compare the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="argumentValue"/> is
        /// greater than <paramref name="bound"/>.</exception>
        public static void RequiresLessThanOrEqual<T>(
            string argumentName,
            T argumentValue,
            T bound)
        {
            Debug.Assert(!string.IsNullOrEmpty(argumentName));

            if (_Compare(argumentValue, bound) > 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        #region private static methods
        /// <summary>
        /// Compares specified objects with default comparer.
        /// </summary>
        /// <typeparam name="T">The type of objects to be compared.</typeparam>
        /// <param name="left">The first object to be compared.</param>
        /// <param name="right">The second object to be compared.</param>
        /// <returns>The result of comparison of specified objects with default comparer.</returns>
        private static int _Compare<T>(T left, T right)
        {
            var comparer = Comparer<T>.Default;

            return comparer.Compare(left, right);
        }
        #endregion
    }
}

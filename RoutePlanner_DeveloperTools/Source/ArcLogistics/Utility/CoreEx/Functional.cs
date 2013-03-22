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
using System.Linq.Expressions;

namespace ESRI.ArcLogistics.Utility.CoreEx
{
    /// <summary>
    /// Provides set of helper methods for simplifying functional style programming.
    /// </summary>
    internal static class Functional
    {
        /// <summary>
        /// Provides type inference for lambda functions.
        /// </summary>
        /// <typeparam name="TResult">The type of the function result.</typeparam>
        /// <param name="func">The function to infer types for.</param>
        /// <returns><paramref name="func"/>.</returns>
        public static Func<TResult> MakeLambda<TResult>(Func<TResult> func)
        {
            return func;
        }

        /// <summary>
        /// Provides type inference for lambda functions.
        /// </summary>
        /// <typeparam name="T">The type of the first function argument.</typeparam>
        /// <typeparam name="TResult">The type of the function result.</typeparam>
        /// <param name="func">The function to infer types for.</param>
        /// <returns><paramref name="func"/>.</returns>
        public static Func<T, TResult> MakeLambda<T, TResult>(Func<T, TResult> func)
        {
            return func;
        }

        /// <summary>
        /// Provides type inference for lambda functions.
        /// </summary>
        /// <param name="action">The action to infer types for</param>
        /// <returns><paramref name="action"/>.</returns>
        public static Action MakeLambda(Action action)
        {
            return action;
        }

        /// <summary>
        /// Provides type inference for lambda function expressions.
        /// </summary>
        /// <typeparam name="T">The type of the first function argument.</typeparam>
        /// <typeparam name="TResult">The type of the function result.</typeparam>
        /// <param name="expr">The function expression to infer types for.</param>
        /// <returns><paramref name="expr"/>.</returns>
        public static Expression<Func<T, TResult>> MakeExpression<T, TResult>(
            Expression<Func<T, TResult>> expr)
        {
            return expr;
        }
    }
}

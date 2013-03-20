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

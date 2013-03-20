using System;

namespace ESRI.ArcLogistics.Utility.CoreEx
{
    /// <summary>
    /// Provides helper extensions for System.Action delegates.
    /// </summary>
    internal static class ActionEx
    {
        /// <summary>
        /// Converts the specified action to function.
        /// </summary>
        /// <param name="action">The reference to the action to be converted.</param>
        /// <returns>A function executing <paramref name="action"/> and returning nothing.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="action"/> argument
        /// is a null reference.</exception>
        public static Func<Nothing> ToFunc(this Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            return () =>
            {
                action();
                return new Nothing();
            };
        }

        /// <summary>
        /// Converts the specified action to function.
        /// </summary>
        /// <typeparam name="T1">The type of the first action argument.</typeparam>
        /// <param name="action">The reference to the action to be converted.</param>
        /// <returns>A function executing <paramref name="action"/> and returning nothing.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="action"/> argument
        /// is a null reference.</exception>
        public static Func<T1, Nothing> ToFunc<T1>(this Action<T1> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            return arg1 =>
            {
                action(arg1);
                return new Nothing();
            };
        }

        /// <summary>
        /// Converts the specified action to function.
        /// </summary>
        /// <typeparam name="T1">The type of the first action argument.</typeparam>
        /// <typeparam name="T2">The type of the second action argument.</typeparam>
        /// <param name="action">The reference to the action to be converted.</param>
        /// <returns>A function executing <paramref name="action"/> and returning nothing.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="action"/> argument
        /// is a null reference.</exception>
        public static Func<T1, T2, Nothing> ToFunc<T1, T2>(this Action<T1, T2> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            return (arg1, arg2) =>
            {
                action(arg1, arg2);
                return new Nothing();
            };
        }
    }
}

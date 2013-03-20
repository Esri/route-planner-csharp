using System;
using ESRI.ArcLogistics.Utility.CoreEx;

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Provides set of helpers for invokation wrapper instances.
    /// </summary>
    internal static class InvokationWrapper
    {
        /// <summary>
        /// Invokes the specified action using the specified invokation wrapper
        /// by turning action into function returning nothing.
        /// </summary>
        /// <param name="wrapper">The service wrapper to be used for invoking the
        /// specified action.</param>
        /// <param name="invokationTarget">The action to be invoked.</param>
        public static void Invoke(
            this IInvocationWrapper wrapper,
            Action invokationTarget)
        {
            wrapper.Invoke(invokationTarget.ToFunc());
        }
    }
}

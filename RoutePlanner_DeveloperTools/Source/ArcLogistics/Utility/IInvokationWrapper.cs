using System;

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Wraps function invocation possibly providing additional services
    /// like exceptions translation or handling, changing result or anything else.
    /// </summary>
    internal interface IInvocationWrapper
    {
        /// <summary>
        /// Invokes the specified function providing additional
        /// implementation-specific services.
        /// </summary>
        /// <typeparam name="TResult">Type of the function result.</typeparam>
        /// <param name="invokationTarget">The function to be invoked.</param>
        /// <returns>The result of the function invocation.</returns>
        /// <remarks>Exceptions thrown from the method are implementation-specific,
        /// they maybe or not the ones thrown from the <paramref name="invokationTarget"/>.</remarks>
        TResult Invoke<TResult>(Func<TResult> invocationTarget);
    }
}

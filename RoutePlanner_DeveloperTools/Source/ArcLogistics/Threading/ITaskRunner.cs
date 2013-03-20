using System;

namespace ESRI.ArcLogistics.Threading
{
    /// <summary>
    /// Provides facilities for simplifying parallel code execution.
    /// </summary>
    internal interface ITaskRunner
    {
        /// <summary>
        /// Executes a for loop with possibly parallel iterations.
        /// </summary>
        /// <param name="fromInclusive">The starting loop index.</param>
        /// <param name="toExclusive">The ending loop index.</param>
        /// <param name="body">The delegate representing loop body.</param>
        /// <exception cref="T:System.AggregateException">
        /// Contains exceptions thrown by one or more iterations.</exception>
        void For(int fromInclusive, int toExclusive, Action<int> body);

        /// <summary>
        /// Executes specified actions possibly in parallel.
        /// </summary>
        /// <param name="actions">The reference to the collection of actions
        /// to be executed.</param>
        /// <exception cref="T:System.AggregateException">
        /// Contains exceptions thrown by one or more actions.</exception>
        void Invoke(params Action[] actions);
    }
}

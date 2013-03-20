using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ESRI.ArcLogistics.Threading
{
    /// <summary>
    /// Implements <see cref="T:ESRI.ArcLogistics.Threading.ITaskRunner"/> interface
    /// using <see cref="T:System.Threading.Tasks.Parallel"/>.
    /// </summary>
    internal sealed class ParallelTaskRunner : ITaskRunner
    {
        #region ITaskRunner Members
        /// <summary>
        /// Executes a for loop with possibly parallel iterations.
        /// </summary>
        /// <param name="fromInclusive">The starting loop index.</param>
        /// <param name="toExclusive">The ending loop index.</param>
        /// <param name="body">The delegate representing loop body.</param>
        /// <exception cref="T:System.AggregateException">
        /// Contains exceptions thrown by one or more iterations.</exception>
        public void For(int fromInclusive, int toExclusive, Action<int> body)
        {
            Debug.Assert(body != null);

            Parallel.For(fromInclusive, toExclusive, body);
        }

        /// <summary>
        /// Executes specified actions possibly in parallel.
        /// </summary>
        /// <param name="actions">The reference to the collection of actions
        /// to be executed.</param>
        /// <exception cref="T:System.AggregateException">
        /// Contains exceptions thrown by one or more actions.</exception>
        public void Invoke(params Action[] actions)
        {
            Debug.Assert(actions != null);
            Debug.Assert(actions.All(action => action != null));

            Parallel.Invoke(actions);
        }
        #endregion
    }
}

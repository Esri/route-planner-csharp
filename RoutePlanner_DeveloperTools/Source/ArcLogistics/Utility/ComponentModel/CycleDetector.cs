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
using System.Linq;

namespace ESRI.ArcLogistics.Utility.ComponentModel
{
    /// <summary>
    /// Detects cycles in a directed graph.
    /// </summary>
    /// <typeparam name="TNode">The type of the nodes in the graph.</typeparam>
    internal sealed class CycleDetector<TNode>
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the CycleDetector class.
        /// </summary>
        /// <param name="getPeers">The function returning a collection
        /// of peer nodes adjacent to any given node.</param>
        public CycleDetector(Func<TNode, IEnumerable<TNode>> getPeers)
        {
            Debug.Assert(getPeers != null);

            _getPeers = getPeers;
            _comparer = EqualityComparer<TNode>.Default;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Finds first cycle in the nodes graph starting at the specified root.
        /// </summary>
        /// <param name="root">The root node to start cycle searching from.</param>
        /// <returns>Collection of nodes denoting a cycle or an empty collection
        /// if no cycles were found.</returns>
        public IEnumerable<TNode> FindCycle(TNode root)
        {
            var path = new Stack<TNode>();
            var visited = new HashSet<TNode>(_comparer);

            if (!_FindCycle(root, path, visited))
            {
                return Enumerable.Empty<TNode>();
            }

            Debug.Assert(path.Count > 0);
            var cycleHead = path.Peek();
            var cycle = path
                .Reverse()
                .SkipWhile(node => !_comparer.Equals(node, cycleHead));

            return cycle;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Implements cycles searching starting at the specified root node.
        /// </summary>
        /// <param name="root">The root node to start cycle searching from.</param>
        /// <param name="path">The current path leading to the root.</param>
        /// <param name="visited">The current collection of already visited nodes.</param>
        /// <returns>True if and only if a cycle was found.</returns>
        private bool _FindCycle(
            TNode root,
            Stack<TNode> path,
            HashSet<TNode> visited)
        {
            path.Push(root);

            if (visited.Contains(root))
            {
                return true;
            }

            visited.Add(root);

            if (_getPeers(root).Any(node => _FindCycle(node, path, visited)))
            {
                return true;
            }

            visited.Remove(root);
            path.Pop();

            return false;
        }
        #endregion

        #region private fields
        /// <summary>
        /// The function returning a collection of peer nodes adjacent to any given node.
        /// </summary>
        private Func<TNode, IEnumerable<TNode>> _getPeers;

        /// <summary>
        /// The reference to the comparer object to be used for distinguishing
        /// graph nodes.
        /// </summary>
        private IEqualityComparer<TNode> _comparer;
        #endregion
    }
}

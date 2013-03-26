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
using System.Linq;

namespace ESRI.ArcLogistics.Utility.ComponentModel
{
    /// <summary>
    /// Exception to be thrown upon detecting cycles in properties dependencies.
    /// </summary>
    internal class PropertyDependenciesCycleException : Exception
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the PropertyDependenciesCycleException class.
        /// </summary>
        public PropertyDependenciesCycleException()
            : this(string.Empty, new string[] { })
        {
        }

        /// <summary>
        /// Initializes a new instance of the PropertyDependenciesCycleException class.
        /// </summary>
        /// <param name="propertyName">The name of the property whose dependecies
        /// contain a cycle.</param>
        /// <param name="dependenciesCycle">The collection of property names denoting
        /// cycle in properties dependencies.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="propertyName"/>
        /// or <paramref name="dependenciesCycle"/> is a null reference.</exception>
        public PropertyDependenciesCycleException(
            string propertyName,
            IEnumerable<string> dependenciesCycle)
            : this(
                propertyName,
                dependenciesCycle == null ? null : dependenciesCycle.ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the PropertyDependenciesCycleException class.
        /// </summary>
        /// <param name="propertyName">The name of the property whose dependecies
        /// contain a cycle.</param>
        /// <param name="dependenciesCycle">The collection of property names denoting
        /// cycle in properties dependencies.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="propertyName"/>
        /// or <paramref name="dependenciesCycle"/> is a null reference.</exception>
        private PropertyDependenciesCycleException(
            string propertyName,
            string[] dependenciesCycle)
            : base(_CreateMessage(propertyName, dependenciesCycle))
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (dependenciesCycle == null || dependenciesCycle.Any(name => name == null))
            {
                throw new ArgumentNullException("dependenciesCycle");
            }

            this.DependenciesCycle = dependenciesCycle;
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets collection of property names denoting cycle in properties dependencies.
        /// </summary>
        public IEnumerable<string> DependenciesCycle
        {
            get;
            private set;
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Creates a message for the specified dependencies cycle.
        /// </summary>
        /// <param name="propertyName">The name of the property whose dependecies
        /// contain a cycle.</param>
        /// <param name="dependenciesCycle">The collection of property names denoting
        /// cycle in properties dependencies.</param>
        /// <returns>Exception message for the specified cycle.</returns>
        private static string _CreateMessage(
            string propertyName,
            string[] dependenciesCycle)
        {
            if (dependenciesCycle == null)
            {
                return string.Empty;
            }

            var format = Properties.Messages.Error_CycleInPropertyDependencies;
            var path = string.Join(
                DEPENDENCIES_PATH_SEPARATOR,
                dependenciesCycle);
            var message = string.Format(
                format,
                propertyName,
                path);

            return message;
        }
        #endregion

        #region private constants
        /// <summary>
        /// A string to be used as a separator for property names in the string
        /// representation of dependencies cycle.
        /// </summary>
        private const string DEPENDENCIES_PATH_SEPARATOR = " -> ";
        #endregion
    }
}

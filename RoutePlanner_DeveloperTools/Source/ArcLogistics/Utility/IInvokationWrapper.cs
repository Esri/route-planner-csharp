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

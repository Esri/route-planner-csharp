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

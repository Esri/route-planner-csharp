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
using System.Text;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Interface that indicates that command support context.
    /// </summary>
    /// <remarks>
    /// Command that implements this interface should also implement IDisposable, because
    /// such command can be instantiated several time and released when context 
    /// is no longer available. When command is not needed it should release all the resources
    /// and stop handling any events.
    /// </remarks>
    interface ISupportContext
    {
        /// <summary>
        /// Command's context, that depends from the place where command's UI control was placed.
        /// </summary>
        object Context
        {
            get;
            set;
        }
    }
}

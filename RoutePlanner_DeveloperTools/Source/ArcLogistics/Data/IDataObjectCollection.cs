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
using System.Collections.Specialized;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Interface that manages collection of data objects.
    /// </summary>
    /// <typeparam name="T">Type of data objects.</typeparam>
    /// <remarks>
    /// <para>Interface throws notifications when collection changes.</para>
    /// <para>When you use syncronized collection and don't need it any more call its Dispose method to stop syncing to avoid performance degradation.</para>
    /// </remarks>
    public interface IDataObjectCollection<T> : IList<T>, INotifyCollectionChanged, IDisposable
        where T : DataObject 
    {
    }
}

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
using System.Diagnostics;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Class that manages data objects of the project.
    /// </summary>
    public class GenericDataObjectManager<T> : DataObjectManager<T>
        where T : DataObject
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal GenericDataObjectManager(DataObjectContext context, string entityName, SpecFields specFields)
            : base(context, entityName, specFields)
        {
        }

        internal GenericDataObjectManager(DataService<T> dataService)
            : base(dataService)
        {
        }

        #endregion constructors
    }
}

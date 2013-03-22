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
using System.Data.Objects.DataClasses;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// RouteCollWrapper class.
    /// </summary>
    internal class RouteCollWrapper : EntityCollWrapper<Route, DataModel.Routes>
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of RouteCollWrapper class.
        /// </summary>
        public RouteCollWrapper(EntityCollection<DataModel.Routes> entities,
            DataObject owner, bool isReadOnly)
            : base(entities, owner, isReadOnly)
        {
        }

        /// <summary>
        /// Initializes a new instance of RouteCollWrapper class.
        /// </summary>
        /// <param name="entities">Entities collection to be wrapped.</param>
        /// <param name="owner">The owner of the entities collection.</param>
        /// <param name="isReadOnly">Indicates if the collection should be read-only.</param>
        /// <param name="collectionInitializedCallback">A callback to be called after collection
        /// initialization.</param>
        public RouteCollWrapper(
            EntityCollection<DataModel.Routes> entities,
            DataObject owner,
            bool isReadOnly,
            Action collectionInitializedCallback)
            : base(entities, owner, isReadOnly, collectionInitializedCallback)
        {
        }
        #endregion constructors

        #region protected overrides
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected override IDataObjectCollection<Route> CreateCollection(
            EntityCollection<DataModel.Routes> entities,
            DataObject owner,
            bool isReadOnly)
        {
            return new RelatedRouteCollection(entities, owner, isReadOnly);
        }
        
        #endregion protected overrides
    }
}

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
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.DomainObjects.Utility
{
    /// <summary>
    /// Provides access to facilities common to a collection of related routes like all default
    /// routes or routes for specific schedule.
    /// </summary>
    internal interface IRoutesCollectionOwner
    {
        /// <summary>
        /// Finds all routes associated with the specified object.
        /// </summary>
        /// <param name="associatedObject">The object to find routes associated with.</param>
        /// <returns>A collection of routes associated with the specified object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="associatedObject"/> is
        /// a null reference.</exception>
        IEnumerable<Route> FindRoutes(DataObject associatedObject);

        /// <summary>
        /// Updates association between the specified route and related object accessed by the
        /// route property with the specified name.
        /// </summary>
        /// <param name="route">The route object to update association for.</param>
        /// <param name="propertyName">The name of the route property to update association
        /// with.</param>
        /// <exception cref="ArgumentNullException"><paramref name="route"/> or
        /// <paramref name="propertyName"/> is a null reference.</exception>
        void UpdateRouteAssociation(Route route, string propertyName);
    }
}

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
using System.Data.Objects.DataClasses;
using System.Data.Objects;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// ContextHelper class.
    /// </summary>
    internal class ContextHelper
    {
        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets data object context by specified relationship object.
        /// </summary>
        /// <param name="relatedEnd">IRelatedEnd object.</param>
        /// <returns>DataObjectContext object.</returns>
        public static DataObjectContext GetObjectContext(IRelatedEnd relatedEnd)
        {
            Debug.Assert(relatedEnd != null);

            // get ICacheableContext interface
            return ((ObjectQuery)relatedEnd.CreateSourceQuery()).Context as
                DataObjectContext;
        }

        /// <summary>
        /// Gets full entity set name by entity set name.
        /// </summary>
        /// <param name="context">ObjectContext object</param>
        /// <param name="entitySetName">entity name</param>
        /// <returns>
        /// entity set name
        /// </returns>
        public static string GetFullEntitySetName(ObjectContext context,
            string entitySetName)
        {
            Debug.Assert(context != null);
            Debug.Assert(entitySetName != null);

            return String.Format("{0}.{1}", context.DefaultContainerName,
                entitySetName);
        }

        /// <summary>
        /// Removes specified data object.
        /// </summary>
        /// <param name="context">ObjectContext object</param>
        /// <param name="obj">Data object to remove</param>
        /// <returns>
        /// entity set name
        /// </returns>
        public static void RemoveObject(ObjectContext context,
            DataObject obj)
        {
            Debug.Assert(context != null);

            context.DeleteObject(DataObjectHelper.GetEntityObject(obj));
        }

        #endregion public methods
    }
}

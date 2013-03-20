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

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// CollectionHelper class.
    /// </summary>
    internal class CollectionHelper
    {
        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static string ToString<T>(IList<T> list)
        {
            Debug.Assert(list != null);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                sb.Append(list[i].ToString());
                if (i < list.Count - 1)
                    sb.Append(", ");
            }

            return sb.ToString();
        }

        #endregion public methods
    }
}

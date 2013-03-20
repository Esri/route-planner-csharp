using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.Data;
using System.Collections;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Comparer for sorting Data Objects in ascending names.
    /// </summary>
    internal class DataObjectNameComparer: IComparer
    {
        #region IComparer> Members

        public int Compare(Object x, Object y)
        {
            if (String.Compare(x.ToString(), y.ToString(), true) > 0)
                return 1;
            else if (String.Compare(x.ToString(), y.ToString()) < 0)
                return -1;
            else
                return 0;
        }

        #endregion

    }
}

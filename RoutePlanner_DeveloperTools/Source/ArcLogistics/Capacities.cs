/*
COPYRIGHT 1995-2010 ESRI
TRADE SECRETS: ESRI PROPRIETARY AND CONFIDENTIAL
Unpublished material - all rights reserved under the 
Copyright Laws of the United States.
For additional information, contact:
Environmental Systems Research Institute, Inc.
Attn: Contracts Dept
380 New York Street
Redlands, California, USA 92373
email: contracts@esri.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics
{
    public class Capacities : ICollection<double>, ICloneable
    {
        public Capacities(int capacityCount)
        {
            throw new NotImplementedException();
        }

        #region ICloneable Members

        public object Clone()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICollection<double> Members

        public void Add(double item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(double item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(double[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(double item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<double> Members

        public IEnumerator<double> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

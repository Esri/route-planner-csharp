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

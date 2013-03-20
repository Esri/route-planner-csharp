using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Interface that indicates that command support context.
    /// </summary>
    /// <remarks>
    /// Command that implements this interface should also implement IDisposable, because
    /// such command can be instantiated several time and released when context 
    /// is no longer available. When command is not needed it should release all the resources
    /// and stop handling any events.
    /// </remarks>
    interface ISupportContext
    {
        /// <summary>
        /// Command's context, that depends from the place where command's UI control was placed.
        /// </summary>
        object Context
        {
            get;
            set;
        }
    }
}

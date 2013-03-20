using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interface for event, which syngnalyze about selection changes finished
    /// </summary>
    interface ISupportSelectionChanged
    {
        event EventHandler SelectionChanged;
    }
}

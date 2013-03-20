using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.App.GraphicObjects
{
    interface IVisibleGraphic
    {
        bool IsVisible
        {
            get;
            set;
        }
    }
}

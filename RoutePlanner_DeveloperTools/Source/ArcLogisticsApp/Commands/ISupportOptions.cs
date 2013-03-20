using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ESRI.ArcLogistics.App.Commands
{
    public interface ICommandOption
    {
        int Id
        {
            get;
        }

        string Title
        {
            get;
        }

        string TooltipText
        {
            get;
        }

        int GroupID
        {
            get;
        }

        bool IsEnabled
        {
            get;
            set;
        }
    }

    public interface ISupportOptions
    {
        ICommandOption[] Options
        {
            get;
        }
    }
}

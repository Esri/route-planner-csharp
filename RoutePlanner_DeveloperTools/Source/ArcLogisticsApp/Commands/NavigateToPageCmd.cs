using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Abstract class that can be used as a base for all other commands that navigate to specific page.
    /// </summary>
    abstract class NavigateToPageCmd : CommandBase
    {
        protected override void _Execute(params object[] args)
        {
            // Application.Current.MainWindow.Navigate(_PagePath);
        }

        protected abstract string _PagePath
        {
            get;
        }
    }
}

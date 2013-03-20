using System;
using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.Routing;

namespace ExtensionPluginTutorial
{
    public class MyCustomExtension : IExtension
    {
        public string Description
        {
            get { return "This extensions shows an alert when a Build Routes operation is completed."; }
        }

        public void Initialize(App app)
        {
            App.Current.ApplicationInitialized += new EventHandler(Current_ApplicationInitialized);            
        }

        public string Name
        {
            get { return "ExtensionPluginTutorial.MyCustomExtension"; }
        }

        private void Current_ApplicationInitialized(object sender, EventArgs e)
        {
            App.Current.Solver.AsyncSolveCompleted += new AsyncSolveCompletedEventHandler(Solver_AsyncSolveCompleted);
        }

        private void Solver_AsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            App.Current.Messenger.AddInfo("Solve completed.");
        }
    }
}

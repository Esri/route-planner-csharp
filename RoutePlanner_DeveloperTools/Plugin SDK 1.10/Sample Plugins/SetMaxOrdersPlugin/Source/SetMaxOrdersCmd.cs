using System.Windows.Input;

using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.App.Commands;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace SetMaxOrdersPlugin
{
    // Add Task to Schedule tab's task pane.
    [CommandPlugIn(new string[1] { "ScheduleTaskWidgetCommands" })]


    public class SetMaxOrdersCmd : AppCommands.ICommand
    {
        #region ICommand Members

        
        public void Execute(params object[] args)
        {
            // Get number of orders for selected date
            int numOrders = App.Current.Project.Orders.GetCount(App.Current.CurrentDate);
            

            foreach (ESRI.ArcLogistics.DomainObjects.Schedule s in App.Current.Project.Schedules.Search(App.Current.CurrentDate))
            {
                // Find Current Schedule
                if (s.Name == "Current")
                {
                    foreach (ESRI.ArcLogistics.DomainObjects.Route r in s.Routes)
                    {
                        // insert custom calculation here
                        if (numOrders > 0)
                            r.MaxOrders = numOrders / (s.Routes.Count) + 1;
                        else
                            r.MaxOrders = 30;
                    }

                    break;// Stop looking at schedules
                }
            }

            // Display status message
            string statusMessage = "Completed calculating and applying Max Orders to all routes.";
            App.Current.Messenger.AddInfo(statusMessage);
        }

        public void Initialize(App app)
        {

        }

        public bool IsEnabled
        {
            get { return true; }
        }
     
        public KeyGesture KeyGesture
        {
            get { return null; }
        }

        public string Name
        {
            get { return "SetMaxOrdersPlugin.SetMaxOrdersCmd"; }
        }

        public string Title
        {
            get { return "Recalculate Max Orders"; }
        }

        public string TooltipText
        {
            get { return "Calculate max orders based on the number of orders today."; }
        }

        #endregion
    }
}

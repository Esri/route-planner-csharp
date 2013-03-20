
using System.Windows.Forms;
using ESRI.ArcLogistics.App.Commands;

namespace CommandPluginTutorial
{
    [CommandPlugIn(new string[1] { "ScheduleTaskWidgetCommands" })]
    public class CommandPluginCmd : ESRI.ArcLogistics.App.Commands.ICommand
    {
        #region ICommand Members

        public void Execute(params object[] args)
        {
            MessageBox.Show("Hello, World!");
        }

        public void Initialize(ESRI.ArcLogistics.App.App app)
        {

        }

        public bool IsEnabled
        {
            get { return true; }
        }

        public System.Windows.Input.KeyGesture KeyGesture
        {
            get { return null; }
        }

        public string Name
        {
            get { return "CommandPluginTutorial.CommandPluginCmd"; }
        }

        public string Title
        {
            get { return "Say Hello"; }
        }

        public string TooltipText
        {
            get { return "Hello World Tooltip"; }
        }

        #endregion
    }
}

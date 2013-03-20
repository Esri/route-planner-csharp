using System.Windows.Input;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Provides access to the common ArcLogistics Application commands.
    /// </summary>
    internal static class ArcLogisticsCommands
    {
        /// <summary>
        /// Initializes static members of the ArcLogisticsCommands class.
        /// </summary>
        static ArcLogisticsCommands()
        {
            ArcLogisticsCommands.ContinueWorking = new RoutedCommand(
                "ContinueWorking",
                typeof(ArcLogisticsCommands));
        }

        /// <summary>
        /// Gets reference to the "Continue Working" command object.
        /// </summary>
        public static RoutedCommand ContinueWorking
        {
            get;
            private set;
        }
    }
}

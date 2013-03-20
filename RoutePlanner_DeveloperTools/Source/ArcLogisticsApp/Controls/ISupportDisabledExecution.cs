
namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interface used for allow execute disabled commands.
    /// </summary>
    internal interface ISupportDisabledExecution
    {
        /// <summary>
        /// Dets bool value to define whether disabled command can be executed.
        /// </summary>
        bool AllowDisabledExecution
        {
            get;
        }
    }
}

using System.ComponentModel;
namespace ESRI.ArcLogistics.App.Commands.TrackingCommandHelpers
{
    /// <summary>
    /// Provides common routes workflow management facilities.
    /// </summary>
    internal interface IRoutesWorkflowManager
    {
        #region properties
        /// <summary>
        /// Gets send routes task object.
        /// </summary>
        ISendRoutesTask SendRoutesTask
        {
            get;
        }
        #endregion
    }
}

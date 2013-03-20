using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Commands.TrackingCommandHelpers
{
    /// <summary>
    /// Provides common routes workflow management facilities.
    /// </summary>
    internal sealed class RoutesWorkflowManager : IRoutesWorkflowManager
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the RoutesWorkflowManager class.
        /// </summary>
        /// <param name="sendRoutesTask">Reference to the routes sending task object.</param>
        public RoutesWorkflowManager(
            SendRoutesTask sendRoutesTask
            )
        {
            Debug.Assert(sendRoutesTask != null);

            this.SendRoutesTask = sendRoutesTask;
        }
        #endregion

        #region IRoutesWorkflowManager Members

        /// <summary>
        /// Gets send routes task object;
        /// </summary>
        public ISendRoutesTask SendRoutesTask
        {
            get;
            private set;
        }

        #endregion
    }
}

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Implements <see cref="T:ESRI.ArcLogistics.IProgressReporter"/> by
    /// updating <see cref="T:ESRI.ArcLogistics.IProgressIndicator"/> instance.
    /// </summary>
    internal sealed class ProgressReporter : IProgressReporter
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the ProgressReporter class.
        /// </summary>
        /// <param name="statusMessages">The collection of messages to be reported.</param>
        /// <param name="progressIndicator">The progress indicator to be used for
        /// reporting progress.</param>
        public ProgressReporter(
            IEnumerable<string> statusMessages,
            IProgressIndicator progressIndicator)
        {
            Debug.Assert(statusMessages != null);
            Debug.Assert(statusMessages.All(message => message != null));
            Debug.Assert(progressIndicator != null);

            _statusMessages = statusMessages.ToList();
            Debug.Assert(_statusMessages.Count > 0);

            _statusMessages.Add(string.Empty);

            _progressIndicator = progressIndicator;
            _progressIndicator.StepCount = _statusMessages.Count - 1;
            _progressIndicator.CurrentStep = 0;
            _progressIndicator.Message = _statusMessages[0];
        }
        #endregion

        #region IProgressReporter Members
        /// <summary>
        /// Reports completion of the current step and beginning of the next one
        /// if any.
        /// </summary>
        public void Step()
        {
            _progressIndicator.CurrentStep = _progressIndicator.CurrentStep + 1;
            _progressIndicator.Message = _statusMessages[_progressIndicator.CurrentStep];
        }
        #endregion

        #region private fields
        /// <summary>
        /// Stores collection of progress status messages.
        /// </summary>
        private readonly List<string> _statusMessages;

        /// <summary>
        /// The reference to the progress indicator object to be used for
        /// reporting progress.
        /// </summary>
        private readonly IProgressIndicator _progressIndicator;
        #endregion
    }
}

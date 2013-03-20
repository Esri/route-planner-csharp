using System.ComponentModel;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Provides access to some operation progress statuses.
    /// </summary>
    internal interface IProgressIndicator : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets number of operation steps to be reported.
        /// </summary>
        int StepCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets current operation step.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">When value
        /// is less than zero or greater than
        /// <see cref="P:ESRI.ArcLogistics.IProgressIndicator.StepCount"/> property
        /// value.</exception>
        int CurrentStep
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets message describing current step.
        /// </summary>
        string Message
        {
            get;
            set;
        }
    }
}

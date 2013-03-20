using System;
using System.ComponentModel;
using System.Diagnostics;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Default implementation of the <see cref="T:ESRI.ArcLogistics.IProgressIndicator"/>
    /// interface.
    /// </summary>
    internal sealed class ProgressIndicator : IProgressIndicator
    {
        #region IProgressIndicator Members
        /// <summary>
        /// Gets or sets number of operation steps to be reported.
        /// </summary>
        public int StepCount
        {
            get
            {
                return _stepCount;
            }
            set
            {
                if (_stepCount != value)
                {
                    _stepCount = value;
                    _NotifyPropertyChanged("StepCount");
                }
            }
        }

        /// <summary>
        /// Gets or sets current operation step.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">When value
        /// is less than zero or greater than
        /// <see cref="P:ESRI.ArcLogistics.IProgressIndicator.StepCount"/> property
        /// value.</exception>
        public int CurrentStep
        {
            get
            {
                return _currentStep;
            }
            set
            {
                if (value < 0 || this.StepCount < value)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (_currentStep != value)
                {
                    _currentStep = value;
                    _NotifyPropertyChanged("CurrentStep");
                }
            }
        }

        /// <summary>
        /// Gets or sets message describing current step.
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                Debug.Assert(_message != null);
                if (_message != value)
                {
                    _message = value;
                    _NotifyPropertyChanged("Message");
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Fired when property value was changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        #endregion

        #region private methods
        /// <summary>
        /// Notifies about property value change.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        private void _NotifyPropertyChanged(string propertyName)
        {
            Debug.Assert(propertyName != null);

            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region private fields
        /// <summary>
        /// Stores number of operation steps to be reported.
        /// </summary>
        private int _stepCount;

        /// <summary>
        /// Stores current operation step.
        /// </summary>
        private int _currentStep;

        /// <summary>
        /// Stores message describing current step.
        /// </summary>
        private string _message = string.Empty;
        #endregion
    }
}

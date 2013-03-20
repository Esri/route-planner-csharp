using System.Windows.Documents;
using ESRI.ArcLogistics.Utility.ComponentModel;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// The model for the not connected state of the license page.
    /// </summary>
    public sealed class NotConnectedStateViewModel : NotifyPropertyChangedBase
    {
        #region public properties
        /// <summary>
        /// Gets or sets a document with information about connection failure.
        /// </summary>
        public FlowDocument ConnectionFailureInfo
        {
            get
            {
                return _connectionFailureInfo;
            }
            set
            {
                if (_connectionFailureInfo != value)
                {
                    _connectionFailureInfo = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_CONNECTION_FAILURE_INFO);
                }
            }
        }
        #endregion

        #region private constants
        /// <summary>
        /// Name of the ConnectionFailureInfo property.
        /// </summary>
        private const string PROPERTY_NAME_CONNECTION_FAILURE_INFO = "ConnectionFailureInfo";
        #endregion

        #region private fields
        /// <summary>
        /// Stores value of the ConnectionFailureInfo property.
        /// </summary>
        private FlowDocument _connectionFailureInfo;
        #endregion
    }
}

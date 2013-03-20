using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides no actual request analysis but always returns the specified constant.
    /// </summary>
    internal sealed class ConstantVrpRequestAnalyzer : IVrpRequestAnalyzer
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the ConstantVrpRequestAnalyzer class.
        /// </summary>
        /// <param name="executeSynchronously">The value indicating the outcome of the
        /// <see cref="CanExecuteSyncronously"/> method.</param>
        public ConstantVrpRequestAnalyzer(bool executeSynchronously)
        {
            _executeSynchronously = executeSynchronously;
        }
        #endregion

        #region IVrpRequestAnalyzer Members
        /// <summary>
        /// Does nothing but returns boolean constant passed to the constructor.
        /// </summary>
        /// <param name="request">The reference to the request object to be
        /// analyzed.</param>
        /// <returns>A value passed to the constructor.</returns>
        public bool CanExecuteSyncronously(SubmitVrpJobRequest request)
        {
            return _executeSynchronously;
        }
        #endregion

        #region private fields
        /// <summary>
        /// The value indicating the outcome of the <see cref="CanExecuteSyncronously"/> method.
        /// </summary>
        private bool _executeSynchronously;
        #endregion
    }
}

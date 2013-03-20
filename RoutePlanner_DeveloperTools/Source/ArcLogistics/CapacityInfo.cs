namespace ESRI.ArcLogistics
{
    /// <summary>
    /// CapacityInfo class contains information about a capacity type used in the current project.
    /// </summary>
    public class CapacityInfo
    {
        #region Constructors
        /// <summary>
        /// Creates a new instance of the <c>CapacityInfo</c> class.
        /// </summary>
        public CapacityInfo(string name, Unit displayUnitUS, Unit displayUnitMetric)
        {
            _name = name;
            _displayUnitUS = displayUnitUS;
            _displayUnitMetric = displayUnitMetric;
        }

        #endregion Constructors

        #region Public properties
        /// <summary>
        /// Name of the capacity.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }
        /// <summary>
        /// Units for the capacity (US).
        /// </summary>
        public Unit DisplayUnitUS
        {
            get { return _displayUnitUS; }
        }
        /// <summary>
        /// Units for the capacity (Metric).
        /// </summary>
        public Unit DisplayUnitMetric
        {
            get { return _displayUnitMetric; }
        }

        #endregion // Public properties

        #region Private members

        private readonly string _name = null;
        private readonly Unit _displayUnitUS = Unit.Unknown;
        private readonly Unit _displayUnitMetric = Unit.Unknown;

        #endregion // Private members
    }
}

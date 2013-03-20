namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Solver constants.
    /// </summary>
    internal sealed class SolverConst
    {
        public const double MAX_TIME_YEARS = 1;
        public const double MAX_TIME_HOURS = MAX_TIME_YEARS * 365 * 24;
        public const double MAX_TIME_MINS = MAX_TIME_HOURS * 60;

        public const double MAX_COST = 500000000;
        public const double MAX_SALARY = 1e9;

        public const double KM_PER_MILE = 1.609344;
        private const double _MAX_TRAVEL_DISTANCE_KM = 1e6;
        public const double MAX_TRAVEL_DISTANCE_MILES = _MAX_TRAVEL_DISTANCE_KM / KM_PER_MILE;
    }
}

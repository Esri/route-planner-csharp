namespace ESRI.ArcLogistics.Geometry
{
    /// <summary>
    /// Class for computing coordinates from geometry string.
    /// </summary>
    internal class CoordinateReader
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="geometryString">String with compressed geometry.</param>
        /// <param name="multiplier">Multiplier, used for coordinate computing.</param>
        public CoordinateReader(string geometryString, double multiplier)
        {
            _geometry = geometryString;
            _lastDiff = 0;
            _multiplier = multiplier;
        }

        /// <summary>
        /// Read next coordinate from string.
        /// </summary>
        /// <param name="currentPosition">Start position from reading should be begin.</param>
        /// <param name="coordinate">Readed coordinate.</param>
        /// <returns>'True' if coordinate was successfully read, and 'false' otherwise.</returns>
        public bool ReadNextCoordinate(ref int currentPosition, ref double coordinate)
        {
            var diff = CompactGeometryStringWorker.ReadInt(_geometry, ref currentPosition);
            if (diff == null)
                return false;

            int n = diff.Value + _lastDiff;
            coordinate = (double)n / _multiplier;
            _lastDiff = n;

            return true;
        }

        #region Private members

        /// <summary>
        /// String with compressed geometry.
        /// </summary>
        private string _geometry;

        /// <summary>
        /// Saved difference to compute next coordinate.
        /// </summary>
        private int _lastDiff;

        /// <summary>
        /// Coordinate multiplier.
        /// </summary>
        private double _multiplier;

        #endregion
    }
}

using System.Diagnostics;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides conversion methods for <see cref="CurbApproach"/> values.
    /// </summary>
    internal class CurbApproachConverter
    {
        /// <summary>
        /// Method converts curb approach into esti Network Analyst Curb Approach type.
        /// </summary>
        /// <param name="ca">Curb Approach to convert.</param>
        /// <returns>esti Network Analyst Curb Approach.</returns>
        public static NACurbApproachType ToNACurbApproach(CurbApproach ca)
        {
            Debug.Assert(ca != null);

            NACurbApproachType naType = NACurbApproachType.esriNAEitherSideOfVehicle;
            switch (ca)
            {
                case CurbApproach.Left:
                    naType = NACurbApproachType.esriNALeftSideOfVehicle;
                    break;

                case CurbApproach.Right:
                    naType = NACurbApproachType.esriNARightSideOfVehicle;
                    break;

                case CurbApproach.Both:
                    naType = NACurbApproachType.esriNAEitherSideOfVehicle;
                    break;

                case CurbApproach.NoUTurns:
                    naType = NACurbApproachType.esriNANoUTurn;
                    break;

                default:
                    // Not supported.
                    Debug.Assert(false);
                    break;
            }

            return naType;
        }
    }
}

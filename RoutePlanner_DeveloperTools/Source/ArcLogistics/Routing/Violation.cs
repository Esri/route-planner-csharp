using System;
using System.Diagnostics;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using System.ComponentModel;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Violation type enumeration.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum ViolationType
    {
        // NA violations.
        MaxOrderCount,
        Capacities,
        MaxTotalDuration,
        MaxTravelDuration,
        MaxTotalDistance,
        HardTimeWindow,
        Specialties,
        Zone,
        OrderPairMaxTransitTime,
        OrderPairOther,
        Unreachable,
        BreakRequired,
        RenewalRequired,
        // Custom AL violations.
        TooFarFromRoad, // esriNAObjectStatusNotLocated or esriNAObjectStatusElementNotLocated
        RestrictedStreet, // esriNAObjectStatusElementNotTraversable
        Ungeocoded,
        EmptyMaxTotalDuration,
        EmptyMaxTravelDuration,
        EmptyMaxTotalDistance,
        EmptyHardTimeWindow,
        EmptyUnreachable,

        /// <summary>
        /// Empty Break Max Travel Time violation.
        /// The solver was unable to finish operation since
        /// constraint violations detected on Routes in their empty state.
        /// </summary>
        EmptyBreakMaxTravelTime,

        /// <summary>
        /// Break Max Travel Time violation.
        /// The solver was unable to insert a break within the time
        /// specified by the break's MaxTravelTimeBetweenBreaks field.
        /// </summary>
        BreakMaxTravelTime,

        /// <summary>
        /// Break Max Cumul Work Time Exceeded violation.
        /// The solver was unable to insert a break within the time
        /// specified by the break's MaxCumulWorkTime field.
        /// </summary>
        BreakMaxCumulWorkTimeExceeded
    }

    /// <summary>
    /// Violation object type enumeration.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum ViolatedObjectType
    {
        Order,
        Route,
        Location
    }

    /// <summary>
    /// Violation class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Violation
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal Violation()
        {
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ViolationType ViolationType
        {
            get; internal set;
        }

        public ViolatedObjectType? ObjectType
        {
            get
            {
                ViolatedObjectType? type = null;
                if (_assocObject != null)
                {
                    if (_assocObject is Order)
                        type = ViolatedObjectType.Order;
                    else if (_assocObject is Route)
                        type = ViolatedObjectType.Route;
                    else if (_assocObject is Location)
                        type = ViolatedObjectType.Location;
                    else
                        Debug.Assert(false);
                }

                return type;
            }
        }

        public DataObject AssociatedObject
        {
            get
            {
                return _assocObject;
            }
            internal set
            {
                if (value == null)
                    throw new ArgumentNullException();

                _assocObject = value;
            }
        }

        #endregion public properties

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DataObject _assocObject;

        #endregion private fields
    }
}

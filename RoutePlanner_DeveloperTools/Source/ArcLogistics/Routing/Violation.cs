/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

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

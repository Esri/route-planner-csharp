using System;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Curb approach options for network locations.
    /// </summary>
    internal enum NACurbApproachType
    {
        esriNAEitherSideOfVehicle = 0,
        esriNARightSideOfVehicle = 1,
        esriNALeftSideOfVehicle = 2,
        esriNANoUTurn = 3,
    }

    /// <summary>
    /// Options for order assignment rule.
    /// </summary>
    internal enum NAOrderAssignmentRule
    {
        esriNAOrderExcludeFromSolve = 0,
        esriNAOrderPreserveRouteAndRelativeSequence = 1,
        esriNAOrderPreserveRoute = 2,
        esriNAOrderOverride = 3
    }

    /// <summary>
    /// Options for route assignment rule.
    /// </summary>
    internal enum NARouteAssignmentRule
    {
        esriNARouteExcludeFromSolve = 0,
        esriNARouteIncludeInSolve = 1
    }

    /// <summary>
    /// States for stops or other analysis objects.
    /// </summary>
    internal enum NAObjectStatus
    {
        esriNAObjectStatusOK = 0,
        esriNAObjectStatusNotLocated = 1,
        esriNAObjectStatusElementNotLocated = 2,
        esriNAObjectStatusElementNotTraversable = 3,
        esriNAObjectStatusInvalidFieldValues = 4,
        esriNAObjectStatusNotReached = 5,
        esriNAObjectStatusTimeWindowViolation = 6
    }

    /// <summary>
    /// Options for route seed point type.
    /// </summary>
    internal enum NARouteSeedPointType
    {
        esriNARouteSeedPointStatic = 0,
        esriNARouteSeedPointDynamic = 1
    }

    /// <summary>
    /// Maneuver types of direction item.
    /// </summary>
    internal enum NADirectionsManeuverType
    {
        esriDMTUnknown = 0,
        esriDMTStop = 1,
        esriDMTStraight = 2,
        esriDMTBearLeft = 3,
        esriDMTBearRight = 4,
        esriDMTTurnLeft = 5,
        esriDMTTurnRight = 6,
        esriDMTSharpLeft = 7,
        esriDMTSharpRight = 8,
        esriDMTUTurn = 9,
        esriDMTFerry = 10,
        esriDMTRoundabout = 11,
        esriDMTHighwayMerge = 12,
        esriDMTHighwayExit = 13,
        esriDMTHighwayChange = 14,
        esriDMTForkCenter = 15,
        esriDMTForkLeft = 16,
        esriDMTForkRight = 17,
        esriDMTDepart = 18,
        esriDMTTripItem = 19,
        esriDMTEndOfFerry = 20,
        esriDMTRampRight = 21,
        esriDMTRampLeft = 22,
        esriDMTTurnLeftRight = 23,
        esriDMTTurnRightLeft = 24,
        esriDMTTurnRightRight = 25,
        esriDMTTurnLeftLeft = 26,
    }

    /// <summary>
    /// Sub-item type of direction item.
    /// </summary>
    internal enum NADirectionsSubItemType
    {
        /// <summary>
        /// The sub-item type is unknown.
        /// </summary>
        None = 0,

        /// <summary>
        /// The sub-item represents maneuver.
        /// </summary>
        ManeuverItem = 1,

        /// <summary>
        /// The sub-item stores additional information like ServiceTime, TimeWindows etc.
        /// </summary>
        InformationalItem = 2,

        /// <summary>
        /// The sub-item stores event.
        /// </summary>
        EventItem = 3,
    }

    /// <summary>
    /// Options for service area line results.
    /// </summary>
    internal enum NAOutputLineType
    {
        esriNAOutputLineNone = 0,
        esriNAOutputLineStraight = 1,
        esriNAOutputLineTrueShape = 2,
        esriNAOutputLineTrueShapeWithMeasure = 3
    }

    /// <summary>
    /// Units of a network dataset attribute.
    /// </summary>
    internal enum NANetworkAttributeUnits
    {
        esriNAUUnknown = 0,
        esriNAUInches = 1,
        esriNAUFeet = 3,
        esriNAUYards = 4,
        esriNAUMiles = 5,
        esriNAUNauticalMiles = 6,
        esriNAUMillimeters = 7,
        esriNAUCentimeters = 8,
        esriNAUMeters = 9,
        esriNAUKilometers = 10,
        esriNAUDecimalDegrees = 11,
        esriNAUDecimeters = 12,
        esriNAUSeconds = 20,
        esriNAUMinutes = 21,
        esriNAUHours = 22,
        esriNAUDays = 23
    }

    /// <summary>
    /// NA boolean values.
    /// </summary>
    internal enum NABool
    {
        True = 1,
        False = 0
    }

    /// <summary>
    /// Violated constraints status (bitmask).
    /// </summary>
    [FlagsAttribute]
    internal enum NAViolatedConstraint : int
    {
        esriNAViolationMaxOrderCount = 1,
        esriNAViolationCapacities = 2,
        esriNAViolationMaxTotalTime = 4,
        esriNAViolationMaxTotalTravelTime = 8,
        esriNAViolationMaxTotalDistance = 16,
        esriNAViolationHardTimeWindow = 32,
        esriNAViolationSpecialties = 64,
        esriNAViolationZone = 128,
        esriNAViolationOrderPairMaxTransitTime = 256,
        esriNAViolationOrderPairOther = 512,
        esriNAViolationUnreachable = 1024,
        esriNAViolationBreakRequired = 2048,
        esriNAViolationRenewalRequired = 4096,

        /// <summary>
        /// Break Max Travel Time violation constraint.
        /// The solver was unable to insert a break within the time
        /// specified by the break's MaxTravelTimeBetweenBreaks field.
        /// </summary>
        esriNAViolationBreakMaxTravelTime = 8192,

        /// <summary>
        /// Break Max Cumul Work Time Exceeded violation constraint.
        /// The solver was unable to insert a break within the time
        /// specified by the break's MaxCumulWorkTime field.
        /// </summary>
        esriNAViolationBreakMaxCumulWorkTime = 16384
    }

    /// <summary>
    /// Network Analyst error codes.
    /// </summary>
    internal enum NAError
    {
        E_NA_NO_EXTENSION = -2147201023,
        E_NA_NO_CLASSDEF = -2147201022,
        E_NA_NO_ATTRIBUTE = -2147201021,
        E_NA_INVALID_ATTRIBUTE_TYPE = -2147201020,
        E_NA_NO_RESULT = -2147201019,
        E_NA_UNLOCATED_FLAG = -2147201018,
        E_NA_NONTRAVERSABLE_FLAG = -2147201017,
        E_NA_DHEAP_INSERT = -2147201016,
        E_NA_INVALID_HIERARCHY_RANGES = -2147201015,
        E_NA_ROUTE_SOLVER_STOP_UNREACHABLE = -2147201014,
        E_NA_NULL_SOURCEID = -2147201013,
        E_NA_NULL_SOURCEOID = -2147201012,
        E_NA_MISSING_FIELD = -2147201011,
        E_NA_BAD_CONNECTIVITY = -2147201010,
        E_NA_NO_IMPEDANCE_ATTRIBUTE = -2147201009,
        E_NA_NO_ACCUMULATE_ATTRIBUTE = -2147201008,
        E_NA_NULL_FIELDVALUE = -2147201007,
        E_NA_NO_ACCUMULATION_RANGES = -2147201006,
        E_NA_INVALID_PARTITION_ATTRIBUTE = -2147201005,
        E_NA_INSUFFICIENT_MEMORY = -2147201004,
        E_NA_UNABLE_TO_CREATE_TIN = -2147201003,
        E_NA_SOURCE_NOT_FEATURECLASS = -2147201002,
        E_NA_NETWORK_NO_EDGES = -2147201001,
        E_NA_NO_NETWORK = -2147201000,
        E_NA_IMPEDANCEATTRIBUTE_NOTTIMEUNITS = -2147200999,
        E_NA_INVALID_TIMEWINDOWS = -2147200998,
        E_NA_ISOLATED_STOP = -2147200997,
        E_NA_DISCONNECTED_STOPS = -2147200996,
        E_NA_INSUFFICIENT_CARDINALITY = -2147200995,
        E_NA_NO_SOLVERSETTINGS = -2147200994,
        E_NA_INVALID_NUMTRANSITIONS = -2147200993,
        E_NA_INVALID_MAXVALUEFORHIERARCHY = -2147200992,
        E_NA_CANNOT_BIND_TO_DATASET = -2147200991,
        E_NA_INVALID_FIELDVALUE = -2147200990,
        E_NA_INVALID_CONTEXT = -2147200989,
        E_NA_NON_SOLVER_CONFIGURATION_FILE = -2147200988,
        E_NA_SOLVER_CONFIGURATION_FILE_PARSE_ERROR = -2147200987,
        E_NA_DIRECTIONS_INVALID_ROUTE = -2147200986,
        E_NA_DIRECTIONS_INVALID_NETWORK = -2147200985,
        E_NA_DIRECTIONS_INVALID_SETUP = -2147200984,
        E_NA_DIRECTIONS_INVALID_CONFIG = -2147200983,
        E_NA_DIRECTIONS_INVALID_XLS = -2147200982,
        E_NA_NO_PATH_FIRSTTOLAST = -2147200981,
        E_NA_INVALID_NALOCATION = -2147200980,
        E_NA_NETWORK_HAS_NO_COST_ATTRIBUTE = -2147200979,
        E_NA_UNABLE_TO_CHANGE_VARIANT_TYPE = -2147200978,
        E_NA_INVALID_SABREAKS = -2147200977,
        E_NA_INSUFFICIENT_CONTIGUOUS_MEMORY_FOR_ROUTE_SHAPE = -2147200976,
        E_NA_INVALID_LOADER_GEOMETRY_TYPE = -2147200975,
        E_NA_ROUTE_SOLVER_TIME_WINDOW_MIXED_DATES = -2147200974,
        E_NA_ROUTE_SOLVER_START_TIME_NO_DATE = -2147200973,
        E_NA_ROUTE_SOLVER_INVALID_ROUTES = -2147200972,
        E_NA_NO_SOLUTION = -2147200971,
        E_NA_ROUTE_SOLVER_SEQUENCE_INVALID = -2147200970,
        E_NA_HIERARCHY_SETTINGS_INVALID = -2147200969,
        E_NA_HIERARCHY_LEVEL_COUNT_NEGATIVE = -2147200968,
        E_NA_HIERARCHY_LEVEL_COUNT_ZERO = -2147200967,
        E_NA_HIERARCHY_LEVEL_NONPOSITIVE = -2147200966,
        E_NA_HIERARCHY_LEVEL_LARGE = -2147200965,
        E_NA_HIERARCHY_MAX_VALUE_NONPOSITIVE = -2147200964,
        E_NA_HIERARCHY_NUM_TRANSITIONS_NONPOSITIVE = -2147200963,
        E_NA_HIERARCHY_MAX_VALUE_NONASCENDING = -2147200962,
        E_NA_ROUTE_SOLVER_START_TIME_NEGATIVE = -2147200961,
        E_NA_ROUTE_SOLVER_INVALID_FIRST_STOP = -2147200960,
        E_NA_ROUTE_SOLVER_INVALID_LAST_STOP = -2147200959,
        E_NA_NETWORK_NO_EDGE_SOURCES = -2147200958,
        E_NA_CLOSEST_FACILITY_SOLVER_PARTIAL_OUTPUT = -2147200957,
        E_NA_OD_COST_MATRIX_SOLVER_PARTIAL_OUTPUT = -2147200956,
        E_NA_INVALID_ATTRIBUTE_PARAMETER = -2147200955,
        E_NA_NULL_NACONTEXT = -2147200954,
        E_NA_NULL_GPMESSAGES = -2147200953,
        E_NA_NULL_ISPARTIALSOLUTION = -2147200952,
        E_NA_NACONTEXT_NULL_NETWORKDATASET = -2147200951,
        E_NA_NACONTEXT_NULL_DENETWORKDATASET = -2147200950,
        E_NA_INVALID_NACLASSCANDIDATEFIELDMAPS = -2147200949,
        E_NA_INVALID_NACANDIDATEFIELDMAPS = -2147200948,
        E_NA_INVALID_PROPERTYSET_VARIANTTYPE = -2147200947,
        E_NA_NACONTEXT_MISSING_NACLASS = -2147200946,
        E_NA_NULL_MAPDESCRIPTION = -2147200945,
        E_NA_CANNOT_ACCESS_MAPSERVER = -2147200944,
        E_NA_CANNOT_ACCESS_NASERVER = -2147200943,
        E_NA_INVALID_MAPEXTENT = -2147200942,
        E_NA_DIRECTIONS_SETUP_NO_SOURCE = -2147200923,
        E_NA_DIRECTIONS_SETUP_NO_NAMES = -2147200922,
        E_NA_DIRECTIONS_SETUP_INVALID_NAME = -2147200921,
        E_NA_DIRECTIONS_SETUP_NO_SHIELDS = -2147200920,
        E_NA_DIRECTIONS_SETUP_NO_LENGTH_ATTR = -2147200919,
        E_NA_DIRECTIONS_SETUP_NO_TIME_ATTR = -2147200918,
        E_NA_DIRECTIONS_SETUP_NO_ROADCLASS_ATTR = -2147200917,
        E_NA_DIRECTIONS_SETUP_INVALID_ROADCLASS = -2147200916,
        E_NA_DIRECTIONS_INVALID_TIME_ATTR = -2147200915,
        E_NA_INVALID_LINE_TYPE = -2147200914,
        E_NA_DOES_NOT_USE_HIERARCY = -2147200913,
        E_NA_DIRECTIONS_INVALID_LENGTH_UNITS = -2147200912,
        E_NA_NO_SA_OUTPUT = -2147200911,
        E_NA_INVALID_ATTRIBUTE_PARAMETER_VALUE = -2147200910,
        E_NA_INCORRECT_SOLVER_CONTEXT = -2147200909,
        E_NA_OD_COST_MATRIX_SOLVER_RESULT_LINES_BOTH = -2147200908,
        E_NA_OD_COST_MATRIX_SOLVER_RESULT_LINES_NONE = -2147200907,
        S_NA_VRP_SOLVER_PARTIAL_SOLUTION = 282625,
        E_NA_VRP_SOLVER_ATTRIBUTE_UNITS_NOT_TIME = -2147200823,
        E_NA_VRP_SOLVER_ATTRIBUTE_UNITS_NOT_DISTANCE = -2147200822,
        E_NA_VRP_SOLVER_IMPEDANCE_ATTRIBUTE_NOT_TIME = -2147200821,
        E_NA_VRP_SOLVER_ACCUMULATE_ATTRIBUTES_TOO_MANY = -2147200820,
        E_NA_VRP_SOLVER_ACCUMULATE_ATTRIBUTE_NOT_DISTANCE = -2147200819,
        E_NA_VRP_SOLVER_DEFAULT_DATE_SMALL = -2147200818,
        E_NA_VRP_SOLVER_DEFAULT_DATE_LARGE = -2147200817,
        E_NA_VRP_SOLVER_CAPACITY_COUNT_NONPOSITIVE = -2147200816,
        E_NA_VRP_SOLVER_OUTPUT_LINES_INVALID = -2147200815,
        E_NA_VRP_SOLVER_INVALID_INPUT = -2147200814,
        E_NA_VRP_SOLVER_INSUFFICIENT_INPUT = -2147200813,
        E_NA_VRP_SOLVER_INTERNAL_OD_ERROR = -2147200812,
        E_NA_VRP_SOLVER_ENGINE_ERROR = -2147200811,
        E_NA_VRP_SOLVER_MATRIX_NONE = -2147200810,
        E_NA_VRP_SOLVER_MATRIX_BOTH = -2147200809,
        E_NA_VRP_SOLVER_TIME_WINDOW_VIOLATION_PENALTY_FACTOR_NEGATIVE = -2147200808,
        E_NA_VRP_SOLVER_TIME_WINDOW_VIOLATION_PENALTY_FACTOR_LARGE = -2147200807,
        E_NA_VRP_SOLVER_EMPTY_INFEASIBLE_ROUTES = -2147200806,
        E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES = -2147200805,
        E_NA_VRP_SOLVER_IGNORE_INVALID_LOCATIONS_UNSUPPORTED = -2147200804,
        E_NA_VRP_SOLVER_EXCESS_TRANSIT_TIME_PENALTY_FACTOR_NEGATIVE = -2147200802,
        E_NA_VRP_SOLVER_EXCESS_TRANSIT_TIME_PENALTY_FACTOR_LARGE = -2147200801,
        E_NA_VRP_SOLVER_EXTERNAL_MATRIX_ORIGINS_DESTINATIONS_NOT_IDENTICAL = -2147200796,
        E_NA_VRP_SOLVER_INTERNAL_ROUTE_ERROR = -2147200795,
        E_NA_VRP_SOLVER_NO_SOLUTION = -2147200794,
        E_NA_VRP_SOLVER_GENERATE_INTERNAL_ROUTE_CONTEXT_INVALID = -2147200793,
        E_NA_VRP_SOLVER_NO_TIME_COST_ATTRIBUTE = -2147200792,
        E_NA_LOCATED_ON_UNLOCATABLE_SORUCE = -2147200791
    }

    /// <summary>
    /// Network Analyst barrier types.
    /// </summary>
    internal enum NABarrierType
    {
        /// <summary>
        /// Restriction (block) barrier.
        /// </summary>
        esriBarrierRestriction = 0,

        /// <summary>
        /// Scaled cost barrier.
        /// </summary>
        esriBarrierScaledCost = 1,

        /// <summary>
        /// Added cost barrier.
        /// </summary>
        esriBarrierAddedCost = 2
    }

    /// <summary>
    /// U-Turn policy values preconfigured in VRP model.
    /// Indicates how the U-turns at junctions that could occur during network traversal between
    /// stops are being handled by the solver.
    /// </summary>
    internal sealed class ModelUTurnPolicy
    {
        #region Public properties

        /// <summary>
        /// U-turns are prohibited at all junctions. Note, however, that U-turns are still permitted
        /// at network locations even when this setting is chosen; however, you can set the
        /// individual network locations' CurbApproach property to prohibit U-turns.
        /// </summary>
        public static string NoUTurns
        {
            get
            {
                return NO_UTURNS;
            }
        }

        /// <summary>
        /// U-turns are prohibited at all junctions, except those that have only one adjacent edge
        /// (a dead end).
        /// </summary>
        public static string AllowDeadEndsOnly
        {
            get
            {
                return ALLOW_DEAD_ENDS_ONLY;
            }
        }

        /// <summary>
        /// U-turns are prohibited at junctions where exactly two adjacent edges meet but are
        /// permitted at intersections (any junction with three or more adjacent edges) or dead
        /// ends (junctions with exactly one adjacent edge).
        /// </summary>
        public static string AllowDeadEndsAndIntersectionsOnly
        {
            get
            {
                return ALLOW_DEAD_ENDS_AND_INTERSECTIONS_ONLY;
            }
        }

        #endregion

        #region Private consts

        /// <summary>
        /// No U-Turns.
        /// </summary>
        private const string NO_UTURNS = "NO_UTURNS";

        /// <summary>
        /// Allow Dead Ends Only.
        /// </summary>
        private const string ALLOW_DEAD_ENDS_ONLY = "ALLOW_DEAD_ENDS_ONLY";

        /// <summary>
        /// Allow Dead Ends And Intersections Only.
        /// </summary>
        private const string ALLOW_DEAD_ENDS_AND_INTERSECTIONS_ONLY =
                                                    "ALLOW_DEAD_ENDS_AND_INTERSECTIONS_ONLY";

        #endregion

    }

    /// <summary>
    /// U-Turn policy values for NA Route service.
    /// Indicates how the U-turns at junctions that could occur during network traversal between
    /// stops are being handled by the solver.
    /// </summary>
    internal sealed class NARouteUTurnPolicy
    {
        #region Public properties

        /// <summary>
        /// U-turns are prohibited at all junctions. Note, however, that U-turns are still permitted
        /// at network locations even when this setting is chosen; however, you can set the
        /// individual network locations' CurbApproach property to prohibit U-turns.
        /// </summary>
        public static string NoUTurns
        {
            get
            {
                return NO_UTURNS;
            }
        }

        /// <summary>
        /// U-turns are prohibited at all junctions, except those that have only one adjacent edge
        /// (a dead end).
        /// </summary>
        public static string AllowDeadEndsOnly
        {
            get
            {
                return ALLOW_DEAD_ENDS_ONLY;
            }
        }

        /// <summary>
        /// U-turns are prohibited at junctions where exactly two adjacent edges meet but are
        /// permitted at intersections (any junction with three or more adjacent edges) or dead
        /// ends (junctions with exactly one adjacent edge).
        /// </summary>
        public static string AllowDeadEndsAndIntersectionsOnly
        {
            get
            {
                return ALLOW_DEAD_ENDS_AND_INTERSECTIONS_ONLY;
            }
        }

        #endregion

        #region Private consts

        /// <summary>
        /// No U-Turns.
        /// </summary>
        private const string NO_UTURNS = "esriNFSBNoBacktrack";

        /// <summary>
        /// Allow Dead Ends Only.
        /// </summary>
        private const string ALLOW_DEAD_ENDS_ONLY = "esriNFSBAtDeadEndsOnly";

        /// <summary>
        /// Allow Dead Ends And Intersections Only.
        /// </summary>
        private const string ALLOW_DEAD_ENDS_AND_INTERSECTIONS_ONLY =
                                                    "esriNFSBAtDeadEndsAndIntersections";

        #endregion
    }

    /// <summary>
    /// Output path shape types.
    /// </summary>
    internal sealed class OutputPathShape
    {
        public const string NO_LINES = "NO_LINES";
        public const string STRAIGHT_LINES = "STRAIGHT_LINES";
        public const string TRUE_LINES_WITH_MEASURES = "TRUE_LINES_WITH_MEASURES";
        public const string TRUE_LINES_WITHOUT_MEASURES = "TRUE_LINES_WITHOUT_MEASURES";
    }

    /// <summary>
    /// Job status values.
    /// </summary>
    internal sealed class NAJobStatus
    {
        public const string esriJobSubmitted = "esriJobSubmitted";
        public const string esriJobExecuting = "esriJobExecuting";
        public const string esriJobSucceeded = "esriJobSucceeded";
        public const string esriJobCancelled = "esriJobCancelled";
        public const string esriJobWaiting = "esriJobWaiting";
        public const string esriJobFailed = "esriJobFailed";
    }

    /// <summary>
    /// Job message types.
    /// </summary>
    internal sealed class NAJobMessageType
    {
        public const string esriJobMessageTypeInformative = "esriJobMessageTypeInformative";
        public const string esriJobMessageTypeWarning = "esriJobMessageTypeWarning";
        public const string esriJobMessageTypeError = "esriJobMessageTypeError";
    }

    /// <summary>
    /// Geometry types.
    /// </summary>
    internal sealed class NAGeometryType
    {
        public const string esriGeometryPoint = "esriGeometryPoint";
        public const string esriGeometryPolyline = "esriGeometryPolyline";
        public const string esriGeometryPolygon = "esriGeometryPolygon";
    }

    /// <summary>
    /// The layers to perform the identify operation on.
    /// </summary>
    internal sealed class NAIdentifyOperationLayers
    {
        #region Public properties

        /// <summary>
        /// The top-most layer.
        /// </summary>
        public static string TopLayer
        {
            get
            {
                return TOP_LAYER;
            }
        }

        /// <summary>
        /// All layers.
        /// </summary>
        public static string AllLayers
        {
            get
            {
                return ALL_LAYERS;
            }
        }

        /// <summary>
        /// All visible layers.
        /// </summary>
        public static string VisibleLayers
        {
            get
            {
                return VISIBLE_LAYERS;
            }
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Top option.
        /// </summary>
        private const string TOP_LAYER = "top";

        /// <summary>
        /// All option.
        /// </summary>
        private const string ALL_LAYERS = "all";

        /// <summary>
        /// Visible option.
        /// </summary>
        private const string VISIBLE_LAYERS = "visible";

        #endregion
    }

    /// <summary>
    /// Output format names.
    /// </summary>
    internal sealed class NAOutputFormat
    {
        public const string JSON = "json";
    }

    /// <summary>
    /// Attribute names.
    /// </summary>
    internal sealed class NAAttribute
    {
        #region Public properties

        /// <summary>
        /// Barrier Type property.
        /// </summary>
        public static string BarrierType
        {
            get
            {
                return BARRIER_TYPE;
            }
        }

        /// <summary>
        /// Time scale attribute property.
        /// </summary>
        public static string AttributeTimeScale
        {
            get
            {
                return ATTRIBUTE_TIME_SCALE;
            }
        }

        /// <summary>
        /// Time delay attribute property.
        /// </summary>
        public static string AttributeTimeDelay
        {
            get
            {
                return ATTRIBUTE_TIME_DELAY;
            }
        }

        /// <summary>
        /// Full edge property.
        /// </summary>
        public static string FullEdge
        {
            get
            {
                return FULL_EDGE;
            }
        }

        /// <summary>
        /// Gets name of the stops record-set field storing type of the stop.
        /// </summary>
        public static string StopType
        {
            get
            {
                return "StopType";
            }
        }

        /// <summary>
        /// Gets name of the stops record-set field storing arrival curb approach of the stop.
        /// </summary>
        public static string ArriveCurbApproach
        {
            get
            {
                return "ArriveCurbApproach";
            }
        }

        /// <summary>
        /// Gets name of the directions feature record-set field storing sub-item type of
        /// the direction item.
        /// </summary>
        public static string DirectionsSubItemType
        {
            get
            {
                return "SubItemType";
            }
        }

        /// <summary>
        /// Gets name of the directions feature record-set field storing type of the directions
        /// string.
        /// </summary>
        public static string DirectionsStringType
        {
            get
            {
                return "Type";
            }
        }

        /// <summary>
        /// Gets name of the directions feature record-set field storing elapsed time value.
        /// </summary>
        public static string DirectionsElapsedTime
        {
            get
            {
                return "ElapsedTime";
            }
        }

        /// <summary>
        /// Gets name of the directions feature record-set field storing driving distance value.
        /// </summary>
        public static string DirectionsDriveDistance
        {
            get
            {
                return "DriveDistance";
            }
        }

        /// <summary>
        /// Gets name of the directions feature record-set field storing driving directions text.
        /// </summary>
        public static string DirectionsText
        {
            get
            {
                return "Text";
            }
        }

        /// <summary>
        /// Gets name of the breaks feature record-set field which means
        /// is break paid or not.
        /// </summary>
        public static string IsPaid
        {
            get
            {
                return "IsPaid";
            }
        }

        /// <summary>
        /// Gets name of the breaks feature record-set field 
        /// </summary>
        public static string MaxCumulWorkTime
        {
            get
            {
                return "MaxCumulWorkTime";
            }
        }

        /// <summary>
        /// Gets name of the breaks feature record-set field storing
        /// maximum travel time between breaks value.
        /// </summary>
        public static string MaxTravelTimeBetweenBreaks
        {
            get
            {
                return "MaxTravelTimeBetweenBreaks";
            }
        }

        /// <summary>
        /// Gets name of the breaks feature record-set field storing
        /// precedence value.
        /// </summary>
        public static string Precedence
        {
            get
            {
                return "Precedence";
            }
        }

        #endregion

        #region Public constants

        public const string NAME = "Name";
        public const string ASSIGNMENT_RULE = "AssignmentRule";
        public const string SPECIALTY_NAMES = "SpecialtyNames";
        public const string ROUTE_NAME = "RouteName";
        public const string STATUS = "Status";
        public const string TW_START = "TimeWindowStart";
        public const string TW_END = "TimeWindowEnd";
        public const string MAX_VIOLATION_TIME = "MaxViolationTime";
        public const string CURB_APPROACH = "CurbApproach";

        // Route result.
        public const string TOTAL_COST = "TotalCost";
        public const string START_TIME = "StartTime";
        public const string END_TIME = "EndTime";
        public const string TOTAL_TIME = "TotalTime";
        public const string TOTAL_DISTANCE = "TotalDistance";
        public const string TOTAL_TRAVEL_TIME = "TotalTravelTime";
        public const string TOTAL_VIOLATION_TIME = "TotalViolationTime";
        public const string TOTAL_WAIT_TIME = "TotalWaitTime";

        // Orders.
        public const string ARRIVE_TIME = "ArriveTime";
        public const string FROM_PREV_DISTANCE = "FromPrevDistance";
        public const string SEQUENCE = "Sequence";
        public const string SERVICE_TIME = "ServiceTime";
        public const string WAIT_TIME = "WaitTime";
        public const string FROM_PREV_TRAVEL_TIME = "FromPrevTravelTime";
        public const string TW_START1 = "TimeWindowStart1";
        public const string TW_START2 = "TimeWindowStart2";
        public const string TW_END1 = "TimeWindowEnd1";
        public const string TW_END2 = "TimeWindowEnd2";
        public const string MAX_VIOLATION_TIME1 = "MaxViolationTime1";
        public const string MAX_VIOLATION_TIME2 = "MaxViolationTime2";
        public const string DELIVERY = "DeliveryQuantities";
        public const string PICKUP = "PickupQuantities";
        public const string VIOLATED_CONSTRAINTS = "ViolatedConstraints";
        public const string REVENUE = "Revenue";

        // Routes.
        public const string START_DEPOT_NAME = "StartDepotName";
        public const string END_DEPOT_NAME = "EndDepotName";
        public const string START_DEPOT_SERVICE_TIME = "StartDepotServiceTime";
        public const string END_DEPOT_SERVICE_TIME = "EndDepotServiceTime";
        public const string FIXED_COST = "FixedCost";
        public const string COST_PER_UNIT_TIME = "CostPerUnitTime";
        public const string COST_PER_UNIT_DISTANCE = "CostPerUnitDistance";
        public const string COST_PER_UNIT_OVERTIME = "CostPerUnitOvertime";
        public const string OVERTIME_START_TIME = "OvertimeStartTime";
        public const string MAX_ORDERS = "MaxOrderCount";
        public const string MAX_TOTAL_TRAVEL_TIME = "MaxTotalTravelTime";
        public const string MAX_TOTAL_TIME = "MaxTotalTime";
        public const string MAX_TOTAL_DISTANCE = "MaxTotalDistance";
        public const string EARLIEST_START_TIME = "EarliestStartTime";
        public const string LATEST_START_TIME = "LatestStartTime";
        public const string ARRIVE_DEPART_DELAY = "ArriveDepartDelay";
        public const string CAPACITIES = "Capacities";

        // Route zones.
        public const string SEED_POINT_TYPE = "SeedPointType";
        public const string IS_HARD_ZONE = "IsHardZone";

        // Depots.
        public const string DEPOT_NAME = "DepotName";

        // Breaks.
        public const string IS_PAID = "IsPaid";

        // Directions.
        public const string DIR_MANEUVER_TYPE = "maneuverType";
        public const string DIR_LENGTH = "length";
        public const string DIR_TIME = "time";
        public const string DIR_TEXT = "text";

        // Network attribute parameters.
        public const string NETWORK_ATTR_NAME = "AttributeName";
        public const string NETWORK_ATTR_PARAM_NAME = "ParameterName";
        public const string NETWORK_ATTR_PARAM_VALUE = "ParameterValue";

        // Order pairs.
        public const string FIRST_ORDER_NAME = "FirstOrderName";
        public const string SECOND_ORDER_NAME = "SecondOrderName";
        public const string MAX_TRANSIT_TIME = "MaxTransitTime";

        #endregion

        #region Private constants

        /// <summary>
        /// Barrier type.
        /// </summary>
        private const string BARRIER_TYPE = "BarrierType";

        /// <summary>
        /// Time scale attribute.
        /// </summary>
        private const string ATTRIBUTE_TIME_SCALE = "Scaled_Time";

        /// <summary>
        /// Time delay attribute.
        /// </summary>
        private const string ATTRIBUTE_TIME_DELAY = "Additional_Time";

        /// <summary>
        /// Full edge property.
        /// </summary>
        private const string FULL_EDGE = "FullEdge";

        #endregion
    }
}

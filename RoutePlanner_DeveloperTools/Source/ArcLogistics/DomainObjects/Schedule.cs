using System;
using System.Diagnostics;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using ESRI.ArcLogistics.DomainObjects.Utility;
using DataModel = ESRI.ArcLogistics.Data.DataModel;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents a schedule.
    /// </summary>
    /// <remarks>
    /// A Schedule is a solution of a routing problem, and contains a set of routes with assigned orders.
    /// </remarks>
    public class Schedule : DataObject
    {
        #region public static properties
        
        /// <summary>
        /// Name of the Name property.
        /// </summary>
        public static string PropertyNameName
        {
            get { return PROP_NAME_NAME; }
        }

        /// <summary>
        /// Name of the Planned Date property.
        /// </summary>
        public static string PropertyNamePlannedDate
        {
            get { return PROP_NAME_PLANNED_DATE;}
        }

        /// <summary>
        /// Name of the Type property.
        /// </summary>
        public static string PropertyNameType
        {
            get { return PROP_NAME_TYPE;}
        }

        /// <summary>
        /// Name of the Cost property.
        /// </summary>
        public static string PropertyNameCost
        {
            get { return PROPERTY_NAME_COST;}
        }

        /// <summary>
        /// Name of the Overtime property.
        /// </summary>
        public static string PropertyNameOverTime
        {
            get { return PROPERTY_NAME_OVERTIME;}
        }

        /// <summary>
        /// Name of the TotalTime property.
        /// </summary>
        public static string PropertyNameTotalTime
        {
            get { return PROPERTY_NAME_TOTAL_TIME;}
        }

        /// <summary>
        /// Name of the TotalDistance property.
        /// </summary>
        public static string PropertyNameTotalDistance
        {
            get { return PROPERTY_NAME_TOTAL_DISTANCE;}
        }

        /// <summary>
        /// Name of the ViolationTime property.
        /// </summary>
        public static string PropertyNameViolationTime
        {
            get { return PROPERTY_NAME_VIOLATION_TIME;}
        }

        /// <summary>
        /// Name of the WaitTime property.
        /// </summary>
        public static string PropertyNameWaitTime
        {
            get { return PROPERTY_NAME_WAIT_TIME;}
        }

        #endregion 

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>Schedule</c> class.
        /// </summary>
        public Schedule()
            : base(DataModel.Schedules.CreateSchedules(Guid.NewGuid()))
        {
            base.SetCreationTime();
            Type = ScheduleType.Current;
        }

        internal Schedule(DataModel.Schedules entity)
            : base(entity)
        {
        }

        #endregion constructors

        #region public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the object's type title.
        /// </summary>
        public override string TypeTitle
        {
            get { return Properties.Resources.Schedule; }
        }
        
        /// <summary>
        /// Gets the object's globally unique identifier.
        /// </summary>
        public override Guid Id
        {
            get { return _Entity.Id; }
        }

        /// <summary>
        /// Gets\sets object creation time.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException">Although property can get null value
        /// (for backward compatibility with existent plug-ins) it is not actually supported.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Although property can get 0 or less value
        /// (for backward compatibility with existent plug-ins) it is not actually supported.</exception>
        public override long? CreationTime
        {
            get
            {
                Debug.Assert(0 < _Entity.CreationTime); // NOTE: must be inited
                return _Entity.CreationTime;
            }
            set
            {
                if (!value.HasValue)
                    throw new ArgumentNullException(); // exception
                if (value.Value <= 0)
                    throw new ArgumentOutOfRangeException(); // exception

                _Entity.CreationTime = value.Value;
            }
        }

        internal override bool CanSave
        {
            get { return base.CanSave; }
            set
            {
                foreach (Route route in this.Routes)
                    route.CanSave = value;

                base.CanSave = value;
            }
        }

        /// <summary>
        /// Schedule name.
        /// </summary>
        public override string Name
        {
            get { return _Entity.Name; }
            set
            {
                _Entity.Name = value;
                NotifyPropertyChanged(PROP_NAME_NAME);
            }
        }

        /// <summary>
        /// Schedule planned date.
        /// </summary>
        public DateTime? PlannedDate
        {
            get { return _Entity.PlannedDate; }
            set
            {
                _Entity.PlannedDate = value;
                NotifyPropertyChanged(PROP_NAME_PLANNED_DATE);
            }
        }

        /// <summary>
        /// Type of schedule.
        /// </summary>
        public ScheduleType Type
        {
            get { return (ScheduleType)_Entity.ScheduleType; }
            set
            {
                _Entity.ScheduleType = (int)value;
                NotifyPropertyChanged(PROP_NAME_TYPE);
            }
        }

        /// <summary>
        /// Gets total cost of all scheduled routes in currency units.
        /// </summary>
        [UnitPropertyAttribute(Unit.Currency, Unit.Currency, Unit.Currency)]
        public double Cost
        {
            get
            {
                double cost = 0.0;
                foreach (Route route in this.Routes)
                    cost += route.Cost;

                return cost;
            }
        }

        /// <summary>
        /// Gets total amount of overtime of all scheduled routes in minutes.
        /// </summary>
        [UnitPropertyAttribute(Unit.Minute, Unit.Hour, Unit.Hour)]
        public double Overtime
        {
            get
            {
                double overtime = 0.0;
                foreach (Route route in this.Routes)
                    overtime += route.Overtime;

                return overtime;
            }
        }

        /// <summary>
        /// Gets total amount of violation time of all scheduled routes in minutes.
        /// </summary>
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        public double ViolationTime
        {
            get
            {
                double violationTime = 0.0;
                foreach (Route route in this.Routes)
                    violationTime += route.ViolationTime;

                return violationTime;
            }
        }

        /// <summary>
        /// Gets total amount of time spent by all scheduled routes in minutes.
        /// </summary>
        [UnitPropertyAttribute(Unit.Minute, Unit.Hour, Unit.Hour)]
        public double TotalTime
        {
            get
            {
                double totalTime = 0.0;
                foreach (Route route in this.Routes)
                    totalTime += route.TotalTime;

                return totalTime;
            }
        }

        /// <summary>
        /// Gets total amount of time of all scheduled routes spent on waiting for the orders in minutes.
        /// </summary>
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        public double WaitTime
        {
            get
            {
                double waitTime = 0.0;
                foreach (Route route in this.Routes)
                    waitTime += route.WaitTime;

                return waitTime;
            }
        }

        /// <summary>
        /// Gets total amount of time of all scheduled routes spent on waiting for the orders in percentage.
        /// </summary>
        public double WaitTimeInPercentage
        {
            get
            {
                double value = 0.0;
                if (this.TotalTime != 0.0)
                    value = (this.WaitTime / this.TotalTime) * 100;

                return value;
            }
        }

        /// <summary>
        /// Gets total distance of all scheduled routes in miles.
        /// </summary>
        [UnitPropertyAttribute(Unit.Mile, Unit.Mile, Unit.Kilometer)]
        public double TotalDistance
        {
            get
            {
                double distance = 0.0;
                foreach (Route route in this.Routes)
                    distance += route.TotalDistance;

                return distance;
            }
        }

        /// <summary>
        /// Total amount of capacities serviced by all the scheduled routes.
        /// </summary>
        public Capacities Capacities
        {
            get
            {
                Capacities capacities = null;
                if (this.Routes.Count > 0)
                {
                    capacities = new Capacities(this.Routes[0].CapacitiesInfo);
                    foreach (Route route in this.Routes)
                    {
                        for (int cap = 0; cap < route.Capacities.Count; cap++)
                            capacities[cap] += route.Capacities[cap];
                    }
                }

                return capacities;
            }
        }

        /// <summary>
        /// Schedule's routes collection.
        /// </summary>
        /// <remarks>
        /// When you add route to schedule and save the project, route will be saved automatically. No need to add this route to any other project collection.
        /// </remarks>
        public IDataObjectCollection<Route> Routes
        {
            get { return _RoutesWrap.DataObjects; }
            internal set { _RoutesWrap.DataObjects = value; }
        }

        /// <summary>
        /// Collection of unassigned orders that have the same planned date.
        /// </summary>
        /// <remarks>
        /// This collection must be set and updated manually. 
        /// <para>Use <c>OrderManager</c> to query unassigned orders for specified schedule and then set this collection to this property so you can keep together schedule and its unassigned orders.</para>
        /// </remarks>
        public IDataObjectCollection<Order> UnassignedOrders
        {
            get { return _unassignedOrders; }
            set { _unassignedOrders = value; }
        }

        #endregion public members

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Returns the name of the schedule.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion public methods

        #region ICloneable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            Schedule obj = new Schedule();
            obj.Name = this.Name;
            obj.PlannedDate = this.PlannedDate;
            obj.Type = this.Type;

            // deep copy of routes
            foreach (Route route in this.Routes)
                obj.Routes.Add((Route)route.Clone());

            // TODO: UnassignedOrders

            return obj;
        }

        #endregion ICloneable interface members

        #region internal events
        /// <summary>
        /// Occurs when schedule routes collection is initialized.
        /// </summary>
        internal event EventHandler<EventArgs> RoutesCollectionInitialized = delegate { };
        #endregion

        #region private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DataModel.Schedules _Entity
        {
            get
            {
                return (DataModel.Schedules)base.RawEntity;
            }
        }

        private RouteCollWrapper _RoutesWrap
        {
            get
            {
                if (_routesColl == null)
                {
                    _routesColl = new RouteCollWrapper(
                        _Entity.Routes,
                        this,
                        false,
                        _RoutesInitialized);
                }

                return _routesColl;
            }
        }
        #endregion private properties

        #region private methods
        /// <summary>
        /// Fires <see cref="RoutesCollectionInitialized"/> event with the specified arguments.
        /// </summary>
        /// <param name="e">Event arguments to be passed to routes initialized event
        /// handlers.</param>
        private void _NotifyRoutesCollectionInitialized(EventArgs e)
        {
            this.RoutesCollectionInitialized(this, e);
        }

        /// <summary>
        /// Handles routes initialization.
        /// </summary>
        private void _RoutesInitialized()
        {
            _NotifyRoutesCollectionInitialized(EventArgs.Empty);

            if (_routesCollectionOwner == null)
            {
                _routesCollectionOwner = new RoutesCollectionOwner(this.Routes);
            }
        }
        #endregion

        #region private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the Name property.
        /// </summary>
        private const string PROP_NAME_NAME = "Name";

        /// <summary>
        /// Name of the Planned Date property.
        /// </summary>
        private const string PROP_NAME_PLANNED_DATE = "PlannedDate";

        /// <summary>
        /// Name of the Type property.
        /// </summary>
        private const string PROP_NAME_TYPE = "Type";

        /// <summary>
        /// Name of the Cost property.
        /// </summary>
        private const string PROPERTY_NAME_COST = "Cost";

        /// <summary>
        /// Name of the Overtime property.
        /// </summary>
        private const string PROPERTY_NAME_OVERTIME = "Overtime";

        /// <summary>
        /// Name of the TotalTime property.
        /// </summary>
        private const string PROPERTY_NAME_TOTAL_TIME = "TotalTime";

        /// <summary>
        /// Name of the TotalDistance property.
        /// </summary>
        private const string PROPERTY_NAME_TOTAL_DISTANCE = "TotalDistance";

        /// <summary>
        /// Name of the ViolationTime property.
        /// </summary>
        private const string PROPERTY_NAME_VIOLATION_TIME = "ViolationTime";

        /// <summary>
        /// Name of the WaitTime property.
        /// </summary>
        private const string PROPERTY_NAME_WAIT_TIME = "WaitTime";

        #endregion

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private RouteCollWrapper _routesColl;
        private IDataObjectCollection<Order> _unassignedOrders;

        /// <summary>
        /// The reference to the routes collection owner for this schedule routes.
        /// </summary>
        private IRoutesCollectionOwner _routesCollectionOwner;
        #endregion private members
    }
}

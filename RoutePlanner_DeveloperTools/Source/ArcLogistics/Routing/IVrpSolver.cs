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
using System.Collections.Generic;
using ESRI.ArcLogistics.DomainObjects;
using System.ComponentModel;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// SolveOptions class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SolveOptions
    {
        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public bool GenerateDirections { get; set; }
        public bool FailOnInvalidOrderGeoLocation { get; set; }

        #endregion public properties
    }

    /// <summary>
    /// SolveOperationType enumeration.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum SolveOperationType
    {
        BuildRoutes,
        SequenceRoutes,
        UnassignOrders,
        AssignOrders,
        GenerateDirections
    }

    /// <summary>
    /// AsyncOperationInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AsyncOperationInfo
    {
        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public Guid Id { get; internal set; }
        public Schedule Schedule { get; internal set; }
        public object InputParams { get; internal set; }
        public SolveOperationType OperationType { get; internal set; }

        #endregion public properties
    }

    /// <summary>
    /// IVrpSolver interface.
    /// Provides a functionality to solve VRP.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IVrpSolver
    {
        #region events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raises when async. solve operation started.
        /// </summary>
        event AsyncSolveStartedEventHandler AsyncSolveStarted;

        /// <summary>
        /// Raises when async. solve operation completed.
        /// </summary>
        event AsyncSolveCompletedEventHandler AsyncSolveCompleted;

        #endregion events

        #region properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets and sets project.
        /// </summary>
        Project Project { get; set; }

        /// <summary>
        /// Gets a boolean value indicating if there are pending
        /// asynchronous operations.
        /// </summary>
        bool HasPendingOperations { get; }

        /// <summary>
        /// Gets network attributes and parameters.
        /// </summary>
        NetworkDescription NetworkDescription { get; }

        /// <summary>
        /// Gets solver configuration settings.
        /// </summary>
        SolverSettings SolverSettings { get; }

        #endregion properties

        #region methods
        /// <summary>
        /// Asynchronously builds routes for specified schedule.
        /// </summary>
        /// <param name="schedule">Schedule object.</param>
        /// <param name="inputParams">Input parameters for build route operation.</param>
        /// <returns>
        /// Operation id.
        /// </returns>
        /// <remarks>
        /// This method may add or update schedule's route results and
        /// associated stops, but it does not automatically saves the
        /// changes in database.
        /// </remarks>
        Guid BuildRoutesAsync(Schedule schedule,
            SolveOptions options,
            BuildRoutesParameters inputParams);

        Guid SequenceRoutesAsync(Schedule schedule,
            ICollection<Route> routesToSequence,
            SolveOptions options);

        Guid UnassignOrdersAsync(Schedule schedule,
            ICollection<Order> ordersToUnassign,
            SolveOptions options);

        Guid AssignOrdersAsync(Schedule schedule,
            ICollection<Order> ordersToAssign,
            ICollection<Route> targetRoutes,
            int? targetSequence,
            bool keepViolatedOrdersUnassigned,
            SolveOptions options);

        Guid GenerateDirectionsAsync(ICollection<Route> routes);

        /// <summary>
        /// Cancels asynchronous operation.
        /// </summary>
        /// <param name="operationId">Operation id.</param>
        /// <returns>
        /// true if operation is successfully cancelled, false if operation
        /// with specified id was not found.
        /// </returns>
        bool CancelAsyncOperation(Guid operationId);

        /// <summary>
        /// Gets asynchronous operation info.
        /// </summary>
        /// <param name="operationId">Operation id.</param>
        /// <param name="info">AsyncOperationInfo object.</param>
        /// <returns>
        /// true if info object is set, false if operation with specified id
        /// was not found.
        /// </returns>
        bool GetAsyncOperationInfo(Guid operationId,
            out AsyncOperationInfo info);

        /// <summary>
        /// Gets asynchronous operations by date.
        /// </summary>
        /// <param name="date">Date to search operations.</param>
        /// <returns>
        /// List of found AsyncOperationInfo objects.
        /// </returns>
        IList<AsyncOperationInfo> GetAsyncOperations(DateTime date);

        #endregion methods
    }
    
}

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
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides access to VRP service via REST API.
    /// </summary>
    internal interface IVrpRestService : IDisposable
    {
        /// <summary>
        /// Executes VRP job synchronously.
        /// </summary>
        /// <param name="request">The reference to the request object describing
        /// job to be executed.</param>
        /// <returns>Result of the synchronous job execution.</returns>
        /// <exception cref="T:ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="T:ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST VRP service.</exception>
        SyncVrpResponse ExecuteJob(SubmitVrpJobRequest request);

        /// <summary>
        /// Submits job to the VRP service.
        /// </summary>
        /// <param name="request">Request describing job to be submitted.</param>
        /// <returns>Result of the job submitting.</returns>
        /// <exception cref="T:ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="T:ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST VRP service.</exception>
        GetVrpJobResultResponse SubmitJob(SubmitVrpJobRequest request);

        /// <summary>
        /// Gets job from the VRP service.
        /// </summary>
        /// <param name="jobID">Identifier of the job to be retrieved.</param>
        /// <returns>Object describing VRP job status.</returns>
        /// <exception cref="T:ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="T:ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST VRP service.</exception>
        GetVrpJobResultResponse GetJobResult(
            string jobID);

        /// <summary>
        /// Gets GP object of the specified type from the VRP service.
        /// </summary>
        /// <typeparam name="TResult">Type of the object to be retrieved.</typeparam>
        /// <param name="jobID">Identifier of the job to retrieve object for.</param>
        /// <param name="objectUrl">Relative url with a path to the object.</param>
        /// <returns>Instance of the GP object associated with the job.</returns>
        /// <exception cref="T:ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="T:ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST VRP service.</exception>
        TResult GetGPObject<TResult>(
            string jobID,
            string objectUrl);

        /// <summary>
        /// Cancels job with the specified identifier.
        /// </summary>
        /// <param name="jobID">Identifier of the job to be cancelled.</param>
        /// <exception cref="T:ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="T:ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST VRP service.</exception>
        void CancelJob(string jobID);
    }
}

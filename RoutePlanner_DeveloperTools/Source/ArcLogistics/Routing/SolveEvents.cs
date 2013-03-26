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
using System.ComponentModel;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// AsyncSolveStartedEventArgs class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AsyncSolveStartedEventArgs : EventArgs
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public AsyncSolveStartedEventArgs(Guid operationId)
        {
            _operationId = operationId;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public Guid OperationId
        {
            get { return _operationId; }
        }

        #endregion public properties

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Guid _operationId;

        #endregion private fields
    }

    /// <summary>
    /// AsyncSolveCompletedEventArgs class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AsyncSolveCompletedEventArgs : AsyncCompletedEventArgs
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public AsyncSolveCompletedEventArgs(Exception error,
            bool cancelled,
            Guid operationId)
            : base(error, cancelled, operationId)
        {
        }

        public AsyncSolveCompletedEventArgs(Exception error,
            bool cancelled,
            Guid operationId,
            SolveResult result)
            : base(error, cancelled, operationId)
        {
            _result = result;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public Guid OperationId
        {
            get { return (Guid)this.UserState; }
        }

        public SolveResult Result
        {
            get { return _result; }
        }

        #endregion public properties

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private SolveResult _result;

        #endregion private fields
    }

    /// <summary>
    /// AsyncSolveStartedEventHandler delegate.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void AsyncSolveStartedEventHandler(
        Object sender,
        AsyncSolveStartedEventArgs e
    );

    /// <summary>
    /// AsyncSolveCompletedEventHandler delegate.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void AsyncSolveCompletedEventHandler(
        Object sender,
        AsyncSolveCompletedEventArgs e
    );

}
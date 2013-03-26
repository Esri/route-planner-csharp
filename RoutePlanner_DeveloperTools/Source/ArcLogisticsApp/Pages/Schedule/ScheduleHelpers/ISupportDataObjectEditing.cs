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
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Pages
{
    public class DataObjectEventArgs : EventArgs
    {
        public DataObjectEventArgs(DataObject obj)
        {
            _object = obj;
        }

        public DataObject Object
        {
            get
            {
                return _object;
            }
        }

        private DataObject _object;
    }

    public class DataObjectCanceledEventArgs : EventArgs
    {
        public DataObjectCanceledEventArgs(DataObject obj)
        {
            _object = obj;
        }

        public DataObjectCanceledEventArgs()
            : base()
        {
        }

        public bool Cancel
        {
            get
            {
                return _cancel;
            }
            set
            {
                _cancel = value;
            }
        }

        public DataObject Object
        {
            get
            {
                return _object;
            }
        }

        private DataObject _object;
        private bool _cancel;
    }

    public delegate void DataObjectEventHandler(
        Object sender,
        DataObjectEventArgs e
    );

    public delegate void DataObjectCanceledEventHandler(
        Object sender,
        DataObjectCanceledEventArgs e
    );

    public interface ISupportDataObjectEditing
    {
        /// <summary>
        /// Returns true if new item is being created or already existent is being edited.
        /// </summary>
        bool IsEditingInProgress
        {
            get;
        }

        event DataObjectCanceledEventHandler BeginningEdit;
        event DataObjectEventHandler EditBegun;

        event DataObjectCanceledEventHandler CommittingEdit;
        event DataObjectEventHandler EditCommitted;

        event DataObjectEventHandler EditCanceled;

        event DataObjectCanceledEventHandler CreatingNewObject;
        event DataObjectEventHandler NewObjectCreated;

        event DataObjectCanceledEventHandler CommittingNewObject;
        event DataObjectEventHandler NewObjectCommitted;

        event DataObjectEventHandler NewObjectCanceled;

    }

}
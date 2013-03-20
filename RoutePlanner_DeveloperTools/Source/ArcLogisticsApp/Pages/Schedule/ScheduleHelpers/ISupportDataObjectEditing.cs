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
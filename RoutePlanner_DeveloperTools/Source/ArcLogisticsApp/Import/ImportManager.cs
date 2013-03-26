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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.DomainObjects;
using AppData = ESRI.ArcLogistics.Data;
using AppGeometry = ESRI.ArcLogistics.Geometry;
using AppPages = ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Class that manages import process.
    /// </summary>
    internal sealed partial class ImportManager : IDisposable
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>ImportManager</c> class.
        /// </summary>
        public ImportManager()
        {}

        #endregion // Constructors

        #region Public events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Fires when import process done.
        /// </summary>
        public event ImportCompletedEventHandler ImportCompleted;

        #endregion // Public events

        #region IDisposable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Deletes created cursors.
        /// </summary>
        public void Dispose()
        {
            // unsubscribe to events
            if (null != App.Current.MainWindow)
                App.Current.MainWindow.Closed -= _MainWindow_Closed;

            if (null != _informer)
                _informer.Dispose();

            _informer = null;
            _importer = null;
            _geocoder = null;
            _worker = null;
        }

        #endregion // IDisposable interface members

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Starts asynchronous import process.
        /// </summary>
        /// <param name="parentPage">Page requirest importing.</param>
        /// <param name="profile">Import profile to importing.</param>
        /// <param name="defaultDate">Default date for default initialize imported objects.</param>
        public void ImportAsync(AppPages.Page parentPage,
                                ImportProfile profile,
                                DateTime defaultDate)
        {
            Debug.Assert(null != parentPage); // created
            Debug.Assert(null != profile); // created

            IDataProvider dataProvider = _InitDataSourceProvider(profile);
            if (null != dataProvider)
            {   // provider must present
                if (0 == dataProvider.RecordsCount)
                {   // empty file detected
                    App currentApp = App.Current;
                    string message = currentApp.FindString("ImportStatusFileIsEmpty");
                    currentApp.Messenger.AddWarning(message);
                }
                else
                {   // provider in valid state - start process
                    _StartImportProcess(parentPage, profile, defaultDate, dataProvider);
                }
            }
        }

        #endregion // Public methods

        #region Private types
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Class contains start process parameters.
        /// </summary>
        private sealed class ProcessParams
        {
            /// <summary>
            /// Creates a new instance of the <c>ProcessParams</c> class.
            /// </summary>
            /// <param name="profile">Import profile to importing.</param>
            /// <param name="defaultDate">Default date for default initialize imported objects.</param>
            /// <param name="dataProvider">Data provider.</param>
            /// <param name="cancelChecker">Cancellation checker.</param>
            /// <param name="informer">Progress informer.</param>
            public ProcessParams(ImportProfile profile,
                                 DateTime defaultDate,
                                 IDataProvider dataProvider,
                                 ICancellationChecker cancelChecker,
                                 IProgressInformer informer)
            {
                Debug.Assert(null != profile); // created
                Debug.Assert(null != dataProvider); // creatde
                Debug.Assert(null != cancelChecker); // created
                Debug.Assert(null != informer); // created

                Profile = profile;
                DefaultDate = defaultDate;
                DataProvider = dataProvider;
                CancelChecker = cancelChecker;
                Informer = informer;
            }

            /// <summary>
            /// Default date for default initialize imported objects.
            /// </summary>
            public readonly DateTime DefaultDate;
            /// <summary>
            /// Data provider interface.
            /// </summary>
            public readonly IDataProvider DataProvider;
            /// <summary>
            /// Import profile.
            /// </summary>
            public readonly ImportProfile Profile;
            /// <summary>
            /// Cancellation checker.
            /// </summary>
            public readonly ICancellationChecker CancelChecker;
            /// <summary>
            /// Progress informer.
            /// </summary>
            public readonly IProgressInformer Informer;
        }

        #endregion // Private types

        #region Private events handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Worker do work handler.
        /// Starts import procedure.
        /// </summary>
        /// <param name="sender">Background worker.</param>
        /// <param name="e">Do work event arguments.</param>
        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var param = e.Argument as ProcessParams;
            Debug.Assert(null != param); // valid call

            try
            {
                _Import(param);
            }
            catch (UserBreakException)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Worker completed event handler.
        /// Parses results.
        /// </summary>
        /// <param name="sender">Background worker.</param>
        /// <param name="e">Run worker completed event arguments.</param>
        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var bw = sender as BackgroundWorker;
            Debug.Assert(null != bw); // supported object

            // stop events
            bw.DoWork -= _worker_DoWork;
            bw.ProgressChanged -= _worker_ProgressChanged;
            bw.RunWorkerCompleted -= _worker_RunWorkerCompleted;

            // free resources
            bw.Dispose();
            _worker = null;

            // parse operation results
            Storage storage = _ParseResults(e.Error, e.Cancelled);

            // fire import completed
            if (null != ImportCompleted)
            {
                Debug.Assert(null != storage); // inited
                var events = new ImportCompletedEventArgs(storage.UpdatedObjects, e.Cancelled);
                ImportCompleted(this, events);
            }
        }

        /// <summary>
        /// Worker progress changed event handler.
        /// Update process status.
        /// </summary>
        /// <param name="sender">Background worker.</param>
        /// <param name="e">Progress changed event arguments.</param>
        private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Debug.Assert(null != _informer); // inited

            var statusParam = e.UserState as SetStatusParams;
            Debug.Assert(null != statusParam);

            _informer.SetStatus(statusParam.StatusNameRsc);
        }

        /// <summary>
        /// MainWindow closed handler.
        /// Aborts import if started.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MainWindow_Closed(object sender, EventArgs e)
        {
            _Abort();
        }

        #endregion // Private events handlers

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes data source provider.
        /// </summary>
        /// <param name="profile">Profile to importing.</param>
        /// <returns>Created data provider interface or NULL.</returns>
        /// <remarks>If problem detected add message to messenger.</remarks>
        private IDataProvider _InitDataSourceProvider(ImportProfile profile)
        {
            Debug.Assert(null != profile); // created

            // check source
            bool isFileExist = true;
            string source = profile.Settings.Source;
            if (DataSourceOpener.IsConnectionString(source))
            {
                DataSourceOpener.ConnectionString = source;
            }
            else
            {
                DataSourceOpener.FilePath = source;
                isFileExist = System.IO.File.Exists(source);
            }

            // get provider
            App currentApp = App.Current;
            IDataProvider dataProvider = null;
            if (isFileExist)
            {   // try open datasource
                string messageFailure = null;
                dataProvider =
                    DataSourceOpener.Open(profile.Settings.TableName, out messageFailure);
                // show connection problem message
                if (!string.IsNullOrEmpty(messageFailure))
                    currentApp.Messenger.AddError(messageFailure);
            }
            else
            {   // file not found
                string messageFailure =
                    currentApp.GetString("DataSourceNotFoundFormat", profile.Settings.Source);
                currentApp.Messenger.AddWarning(messageFailure); // show error
            }

            return dataProvider;
        }

        /// <summary>
        /// Showes process results.
        /// </summary>
        /// <param name="storage">Imported objects storage.</param>
        /// <param name="errorInAdd">Detected error in store imported object procedure flag.</param>
        private void _PopulateProcessResults(bool errorInAdd, Storage storage)
        {
            // initialize status of process
            App currentApp = App.Current;
            string statusRscName = (errorInAdd) ? "ImportProcessFailed" : "ImportProcessDone";
            string statusText =
                    currentApp.GetString(statusRscName, _informer.ObjectName);

            // update status
            currentApp.MainWindow.StatusBar.SetStatus(_informer.ParentPage, statusText);

            if (errorInAdd)
            {   // error in adding routine detected - show only status text
                currentApp.Messenger.AddMessage(MessageType.Error, statusText);
            }
            else
            {   // successed process ending - show statistics
                var statisticCreator = new Informer();
                statisticCreator.Inform(_importer, _geocoder, storage, statusText);
            }
        }

        /// <summary>
        /// Checks is mandatory fields mapped.
        /// </summary>
        /// <param name="fieldMap">Map of object to source fields.</param>
        /// <param name="type">Imported objects type.</param>
        /// <param name="isAllNeed">All mandatory fields must mapped.</param>
        /// <returns>Return TRUE if mapped.</returns>
        private bool _IsMandatoryFieldsMapped(IList<FieldMap> fieldMap, Type type, bool isAllNeed)
        {
            Debug.Assert(null != fieldMap);

            StringCollection names = PropertyHelpers.GetDestinationPropertiesName(type);

            bool isFieldsMapped = false;
            for (int index = 0; index < names.Count; ++index)
            {
                string name = names[index];

                bool isPresent = false;
                foreach (FieldMap map in fieldMap)
                {
                    if (name == map.ObjectFieldName)
                    {
                        isPresent = !string.IsNullOrEmpty(map.SourceFieldName);
                        break; // NOTE: result founded - stop process
                    }
                }

                isFieldsMapped = isPresent;
                if (isAllNeed)
                {
                    if (!isPresent)
                        break; // NOTE: result founded - stop process
                }
                else if (isPresent)
                {
                    break; // NOTE: result founded - stop process
                }
            }

            return isFieldsMapped;
        }

        /// <summary>
        /// Gets geocoding type.
        /// </summary>
        /// <param name="settings">Import settings.</param>
        /// <returns>Geocoding type.</returns>
        private Geocoder.GeocodeType _GetGeocodeType(ImportSettings settings)
        {
            Debug.Assert(null != settings); // created

            IList<FieldMap> fieldsMap = settings.FieldsMap;
            bool isAddressFieldsMapped = _IsMandatoryFieldsMapped(fieldsMap, typeof(Address), false);

            bool isGeoLocationFieldsMapped =
                _IsMandatoryFieldsMapped(fieldsMap, typeof(AppGeometry.Point), true);
            bool isGeometryUsed = FileHelpers.IsShapeFile(settings.Source);

            var type = Geocoder.GeocodeType.NotSet;
            if (isAddressFieldsMapped &&
                (isGeoLocationFieldsMapped || isGeometryUsed))
                type = Geocoder.GeocodeType.Complete;
            else if (isAddressFieldsMapped)
                type = Geocoder.GeocodeType.Batch;
            else if (isGeoLocationFieldsMapped || isGeometryUsed)
                type = Geocoder.GeocodeType.Reverse;
            // else type = Geocoder.GeocodeType.NotSet;

            return type;
        }

        /// <summary>
        /// Import procedure.
        /// </summary>
        /// <param name="parameters">Process parameters.</param>
        private void _Import(ProcessParams parameters)
        {
            Debug.Assert(null != _informer); // inited
            Debug.Assert(null != parameters); // created

            ImportProfile profile = parameters.Profile;
            ICancellationChecker cancelChecker = parameters.CancelChecker;

            // do import operation from source
            var projectData = new ProjectDataContext(App.Current.Project);
            _importer.Import(profile,
                             parameters.DataProvider,
                             parameters.DefaultDate,
                             projectData,
                             cancelChecker);

            cancelChecker.ThrowIfCancellationRequested();

            IList<AppData.DataObject> importedObjects = _importer.ImportedObjects;

            // do geocode imported objects
            if ((null != _geocoder) &&
                (0 < importedObjects.Count))
            {
                Geocoder.GeocodeType type = _GetGeocodeType(profile.Settings);
                _geocoder.Geocode(importedObjects, type, cancelChecker);
            }

            _AddObjectsToFeatureServiceIfNeeded(importedObjects);

            cancelChecker.ThrowIfCancellationRequested();

            // commit additions of related objects
            _informer.ParentPage.Dispatcher.BeginInvoke(new Action(() =>
            {
                projectData.Commit();
                projectData.Dispose();
            }), System.Windows.Threading.DispatcherPriority.Send);

            cancelChecker.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// If imported objects are mobile devices - add them to Feature Service.
        /// </summary>
        /// <param name="importedObjects">Collection of imported objects.</param>
        private void _AddObjectsToFeatureServiceIfNeeded(IList<AppData.DataObject> importedObjects)
        {
            // If imported objects are mobile devices - add them to Feature Service.
            if (importedObjects.Any() && importedObjects.First().GetType() == typeof(MobileDevice))
            {
                var devices = new List<MobileDevice>();
                foreach (MobileDevice device in importedObjects)
                    devices.Add(device);
                App.Current.Tracker.DeployDevices(devices);
            }
        }

        /// <summary>
        /// Stores imported objects.
        /// </summary>
        /// <param name="storage">Imported objects storage.</param>
        /// <returns>TRUE if operation ended successed.</returns>
        private bool _StoreImportedObjects(Storage storage)
        {
            Debug.Assert(null != storage); // created

            bool result = false;
            try
            {
                result = storage.AddToProject(_importer.ImportedObjects,
                                              _informer.ObjectName,
                                              _informer.ObjectsName);

            }
            catch (UserBreakException)
            {} // NOTE: hide exception

            return result;
        }

        /// <summary>
        /// Parses import operation results.
        /// </summary>
        /// <param name="ex">Generated exception in import procedure or NULL.</param>
        /// <param name="cancelled">Operation was cancelled flag.</param>
        /// <returns>Imported objects storage.</returns>
        private Storage _ParseResults(Exception ex, bool cancelled)
        {
            // create storage
            var storage = new Storage();

            // store imported objects
            bool errorInAdd = false;
            if ((null == ex) && !cancelled)
            {
                errorInAdd = !_StoreImportedObjects(storage);
            }

            // do empty status
            _informer.SetStatus(string.Empty);

            // unlock GUI
            App currentApp = App.Current;
            currentApp.UIManager.Unlock();

            if ((null == ex) && !cancelled)
            {   // successed
                _PopulateProcessResults(errorInAdd, storage);
            }
            else
            {   // error occured during operation OR operation was cancelled

                // show notification
                MessageType type = MessageType.Warning;
                string resourceStatus = "ImportProcessCanceled";

                if (null != ex)
                {   // error
                    type = MessageType.Error;
                    resourceStatus = "ImportProcessFailed";
                }

                string statusText =
                    currentApp.GetString(resourceStatus, _informer.ObjectName);
                currentApp.Messenger.AddMessage(type, statusText);

                currentApp.MainWindow.StatusBar.SetStatus(_informer.ParentPage, statusText);
            }

            return storage;
        }

        /// <summary>
        /// Creates background worker.
        /// </summary>
        /// <returns>Created background worker.</returns>
        private SuspendBackgroundWorker _CreateBackgroundWorker()
        {
            var worker = new SuspendBackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            // subscribe to worker's events
            worker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);
            worker.ProgressChanged += new ProgressChangedEventHandler(_worker_ProgressChanged);
            worker.DoWork += new DoWorkEventHandler(_worker_DoWork);

            return worker;
        }

        /// <summary>
        /// Starts import process.
        /// </summary>
        /// <param name="parentPage">Page requirest importing.</param>
        /// <param name="profile">Import profile.</param>
        /// <param name="defaultDate">Default date for default initialize imported objects.</param>
        /// <param name="dataProvider">Data provider.</param>
        private void _StartImportProcess(AppPages.Page parentPage,
                                         ImportProfile profile,
                                         DateTime defaultDate,
                                         IDataProvider dataProvider)
        {
            Debug.Assert(null != parentPage); // created
            Debug.Assert(null != profile); // created
            Debug.Assert(null != dataProvider); // creatde

            // reset state
            _importer = null;
            _geocoder = null;

            // subscribe to events
            App currentApp = App.Current;
            currentApp.MainWindow.Closed += new EventHandler(_MainWindow_Closed);

            // create background worker
            Debug.Assert(null == _worker); // only once
            SuspendBackgroundWorker worker = _CreateBackgroundWorker();

            // create internal objects
            var tracker = new ImportCancelTracker(worker);
            var cancelTracker = new CancellationTracker(tracker);
            _informer = new ProgressInformer(parentPage, profile.Type, tracker);
            _informer.SetStatus("ImportLabelImporting");

            var infoTracker = new ProgressInfoTracker(worker,
                                                      _informer.ParentPage,
                                                      _informer.ObjectName,
                                                      _informer.ObjectsName);
            _importer = new Importer(infoTracker);

            if (PropertyHelpers.IsGeocodeSupported(profile.Type))
            {
                _geocoder = new Geocoder(infoTracker);
            }

            // set precondition
            string message = currentApp.GetString("ImportProcessStarted", _informer.ObjectName);
            currentApp.Messenger.AddInfo(message);

            // lock GUI
            currentApp.UIManager.Lock(true);

            // run worker
            var parameters = new ProcessParams(profile,
                                               defaultDate,
                                               dataProvider,
                                               cancelTracker,
                                               infoTracker);
            worker.RunWorkerAsync(parameters);
            _worker = worker;
        }

        /// <summary>
        /// Stops asynchronous import.
        /// </summary>
        private void _Abort()
        {
            if (_worker != null)
            {
                if (_worker.IsBusy)
                    _worker.CancelAsync();
                _worker = null;
            }
        }

        #endregion // Private methods

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Progress informer.
        /// </summary>
        private ProgressInformer _informer;

        /// <summary>
        /// Importer (first step in import process).
        /// </summary>
        private Importer _importer;
        /// <summary>
        /// Geocoder (second step in import process or NULL if not need).
        /// </summary>
        private Geocoder _geocoder;

        /// <summary>
        /// Operation worker.
        /// </summary>
        private SuspendBackgroundWorker _worker;

        #endregion // Private fields
    }
}

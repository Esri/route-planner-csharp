using System;
using System.Diagnostics;
using System.Collections.Generic;

using ESRI.ArcLogistics.DomainObjects;
using AppData = ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Class that provides import functionality (creates object and initites from source data).
    /// </summary>
    internal sealed class Importer
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>Importer</c> class.
        /// </summary>
        /// <param name="informer">Progress informer.</param>
        public Importer(IProgressInformer informer)
        {
            Debug.Assert(null != informer); // created

            _informer = informer;
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Count of records in source (not empty).
        /// Valid after Import call.
        /// </summary>
        public int RecordCount
        {
            get { return _readCount; }
        }

        /// <summary>
        /// Count of failed records (invalid value detected in record).
        /// Valid after Import call.
        /// </summary>
        public int FailedCount
        {
            get { return _failedCount; }
        }

        /// <summary>
        /// Count of skipped records (empty name or same name).
        /// Valid after Import call.
        /// </summary>
        public int SkippedCount
        {
            get { return _skippedCount; }
        }

        /// <summary>
        /// Imported objects.
        /// Valid after Import call.
        /// </summary>
        public IList<AppData.DataObject> ImportedObjects
        {
            get { return _importedObjects; }
        }

        /// <summary>
        /// Import procedure detail list.
        /// Valid after Import call.
        /// </summary>
        public IList<MessageDetail> Details
        {
            get { return _details; }
        }

        #endregion // Public properties

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Importes objects from source.
        /// </summary>
        /// <param name="profile">Import source settings.</param>
        /// <param name="provider">Data provider.</param>
        /// <param name="defaultDate">Default date for default initialize imported objects.</param>
        /// <param name="projectData">Project data.</param>
        /// <param name="checker">Cancellation checker.</param>
        public void Import(ImportProfile profile,
                           IDataProvider provider,
                           DateTime defaultDate,
                           IProjectDataContext projectData,
                           ICancellationChecker checker)
        {
            Debug.Assert(null != profile); // created
            Debug.Assert(null != provider); // created
            Debug.Assert(null != projectData); // created
            Debug.Assert(null != checker); // created

            // reset internal state first
            _Reset();

            // store contects
            _profile = profile;
            _provider = provider;
            _defaultDate = defaultDate;
            _projectData = projectData;

            // start process
            _Import(checker);
        }

        #endregion Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Resets internal state.
        /// </summary>
        private void _Reset()
        {
            _readCount = 0;
            _failedCount = 0;
            _skippedCount = 0;

            _details.Clear();
            _importedObjects.Clear();
        }

        /// <summary>
        /// Calculates real record count.
        /// </summary>
        /// <returns>Number of not empty records.</returns>
        private int _GetNotEmptyRecordNumber()
        {
            Debug.Assert(null != _provider); // inited

            int count = 0;
            _provider.MoveFirst();
            do
            {
                if (!_provider.IsRecordEmpty)
                    ++count;

                _provider.MoveNext();
            }
            while (!_provider.IsEnd());

            return count;
        }

        /// <summary>
        /// Checks is orders equals.
        /// </summary>
        /// <param name="order1">First order to checking.</param>
        /// <param name="order2">Second order to checking.</param>
        /// <returns>Return TRUE if orders is equals</returns>
        private bool _IsOrdersEquals(Order order1, Order order2)
        {
            Debug.Assert(null != order1); // created
            Debug.Assert(null != order2); // created

            bool isEquals =
                (order1.Name.Equals(order2.Name, StringComparison.OrdinalIgnoreCase) &&
                 order1.PlannedDate.Equals(order2.PlannedDate));

            return isEquals;
        }

        /// <summary>
        /// Checks is barriers equals.
        /// </summary>
        /// <param name="barrier1">First barrier to checking.</param>
        /// <param name="barrier2">Second barrier to checking.</param>
        /// <returns>Return TRUE if barriers is equals</returns>
        private bool _IsBarriersEquals(Barrier barrier1, Barrier barrier2)
        {
            Debug.Assert(null != barrier1); // created
            Debug.Assert(null != barrier2); // created

            bool isEquals =
                (barrier1.Name.Equals(barrier2.Name.Trim(), StringComparison.OrdinalIgnoreCase) &&
                 barrier1.StartDate.Equals(barrier2.StartDate) &&
                 barrier1.FinishDate.Equals(barrier2.FinishDate));

            return isEquals;
        }

        /// <summary>
        /// Checks is objects equals.
        /// </summary>
        /// <param name="object1">First object to checking.</param>
        /// <param name="object2">Second object to checking.</param>
        /// <returns>Return TRUE if objects is equals</returns>
        private bool _IsObjectsEquals(AppData.DataObject object1, AppData.DataObject object2)
        {
            Debug.Assert(null != object1); // created
            Debug.Assert(null != object2); // created

            bool isEquals = false;
            var order1 = object1 as Order;
            // special check for Orders
            if (null != order1)
            {
                // check by Name and PlannedDate
                var order2 = object2 as Order;
                isEquals = _IsOrdersEquals(order1, order2);
            }

            else
            {   // special check for Barrires
                var barrier1 = object1 as Barrier;
                if (null != barrier1)
                {   // check by Name, StartDate and FinishDate
                    var barrier2 = object2 as Barrier;
                    isEquals = _IsBarriersEquals(barrier1, barrier2);
                }
                // all other objects - check by Name
                else
                {
                    string object1Name = object1.ToString().Trim();
                    isEquals = (object1Name.Equals(object2.ToString().Trim(),
                                                   StringComparison.OrdinalIgnoreCase));
                }
            }

            return isEquals;
        }

        /// <summary>
        /// Finds source field name.
        /// </summary>
        /// <param name="name">Object's field name.</param>
        /// <param name="fieldsMap">Fields map (object to source field name).</param>
        /// <returns>Founded source field name or NULL.</returns>
        private string _FindSourceFieldName(string name, IList<FieldMap> fieldsMap)
        {
            Debug.Assert(!string.IsNullOrEmpty(name)); // not empty
            Debug.Assert(null != fieldsMap); // created

            string sourceFieldName = null;
            foreach (FieldMap map in fieldsMap)
            {
                if (name == map.ObjectFieldName)
                {
                    sourceFieldName = map.SourceFieldName;
                    break; // NOTE: result founded
                }
            }

            Debug.Assert(!string.IsNullOrEmpty(sourceFieldName)); // result valid
            return sourceFieldName;
        }

        /// <summary>
        /// Checks is imported data source invalid.
        /// </summary>
        /// <param name="result">Import process result.</param>
        /// <param name="currentNumber">Current record number to message generation.</param>
        /// <returns>TRUE if data source has invalid values.</returns>
        /// <remarks>Stores problem description.</remarks>
        private bool _IsImportDataSourceInvalid(ImportResult result, int currentNumber)
        {
            Debug.Assert(null != _profile); // created

            bool isErrorDetected = false;

            // check import process description
            List<FieldMap> fieldsMap = _profile.Settings.FieldsMap;
            ICollection<ImportedValueInfo> desciptions = result.Desciptions;
            foreach (ImportedValueInfo description in desciptions)
            {
                if (description.Status == ImportedValueStatus.Failed)
                {   // process only failed
                    // store problem description
                    string sourceFieldName = _FindSourceFieldName(description.Name, fieldsMap);
                    string format = App.Current.FindString("ImportProcessStatusRecordValueSkiped");
                    Debug.Assert(null != result.Object); // created
                    string text = string.Format(format,
                                                currentNumber,
                                                result.Object.ToString(),
                                                sourceFieldName,
                                                description.ReadedValue);
                    _details.Add(new MessageDetail(MessageType.Warning, text));

                    isErrorDetected = true;
                    break; // NOTE: stop after first founded error
                }
                // else Do nothing, other ignored
            }

            return isErrorDetected;
        }

        /// <summary>
        /// Checks is object unique.
        /// </summary>
        /// <param name="obj">Object to check.</param>
        /// <param name="objects">Previous object collection.</param>
        /// <returns>Return TRUE if newObject not present in objects.</returns>
        private bool _IsObjectUnique(AppData.DataObject obj, IList<AppData.DataObject> objects)
        {
            Debug.Assert(null != obj); // created
            Debug.Assert(null != objects); // created

            bool isUnique = true;
            foreach (var currentObj in objects)
            {
                if (_IsObjectsEquals(obj, currentObj))
                {
                    isUnique = false;
                    break; // NOTE: result founded
                }
            }

            return isUnique;
        }

        /// <summary>
        /// Checks need object skip.
        /// </summary>
        /// <param name="result">Import process result.</param>
        /// <param name="currentNumber">Current record number.</param>
        private void _IsObjectSkipped(ImportResult result, int currentNumber)
        {
            // get object name
            AppData.DataObject obj = result.Object;
            Debug.Assert(null != obj); // created

            string importedObjName = null;
            if (!string.IsNullOrEmpty(obj.ToString()))
                importedObjName = obj.ToString().Trim();

            string text = null;
            if (string.IsNullOrEmpty(importedObjName))
            {   // import object with empty name - skip
                text = App.Current.GetString("ImportProcessStatusRecordSkiped", currentNumber);
                ++_skippedCount;
            }
            else if (_IsImportDataSourceInvalid(result, currentNumber))
            {   // import object convert with error - skip
                ++_skippedCount;
            }
            else
            {   // ignore object with equals names
                //  for Order and Barrier special routine: they can be have different PlannedDate
                //  and name don't check
                if ((obj is Order) ||
                    (obj is Barrier) ||
                    _IsObjectUnique(obj, _importedObjects))
                {
                    _importedObjects.Add(obj);
                }
                else
                {   // ignore object with dublicated names from resource - skip
                    text = App.Current.GetString("ImportProcessStatusSkiped",
                                                 _informer.ObjectName,
                                                 importedObjName,
                                                 currentNumber);
                    ++_skippedCount;
                }
            }

            // store description
            if (!string.IsNullOrEmpty(text))
            {
                var description = new MessageDetail(MessageType.Warning, text);
                _details.Add(description);
            }
        }

        /// <summary>
        /// Does import object.
        /// </summary>
        /// <param name="references">Dictionary property name to field position in data source.</param>
        /// <param name="currentNumber">Current record number.</param>
        private void _ImportObject(Dictionary<string, int> references, int currentNumber)
        {
            Debug.Assert(null != _profile); // inited
            Debug.Assert(null != _provider); // inited
            Debug.Assert(null != _projectData); // inited
            Debug.Assert(null != references); // created

            try
            {
                ImportType type = _profile.Type;
                ImportResult res = CreateHelpers.Create(type,
                                                        references,
                                                        _provider,
                                                        _projectData,
                                                        _defaultDate);
                _IsObjectSkipped(res, currentNumber);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                // store problem description
                string text =
                    App.Current.GetString("ImportProcessStatusRecordFailed",
                                          _informer.ObjectName,
                                          currentNumber);
                var description = new MessageDetail(MessageType.Warning, text);
                _details.Add(description);
                ++_failedCount;
            }
        }

        /// <summary>
        /// Starts import process.
        /// </summary>
        /// <param name="checker">Cancellation checker.</param>
        private void _Import(ICancellationChecker checker)
        {
            Debug.Assert(null != _profile); // inited
            Debug.Assert(null != _provider); // inited
            Debug.Assert(null != checker); // created

            // store records count
            _readCount = _GetNotEmptyRecordNumber();

            Dictionary<string, int> references =
                PropertyHelpers.CreateImportMap(_profile.Settings.FieldsMap, _provider);

            // read source
            _provider.MoveFirst();

            int index = 0;
            while (!_provider.IsEnd())
            {   // do import process
                checker.ThrowIfCancellationRequested();

                if (!_provider.IsRecordEmpty)
                {
                    _ImportObject(references, index);

                    ++index;
                }

                _provider.MoveNext();
            }
        }

        #endregion // Private methods

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Data provider interface.
        /// </summary>
        private IDataProvider _provider;
        /// <summary>
        /// Default initialize date.
        /// </summary>
        private DateTime _defaultDate;
        /// <summary>
        /// Import profile.
        /// </summary>
        private ImportProfile _profile;
        /// <summary>
        /// Project data.
        /// </summary>
        private IProjectDataContext _projectData;

        /// <summary>
        /// Progress tracker.
        /// </summary>
        private IProgressInformer _informer;

        /// <summary>
        /// Counter of readed records.
        /// </summary>
        private int _readCount;
        /// <summary>
        /// Counter of failed in parse records.
        /// </summary>
        private int _failedCount;
        /// <summary>
        /// Counter of skipped records.
        /// </summary>
        private int _skippedCount;

        /// <summary>
        /// Import detail list.
        /// </summary>
        private List<MessageDetail> _details = new List<MessageDetail> ();
        /// <summary>
        /// Imported objects.
        /// </summary>
        private List<AppData.DataObject> _importedObjects = new List<AppData.DataObject> ();

        #endregion // Private fields
    }
}

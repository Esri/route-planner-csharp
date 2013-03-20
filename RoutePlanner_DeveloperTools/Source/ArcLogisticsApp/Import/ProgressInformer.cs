using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Diagnostics;

using AppPages = ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Class that provides import progress inform tracker functionality.
    /// </summary>
    internal sealed class ProgressInformer : IProgressInformer, IDisposable
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates and initializes a new instance of the <c>ProgressTracker</c> class.
        /// </summary>
        /// <param name="parentPage">Parent page for status panel.</param>
        /// <param name="type">Import objects type.</param>
        /// <param name="canceler">Operation canceler interface (can be NULL).</param>
        public ProgressInformer(AppPages.Page parentPage, ImportType type, ICanceler canceler)
        {
            Debug.Assert(null != parentPage); // created

            // store context
            _parentPage = parentPage;
            _canceler = canceler;

            // initialize state
            _InitStrings(type);
            _InitStatusStack();
        }

        #endregion // Constructors

        #region IProgressInformer interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Import object face name.
        /// </summary>
        public string ObjectName
        {
            get { return _objectName; }
        }

        /// <summary>
        /// Import objects face name.
        /// </summary>
        public string ObjectsName
        {
            get { return _objectsName; }
        }

        /// <summary>
        /// Parent page for status panel.
        /// </summary>
        public AppPages.Page ParentPage
        {
            get { return _parentPage; }
        }

        /// <summary>
        /// Sets status message. Hide progress bar and button.
        /// </summary>
        /// <param name="statusNameRsc">Name of resource for status text (can be NULL).</param>
        /// <remarks>If statusNameRsc is NULL set to status empty string.</remarks>
        public void SetStatus(string statusNameRsc)
        {
            // this thread has access to the object
            Debug.Assert(_parentPage.Dispatcher.CheckAccess());

            // set status as string
            App currentApp = App.Current;
            object status = string.Empty;
            if (!string.IsNullOrEmpty(statusNameRsc))
            {
                _statusLabel.Content = currentApp.GetString(statusNameRsc, _objectsName);
                status = _statusPanel;
            }

            currentApp.MainWindow.StatusBar.SetStatus(_parentPage, status);
        }

        #endregion // IProgressInformer interface members

        #region IDisposable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Deletes created cursors.
        /// </summary>
        public void Dispose()
        {
            _buttonCancel.Click -= _ButtonCancel_Click;

            _statusPanel = null;
            _statusLabel = null;
            _buttonCancel = null;
            _statusPanel = null;

            _canceler = null;
        }

        #endregion // IDisposable interface members

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cancel button event handler. Show user's question.
        /// </summary>
        /// <param name="sender">Igmored.</param>
        /// <param name="e">Igmored.</param>
        private void _ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (null != _canceler)
            {   // do cancel
                _canceler.Cancel();
            }
        }

        /// <summary>
        /// Inits status stack control.
        /// </summary>
        private void _InitStatusStack()
        {
            // load status control
            _statusPanel = (StackPanel)App.Current.FindResource("textStatusStack");
            foreach (UIElement element in _statusPanel.Children)
            {
                var label = element as Label;
                var button = element as Button;

                if (null != label)
                {
                    if (null == _statusLabel)
                        _statusLabel = label;
                }

                else if (null != button)
                    _buttonCancel = button;

                // else Do nothing
            }

            // all controls must be inited
            Debug.Assert((null != _statusPanel) &&
                         (null != _statusLabel) &&
                         (null != _buttonCancel));

            // init status state
            _buttonCancel.Click += new RoutedEventHandler(_ButtonCancel_Click);
            _buttonCancel.IsEnabled = true;
        }

        /// <summary>
        /// Initializes import object(s) name by import type.
        /// </summary>
        /// <param name="type">Import type.</param>
        private void _InitStrings(ImportType type)
        {
            string objectsNameRsc = null;
            string objectNameRsc = null;
            switch (type)
            {
                case ImportType.Orders:
                    objectsNameRsc = "Orders";
                    objectNameRsc = "Order";
                    break;

                case ImportType.Locations:
                    objectsNameRsc = "Locations";
                    objectNameRsc = "Location";
                    break;

                case ImportType.Drivers:
                    objectsNameRsc = "Drivers";
                    objectNameRsc = "Driver";
                    break;

                case ImportType.Vehicles:
                    objectsNameRsc = "Vehicles";
                    objectNameRsc = "Vehicle";
                    break;

                case ImportType.MobileDevices:
                    objectsNameRsc = "MobileDevices";
                    objectNameRsc = "MobileDevice";
                    break;

                case ImportType.DefaultRoutes:
                    objectsNameRsc = "DefaultRoutes";
                    objectNameRsc = "DefaultRoute";
                    break;

                case ImportType.DriverSpecialties:
                    objectsNameRsc = "DriverSpecialties";
                    objectNameRsc = "DriverSpecialty";
                    break;

                case ImportType.VehicleSpecialties:
                    objectsNameRsc = "VehicleSpecialties";
                    objectNameRsc = "VehicleSpecialty";
                    break;

                case ImportType.Barriers:
                    objectsNameRsc = "Barriers";
                    objectNameRsc = "Barrier";
                    break;

                case ImportType.Zones:
                    objectsNameRsc = "Zones";
                    objectNameRsc = "Zone";
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported type
                    break;
            }

            _objectsName = App.Current.FindString(objectsNameRsc);
            _objectName = App.Current.FindString(objectNameRsc);
        }

        #endregion // Private methods

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Parent page for status panel.
        /// </summary>
        private AppPages.Page _parentPage;

        /// <summary>
        /// Status control label.
        /// </summary>
        private Label _statusLabel;
        /// <summary>
        /// Status control 'Cancel' button.
        /// </summary>
        private Button _buttonCancel;
        /// <summary>
        /// Status panel.
        /// </summary>
        private StackPanel _statusPanel;

        /// <summary>
        /// Cancel traker.
        /// </summary>
        private ICanceler _canceler;

        /// <summary>
        /// Import objects name to status message generation.
        /// </summary>
        private string _objectsName;
        /// <summary>
        /// Import object name to progress message generation.
        /// </summary>
        private string _objectName;

        #endregion // Private fields
    }
}

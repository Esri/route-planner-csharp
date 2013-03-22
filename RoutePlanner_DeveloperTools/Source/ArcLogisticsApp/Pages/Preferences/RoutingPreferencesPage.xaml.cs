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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.Routing;
using Xceed.Wpf.Controls;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for RoutingPreferencesPage.xaml.
    /// </summary>
    internal partial class RoutingPreferencesPage : PageBase
    {
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Page name.
        /// </summary>
        public static string PageName
        {
            get { return PAGE_NAME; }
        }

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>RoutingPreferencesPage</c> class.
        /// </summary>
        public RoutingPreferencesPage()
        {
            InitializeComponent();

            IsRequired = true;
            IsAllowed = true;
            DoesSupportCompleteStatus = false;

            App.Current.ProjectClosed += new EventHandler(_App_ProjectClosed);
            App.Current.Exit += new ExitEventHandler(_Current_Exit);
            this.Loaded += new RoutedEventHandler(_Page_Loaded);
            this.Unloaded += new RoutedEventHandler(_Page_Unloaded);
        }

        #endregion // Constructors

        #region Page overrided members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns unique page name.
        /// </summary>
        public override string Name
        {
            get { return PAGE_NAME; }
        }

        /// <summary>
        /// Returns page title.
        /// </summary>
        public override string Title
        {
            get { return App.Current.FindString("RoutingPreferencesPageCaption"); }
        }

        /// <summary>
        /// Returns page icon as a TileBrush (ImageBrush).
        /// </summary>
        public override TileBrush Icon
        {
            get { return (ImageBrush)App.Current.FindResource("RoutingPreferencesBrush"); }
        }

        /// <summary>
        /// Returns name of Help Topic.
        /// </summary>
        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.RoutingPreferencesPagePath); }
        }

        /// <summary>
        /// Returns category name of commands that will be present in Tasks widget.
        /// </summary>
        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        #endregion // Page overrided members

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handles project AlwaysRoute button checked event. Update relative settings value.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _AlwaysRouteButton_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsRoutingConstraintCheckEnabled = false;
        }

        /// <summary>
        /// Handles project CheckConstraint button checked event. Update relative settings value.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CheckConstraintButton_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsRoutingConstraintCheckEnabled = true;
        }

        /// <summary>
        /// React on page unloaded. Store changes.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            App.Current.MainWindow.NavigationCalled -= _NavigationCalled;

            textBoxArriveDepartDelay.KeyDown -= numericTextBox_KeyDown;
            textBoxArriveDepartDelay.TextChanged -= numericTextBox_TextChanged;
            textBoxArriveDepartDelay.MouseWheel -= numericTextBox_MouseWheel;

            MakeUTurnsAtIntersections.Checked -= _MakeUTurnsAtIntersectionsChecked;
            MakeUTurnsAtIntersections.Unchecked -= _MakeUTurnsAtIntersectionsUnchecked;
            MakeUTurnsAtDeadEnds.Checked -= _MakeUTurnsAtDeadEndsChecked;
            MakeUTurnsAtDeadEnds.Unchecked -= _MakeUTurnsAtDeadEndsUnchecked;
            MakeUTurnsAtStops.Checked -= _MakeUTurnsAtStopsChecked;
            MakeUTurnsAtStops.Unchecked -= _MakeUTurnsAtStopsUnchecked;
            StopOnOrderSide.Checked -= _StopOnOrderSideChecked;
            StopOnOrderSide.Unchecked -= _StopOnOrderSideUnchecked;
        }

        /// <summary>
        /// React on application's project closed. Store changes.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _App_ProjectClosed(object sender, EventArgs e)
        {
            Settings.Default.Save();
        }

        /// <summary>
        /// React on application's exit. Store changes.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _Current_Exit(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
        }

        /// <summary>
        /// React on page loaded. Inits page content.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _Page_Loaded(object sender, RoutedEventArgs e)
        {
            _InitPageContent();
            App.Current.MainWindow.NavigationCalled +=
                new EventHandler(_NavigationCalled);
        }

        /// <summary>
        /// Occurs when user switches the page.
        /// </summary>
        /// <param name="sender">Main window of application.</param>
        /// <param name="e">Event args.</param>
        private void _NavigationCalled(object sender, EventArgs e)
        {
            // forcibly cancel editing to avoid invalid editing state
            // when page will be loaded again.
            xceedGrid.CancelEdit();
        }

        /// <summary>
        /// React on is enabled click for CheckBoxCellTemplate. Converts template state
        /// to restriction value.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _IsEnabled_Click(object sender, RoutedEventArgs e)
        {
            RestrictionDataWrapper selectedItem = xceedGrid.CurrentItem as RestrictionDataWrapper;
            IVrpSolver solver = App.Current.Solver;
            ICollection<Restriction> restrictions = solver.SolverSettings.Restrictions;
            Restriction restriction = _FindRestriction(selectedItem.Restriction.Name, restrictions);
            restriction.IsEnabled = selectedItem.IsEnabled;
        }

        /// <summary>
        /// React on DataGridCollectionViewSource BeginningEdit.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Data grid item cancel event arguments </param>
        private void DataGridCollectionViewSource_BeginningEdit(object sender,
                                                                DataGridItemCancelEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// React on DataGridCollectionViewSource CancelingEdit.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Data grid item handled event argsuments.</param>
        private void DataGridCollectionViewSource_CancelingEdit(object sender,
                                                                DataGridItemHandledEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// React on DataGridCollectionViewSource CommittingEdit.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Data grid item cancel event arguments </param>
        private void DataGridCollectionViewSource_CommittingEdit(object sender,
                                                                 DataGridItemCancelEventArgs e)
        {
            try
            {
                var selectedItem = xceedGrid.CurrentItem as RestrictionDataWrapper;

                IVrpSolver solver = App.Current.Solver;
                SolverSettings solverSettings = solver.SolverSettings;
                NetworkDescription networkDescription = solver.NetworkDescription;

                ICollection<NetworkAttribute> networkAttributes =
                    networkDescription.NetworkAttributes;
                foreach (NetworkAttribute attribute in networkAttributes)
                {
                    if (selectedItem.Restriction.Name.Equals(attribute.Name,
                                                             StringComparison.OrdinalIgnoreCase))
                    {
                        ICollection<NetworkAttributeParameter> paramColl = attribute.Parameters;
                        Debug.Assert(null != paramColl);
                        _UpdateValue(selectedItem, solverSettings, attribute);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// React on numericTextBox KeyDown.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Key event arguments.</param>
        private void numericTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // determine whether the keystroke is a number from the keypad.
            bool isNumPadNumeric = ((Key.NumPad0 <= e.Key) && (e.Key <= Key.NumPad9));
            // determine whether the keystroke is a number from the top of the keyboard.
            bool isNumeric = ((Key.D0 <= e.Key) && (e.Key <= Key.D9));
            // ignore all not numeric keys or Tab
            e.Handled = (!isNumeric && !isNumPadNumeric && (e.Key != Key.Tab));
        }

        /// <summary>
        /// numericTextBox mousewheel handler. Increment/decrement value in TextBox
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Mouse wheel event arguments.</param>
        private void numericTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var textBox = sender as NumericTextBox;

            int delta = e.Delta;
            if (delta > 0)
                _IncrementDelay(textBox);
            else if (delta < 0)
                _DecrementDelay(textBox);
            // else do nothing
        }

        /// <summary>
        /// arrivalIncrementButton click handler.Incremet arrival delay.
        /// </summary>
        private void arrivalIncrementButton_Click(object sender, RoutedEventArgs e)
        {
            _IncrementDelay(textBoxArriveDepartDelay);
        }

        /// <summary>
        /// arrivalDecrementButton click handler. Decrement arrival delay.
        /// </summary>
        private void arrivalDecrementButton_Click(object sender, RoutedEventArgs e)
        {
            _DecrementDelay(textBoxArriveDepartDelay);
        }

        /// <summary>
        /// React on NumericTextBox Arrival/Departure Delay TextChanged.
        /// </summary>
        /// <param name="sender">Related NumericTextBox.</param>
        /// <param name="e">Ignored.</param>
        private void numericTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as NumericTextBox;
            Debug.Assert(null != textBox);

            string text = textBox.Text;
            if (!textBox.HasParsingError &&
                !textBox.HasValidationError &&
                !string.IsNullOrEmpty(text))
            {
                int? value = _ConvertStringToInt(text);
                if (value.HasValue)
                {
                    IVrpSolver solver = App.Current.Solver;
                    Debug.Assert(null != solver);

                    solver.SolverSettings.ArriveDepartDelay = value.Value;
                }
            }
        }

        /// <summary>
        /// Make UTurns at intersections option checked.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void _MakeUTurnsAtIntersectionsChecked(object sender, RoutedEventArgs e)
        {
            MakeUTurnsAtDeadEnds.IsChecked = true;
            MakeUTurnsAtDeadEnds.IsEnabled = false;

            _SaveUTurnSettings();
        }

        /// <summary>
        /// Make UTurns at intersections option unchecked.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void _MakeUTurnsAtIntersectionsUnchecked(object sender, RoutedEventArgs e)
        {
            if (!MakeUTurnsAtDeadEnds.IsEnabled)
            {
                MakeUTurnsAtDeadEnds.IsEnabled = true;
            }

            _SaveUTurnSettings();
        }

        /// <summary>
        /// Make UTurns at Dead Ends option checked.
        /// </summary>
        /// <param name="sender">Not used.</param>>
        /// <param name="e">Not used.</param>
        private void _MakeUTurnsAtDeadEndsChecked(object sender, RoutedEventArgs e)
        {
            _SaveUTurnSettings();
        }

        /// <summary>
        /// Make UTurns at Dead Ends option unchecked.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void _MakeUTurnsAtDeadEndsUnchecked(object sender, RoutedEventArgs e)
        {
            if (MakeUTurnsAtIntersections.IsChecked == true)
            {
                MakeUTurnsAtDeadEnds.IsChecked = true;
                MakeUTurnsAtDeadEnds.IsEnabled = false;
            }

            _SaveUTurnSettings();
        }

        /// <summary>
        /// Make UTurns at stops option checked.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void _MakeUTurnsAtStopsChecked(object sender, RoutedEventArgs e)
        {
            //if (StopOnOrderSide.IsChecked == false)
            //{
                StopOnOrderSide.IsChecked = true;
                StopOnOrderSide.IsEnabled = false;
            //}

            _SaveCurbApproachSettings();
        }

        /// <summary>
        /// Make UTurns at stops option unchecked.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void _MakeUTurnsAtStopsUnchecked(object sender, RoutedEventArgs e)
        {
            if (!StopOnOrderSide.IsEnabled)
            {
                StopOnOrderSide.IsEnabled = true;
            }

            _SaveCurbApproachSettings();
        }

        /// <summary>
        /// Stop on order side option checked.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void _StopOnOrderSideChecked(object sender, RoutedEventArgs e)
        {
            _SaveCurbApproachSettings();
        }

        /// <summary>
        /// Stop on order side option unchecked.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void _StopOnOrderSideUnchecked(object sender, RoutedEventArgs e)
        {
            if (MakeUTurnsAtStops.IsChecked == true)
            {
                StopOnOrderSide.IsChecked = true;
                StopOnOrderSide.IsEnabled = false;
            }

            _SaveCurbApproachSettings();
        }

        #endregion // Event handlers

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Finds restriction by name.
        /// </summary>
        /// <param name="name">Restriction name to find.</param>
        /// <param name="restrictions">Applictaion restrictions.</param>
        /// <returns>Founded restriction or null.</returns>
        private Restriction _FindRestriction(string name, ICollection<Restriction> restrictions)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            Debug.Assert(null != restrictions);

            Restriction foundedRestriction = null;
            foreach (Restriction restriction in restrictions)
            {
                if (name.Equals(restriction.NetworkAttributeName,
                                StringComparison.OrdinalIgnoreCase))
                {
                    foundedRestriction = restriction;
                    break; // NOTE: founded - EXIT
                }
            }

            return foundedRestriction;
        }

        /// <summary>
        /// Checks is current restriction editable.
        /// </summary>
        /// <param name="name">Restriction name to find.</param>
        /// <param name="restrictions">Applictaion restrictions.</param>
        /// <returns>TRUE if founded.</returns>
        private bool _IsRestrictionEditable(string name, ICollection<Restriction> restrictions)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            Debug.Assert(null != restrictions);

            Restriction restriction = _FindRestriction(name, restrictions);
            return restriction.IsEditable;
        }

        /// <summary>
        /// Convert value to string.
        /// </summary>
        /// <param name="value">Value to conversion.</param>
        /// <returns>Converted string value or empty string.</returns>
        private string _ConvertValue2String(object value)
        {
            // NOTE: empty values null) - set as string.Empty
            return (null == value) ? string.Empty :
                                     (string)Convert.ChangeType(value, typeof(string));
        }

        /// <summary>
        /// Updates value.
        /// </summary>
        /// <param name="wrapper">Restriction data wrapper.</param>
        /// <param name="solverSettings">Solver settings.</param>
        /// <param name="attribute">Network attribute.</param>
        private void _UpdateValue(RestrictionDataWrapper wrapper, SolverSettings solverSettings, 
            NetworkAttribute attribute)
        {
            Debug.Assert(null != wrapper);
            Debug.Assert(null != attribute);
            Debug.Assert(null != solverSettings);
            
            // Update restriction usage parameter if it exist.
            if (wrapper.RestrictionUsageParameter != null &&
                wrapper.RestrictionUsageParameter.Name != null)
            {
                _UpdateParameterIfNeeded(solverSettings, attribute, 
                    wrapper.RestrictionUsageParameter.Name, wrapper.RestrictionUsageParameter.Value);
            }

            // Update all other parameters.
            foreach (Parameter parameter in wrapper.Parameters)
            {
                if(!string.IsNullOrEmpty(parameter.Name))
                    _UpdateParameterIfNeeded(solverSettings, attribute, parameter.Name, parameter.Value);
            }
        }

        /// <summary>
        /// Update parameter if it has been changed.
        /// </summary>
        /// <param name="solverSettings">Solver settings in which 
        /// parameter value will be updated.</param>
        /// <param name="attribute">Network attribute, which parameter must be updated.</param>
        /// <param name="parameterName">Name of the parameter which must be updated.</param>
        /// <param name="parameterValue">Value, which must be set in parameter.</param>
        /// <exception cref="System.ArgumentException">Is thrown when there is no 
        /// attribute with such parameter name in solver settings.</exception>
        private void _UpdateParameterIfNeeded(SolverSettings solverSettings,
            NetworkAttribute attribute, string parameterName, string parameterValue)
        {
            Debug.Assert(solverSettings != null);
            Debug.Assert(attribute != null);
            Debug.Assert(parameterName != null);

            // Get parameter.
            var parameter = attribute.Parameters.First(par => par.Name == parameterName);

            // Get current parameter value.
            object valueObj = null;
            if (!solverSettings.GetNetworkAttributeParameterValue(attribute.Name, parameterName, out valueObj))
                throw new ArgumentException("parameterName");
            string value = _ConvertValue2String(valueObj);

            // If value has changed - set new value.
            if (!value.Equals(parameterValue, StringComparison.OrdinalIgnoreCase))
            {
                if (parameterValue != null)
                {
                    try
                    {
                        solverSettings.SetNetworkAttributeParameterValue(attribute.Name,
                            parameter.Name, parameterValue);
                    }
                    // Inputed value is in wrong format - do not change solver settings.
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Builds collection of item properties.
        /// </summary>
        /// <param name="parametersCount">Parameters count.</param>
        /// <param name="itemPorpertiesCollection">Item properties collection.</param>
        /// <param name="collectionSource">Collection source.</param>
        private void _BuildCollectionSource(int parametersCount,
                                            ArrayList itemPorpertiesCollection,
                                            DataGridCollectionViewSource collectionSource)
        {
            Debug.Assert(null != itemPorpertiesCollection);
            Debug.Assert(null != collectionSource);

            collectionSource.ItemProperties.Clear();
            foreach (DataGridItemProperty property in itemPorpertiesCollection)
            {
                if (!property.Name.Equals(DYNAMIC_FIELDS_ALIAS))
                    collectionSource.ItemProperties.Add(property);
                else
                {
                    for (int index = 0; index < parametersCount; ++index)
                    {
                        string valuePath =
                            property.ValuePath + string.Format(DYNAMIC_VALUE_NAME_FORMAT, index);
                        string valueName =
                            _GetDynamicFieldName(index);

                        var newProperty =
                            new DataGridItemProperty(valueName, valuePath, typeof(string));
                        collectionSource.ItemProperties.Add(newProperty);
                    }
                }
            }
        }

        /// <summary>
        /// Makes name for dynamical field.
        /// </summary>
        /// <param name="index">Postfix for dynamic field name.</param>
        private string _GetDynamicFieldName(int index)
        {
            return Parameters.GetFieldName(index);
        }

        /// <summary>
        /// Builds collection of columns.
        /// </summary>
        /// <param name="parametersCount">Parameters count.</param>
        /// <param name="readedColumns">Readed columns.</param>
        /// <param name="columns">Colums.</param>
        private void _BuildColumnsCollection(int parametersCount,
                                             ArrayList readedColumns,
                                             ColumnCollection columns)
        {
            Debug.Assert(null != readedColumns);
            Debug.Assert(null != columns);

            columns.Clear();

            foreach (Column column in readedColumns)
            {
                if (!column.FieldName.Equals(DYNAMIC_FIELDS_ALIAS))
                    columns.Add(column);
                else
                {
                    string parameterColumnTitleFormat = 
                        App.Current.FindString("ParameterColumnHeaderFormat");

                    for (int index = 0; index < parametersCount; ++index)
                    {
                        var col = new Column();
                        col.FieldName = _GetDynamicFieldName(index);
                        col.Title = string.Format(parameterColumnTitleFormat, 
                            (0 == index) ? "" : (index + 1).ToString());
                        col.CellContentTemplate = column.CellContentTemplate;
                        col.CellEditor = column.CellEditor;
                        col.Width = column.Width;
                        col.MinWidth = column.MinWidth;
                        col.MaxWidth = column.MaxWidth;
                        col.CellValidationRules.Add(new ParameterValidationRule());
                        columns.Add(col);
                    }
                }
            }
        }

        /// <summary>
        /// Loads grid structure (ItemPorperties and Columns) from xaml.
        /// </summary>
        /// <param name="key">Resource key.</param>
        /// <param name="parametersCount">Parameters count.</param>
        /// <param name="collectionSource">Collection source.</param>
        /// <param name="xceedGrid">Xceed grid control.</param>
        private void _CreateStructureFromXAML(string key,
                                              int parametersCount,
                                              DataGridCollectionViewSource collectionSource,
                                              DataGridControl xceedGrid)
        {
            Debug.Assert(!string.IsNullOrEmpty(key));
            Debug.Assert(null != collectionSource);
            Debug.Assert(null != xceedGrid);

            try
            {
                // load structure from XAML
                ArrayList itemPorpertiesCollection = null;
                ArrayList columns = null;
                using (Stream stream = this.GetType().Assembly.GetManifestResourceStream(key))
                {
                    string template = new StreamReader(stream).ReadToEnd();
                    using (StringReader stringReader = new StringReader(template))
                    {
                        using (XmlTextReader xmlReader = new XmlTextReader(stringReader))
                        {
                            var resource = XamlReader.Load(xmlReader) as ResourceDictionary;
                            itemPorpertiesCollection =
                                resource[ITEM_PROPERTIES_RESOURCE_NAME] as ArrayList;

                            columns = resource[COLUMNS_RESOURCE_NAME] as ArrayList;
                        }
                    }
                }

                _BuildCollectionSource(parametersCount, itemPorpertiesCollection, collectionSource);
                _BuildColumnsCollection(parametersCount, columns, xceedGrid.Columns);
            }
            catch (Exception ex)
            {
                Logger.Info(ex.Message);
            }
        }

        /// <summary>
        /// Inits grid source.
        /// </summary>
        /// <param name="maxParametersCount">Maximum number of attribute parameters.</param>
        /// <param name="collectionSource">Data grid collection source.</param>
        private void _InitGridSource(int maxParametersCount,
                                     DataGridCollectionViewSource collectionSource)
        {
            IVrpSolver solver = App.Current.Solver;
            SolverSettings solverSettings = solver.SolverSettings;
            NetworkDescription networkDescription = solver.NetworkDescription;
            ICollection<Restriction> restrictions = solverSettings.Restrictions;

            var restrictionWrappers = new List<RestrictionDataWrapper>();

            var networkAttributes = networkDescription.NetworkAttributes;
            foreach (NetworkAttribute attribute in networkAttributes)
            {
                if (attribute.UsageType == NetworkAttributeUsageType.Restriction)
                {
                    Restriction restriction = _FindRestriction(attribute.Name, restrictions);
                    if (restriction.IsEditable)
                    {
                        Debug.Assert(null != attribute.Parameters);

                        // Create collection of all non "restriction usage" attribute parameters.
                        IList<NetworkAttributeParameter> attrParams;
                        if (attribute.RestrictionUsageParameter != null)
                            attrParams = attribute.Parameters.Where(
                                param => param.Name != attribute.RestrictionUsageParameter.Name).ToList();
                        else
                            attrParams = attribute.Parameters.ToList();

                        var parameters = new Parameters(maxParametersCount);
                        for (int index = 0; index < maxParametersCount; ++index)
                        {
                            string value = null;
                            if (index < attrParams.Count())
                            {
                                NetworkAttributeParameter param = attrParams.ElementAt(index);
                                value = _GetParameterValue(attribute.Name, param.Name, solverSettings);
                                parameters[index] = new Parameter(param.Name, value);
                            }
                            else
                                parameters[index] = new Parameter();
                        }

                        // Create wrapper for restriction.
                        var wrapper = new RestrictionDataWrapper(restriction.IsEnabled,
                                                                 restriction.NetworkAttributeName,
                                                                 restriction.Description, parameters);

                        // If attribute has restriction usage parameter - add this parameter 
                        // to wrapper.
                        if (attribute.RestrictionUsageParameter != null)
                        {
                            var restrictionUsageParameterValue = _GetParameterValue(attribute.Name,
                                attribute.RestrictionUsageParameter.Name, solverSettings);
                            var restrictionParameter = new Parameter(attribute.RestrictionUsageParameter.Name, 
                                restrictionUsageParameterValue);
                            wrapper.RestrictionUsageParameter = restrictionParameter;
                        }

                        restrictionWrappers.Add(wrapper);
                    }
                }
            }

            collectionSource.Source = restrictionWrappers;
        }

        /// <summary>
        /// Get value for parameter.
        /// </summary>
        /// <param name="attributeName">Attribute name.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="solverSettings">Solver settings from which 
        /// parameter value will be taken.</param>
        /// <returns>Parameter value or empty string if no such parameter.</returns>
        private string _GetParameterValue(string attributeName, string parameterName, 
            SolverSettings solverSettings)
        {
            object valueObj = null;
            if (!solverSettings.GetNetworkAttributeParameterValue(attributeName, 
                parameterName, out valueObj))
                valueObj = null;

            return _ConvertValue2String(valueObj);
        }

        /// <summary>
        /// Inits data grid.
        /// </summary>
        private void _InitDataGrid()
        {
            try
            {
                // If active schedule hasn't been set - do it.
                if (App.Current.Project != null &&
                    App.Current.Project.Schedules.ActiveSchedule == null)
                {
                    // Load schedule for current date.
                    var currentSchedule = OptimizeAndEditHelpers.LoadSchedule(
                        App.Current.Project,
                        App.Current.CurrentDate,
                        OptimizeAndEditHelpers.FindScheduleToSelect);

                    // If current date schedule have routes - 
                    // select current schedule as active.
                    if (currentSchedule.Routes.Count > 0)
                        App.Current.Project.Schedules.ActiveSchedule = currentSchedule;
                }

                _ClearGridSource();

                IVrpSolver solver = App.Current.Solver;
                SolverSettings solverSettings = solver.SolverSettings;
                ICollection<Restriction> restrictions = solverSettings.Restrictions;
                if (0 < restrictions.Count)
                {
                    NetworkDescription networkDescription = solver.NetworkDescription;

                    // Obtain max "non-restriction" parameters count.
                    int maxParametersCount = _GetNonRestrictionParametersMaximumCount(
                        networkDescription, restrictions);

                    _InitDataGridLayout(maxParametersCount);
                }
            }
            catch (Exception ex)
            {
                Logger.Critical(ex);
            }
        }

        /// <summary>
        /// Clear grid collection source.
        /// </summary>
        private void _ClearGridSource()
        {
            var collectionSource =
                mainGrid.FindResource(COLLECTION_SOURCE_KEY) as DataGridCollectionViewSource;

            collectionSource.Source = null;
        }

        /// <summary>
        /// Inits grid structure.
        /// </summary>
        /// <param name="maxParametersCount">Maximum number of attribute parameters.</param>
        private void _InitDataGridLayout(int maxParametersCount)
        {
            var collectionSource =
                mainGrid.FindResource(COLLECTION_SOURCE_KEY) as DataGridCollectionViewSource;

            _CreateStructureFromXAML(GridSettingsProvider.RoutingPreferencesGridStructure,
                                     maxParametersCount,
                                     collectionSource,
                                     xceedGrid);
            xceedGrid.Columns["IsEnabled"].CellEditor =
                mainGrid.FindResource("CheckBoxCellEditor") as CellEditor;

            _InitGridSource(maxParametersCount, collectionSource);

            xceedGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        ///  Get max number of "non-restrictionusage" parameters which restrictions has.
        /// </summary>
        /// <param name="description">Network description.</param>
        /// <param name="restrictions">List of restrictions.</param>
        /// <returns>Max number.</returns>
        private int _GetNonRestrictionParametersMaximumCount(NetworkDescription description, 
            ICollection<Restriction> restrictions)
        {
            var count = 0;
            ICollection<NetworkAttribute> networkAttributes =
                description.NetworkAttributes;

            foreach (NetworkAttribute attribute in networkAttributes)
            {
                if (attribute.UsageType == NetworkAttributeUsageType.Restriction &&
                    _IsRestrictionEditable(attribute.Name, restrictions))
                {
                    var attributeParametersCount = attribute.Parameters.Count;

                    // If attribute have restrictionusage parameter - reduce number of parameters.
                    if (attribute.RestrictionUsageParameter != null)
                        attributeParametersCount--;

                    count = Math.Max(attributeParametersCount, count);
                }
            }

            return count;
        }

        /// <summary>
        /// Inits page controls.
        /// </summary>
        private void _InitPageContent()
        {
            App.Current.MainWindow.StatusBar.SetStatus(this, null);

            checkConstraintButton.IsChecked =
                Properties.Settings.Default.IsRoutingConstraintCheckEnabled;
            alwaysRouteButton.IsChecked = !checkConstraintButton.IsChecked;

            checkConstraintButton.Checked += new RoutedEventHandler(_CheckConstraintButton_Checked);
            alwaysRouteButton.Checked += new RoutedEventHandler(_AlwaysRouteButton_Checked);

            _InitDataGrid();

            IVrpSolver solver = App.Current.Solver;
            SolverSettings settings = _GetSolverSettings(solver);

            if (null == settings)
            {
                textBoxArriveDepartDelay.Text = string.Empty;
                textBoxArriveDepartDelay.IsEnabled = false;

                MakeUTurnsAtIntersections.IsEnabled = false;
                MakeUTurnsAtDeadEnds.IsEnabled = false;
                MakeUTurnsAtStops.IsEnabled = false;
                StopOnOrderSide.IsEnabled = false;
            }
            else
            {
                textBoxArriveDepartDelay.Text = settings.ArriveDepartDelay.ToString();
                textBoxArriveDepartDelay.IsEnabled = true;

                textBoxArriveDepartDelay.KeyDown +=
                    new System.Windows.Input.KeyEventHandler(numericTextBox_KeyDown);
                textBoxArriveDepartDelay.TextChanged +=
                    new TextChangedEventHandler(numericTextBox_TextChanged);
                textBoxArriveDepartDelay.MouseWheel +=
                    new MouseWheelEventHandler(numericTextBox_MouseWheel);

                // Enable all check boxes.
                MakeUTurnsAtIntersections.IsEnabled = true;
                MakeUTurnsAtDeadEnds.IsEnabled = true;
                MakeUTurnsAtStops.IsEnabled = true;
                StopOnOrderSide.IsEnabled = true;

                // U-Turn policies.
                MakeUTurnsAtIntersections.Checked += _MakeUTurnsAtIntersectionsChecked;
                MakeUTurnsAtIntersections.Unchecked += _MakeUTurnsAtIntersectionsUnchecked;
                MakeUTurnsAtDeadEnds.Checked += _MakeUTurnsAtDeadEndsChecked;
                MakeUTurnsAtDeadEnds.Unchecked += _MakeUTurnsAtDeadEndsUnchecked;
                MakeUTurnsAtStops.Checked += _MakeUTurnsAtStopsChecked;
                MakeUTurnsAtStops.Unchecked += _MakeUTurnsAtStopsUnchecked;
                StopOnOrderSide.Checked += _StopOnOrderSideChecked;
                StopOnOrderSide.Unchecked += _StopOnOrderSideUnchecked;

                // Initialize U-Turn policies.
                MakeUTurnsAtIntersections.IsChecked = settings.UTurnAtIntersections;
                MakeUTurnsAtDeadEnds.IsChecked = settings.UTurnAtDeadEnds;
                MakeUTurnsAtStops.IsChecked = settings.UTurnAtStops;
                StopOnOrderSide.IsChecked = settings.StopOnOrderSide;
            }
        }

        /// <summary>
        /// Gets solver settings.
        /// </summary>
        /// <param name="solver">The reference to the VRP solver.</param>
        /// <returns>Solver settings or NULL.</returns>
        private SolverSettings _GetSolverSettings(IVrpSolver solver)
        {
            Debug.Assert(solver != null);

            SolverSettings settings = null;
            try
            {
                settings = solver.SolverSettings;
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException ||
                    e is AuthenticationException ||
                    e is CommunicationException)
                {
                    Logger.Info(e);
                }
                else
                {
                    throw; // exception
                }
            }

            return settings;
        }

        /// <summary>
        /// Converts string to integer.
        /// </summary>
        /// <param name="text">Text to parsing.</param>
        /// <returns>Parsed value or null.</returns>
        private int? _ConvertStringToInt(string text)
        {
            int? value = null;
            try
            {
                int val = int.Parse(text);
                if (0 <= val)
                    value = val;
            }
            catch
            {
            }

            return value;
        }

        /// <summary>
        /// Incrementes numeric TextBox value.
        /// </summary>
        /// <param name="textBox">Numeric TextBox to update.</param>
        private void _IncrementDelay(NumericTextBox textBox)
        {
            Debug.Assert(null != textBox);

            var value = (UInt32)textBox.Value;
            var maxValue = (UInt32)textBox.MaxValue;
            if (value < maxValue)
                textBox.Value = value + 1;
        }

        /// <summary>
        /// Decrementes numeric TextBox value.
        /// </summary>
        /// <param name="textBox">Numeric TextBox to update.</param>
        private void _DecrementDelay(NumericTextBox textBox)
        {
            Debug.Assert(null != textBox);

            if (textBox.HasValidationError)
                return;

            var value = (UInt32)textBox.Value;
            var minValue = (UInt32)textBox.MinValue;
            if (minValue < value)
                textBox.Value = value - 1;
        }

        /// <summary>
        /// Saves UTurn settings.
        /// </summary>
        private void _SaveUTurnSettings()
        {
            IVrpSolver solver = App.Current.Solver;
            SolverSettings settings = _GetSolverSettings(solver);

            if (settings != null)
            {
                settings.UTurnAtIntersections = MakeUTurnsAtIntersections.IsChecked ?? false;
                settings.UTurnAtDeadEnds = MakeUTurnsAtDeadEnds.IsChecked ?? false;
            }
        }


        /// <summary>
        /// Saves Curb approach settings.
        /// </summary>
        private void _SaveCurbApproachSettings()
        {
            IVrpSolver solver = App.Current.Solver;
            SolverSettings settings = _GetSolverSettings(solver);

            if (settings != null)
            {
                settings.UTurnAtStops = MakeUTurnsAtStops.IsChecked ?? false;
                settings.StopOnOrderSide = StopOnOrderSide.IsChecked ?? false;
            }
        }

        #endregion // Private helpers

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Page name.
        /// </summary>
        private const string PAGE_NAME = "Routing";

        /// <summary>
        /// Dynamic fields alias.
        /// </summary>
        private const string DYNAMIC_FIELDS_ALIAS = "Parameters";

        /// <summary>
        /// Dynamic value name format.
        /// </summary>
        private const string DYNAMIC_VALUE_NAME_FORMAT = "Parameters[{0}].Value";

        /// <summary>
        /// Collection source key.
        /// </summary>
        private const string COLLECTION_SOURCE_KEY = "restrictionsCollection";
        /// <summary>
        /// Item properties resource name.
        /// </summary>
        private const string ITEM_PROPERTIES_RESOURCE_NAME = "itemProperties";
        /// <summary>
        /// Columns resource name.
        /// </summary>
        private const string COLUMNS_RESOURCE_NAME = "columns";

        #endregion // Private constants
    }

    /// <summary>
    /// Parameter validation rule - check type.
    /// </summary>
    internal sealed class ParameterValidationRule :
        Xceed.Wpf.DataGrid.ValidationRules.CellValidationRule
    {
        /// <summary>
        /// Creates a new instance of the <c>ParameterValidationRule</c> class.
        /// </summary>
        public ParameterValidationRule()
        { }

        /// <summary>
        /// Does validate.
        /// </summary>
        /// <param name="value">Value to validation.</param>
        /// <param name="culture">Culture info.</param>
        /// <param name="context">Cell validation context.</param>
        /// <returns>Validation result.</returns>
        public override ValidationResult Validate(object value,
                                                  CultureInfo culture,
                                                  CellValidationContext context)
        {
            bool isValid = true;

            string newValue = value as string;
            IVrpSolver solver = App.Current.Solver;
            NetworkDescription networkDescription = solver.NetworkDescription;

            if (newValue != null && networkDescription != null)
            {
                var wrapper = context.DataItem as RestrictionDataWrapper;

                ICollection<NetworkAttribute> networkAttributes = networkDescription.NetworkAttributes;
                foreach (NetworkAttribute attribute in networkAttributes)
                {
                    // If it is current attribute - find parameter to validate.
                    if (wrapper.Restriction.Name.Equals(attribute.Name,
                                                        StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.Assert(null != attribute.Parameters);
                        NetworkAttributeParameter[] parameters = attribute.Parameters.ToArray();

                        int paramIndex = Parameters.GetIndex(context.Cell.FieldName);

                        // If parameter index was found.
                        if(paramIndex != -1)
                        {
                            var parameter = wrapper.Parameters[paramIndex];
                            
                            // Get corresponding network attribute parameter 
                            // and check that value can be converted to parameter type.
                            NetworkAttributeParameter param = parameters.FirstOrDefault(
                                x => x.Name == parameter.Name);

                            // If string is not empty or if parameter doesn't accept empty string - 
                            // try to convert value.
                            if ((string)value != string.Empty || !param.IsEmptyStringValid)
                            {
                                try
                                {
                                    Convert.ChangeType(value, param.Type); // NOTE: ignore result
                                }
                                catch
                                {
                                    isValid = false;
                                }
                            }
                        }

                        break;
                    }
                }
            }

            return (isValid) ? ValidationResult.ValidResult :
                               new ValidationResult(false, App.Current.FindString("NotValidValueText"));
        }
    }
}

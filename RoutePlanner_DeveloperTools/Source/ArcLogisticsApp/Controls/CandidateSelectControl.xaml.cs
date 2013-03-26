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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Geocoded address candidates window.
    /// </summary>
    internal sealed partial class CandidateSelectControl: UserControl
    {
        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public CandidateSelectControl()
        {
            InitializeComponent();
            this.Resources = Application.Current.Resources;

            // Init datagrid control.
            _collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource("candidatesCollection");
            _addressCandidates = new List<AddressCandidate>();
            DataGridControl.SelectionChanged += new DataGridSelectionChangedEventHandler(_DataGridControlSelectionChanged);
        }

        #endregion constructors

        #region public events

        /// <summary>
        /// Occurs when position and address applied.
        /// </summary>
        public event EventHandler CandidateApplied;

        /// <summary>
        /// Occurs when position applied.
        /// </summary>
        public event EventHandler CandidatePositionApplied;

        /// <summary>
        /// Occurs when candidates not applied.
        /// </summary>
        public event EventHandler CandidateLeaved;

        /// <summary>
        /// Occurs when candidate changed.
        /// </summary>
        public event EventHandler CandidateChanged;

        #endregion public events

        #region public methods

        /// <summary>
        /// Set geocodable type(location or order).
        /// </summary>
        /// <param name="geocodableType">Geocodable type.</param>
        public void Initialize(Type geocodableType)
        {
            if (geocodableType == typeof(Order))
            {
                ButtonApply.ToolTip = (string)App.Current.FindResource(APPLY_ORDER_TOOLTIP_TEXT_RESOURCE_NAME);
                ButtonApplyAddress.ToolTip = (string)App.Current.FindResource(UPDATE_ORDER_TOOLTIP_TEXT_RESOURCE_NAME);
            }
            else if (geocodableType == typeof(Location))
            {
                ButtonApply.ToolTip = (string)App.Current.FindResource(APPLY_LOCATION_TOOLTIP_TEXT_RESOURCE_NAME);
                ButtonApplyAddress.ToolTip = (string)App.Current.FindResource(UPDATE_LOCATION_TOOLTIP_TEXT_RESOURCE_NAME);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        /// <summary>
        /// Add candidates array to list.
        /// </summary>
        /// <param name="candidates">Candidates array.</param>
        public void AddCandidates(List<AddressCandidate> candidates)
        {
            Debug.Assert(candidates.Count > 0);

            _InitCandidatesGridCollection(candidates);
        }

        /// <summary>
        /// Get current candidate. Null if selection is empty.
        /// </summary>
        /// <returns>Current candidate.</returns>
        public AddressCandidate GetCandidate()
        {
            AddressCandidate candidate = null;

            // Selection can be empty.
            if (DataGridControl.SelectedItems.Count > 0)
            {
                candidate = (AddressCandidate)DataGridControl.SelectedItems[0];
            }

            return candidate;
        }

        /// <summary>
        /// Clear list.
        /// </summary>
        public void ClearList()
        {
            _addressCandidates.Clear();
        }

        /// <summary>
        /// Select candidate.
        /// </summary>
        /// <param name="item">Candidate.</param>
        public void SelectCandidate(AddressCandidate item)
        {
            if (item == null)
            {
                if (DataGridControl.SelectedItems.Count > 0)
                {
                    DataGridControl.SelectedItems.Clear();
                }
                Debug.Assert(DataGridControl.SelectedItems.Count == 0);
            }
            else
            {
                if (DataGridControl.SelectedItems.Count == 0)
                {
                    DataGridControl.SelectedItems.Add(item);
                }
                else
                {
                    Debug.Assert(DataGridControl.SelectedItems.Count == 1);
                    DataGridControl.SelectedItems.Clear();
                    DataGridControl.SelectedItems.Add(item);
                }

                DataGridControl.BringItemIntoView(item);
            }

            _SetButtonApplyIsEnabled();
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// React on candidate position applied.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ButtonApplyClick(object sender, RoutedEventArgs e)
        {
            if (CandidatePositionApplied != null)
                CandidatePositionApplied(this, EventArgs.Empty);
        }

        /// <summary>
        /// React on candidate applied.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ButtonApplyAddressClick(object sender, RoutedEventArgs e)
        {
            if (CandidateApplied != null)
                CandidateApplied(this, EventArgs.Empty);
        }

        /// <summary>
        /// React on candidate select canceled.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ButtonLeaveClick(object sender, RoutedEventArgs e)
        {
            if (CandidateLeaved != null)
                CandidateLeaved(this, EventArgs.Empty);
        }

        /// <summary>
        /// React on mouse double click in grid.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridControlMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CandidatePositionApplied != null)
                CandidatePositionApplied(this, EventArgs.Empty);
        }

        /// <summary>
        /// Initializes source collection for datagrid control. Binding sets automatically.
        /// </summary>
        /// <param name="candidates">Candidates list.</param>
        private void _InitCandidatesGridCollection(List<AddressCandidate> candidates)
        {
            foreach (AddressCandidate candidate in candidates)
                _addressCandidates.Add(candidate);

            DataGridControl.SelectionChanged -= new DataGridSelectionChangedEventHandler(_DataGridControlSelectionChanged);
            _collectionSource.Source = _addressCandidates;
            DataGridControl.SelectionChanged += new DataGridSelectionChangedEventHandler(_DataGridControlSelectionChanged);

            // Clear selection.
            SelectCandidate(null);
        }

        /// <summary>
        /// React on candidate selection changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridControlSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            _SetButtonApplyIsEnabled();

            if (CandidateChanged != null && DataGridControl.SelectedItems.Count != 0)
                CandidateChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Set apply button enability.
        /// </summary>
        private void _SetButtonApplyIsEnabled()
        {
            if (DataGridControl.SelectedItems.Count == 0)
            {
                ButtonApply.IsEnabled = false;
                ButtonApplyAddress.IsEnabled = false;
            }
            else
            {
                ButtonApply.IsEnabled = true;
                ButtonApplyAddress.IsEnabled = true;
            }
        }

        #endregion private methods

        #region Private constants

        /// <summary>
        /// Resource name for tool tip for apply button.
        /// </summary>
        private const string APPLY_LOCATION_TOOLTIP_TEXT_RESOURCE_NAME = "ApplyLocationTooltip";
        
        /// <summary>
        /// Resource name for tool tip for update button.
        /// </summary>
        private const string UPDATE_LOCATION_TOOLTIP_TEXT_RESOURCE_NAME = "UpdateLocationTooltip";

        /// <summary>
        /// Resource name for tool tip for apply button.
        /// </summary>
        private const string APPLY_ORDER_TOOLTIP_TEXT_RESOURCE_NAME = "ApplyOrderTooltip";

        /// <summary>
        /// Resource name for tool tip for update button.
        /// </summary>
        private const string UPDATE_ORDER_TOOLTIP_TEXT_RESOURCE_NAME = "UpdateOrderTooltip";

        #endregion

        #region private fields

        /// <summary>
        /// Datagrid control collection source.
        /// </summary>
        private DataGridCollectionViewSource _collectionSource;

        /// <summary>
        /// Geocoded address candidates.
        /// </summary>
        private List<AddressCandidate> _addressCandidates;

        #endregion
    }
}

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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interaction logic for internal barrier editor.
    /// </summary>
    internal partial class BarrierEditor : UserControl
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public BarrierEditor()
        {
            InitializeComponent();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Save control state.
        /// </summary>
        internal void SaveState()
        {
            _SaveState();
        }

        #endregion

        #region Public members

        /// <summary>
        /// Edited barrier.
        /// </summary>
        public Barrier Barrier
        {
            get
            {
                return _barrier;
            }
            set
            {
                if (_barrier != null)
                {
                    _barrier.PropertyChanged -= new PropertyChangedEventHandler(_BarrierPropertyChanged);
                }

                _barrier = value;

                if (_barrier != null)
                {
                    _barrier.PropertyChanged += new PropertyChangedEventHandler(_BarrierPropertyChanged);
                    if (_barrier.Geometry != null)
                    {
                        _SetControlState(_barrier);
                    }
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// React on barrier property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event args.</param>
        private void _BarrierPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(Barrier.PropertyNameGeometry) && _barrier.Geometry != null)
            {
                _SetControlState(_barrier);
            }
        }

        /// <summary>
        /// Set control state.
        /// </summary>
        /// <param name="barrier">Source barrier.</param>
        private void _SetControlState(Barrier barrier)
        {
            _isDuringInternalStateChanging = false;

            // Set block travel button is checked if true.
            if (barrier.BarrierEffect.BlockTravel)
            {
                BlockTravelButton.IsChecked = true;
            }

            if (barrier.Geometry is ESRI.ArcLogistics.Geometry.Point)
            {
                _SetPointBarrierControlState(barrier);
            }
            else if (barrier.Geometry is Polyline || barrier.Geometry is Polygon)
            {
                _SetPolyBarrierControlState(barrier);
            }
            else
                Debug.Assert(false);

            _isDuringInternalStateChanging = true;
        }

        /// <summary>
        /// Set control state for point barrier.
        /// </summary>
        /// <param name="barrier">Source barrier.</param>
        private void _SetPointBarrierControlState(Barrier barrier)
        {
            // Set panels visibility.
            textBoxSlowdown.Visibility = Visibility.Collapsed;
            TextSlowdownPercent.Visibility = Visibility.Collapsed;
            SlowdownButton.Visibility = Visibility.Collapsed;
            textBoxSpeedUp.Visibility = Visibility.Collapsed;
            TextSpeedupPercent.Visibility = Visibility.Collapsed;
            SpeedUpButton.Visibility = Visibility.Collapsed;

            DelayPanel.Visibility = Visibility.Visible;

            textBoxDelay.DataContext = barrier;

            // Set delay time
            BlockTravelButton.IsChecked = barrier.BarrierEffect.BlockTravel;
            if (!barrier.BarrierEffect.BlockTravel)
            {
                // Set barrier delay time.
                DelayButton.IsChecked = true;
            }
            else
            {
                // Set last used delay time.
                textBoxDelay.Value = Settings.Default.LastUsedDelayBarrierTime;
            }
        }

        /// <summary>
        /// Set control state for polyline or polygon barrier.
        /// </summary>
        /// <param name="barrier">Source barrier.</param>
        private void _SetPolyBarrierControlState(Barrier barrier)
        {
            // Set panels visibility.
            textBoxSlowdown.Visibility = Visibility.Visible;
            TextSlowdownPercent.Visibility = Visibility.Visible;
            SlowdownButton.Visibility = Visibility.Visible;
            textBoxSpeedUp.Visibility = Visibility.Visible;
            TextSpeedupPercent.Visibility = Visibility.Visible;
            SpeedUpButton.Visibility = Visibility.Visible;

            DelayPanel.Visibility = Visibility.Collapsed;

            // Set speed factor.
            double speedFactor = barrier.BarrierEffect.SpeedFactorInPercent;
            if (!barrier.BarrierEffect.BlockTravel)
            {
                if (speedFactor >= 0)
                {
                    SpeedUpButton.IsChecked = true;
                }
                else
                {
                    SlowdownButton.IsChecked = true;
                }
            }

            textBoxSlowdown.Text = Math.Abs(speedFactor).ToString();
            textBoxSpeedUp.Text = Math.Abs(speedFactor).ToString();
        }

        /// <summary>
        /// Save control state.
        /// </summary>
        private void _SaveState()
        {
            // If barrier is saving now or it is null - dont save it.
            if (!_isDuringInternalStateChanging || _barrier == null)
                return;

            BarrierEffect barrierEffect = _barrier.BarrierEffect;
            if (BlockTravelButton.IsChecked.Value)
            {
                // Set block travel.
                barrierEffect.BlockTravel = true;
            }
            else
            {
                barrierEffect.BlockTravel = false;

                if (_barrier.Geometry is ESRI.ArcLogistics.Geometry.Point)
                {
                    // Set delay time.
                    if (DelayButton.IsChecked.Value)
                    {
                        barrierEffect.DelayTime = (double)textBoxDelay.Value;
                        Settings.Default.LastUsedDelayBarrierTime = barrierEffect.DelayTime;
                        Settings.Default.Save();
                    }
                }
                else if (_barrier.Geometry is ESRI.ArcLogistics.Geometry.Polygon)
                {
                    double speedFactor = 0;

                    if (SlowdownButton.IsChecked.Value)
                    {
                        speedFactor = - Math.Abs((double)textBoxSlowdown.Value);
                    }
                    else if (SpeedUpButton.IsChecked.Value)
                    {
                        speedFactor = Math.Abs((double)textBoxSpeedUp.Value);
                    }

                    // Update barrierEffect.SpeedFactorInPercent.
                    barrierEffect.SpeedFactorInPercent = speedFactor;
                    Settings.Default.LastUsedBarrierSpeedFactor = speedFactor;
                    Settings.Default.Save();
                }
                else
                    Debug.Assert(false);
            }
        }

        /// <summary>
        /// React on radio button clicked.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _RadioButtonClick(object sender, RoutedEventArgs e)
        {
            _SaveState();
        }

        /// <summary>
        /// React on numeric value changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _NumericTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            _SaveState();
        }

        #endregion

        #region Private members

        /// <summary>
        /// Edited barrier.
        /// </summary>
        private Barrier _barrier;

        /// <summary>
        /// Is during internal state changing.
        /// </summary>
        private bool _isDuringInternalStateChanging;

        #endregion
    }
}

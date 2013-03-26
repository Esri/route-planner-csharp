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
using System.Text;
using System.Windows.Media.Animation;
using System.Windows;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Class presents methods to animate GridLength property
    /// </summary>
    internal class GridLengthAnimation : AnimationTimeline
    {
        #region Override Properties

        /// <summary>
        /// Gets animated property type
        /// </summary>
        public override Type TargetPropertyType
        {
            get 
            {
                return typeof(GridLength);
            }
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Overrided. Creates new Instance of animation.
        /// </summary>
        /// <returns></returns>
        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        /// <summary>
        /// Overrided. Returns new value of grid length each AnimationClock "tick" 
        /// </summary>
        /// <param name="defaultOriginValue"></param>
        /// <param name="defaultDestinationValue"></param>
        /// <param name="animationClock"></param>
        /// <returns></returns>
        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            double fromVal = ((GridLength)GetValue(GridLengthAnimation.FromProperty)).Value;
            double toVal = ((GridLength)GetValue(GridLengthAnimation.ToProperty)).Value;

            if (fromVal > toVal)
            {
                return new GridLength((1 - animationClock.CurrentProgress.Value) * (fromVal - toVal) + toVal, GridUnitType.Pixel);
            }
            else
                return new GridLength(animationClock.CurrentProgress.Value * (toVal - fromVal) + fromVal, GridUnitType.Pixel);
        }

        #endregion

        #region Public Properties

        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(GridLength),
                typeof(GridLengthAnimation));

        /// <summary>
        /// Gets/sets start animation length
        /// </summary>
        public GridLength From
        {
            get
            {
                return (GridLength)GetValue(GridLengthAnimation.FromProperty);
            }
            set
            {
                SetValue(GridLengthAnimation.FromProperty, value);
            }
        }

        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(GridLength),
                typeof(GridLengthAnimation));
        
        /// <summary>
        /// Gets/sets end animation length
        /// </summary>
        public GridLength To
        {
            get
            {
                return (GridLength)GetValue(GridLengthAnimation.ToProperty);
            }
            set
            {
                SetValue(GridLengthAnimation.ToProperty, value);
            }
        }

        #endregion
    }
}

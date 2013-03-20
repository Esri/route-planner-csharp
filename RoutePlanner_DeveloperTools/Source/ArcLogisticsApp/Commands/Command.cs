/*
COPYRIGHT 1995-2010 ESRI
TRADE SECRETS: ESRI PROPRIETARY AND CONFIDENTIAL
Unpublished material - all rights reserved under the 
Copyright Laws of the United States.
For additional information, contact:
Environmental Systems Research Institute, Inc.
Attn: Contracts Dept
380 New York Street
Redlands, California, USA 92373
email: contracts@esri.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Basic class for command. It's inherits from Dependency object for automatic update UI
    /// when any property changes
    /// </summary>
    public abstract class Command : DependencyObject
    {
        #region properties

        public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register("IsEnabled", typeof(bool), typeof(Command));

        /// <summary>
        /// Name of the command. Must be unique and unchanging.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Title of the command that can be shown in UI.
        /// </summary>
        public abstract string Title
        {
            get;
        }

        /// <summary>
        /// Tooltip text.
        /// </summary>
        public abstract string TooltipText
        {
            get;
        }

        /// <summary>
        /// Indicates either command is enabled.
        /// </summary>
        public abstract bool IsEnabled
        {
            get;
        }

        #endregion

        #region methods

        /// <summary>
        /// Initalizes command with the application.
        /// </summary>
        /// <param name="app"></param>
        public abstract void Initialize(App app);

        /// <summary>
        /// Executes command. Parameters number depends on specific command.
        /// </summary>
        /// <param name="args"></param>
        public abstract void Execute(params object[] args);

        #endregion
    }
}

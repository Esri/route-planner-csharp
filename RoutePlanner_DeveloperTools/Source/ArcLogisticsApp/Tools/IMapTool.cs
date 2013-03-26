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
using System.Windows.Input;
using ESRI.ArcLogistics.App.Controls;

namespace ESRI.ArcLogistics.App.Tools
{
    internal interface IMapTool
    {
        /// <summary>
        /// Initializes tool with map control.
        /// </summary>
        /// <param name="mapControl"></param>
        void Initialize(MapControl mapControl);

        /// <summary>
        /// Tool's cursor.
        /// </summary>
        Cursor Cursor { get; }

        /// <summary>
        /// Is tool enabled.
        /// </summary>
        bool IsEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Is tool activated.
        /// </summary>
        bool IsActivated
        {
            get;
        }

        /// <summary>
        /// Tool's tooltip text.
        /// </summary>
        string TooltipText { get; }

        /// <summary>
        /// Tool's title text.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Icon's URI source.
        /// </summary>
        string IconSource { get; }

        /// <summary>
        /// Called when tool is activated on toolbar.
        /// </summary>
        void Activate();

        /// <summary>
        /// Called when tool is deactivated on toolbar.
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Called on Key down event raised from Map Control when tool is activated.
        /// </summary>
        void OnKeyDown(int keyCode, int shift);

        /// <summary>
        /// Called on Key up event raised from Map Control when tool is activated.
        /// </summary>
        void OnKeyUp(int keyCode, int shift);

        /// <summary>
        /// Called on Mouse double click event raised from Map Control when tool is activated.
        /// </summary>
        void OnDblClick(ModifierKeys modifierKeys, double x, double y);

        /// <summary>
        /// Called on MouseDown event raised from Map Control when tool is activated.
        /// </summary>
        void OnMouseDown(MouseButton pressedButton,
            ModifierKeys modifierKeys, double x, double y);

        /// <summary>
        /// Called on MouseMove event raised from Map Control when tool is activated.
        /// </summary>
        void OnMouseMove(MouseButtonState left, MouseButtonState right,
            MouseButtonState middle, ModifierKeys modifierKeys,
            double x, double y);

        /// <summary>
        /// Called on MouseUp event raised from Map Control when tool is activated.
        /// </summary>
        void OnMouseUp(MouseButton pressedButton,
            ModifierKeys modifierKeys, double x, double y);

        /// <summary>
        /// Event is raised when tool finished its job.
        /// </summary>
        event EventHandler OnComplete;

        /// <summary>
        /// Event is raised when tool enability changed.
        /// </summary>
        event EventHandler EnabledChanged;

        /// <summary>
        /// Event is raised when tool activated.
        /// </summary>
        event EventHandler ActivatedChanged;
    }
}
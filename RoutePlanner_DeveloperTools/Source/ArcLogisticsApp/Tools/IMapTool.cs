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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AppCommands = ESRI.ArcLogistics.App.Commands;
using System.Diagnostics;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Button that executes underlying command. Binds to command properties and reflects its state.
    /// </summary>
    internal partial class CommandButton : Button
    {
        #region Constructors

        public CommandButton()
        {
            InitializeComponent();
            _InitEvents();
        }

        #endregion

        #region public Properties

        /// <summary>
        /// Commands that is executed each time when user presses the button.
        /// </summary>
        public AppCommands.ICommand ApplicationCommand
        {
            get
            {
                return _cmd;
            }
            set
            {
                Debug.Assert(_cmd == null); // Command defines one time for each button.

                _cmd = value;

                // Next code define whether command support ISupportDisabledExecution
                // If "yes" - style will be updated by handle PropertyChanged event (IsEnabled property can be in 3 states : Enabled, Disabled and Clickable Disabled.)
                // If "no" - set binding to "IsEnabled" property. 
                if (_cmd is ISupportDisabledExecution)
                {
                    ((INotifyPropertyChanged)_cmd).PropertyChanged += new PropertyChangedEventHandler(_CommandPropertyChanged); // Add handler to update command when some property changed.
                    _SetCommandButtonStyle();
                }
                else
                {
                    _SetIsEnabledPropertyBinding();
                }

                _SetTooltipPropertyBinding();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method inits necessary event handlers.
        /// </summary>
        private void _InitEvents()
        {
            this.Click += new RoutedEventHandler(_Click);
        }

        /// <summary>
        /// Sets binding to "Tooltip" property
        /// </summary>
        private void _SetTooltipPropertyBinding()
        {
            _CreatePropertyBinding(TOOLTIP_PROPERTY_STRING, _cmd, this, ToolTipProperty);
        }


        /// <summary>
        /// Sets binding to "IsEnabled" command property
        /// </summary>
        private void _SetIsEnabledPropertyBinding()
        {
            _CreatePropertyBinding(IS_ENABLED_PROPERTY_STRING, _cmd, this, IsEnabledProperty);
        }

        /// <summary>
        /// Creates property binding with necessary parameters.
        /// </summary>
        /// <param name="propertyName">Name of property where binding should be created.</param>
        /// <param name="bindingSource">Object which property is used like binding source.</param>
        /// <param name="boundTarget">Object which property will be changed during binding.</param>
        /// <param name="boundProperty">Property what will be changed during binding.</param>
        private void _CreatePropertyBinding(string propertyName, object bindingSource, DependencyObject boundTarget, DependencyProperty boundProperty)
        {
            Binding propertyBinding = new Binding(propertyName);
            propertyBinding.NotifyOnSourceUpdated = true;
            propertyBinding.Mode = BindingMode.OneWay;
            propertyBinding.Source = bindingSource;
            BindingOperations.SetBinding(boundTarget, boundProperty, propertyBinding);
        }

        /// <summary>
        /// Updates command button style.
        /// </summary>
        private void _SetCommandButtonStyle()
        {
            // Command is Enabled.
            if (_cmd.IsEnabled)
                this.Style = (Style)App.Current.FindResource(COMMAND_BUTTON_STYLE_RESOURCE);

            // Command is disabled and implements ISupportDisabledExecution.
            else if (_cmd is ISupportDisabledExecution)
            {
                if (((ISupportDisabledExecution)_cmd).AllowDisabledExecution)
                    this.Style = (Style)App.Current.FindResource(DISABLED_EXECUTABLE_COMMAND_BUTTON_STYLE_RESOURCE);
                else
                    this.Style = (Style)App.Current.FindResource(DISABLED_COMMAND_BUTTON_STYLE_RESOURCE);
            }

            // Command is disabled and not implements ISupportDisabledExecution.
            else
                this.Style = (Style)App.Current.FindResource(DISABLED_COMMAND_BUTTON_STYLE_RESOURCE);
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Handler executes bound command.
        /// </summary>
        /// <param name="sender">Clicked button (this).</param>
        /// <param name="e">Event args.</param>
        private void _Click(object sender, RoutedEventArgs e)
        {
            ISupportDisabledExecution supportDisabledExecutionCommand = _cmd as ISupportDisabledExecution;

            bool isDisabledExecutionAllowed = true; // True by default to allow execute commands which don't implement ISupportDisabledExecution. 

            // If commanf implements ISupportDisabledExecution, define isDisabledExecutionAllowed flag.
            if (supportDisabledExecutionCommand != null)
                isDisabledExecutionAllowed = supportDisabledExecutionCommand.AllowDisabledExecution;

            if (_cmd != null && (_cmd.IsEnabled || isDisabledExecutionAllowed))
                _cmd.Execute(); // No arguments.
        }

        /// <summary>
        /// Handler changes button style dependent on AllowDisabled command falg value.
        /// </summary>
        /// <param name="sender">Command sender.</param>
        /// <param name="e">Event args.</param>
        /// <remarks>
        /// We need this code for support click command in "Disabled" state. Actually command is enabled, but looks like disabled.
        /// </remarks>
        private void _CommandPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // If any property which can affect to button enabled state changed - update button's style.
            if (e.PropertyName == IS_ENABLED_PROPERTY_STRING || e.PropertyName == ALLOW_DISABLED_EXECUTION_PROPETY_NAME) 
            {
                _SetCommandButtonStyle();
            }
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// "AllowDisabledExecution" string.
        /// </summary>
        private const string ALLOW_DISABLED_EXECUTION_PROPETY_NAME = "AllowDisabledExecution";

        /// <summary>
        /// "IsEnabled" string.
        /// </summary>
        private const string IS_ENABLED_PROPERTY_STRING = "IsEnabled";

        /// <summary>
        /// "TooltipText" string.
        /// </summary>
        private const string TOOLTIP_PROPERTY_STRING = "TooltipText";

        /// <summary>
        /// Command button style resource name.
        /// </summary>
        private const string COMMAND_BUTTON_STYLE_RESOURCE = "CommandButtonStyle";

        /// <summary>
        /// default command button style resource name.
        /// </summary>
        private const string DEFAULT_COMMAND_BUTTON_STYLE_RESOURCE = "CommandButtonInGroupStyle";

        /// <summary>
        /// Disabled command button style resource name.
        /// </summary>
        private const string DISABLED_COMMAND_BUTTON_STYLE_RESOURCE = "DisabledCommandButtonStyle";

        /// <summary>
        /// Execute disabled style resource name.
        /// </summary>
        private const string DISABLED_EXECUTABLE_COMMAND_BUTTON_STYLE_RESOURCE = "CommandButtonStyle";

        #endregion

        #region Private Fields

        /// <summary>
        /// Bound command.
        /// </summary>
        private AppCommands.ICommand _cmd = null;

        #endregion
    }
}

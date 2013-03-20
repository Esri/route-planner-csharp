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
using ESRI.ArcLogistics.App.Commands;
using System.Diagnostics;
using AppCommands = ESRI.ArcLogistics.App.Commands;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interaction logic for OptionsCommandButton.xaml
    /// </summary>
    internal partial class OptionsCommandButton : Button
    {
        public OptionsCommandButton()
        {
            InitializeComponent();
        }

        #region Public Properties

        /// <summary>
        /// Commands that is executed each time when user presses the menu item.
        /// </summary>
        public AppCommands.ICommand ApplicationCommand
        {
            get
            {
                return _cmd;
            }
            set
            {
                if (_cmd != null)
                {
                    ((INotifyPropertyChanged)_cmd).PropertyChanged -= OptionsCommandButton_PropertyChanged;
                }
                _cmd = value;
                _cmd.Initialize(App.Current);
                _SetIsEnabledPropertyBinding(this, _cmd);                

                ((INotifyPropertyChanged)_cmd).PropertyChanged += new PropertyChangedEventHandler(OptionsCommandButton_PropertyChanged);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets binding to "IsEnabled" command property
        /// </summary>
        private void _SetIsEnabledPropertyBinding(DependencyObject obj, object source)
        {
            Binding propertyBinding = new Binding(BINDING_ENABLED_PROPERTY_NAME);
            propertyBinding.NotifyOnSourceUpdated = true;
            propertyBinding.Mode = BindingMode.OneWay;
            propertyBinding.Source = source;
            BindingOperations.SetBinding(obj, IsEnabledProperty, propertyBinding);
        }

        /// <summary>
        /// Sets binding to "Tooltip" property
        /// </summary>
        private void _SetTooltipPropertyBinding(DependencyObject obj, object source)
        {
            Binding tooltipBinding = new Binding(TOOLTIP_PROPERTY_NAME);
            tooltipBinding.NotifyOnSourceUpdated = true;
            tooltipBinding.Mode = BindingMode.OneWay;
            tooltipBinding.Source = source;
            BindingOperations.SetBinding(obj, ToolTipProperty, tooltipBinding);
        }

        /// <summary>
        /// Updates options menu and reset that flag about update necessity.
        /// </summary>
        private void _UpdateOptionsMenu()
        {
            if (_needToUpdateOptionsMenu)
            {
                _CreateOptionsItems();

                // Reset flag.
                _needToUpdateOptionsMenu = false;
            }
        }

        /// <summary>
        /// Method creates collection of menu items
        /// </summary>
        protected void _CreateOptionsItems()
        {
            OptionsMenu.Items.Clear();

            if (((ISupportOptions)_cmd).Options != null)
            {
                int groupId = ((ISupportOptions)_cmd).Options[0].GroupID;

                foreach (ICommandOption option in ((ISupportOptions)_cmd).Options)
                {
                    if (option.GroupID != groupId)
                    {
                        Separator separator = new Separator();
                        OptionsMenu.Items.Add(separator);
                        groupId = option.GroupID;
                    }

                    MenuItem newItem = new MenuItem();
                    newItem.Header = option.Title;
                    newItem.Tag = option;

                    newItem.IsEnabled = option.IsEnabled;
                    _SetIsEnabledPropertyBinding(newItem, option);
                    _SetTooltipPropertyBinding(newItem, option);                   

                    newItem.Click += new RoutedEventHandler(newItem_Click);
                    OptionsMenu.Items.Add(newItem);
                }
            }
            else
                OptionsMenu.Items.Clear();
        }

        #endregion

        #region Event Handlers

        private void OptionsCommandButton_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // If options have changed - raise the flag that options menu should be updated.
            if (e.PropertyName.Equals(OPTIONS_PROPERTY_NAME))
                _needToUpdateOptionsMenu = true;
        }

        private void RootButton_Click(object sender, RoutedEventArgs e)
        {
            // Update options menu before it will be shown.
            _UpdateOptionsMenu();

            OptionsMenu.IsOpen = true;
        }

        private void newItem_Click(object sender, RoutedEventArgs e)
        {
            if (_cmd != null && _cmd.IsEnabled)
                _cmd.Execute(((MenuItem)sender).Tag); //execute with option
        }

        private void OptionsMenu_Opened(object sender, RoutedEventArgs e)
        {
            OptionsMenu.PlacementTarget = this;
            OptionsMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        }

        private void OptionsMenu_Initialized(object sender, EventArgs e)
        {
            OptionsMenu.PlacementTarget = this;
            OptionsMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        }

        #endregion

        #region Private Fields

        private const string BINDING_ENABLED_PROPERTY_NAME = "IsEnabled";
        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";
        private const string OPTIONS_PROPERTY_NAME = "Options";

        /// <summary>
        /// Command to execute by click.
        /// </summary>
        private AppCommands.ICommand _cmd;

        /// <summary>
        /// Flag that indicates that options menu is state and needs to be updated.
        /// By default it is true, it means that menu should be created.
        /// </summary>
        private bool _needToUpdateOptionsMenu = true;

        #endregion
    }
}

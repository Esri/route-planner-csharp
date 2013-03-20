using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;
using System.Collections;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command geocode locations
    /// </summary>
    class GeocodeOrdersCmd : OrdersCommandBase
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.GeocodeOrdersCmd";

        #endregion

        #region Override members

        public override string Name
        {
            get
            {
                return COMMAND_NAME;
            }
        }

        public override string Title
        {
            get
            {
                return (string)App.Current.FindResource("GeocodeCommandTitle");
            }
        }

        public override bool IsEnabled
        {
            get
            {
                return base.IsEnabled;
            }
            protected set
            {
                base.IsEnabled = value;

                if (value)
                    TooltipText = (string)App.Current.FindResource("RematchAddressCommandEnabledTooltip");
                else
                    TooltipText = (string)App.Current.FindResource("RematchAddressCommandDisabledTooltip");
            }
        }

        public override string TooltipText
        {
            get
            {
                return _tooltipText;
            }
            protected set
            {
                _tooltipText = value;
                _NotifyPropertyChanged(TOOLTIP_PROPERTY_NAME);
            }
        }

        protected override void _Execute(params object[] args)
        {
            _Geocode();
        }

        /// <summary>
        ///  Method checks is command enabled
        /// </summary>
        protected override void _CheckEnabled()
        {
            IsEnabled = !OptimizePage.IsLocked && !OptimizePage.IsEditingInProgress && 
                OptimizePage.SelectedItems.Count == 1 && OptimizePage.SelectedItems[0] is Order;
        }

        #endregion

        #region GeocodeCommandBase Protected Methods

        /// <summary>
        /// Geocode order
        /// </summary>
        private void _Geocode()
        {
            if (OptimizePage.SelectedItems.Count == 1)
            {
                IGeocodable order = (IGeocodable)OptimizePage.SelectedItems[0];
                OptimizePage.StartGeocoding(order);
            }
        }

        #endregion GeocodeCommandBase Protected Methods

        #region Private Fields

        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";

        private string _tooltipText = null;

        #endregion
    }
}
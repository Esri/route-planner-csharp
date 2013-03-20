using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;
using System.Collections;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command geocode locations
    /// </summary>
    class GeocodeLocationsCmd : GeocodeCommandBase
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.GeocodeLocationsCmd";

        public override string Name
        {
            get
            {
                return COMMAND_NAME;
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

        #endregion

        #region GeocodeCommandBase Protected Methods

        /// <summary>
        /// Geocode locations
        /// </summary>
        protected override void _Geocode()
        {
            if (((ISupportSelection)ParentPage).SelectedItems.Count == 1)
            {
                IGeocodable location = (IGeocodable)((ISupportSelection)ParentPage).SelectedItems[0];
                ((LocationsPage)ParentPage).StartGeocoding(location);
            }
        }

        #endregion GeocodeCommandBase Protected Methods

        #region GeocodeCommandBase Protected Properties

        protected override ISupportDataObjectEditing ParentPage
        {
            get 
            {
                if (_parentPage == null)
                {
                    LocationsPage page = (LocationsPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.LocationsPagePath);
                    _parentPage = page;
                }

                return _parentPage;
            }
        }

        #endregion GeocodeCommandBase Protected Properties

        #region Private Members

        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";

        private string _tooltipText = null;
        private ISupportDataObjectEditing _parentPage;

        #endregion Private Members
    }
}

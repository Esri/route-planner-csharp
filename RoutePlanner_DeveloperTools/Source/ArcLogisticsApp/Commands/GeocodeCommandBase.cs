using System;
using System.Collections.Generic;
using ESRI.ArcLogistics.App.Pages;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Base class for geocode command
    /// </summary>
    abstract class GeocodeCommandBase : CommandBase
    {
        #region Public Command Base Members

        public override bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            protected set
            {
                _isEnabled = value;
                _NotifyPropertyChanged(ENABLED_PROPERTY_NAME);
            }
        }

        public override string Name
        {
            get { return null; }
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("GeocodeCommandTitle"); }
        }

        public override void Initialize(App app)
        {
            base.Initialize(app);
            App.Current.ApplicationInitialized += new EventHandler(Current_ApplicationInitialized);
        }

        #endregion Public Command Base Members

        #region Protected Command Base Methods

        protected override void _Execute(params object[] args)
        {
            _Geocode();
        }

        #endregion Protected Command Base Methods

        #region Protected Properties

        /// <summary>
        /// Page/panel which contains command 
        /// </summary>
        protected abstract ISupportDataObjectEditing ParentPage
        {
            get;
        }

        #endregion Protected Properties

        #region Protected Event Handlers

        protected void Current_ApplicationInitialized(object sender, EventArgs e)
        {
            _InitParentPageEventHandlers();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Geocode selected item (should be override in each child class)
        /// </summary>
        /// <param name="parentPage"></param>
        protected abstract void _Geocode();

        /// <summary>
        /// Checks is command enabled and sets property IsEnabled to false or true
        /// </summary>
        protected void _CheckEnabled()
        {
            IsEnabled = ((ISupportSelection)ParentPage).SelectedItems.Count == 1;
        }

        /// <summary>
        /// Creates parent page event handlers
        /// </summary>
        protected void _InitParentPageEventHandlers()
        {
            if (null != ParentPage)
            {
                ParentPage.EditBegun += new DataObjectEventHandler(ParentPage_EditBegun);
                ParentPage.EditCommitted += new DataObjectEventHandler(ParentPage_EditCommitted);
                ParentPage.EditCanceled += new DataObjectEventHandler(ParentPage_EditCanceled);
                
            if (ParentPage is ISupportSelectionChanged)
                ((ISupportSelectionChanged)ParentPage).SelectionChanged += new EventHandler(GeocodeCommandBase_SelectionChanged);

            }
        }

        #endregion

        #region Private Event Handlers

        private void ParentPage_EditCanceled(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void ParentPage_EditCommitted(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void ParentPage_EditBegun(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void GeocodeCommandBase_SelectionChanged(object sender, EventArgs e)
        {
            _CheckEnabled();
        }

        #endregion Event Handlers

        #region Private Fields

        private bool _isEnabled = false;
        private const string ENABLED_PROPERTY_NAME = "IsEnabled";

        #endregion
    }
}

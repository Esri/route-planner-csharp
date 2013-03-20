using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ESRI.ArcLogistics.App.Dialogs;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Base class for duplicate command
    /// </summary>
    /// <typeparam name="T">The type of objects to be deleted by the command.</typeparam>
    abstract class DeleteCommandBase<T> : CommandBase
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

                if (value)
                    TooltipText = (string)App.Current.FindResource("DeleteCommandEnabledTooltip");
                else
                    TooltipText = (string)App.Current.FindResource("DeleteCommandDisabledTooltip"); 
            }
        }

        public override string Name
        {
            get { return null; }
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("DeleteCommandTitle"); }
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

        public override void Initialize(App app)
        {
            base.Initialize(app);
            IsEnabled = false;
            App.Current.ApplicationInitialized += new EventHandler(Current_ApplicationInitialized);

            KeyGesture = new KeyGesture(INVOKE_KEY);
        }

        #endregion Public Command Base Members

        #region Protected Command Base Methods

        protected override void _Execute(params object[] args)
        {
            // if editing is process in ParentPage - cancel editing
            if (ParentPage.IsEditingInProgress)
                ((ICancelDataObjectEditing)ParentPage).CancelObjectEditing();

            var selector = (ISupportSelection)ParentPage;
            var selectedObjects = selector.SelectedItems.Cast<T>().ToList();
            if (selectedObjects.Count == 0)
            {
                return;
            }

            if (Settings.Default.IsAllwaysAskBeforeDeletingEnabled)
            {
                // show warning dialog
                var doProcess = DeletingWarningHelper.Execute(selectedObjects);
                if (!doProcess)
                {
                    return;
                }
            }

            // do process
            _Delete(selectedObjects);
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

        #endregion Protected Porperties

        #region Protected Event Handlers

        protected void Current_ApplicationInitialized(object sender, EventArgs e)
        {
            _InitParentPageEventHandlers();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Deletes selected items (should be override in each child class)
        /// </summary>
        protected abstract void _Delete(IList<T> selectedObjects);

        /// <summary>
        /// Checks is command enabled and sets property IsEnabled to false or true
        /// </summary>
        protected void _CheckEnabled()
        {
            // According new requirements we should enable "Delete" command when selection contains at list one item including insertion row
            // But Xceed data grid not consider item in insertion row like one of item in selection  
            // and the collection of selected items is empty when user create new item
            // Therefore we set command "enabled" when selection isn't empty or editing is in progress (case when user adding new item in insertion row)
            IsEnabled = (((ISupportSelection)ParentPage).SelectedItems.Count > 0 || ParentPage.IsEditingInProgress);
        }

        /// <summary>
        /// Creates parent page event handlers
        /// </summary>
        protected void _InitParentPageEventHandlers()
        {
            if (null != ParentPage)
            {
                ParentPage.EditBegun += (e, args) => _CheckEnabled();
                ParentPage.EditCommitted += (e, args) => _CheckEnabled();
                ParentPage.EditCanceled += (e, args) => _CheckEnabled();
                ParentPage.NewObjectCreated += (e, args) => _CheckEnabled();
                ParentPage.NewObjectCommitted += (e, args) => _CheckEnabled();
                ParentPage.NewObjectCanceled += (e, args) => _CheckEnabled();
                
                if (ParentPage is ISupportSelectionChanged)
                    ((ISupportSelectionChanged)ParentPage).SelectionChanged += (e, args) => _CheckEnabled();

                if (ParentPage is SpecialtiesPanelBase)
                    ((SpecialtiesPanelBase)ParentPage).Loaded += (e, args) => _CheckEnabled();
            }
        }

        /// <summary>
        /// Filteres object
        /// </summary>
        /// <param name="obj">Object to filtration</param>
        /// <param name="filterObjects">Filter collection</param>
        /// <param name="usedObjects">Filtred objects</param>
        /// <remarks>To filtredObjects adding unique object only from filterObjects</remarks>
        protected void _FilterObject<T>(T obj, IList filterObjects,
                                        ref Collection<ESRI.ArcLogistics.Data.DataObject> filtredObjects)
            where T : ESRI.ArcLogistics.Data.DataObject
        {
            if ((null != obj) && filterObjects.Contains(obj))
            {   // add only unique
                if (!filtredObjects.Contains(obj))
                    filtredObjects.Add(obj);
            }
        }

        /// <summary>
        /// Filteres objects
        /// </summary>
        /// <param name="objects">Collection of objects to filtration</param>
        /// <param name="filterObjects">Filter collection</param>
        /// <param name="usedObjects">Filtred objects</param>
        /// <remarks>To filtredObjects adding unique object only from filterObjects</remarks>
        protected void _FilterObjects<T>(IDataObjectCollection<T> objects, IList filterObjects,
                                         ref Collection<ESRI.ArcLogistics.Data.DataObject> usedObjects)
            where T : ESRI.ArcLogistics.Data.DataObject
        {
            for (int specIndex = 0; specIndex < objects.Count; ++specIndex)
                _FilterObject(objects[specIndex], filterObjects, ref usedObjects);
        }

        #endregion

        #region Private Event Handlers

        private void _CheckEnabled(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        #endregion Event Handlers

        #region Private constants

        /// <summary>
        /// Key to invoke command.
        /// </summary>
        private const Key INVOKE_KEY = Key.Delete;

        #endregion

        #region Private Fields

        private bool _isEnabled = false;
        private const string ENABLED_PROPERTY_NAME = "IsEnabled";
        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";
        private string _tooltipText = null;

        #endregion
    }
}

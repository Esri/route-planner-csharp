using System;
using System.Diagnostics;
using System.Collections.ObjectModel;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// NavigationCommandSimple class
    /// </summary>
    internal class NavigationCommandSimple : System.Windows.Input.ICommand
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public NavigationCommandSimple(string navigateLink)
        {
            Debug.Assert(!string.IsNullOrEmpty(navigateLink));

            _navigateLink = navigateLink;
        }

        #endregion // Constructors

        #region Public function
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public bool CanExecute(object parameter)
        {
            return (!string.IsNullOrEmpty(_navigateLink) && !App.Current.UIManager.IsLocked);
        }

        public void Execute(object parameter)
        {
            try
            {
                App.Current.MainWindow.Navigate(_navigateLink);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public event EventHandler CanExecuteChanged;

        #endregion // Public function

        #region Private function
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _OnFireCanExecuteChanged()
        {
            if (null != CanExecuteChanged)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        #endregion // Private function

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _navigateLink = null;

        #endregion // Private members
    }

    /// <summary>
    /// NavigationCommand class
    /// </summary>
    internal class NavigationCommand : System.Windows.Input.ICommand
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public NavigationCommand(string navigateLink, Type type, Guid id)
        {
            Debug.Assert(!string.IsNullOrEmpty(navigateLink));

            _navigateLink = navigateLink;
            _type = type;
            _id = id;
        }

        #endregion // Constructors

        #region Public function
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public bool CanExecute(object parameter)
        {
            DataObject obj = DataObjectHelper.GetPrescribedObject(_type, _id);
            return (!string.IsNullOrEmpty(_navigateLink) && (null != obj) && !App.Current.UIManager.IsLocked);
        }

        public void Execute(object parameter)
        {
            try
            {
                DataObject obj = DataObjectHelper.GetPrescribedObject(_type, _id);
                if (null != obj)
                {
                    INavigationItem item = null;
                    if (App.Current.MainWindow.NavigationTree.FindItem(_navigateLink, out item))
                    {
                        if ((item as PageItem).Page != App.Current.MainWindow.CurrentPage)
                            App.Current.MainWindow.Navigate(_navigateLink);

                    Page pageBase = App.Current.MainWindow.CurrentPage;

                    ISupportDataObjectEditing objectEditing = _GetDataObjectEditing(obj, pageBase);
                    ISupportSelection selection = _GetSelection(obj, pageBase);

                    bool isEditingInProgress = false;
                    if (null != objectEditing)
                        isEditingInProgress = objectEditing.IsEditingInProgress;

                    if (null != selection)
                    {
                        // if any object is editing - try to save changed 
                        if (isEditingInProgress)
                            isEditingInProgress = !selection.SaveEditedItem();

                        // if grid isn't editing or changes saved successfully - select items
                        if (!isEditingInProgress)
                            selection.Select(new Collection<DataObject>() { obj });
                    }
}
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public event EventHandler CanExecuteChanged;

        #endregion // Public function

        #region Private function
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _OnFireCanExecuteChanged()
        {
            if (null != CanExecuteChanged)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        private ISupportDataObjectEditing _GetDataObjectEditing(DataObject obj, Page pageBase)
        {
            ISupportDataObjectEditing objectEditing = null;

            SpecialtiesPage specialtiesPage = pageBase as SpecialtiesPage;
            if (null == specialtiesPage)
                objectEditing = pageBase as ISupportDataObjectEditing;
            else
            {
                if (obj is VehicleSpecialty)
                    objectEditing = specialtiesPage.VehicleSpecialtiesPanel;
                else
                {
                    Debug.Assert(obj is DriverSpecialty);
                    objectEditing = specialtiesPage.DriverSpecialtiesPanel;
                }
            }

            return objectEditing;
        }

        private ISupportSelection _GetSelection(DataObject obj, Page pageBase)
        {
            ISupportSelection selection = null;

            SpecialtiesPage specialtiesPage = pageBase as SpecialtiesPage;
            if (null == specialtiesPage)
                selection = pageBase as ISupportSelection;
            else
            {
                if (obj is VehicleSpecialty)
                    selection = specialtiesPage.VehicleSpecialtiesPanel as ISupportSelection;
                else
                {
                    Debug.Assert(obj is DriverSpecialty);
                    selection = specialtiesPage.DriverSpecialtiesPanel as ISupportSelection;
                }
            }

            return selection;
        }

        #endregion // Private function

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _navigateLink = null;
        private Type _type;
        private Guid _id;

        #endregion // Private members
    }
}

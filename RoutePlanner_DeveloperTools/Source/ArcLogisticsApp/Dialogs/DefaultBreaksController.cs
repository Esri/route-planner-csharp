using System.Windows;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.DomainObjects;


namespace ESRI.ArcLogistics.App.Dialogs
{
    /// <summary>
    /// Class, which control default routes breaks update.
    /// </summary>
    internal class DefaultBreaksController
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public DefaultBreaksController()
        {
            IsCheckEnabled = true;
            _oldDefaultBreaks = new Breaks();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// If this property is 'false' then controller wont check breaks for update.
        /// </summary>
        public bool IsCheckEnabled { get; set; }

        /// <summary>
        /// Collection with breaks, which needs to be compared to current default
        /// breaks collection. 
        /// This property cannot be set to new value before current value wouldnt
        /// be compared with default breaks.
        /// </summary>
        public Breaks OldDefaultBreaks
        {
            set
            {
                // If we have checked current collection, then we can apply new value.
                if (_wasChecked)
                {
                    _oldDefaultBreaks = value.Clone() as Breaks;
                    _wasChecked = false;
                }
            }
        }

        #endregion

        #region Public Method

        /// <summary>
        /// Method, comparing current default breaks and "OldDefaultBreaks" collection.
        /// If they differs then it ask/dont ask user about updating default routes, 
        /// </summary>
        public void CheckDefaultBreaksForUpdates()
        {
            // If checking is enabled and if we havent checked current
            // value of "OldDefaultBreaks" yet, the we can check it.
            if (IsCheckEnabled && !_wasChecked)
            {
                // If we have default projects and default breaks are valid 
                // and they have changed then check settings.
                if (App.Current.Project.DefaultRoutes.Count != 0 && _DefaultBreaksAreValid() &&
                    !_oldDefaultBreaks.EqualsByValue(App.Current.Project.BreaksSettings.DefaultBreaks))
                {
                    // If corresponding property is true - ask user about updating.
                    if (Settings.Default.IsAlwaysAskAboutApplyingBreaksToDefaultRoutes)
                    {
                        // If he pressed "Yes" button - update default routes breaks.
                        if (_ShowDialog())
                            _ApplyBreaksToDefaultRoutes();
                    }
                    // If we dont have to ask user and must update breaks - update them.
                    else if (Settings.Default.ApplyBreaksChangesToDefaultRoutes)
                        _ApplyBreaksToDefaultRoutes();
                }

                // We have checked collection - set flag to 'true'.
                _wasChecked = true;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check that default breaks are valid.
        /// </summary>
        /// <returns>'True' if breaks are valid, 'false' otherwise.</returns>
        private bool _DefaultBreaksAreValid()
        {
            // If we find error message for any break then default breaks are not valid.
            foreach(var breakObject in App.Current.Project.BreaksSettings.DefaultBreaks)
                if (!string.IsNullOrEmpty(breakObject.Error))
                    return false;

            // All breaks are valid.
            return true;
        }

        /// <summary>
        /// Show dialog, where user can choose update breaks or not.
        /// </summary>
        /// <returns>"True" if user decided to update breaks, "false" otherwise.</returns>
        private bool _ShowDialog()
        {
            // Get strings from resources.
            string question = (string)App.Current.FindResource("BreaksApplyingDialogText");
            string checboxLabel = (string)App.Current.FindResource("BreaksApplyingDialogCheckbox");
            string title = (string)App.Current.FindResource("BreaksApplyingDialogTitle");

            // Flag - show this dialog in future or not.
            bool dontAsk = true;

            // Show dialog and convert result to bool.
            MessageBoxExButtonType pressedButton = MessageBoxEx.Show(App.Current.MainWindow, question,
                title, System.Windows.Forms.MessageBoxButtons.YesNo, MessageBoxImage.Question,
                checboxLabel, ref dontAsk);
            bool result = (pressedButton == MessageBoxExButtonType.Yes);

            // If user checked checbox - we dont need to show this dialog in future, so
            // update corresponding settings.
            if (dontAsk)
            {
                Properties.Settings.Default.IsAlwaysAskAboutApplyingBreaksToDefaultRoutes = false;
                Settings.Default.ApplyBreaksChangesToDefaultRoutes = result;
                Properties.Settings.Default.Save();
            }

            // Return user choise.
            return result;
        }

        /// <summary>
        /// Apply default breaks to all default routes.
        /// </summary>
        private void _ApplyBreaksToDefaultRoutes()
        {
            // Init new default routes controller fo cascade updating.
            var routesController = new DefaultRoutesController(App.Current.Project.DefaultRoutes);

            // For each default route set new breaks.
            foreach (Route route in App.Current.Project.DefaultRoutes)
                route.Breaks = App.Current.Project.BreaksSettings.DefaultBreaks.Clone() as Breaks;

            // Update routes if needed.
            routesController.CheckDefaultRoutesForUpdates();
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Flag, which show, was current OldDefaultBreaks collection checked or not.
        /// </summary>
        private bool _wasChecked = true;

        /// <summary>
        /// Collection with breaks, which needs to be compared to current default
        /// breaks collection. 
        /// </summary>
        private Breaks _oldDefaultBreaks;

        #endregion
    }
}

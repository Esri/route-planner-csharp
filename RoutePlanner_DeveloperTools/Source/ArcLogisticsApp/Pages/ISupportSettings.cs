using System;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Implement this interface in your page to support settings for their storing\restoring.
    /// </summary>
    public interface ISupportSettings
    {
        /// <summary>
        /// Saves user settigns to a string. This methid is called by application directly after the page is initialized.
        /// </summary>
        string SaveUserSettings();

        /// <summary>
        /// Loads user settings from a string. This method is called by application when it is closing.
        /// </summary>
        void LoadUserSettings(string settingsString);
    }
}

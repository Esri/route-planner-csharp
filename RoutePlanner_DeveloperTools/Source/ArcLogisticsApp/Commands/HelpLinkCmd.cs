using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Input;
using ESRI.ArcLogistics.App.Help;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// HelpLinkCommand class
    /// </summary>
    internal class HelpLinkCommand : System.Windows.Input.ICommand
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public HelpLinkCommand(string path) :
            this(path, null)
        {
        }

        public HelpLinkCommand(string path, string key)
        {
            _path = string.IsNullOrEmpty(path)? App.Current.HelpTopics.Path : path;
            _key = key;
        }

        #endregion // Constructors

        #region Public function
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public bool CanExecute(object parameter)
        {
            return (!string.IsNullOrEmpty(_path));
        }

        public void Execute(object parameter)
        {
            try
            {
                if (string.IsNullOrEmpty(_path))
                    App.Current.Messenger.AddWarning((string)App.Current.FindResource("HelpSystemFileNotFound"));
                else
                {
                    bool isChmFile = false;
                    if (Help.LinkType.Chm == App.Current.HelpTopics.Type)
                        isChmFile = Path.GetExtension(_path).Equals(".CHM", StringComparison.OrdinalIgnoreCase);

                    if (isChmFile)
                    {
                        if (File.Exists(_path))
                            System.Windows.Forms.Help.ShowHelp(null, _path, _key);
                        else
                            App.Current.Messenger.AddWarning((string)App.Current.FindResource("HelpSystemFileNotFound"));
                    }
                    else
                    {
                        string fullPath = null;
                        if (string.IsNullOrEmpty(_key))
                            fullPath = _path;
                        else
                            fullPath = _path + _key;
                        Process.Start(new ProcessStartInfo(fullPath));
                    }
                }
            }
            catch(Exception e)
            {
                App.Current.Messenger.AddError(e.Message);
            }
        }

        public event EventHandler CanExecuteChanged; // NOTE: note see in RSDN

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

        private string _path = null;
        private string _key = null;

        #endregion // Private members
    }
}

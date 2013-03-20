using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace ESRI.ArcLogistics.App.Commands
{
    internal abstract class CommandBase : ESRI.ArcLogistics.App.Commands.ICommand, INotifyPropertyChanged
    {
        public CommandBase()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract string Name
        {
            get;
        }

        public abstract string Title
        {
            get;
        }

        public abstract string TooltipText
        {
            get;
            protected set;
        }

        /// <summary>
        /// Key combination to invoke command.
        /// </summary>
        public KeyGesture KeyGesture
        {
            get;
            protected set;
        }

        public virtual bool IsEnabled
        {
            get
            {
                return true;
            }
            protected set { }
        }

        public virtual void Initialize(App app)
        {
            Debug.Assert(app != null);

            _app = app;
        }

        ///// <summary>
        ///// Methods raises events about command execution and call _Execute method that does all things.
        ///// </summary>
        ///// <param name="args"></param>
        public void Execute(params object[] args)
        {
            _Execute(args);
        }

        #region protected members

        protected void _NotifyPropertyChanged(string propName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        /// <summary>
        /// Returns application that command got during initialization.
        /// </summary>
        protected App _Application
        {
            get 
            {
                return _app;
            }
        }

        /// <summary>
        /// Override this method and implement all the actions here.
        /// </summary>
        /// <param name="args"></param>
        protected abstract void _Execute(params object[] args);

        #endregion

        #region private members

        /// <summary>
        /// Reference to the application.
        /// </summary>
        private App _app;

        #endregion

    }
}

using System.Windows.Input;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Interface class for commands.All of its members must be implemented by every command class.
    /// </summary>
    public interface ICommand
    {
        #region properties

        /// <summary>
        /// Name of the command. It must be unique and unchanging.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Title of the command that will be shown in the UI.
        /// </summary>
        string Title
        {
            get;
        }

        /// <summary>
        /// Tooltip text.
        /// </summary>
        string TooltipText
        {
            get;
        }

        /// <summary>
        /// Indicates whether command is enabled.
        /// </summary>
        bool IsEnabled
        {
            get;
        }

        /// <summary>
        /// Key combination to invoke the command.
        /// </summary>
        KeyGesture KeyGesture
        {
            get;
        }

        #endregion

        #region methods

        /// <summary>
        /// Initializes the command with the application.
        /// </summary>
        /// <param name="app"></param>
        void Initialize(App app);

        /// <summary>
        /// Executes command. The number of parameters depends on the specific command.
        /// </summary>
        /// <param name="args"></param>
        void Execute(params object[] args);

        #endregion
    }
}

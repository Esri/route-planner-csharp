using System.Windows.Controls;
using System.Windows.Input;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// UserControl for editing <c>WorkTimeBreakEditor</c>.
    /// </summary>
    internal partial class WorkTimeBreakEditor : UserControl
    {
        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public WorkTimeBreakEditor()
        {
            InitializeComponent();

            // Set width to default.
            Width = EDITOR_WIDTH;
            
            //Initializing Event Handlers.
            DurationTextBox.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_TextBoxPreviewMouseLeftButtonDown);
            TimeIntervalTextBox.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_TextBoxPreviewMouseLeftButtonDown);
        }
        #endregion

        #region Public properties

        public static double EditorWidth
        {

            get
            {
                return EDITOR_WIDTH;
            }
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Select all in TextBox.
        /// </summary>
        /// <param name="sender">TextBox.</param>
        /// <param name="e">MouseButtonEventArgs.</param>
        private void _TextBoxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((TextBox)sender).SelectAll();
            Keyboard.Focus((TextBox)sender);
            e.Handled = true;
        }
        #endregion

        #region private constants

        /// <summary>
        /// Editor's width.
        /// </summary>
        private const double EDITOR_WIDTH = 200;

        #endregion
    }
}

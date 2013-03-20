using System.Windows.Controls;
using System.Windows.Input;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// UserControl for editing <c>DriveTimeBreakEditor</c>.
    /// </summary>
    internal partial class DriveTimeBreakEditor : UserControl
    {
        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public DriveTimeBreakEditor()
        {
            InitializeComponent();

            // Set width to default.
            Width = EDITOR_WIDTH;

            //Initializing Event Handlers
            DurationTextBox.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_DurationTextBoxPreviewMouseLeftButtonDown);
            TimeIntervalTextBox.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_DurationTextBoxPreviewMouseLeftButtonDown);
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
        /// Select all text in textbox.
        /// </summary>
        /// <param name="sender">Duration TextBox.</param>
        /// <param name="e">EventArgs.</param>
        private void _DurationTextBoxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

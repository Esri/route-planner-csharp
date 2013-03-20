using System.Windows;
using System.Windows.Controls;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interaction logic for DeleteBreakButton.xaml
    /// </summary>
    internal partial class DeleteBreakButton : UserControl
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public DeleteBreakButton()
        {
            InitializeComponent();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Delete break when button clicked.
        /// </summary>
        /// <param name="sender">Ingored.</param>
        /// <param name="e">Ingored.</param>
        private void _DeleteButtonClicked(object sender, RoutedEventArgs e)
        {
            var breakObject = DataContext as Break;
            breakObject.Breaks.Remove(breakObject);
        }
        
        #endregion
    }
}

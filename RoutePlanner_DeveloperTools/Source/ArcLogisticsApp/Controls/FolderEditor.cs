using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Controls
{
    [TemplatePart(Name = "PART_Folder", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_Button", Type = typeof(Button))]
    internal class FolderEditor : Control
    {
        static FolderEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FolderEditor), new FrameworkPropertyMetadata(typeof(FolderEditor)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _OpenButton = this.GetTemplateChild("PART_Button") as Button;
            _folderText = this.GetTemplateChild("PART_Folder") as TextBox;
            _folderText.TextChanged += new TextChangedEventHandler(_folderText_TextChanged);
            this.Loaded += new RoutedEventHandler(FolderEditor_Loaded);
            _OpenButton.Click += new RoutedEventHandler(_OpenButton_Click);
        }

        #region Public Properties

        public static readonly DependencyProperty FolderProperty =
            DependencyProperty.Register("Folder", typeof(string), typeof(FolderEditor));

        /// <summary>
        /// Gets/sets TextBox content text.
        /// </summary>
        public string Folder
        {
            get
            {
                return (string)GetValue(FolderProperty);
            }
            set
            {
                SetValue(FolderProperty, value);
                _folderText.Text = value;
            }
        }

        #endregion

        #region Protected Methods

        private void _OpenButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();
            folderDlg.RootFolder = Environment.SpecialFolder.MyComputer;
            if (folderDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Folder = folderDlg.SelectedPath;
            }
        }

        private void FolderEditor_Loaded(object sender, RoutedEventArgs e)
        {
            _folderText.Text = Folder;

            //NOTE - set focus to editable text field
            _folderText.Focus();
        }

        private void _folderText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Folder = _folderText.Text;

            //NOTE - set focus to editable text field
            _folderText.Focus();
        }

        #endregion

        #region Private Fields

        private TextBox _folderText;
        private Button _OpenButton;

        #endregion
    }
}
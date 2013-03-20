using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using ESRI.ArcLogistics.App.Pages;
using Params = ImportExportDataPlugin.ImportExportPluginPreferencesPageParams;

namespace ImportExportDataPlugin
{
    [PagePlugInAttribute("Preferences")]
    public partial class ImportExportPluginPreferencesPage : PageBase, ISupportSettings
    {
        public ImportExportPluginPreferencesPage()
        {
            InitializeComponent();
            IsAllowed = true;    // "Enabled"
            IsRequired = true;   // makes text bold 
            this.Plugin_exportName.Text = Params.Instance.exportName;
            this.Plugin_importName.Text = Params.Instance.importName;
        }
         
        public void LoadUserSettings(string settingsString)
        {
            string[] stringSeparators = new string[] { Params.DELIM };
            string[] result = settingsString.Split(stringSeparators, StringSplitOptions.None);

            if (result != null)
            {
                Params.Instance.exportName = result[0];
                this.Plugin_exportName.Text = Params.Instance.exportName;

                if (result.Length > 1)
                    if (result[1] != "")
                    {
                        Params.exportPath = result[1];
                        this.Plugin_exportPath.Text = Params.exportPath;
                        ExportDataCmd.setEnable(true);
                    }

                if (result.Length > 2)
                {
                    Params.Instance.importName = result[2];
                    this.Plugin_importName.Text = Params.Instance.importName;
                }

                if (result.Length > 3)
                    if (result[3] != "")
                    {
                        Params.importPath = result[3];
                        this.Plugin_importPath.Text = Params.importPath;
                        ImportDataCmd.setEnable(true);
                    }
            }
        }

        public string SaveUserSettings()
        {
            string[] A = new string[] { Params.Instance.exportName, Params.exportPath, Params.Instance.importName, Params.importPath };
            return string.Join(Params.DELIM, A);
        }

        public override ESRI.ArcLogistics.App.Help.HelpTopic HelpTopic
        {
            get { return null; }
        }

        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        public override TileBrush Icon
        {
            get
            {
                return null;
            }
        }

        public override string Name
        {
            get
            {
                return "ExportDataCmd";
            }
        }

        public override string Title
        {
            get
            {
                return @"Import/Export";
            }
        }


        private void Plugin_exportName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.Plugin_exportName.Text != "")
            {
                Params.Instance.exportName = this.Plugin_exportName.Text;
                if (Params.exportPath != null && Params.exportPath != "")
                {
                    ExportDataCmd.setEnable(true);
                }
            }
            else
            {
                Params.Instance.exportName = Params.DEFAULT_EXPORTNAME;
            }
        }

        private void Plugin_importName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.Plugin_importName.Text != "" )
            {
                Params.Instance.importName = this.Plugin_importName.Text;
                if (Params.importPath != null && Params.importPath != "")
                {
                    ImportDataCmd.setEnable(true);
                }
            }
            else
            {
                Params.Instance.importName = Params.DEFAULT_IMPORTNAME;
            }
        }

        private void Plugin_exportPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Params.exportPath = this.Plugin_exportPath.Text;
            if (this.Plugin_exportPath.Text != "")
            {   
                ExportDataCmd.setEnable(true);
            }
            else
            {
                ExportDataCmd.setEnable(false);
            }
        }

        private void Plugin_importPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Params.importPath = this.Plugin_importPath.Text;
            if (this.Plugin_importPath.Text != "")
            {
                ImportDataCmd.setEnable(true);
            }
            else
            {
                ImportDataCmd.setEnable(false);
            }

        }

        private void ImportBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                this.Plugin_importPath.Text = dlg.FileName;
                Params.importPath = this.Plugin_importPath.Text;
            }
            
            if (this.Plugin_importPath.Text != "")
            {
                ImportDataCmd.setEnable(true);
            }
            else
            {
                ImportDataCmd.setEnable(false);
            }

        }

        private void ExportBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                this.Plugin_exportPath.Text = dlg.FileName;
                Params.exportPath = this.Plugin_exportPath.Text;
            }

            if (this.Plugin_exportPath.Text != "")
            {
                ExportDataCmd.setEnable(true);
            }
            else
            {
                ExportDataCmd.setEnable(false);
            }

        }
    }
}

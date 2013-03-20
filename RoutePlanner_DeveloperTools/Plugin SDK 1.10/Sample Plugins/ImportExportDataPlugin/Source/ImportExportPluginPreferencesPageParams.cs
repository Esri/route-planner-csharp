using System.ComponentModel;

namespace ImportExportDataPlugin
{
    class ImportExportPluginPreferencesPageParams: INotifyPropertyChanged 
    {
        public static ImportExportPluginPreferencesPageParams Instance
        {
            get
            {
                return _params;
            }
        }

        private static ImportExportPluginPreferencesPageParams _params = new ImportExportPluginPreferencesPageParams();
        
        public static string exportPath = null;
        public static string importPath = null;
        
        public static string exportTooltip = DISABLED_EXPORT_TOOLTIP;
        public static string importTooltip = DISABLED_IMPORT_TOOLTIP;
      
        public event PropertyChangedEventHandler PropertyChanged;

        public bool exportButtonEnabled
        {
            get{return _exportButtonEnabled;}
            set
            {
                _exportButtonEnabled = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("exportButtonEnabled"));
            }
        }

        public bool importButtonEnabled
        {
            get { return _importButtonEnabled; }
            set
            {
                _importButtonEnabled = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("importButtonEnabled"));
            }
        }

        public string exportName
        {
            get { return _exportName; }
            set
            {
                _exportName = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("exportName"));
            }
        }

        public string importName
        {
            get { return _importName; }
            set
            {
                _importName = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("importName"));
            }
        }


        public const string DELIM = @"ImportExportDataPluginSettingsDelimiter#";
        public const string DEFAULT_EXPORTNAME = "ExportProgram";
        public const string DEFAULT_IMPORTNAME = "ImportProgram";
        
        public const string ENABLED_EXPORT_TOOLTIP = "Export Data to exectutable";
        public const string ENABLED_IMPORT_TOOLTIP = "Import Data from exectutable";
        
        public const string DISABLED_EXPORT_TOOLTIP = "Please set Export Preferences";
        public const string DISABLED_IMPORT_TOOLTIP = "Please set Import Preferences";

        private bool _exportButtonEnabled = false;
        private bool _importButtonEnabled = false;
        private string _exportName = DEFAULT_EXPORTNAME;
        private string _importName = DEFAULT_IMPORTNAME;
    }
}

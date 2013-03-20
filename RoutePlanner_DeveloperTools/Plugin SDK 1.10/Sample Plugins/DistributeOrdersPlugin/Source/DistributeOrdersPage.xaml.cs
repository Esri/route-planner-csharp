using System;
using System.Windows;

using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Widgets;

namespace DistributeOrdersPlugin
{
    [PagePlugInAttribute("Schedule")]
    public partial class DistributeOrdersPage : PageBase
        
    {
        public DistributeOrdersPage()
        {            
            _helpTopic = new HelpTopic(null, QUICK_HELP);            
            IsAllowed = true;
            InitializeComponent();            
        }

        public override HelpTopic HelpTopic
        {
            get { return _helpTopic; }
        }

        public override string PageCommandsCategoryName
        {
            get { return "DistributeOrdersTaskWidgetCommands"; }
        }

        public override System.Windows.Media.TileBrush Icon
        {
            get{ return null; }
        }

        public override string Name
        {
            get { return "DistributeOrdersPlugin.DistributeOrdersPage"; }
        }

        public override string Title
        {
            get { return "Distribute Orders"; }
        }

        protected override void CreateWidgets()
        {
            base.CreateWidgets();

            // add calendar widget
            var calendarWidget = new CalendarWidget();
            calendarWidget.Initialize(this);
            this.EditableWidgetCollection.Insert(0, calendarWidget);
        }

        private void ImportBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "CSV file (*.csv)|*.csv";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                this.Plugin_importPath.Text = dlg.FileName;
                importpath = dlg.FileName;
            }
        }

        private void NumDaysBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Int32 result;

            if (Int32.TryParse(NumDaysBox.Text, out result))
            {
                numDays = result;
            }
        }

        private HelpTopic _helpTopic;
        private string QUICK_HELP = "Select Start date in the calendar, import orders from a csv file, and specify the number of days. Click Distribute Orders to spread the orders over the selected date range.";
        public static Int32 numDays = 0;
        public static string importpath = "";
    }
}

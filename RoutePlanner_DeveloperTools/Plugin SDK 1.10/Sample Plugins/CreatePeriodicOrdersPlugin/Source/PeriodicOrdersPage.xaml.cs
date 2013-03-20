using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;

using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Widgets;
using System.Windows.Controls;
using System.Data;
using System.Data.OleDb;


namespace CreatePeriodicOrdersPlugin
{
    /// <summary>
    /// Interaction logic for PeriodicOrdersPage.xaml
    /// </summary>

    [PagePlugInAttribute("Schedule")]
    /// <summary>
    /// Class that represents The Periodic Orders Page.
    /// </summary>
    public partial class PeriodicOrdersPage : PageBase
    {
        
        public PeriodicOrdersPage()
        {
            _helpTopic = new HelpTopic(null, QUICK_HELP);
            //IsRequired = true;
            IsAllowed = true;
            InitializeComponent();
        }

        
        public override HelpTopic HelpTopic
        {
            get { return _helpTopic; }
        }

        public override string PageCommandsCategoryName
        {
            get { return "PeriodicOrdersTaskWidgetCommands"; }
        }

        public override System.Windows.Media.TileBrush Icon
        {
            get { return null; }
        }

        public override string Name
        {
            get { return "PeriodicOrdersPage"; }
        }

        public override string Title
        {
            get { return "Periodic Orders"; }
        }

        protected override void CreateWidgets()
        {
            base.CreateWidgets();

            // add calendar widget
            var calendarWidget = new CalendarWidget();
            calendarWidget.Initialize(this);
            this.EditableWidgetCollection.Insert(0, calendarWidget);
        }


        private HelpTopic _helpTopic;
        private string QUICK_HELP = "Select start date on the calendar, import customer info from a csv file, and select days of visit. Click Create Periodic Orders to populate the orders over the selected date range.";
        public static Int32 numDays = 1;
        public static string importpath = "";
        public static List<CheckBox> checkBoxList;
        public static List<ComboBox> comboBoxList;
        public static DataTable dt = new DataTable();

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
                
                dt = ReadCSV(importpath);

                populateGrid();
            }
        }

        private void populateGrid()
        {
            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();

            checkBoxList = new List<CheckBox>();
            comboBoxList = new List<ComboBox>();

            if (!dt.Columns.Contains("Name"))
                dt.Columns.Add("Name");
            
            if (!dt.Columns.Contains("Address"))
                dt.Columns.Add("Address");
            
            if (!dt.Columns.Contains("City"))
                dt.Columns.Add("City");
            
            if (!dt.Columns.Contains("State"))
                dt.Columns.Add("State");
            
            if (!dt.Columns.Contains("Zip"))
                dt.Columns.Add("Zip");
            
            if (!dt.Columns.Contains("Days"))
                dt.Columns.Add("Days");

            if (!dt.Columns.Contains("Weekly Periodicity"))
                dt.Columns.Add("Weekly Periodicity");
            

            // For Combobox
            ColumnDefinition cd1 = new ColumnDefinition();
            cd1.Width = new GridLength(1, GridUnitType.Auto);
            grid.ColumnDefinitions.Add(cd1);

            // For Checkboxes
            for (int i = 0; i < 7; i++)
            {
                ColumnDefinition cd2 = new ColumnDefinition();
                cd2.Width = new GridLength(1, GridUnitType.Auto);
                grid.ColumnDefinitions.Add(cd2);
            }

            // For Name and address
            for (int i = 0; i < 5; i++)
            {
                ColumnDefinition cd2 = new ColumnDefinition();
                cd2.Width = new GridLength(1, GridUnitType.Auto);
                grid.ColumnDefinitions.Add(cd2);
            }

            //////////////////////////////////////
            //////////////////////////////////////
            // Header Row

            RowDefinition r = new RowDefinition();
            r.Height = new GridLength(1, GridUnitType.Star);
            grid.RowDefinitions.Add(r);

            TextBlock txt1 = new TextBlock();
            txt1.Text = "Every:";
            txt1.FontWeight = FontWeights.Bold;
            txt1.TextAlignment = TextAlignment.Left;
            txt1.Margin = new Thickness(10, 2, 2, 10);
            txt1.SetValue(Grid.ColumnProperty, 0);
            txt1.SetValue(Grid.RowProperty, 0);
            grid.Children.Add(txt1);

            String[] dayStrings =  { "S", "M", "T", "W", "T", "F", "S" };
            for (int i = 0; i < 7; i++)
            {
                TextBlock txt2 = new TextBlock();
                txt2.Text = dayStrings[i];
                txt2.FontWeight = FontWeights.Bold;
                txt2.TextAlignment = TextAlignment.Center;
                txt2.SetValue(Grid.ColumnProperty, i+1);
                txt2.SetValue(Grid.RowProperty, 0);
                grid.Children.Add(txt2);
            }

            TextBlock txt3 = new TextBlock();
            txt3.Text = "Customer Name";
            txt3.FontWeight = FontWeights.Bold;
            txt3.Margin = new Thickness(10, 2, 2, 10);
            txt3.SetValue(Grid.ColumnProperty, 8);
            txt3.SetValue(Grid.RowProperty, 0);
            grid.Children.Add(txt3);

            txt3 = new TextBlock();
            txt3.Text = "Address";
            txt3.FontWeight = FontWeights.Bold;
            txt3.Margin = new Thickness(10, 2, 2, 10);
            txt3.SetValue(Grid.ColumnProperty, 9);
            txt3.SetValue(Grid.RowProperty, 0);
            grid.Children.Add(txt3);

            txt3 = new TextBlock();
            txt3.Text = "City";
            txt3.FontWeight = FontWeights.Bold;
            txt3.Margin = new Thickness(10, 2, 2, 10);
            txt3.SetValue(Grid.ColumnProperty, 10);
            txt3.SetValue(Grid.RowProperty, 0);
            grid.Children.Add(txt3);

            txt3 = new TextBlock();
            txt3.Text = "State";
            txt3.FontWeight = FontWeights.Bold;
            txt3.Margin = new Thickness(10, 2, 2, 10);
            txt3.SetValue(Grid.ColumnProperty, 11);
            txt3.SetValue(Grid.RowProperty, 0);
            grid.Children.Add(txt3);

            txt3 = new TextBlock();
            txt3.Text = "Zip Code";
            txt3.FontWeight = FontWeights.Bold;
            txt3.Margin = new Thickness(10, 2, 2, 10);
            txt3.SetValue(Grid.ColumnProperty, 12);
            txt3.SetValue(Grid.RowProperty, 0);
            grid.Children.Add(txt3);




            //////////////////////////////////////
            //////////////////////////////////////
            // Populate Content Rows
            String[] days = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

            for (int j = 0; j < dt.Rows.Count; j++)
            {
                RowDefinition r2 = new RowDefinition();
                r2.Height = new GridLength(1, GridUnitType.Star);
                grid.RowDefinitions.Add(r2);

                ComboBox combo = new ComboBox();
                combo.Items.Add("   Week  ");
                combo.Items.Add(" 2 Weeks ");
                combo.Items.Add(" 3 Weeks ");
                combo.Items.Add(" 4 Weeks ");
                combo.SelectedIndex = 0;
                int result=-1;
                if(Int32.TryParse(dt.Rows[j]["Weekly Periodicity"].ToString(), out result))
                    if(result > 0 & result <= combo.Items.Count)
                        combo.SelectedIndex = result-1;

                combo.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
                combo.SetValue(Grid.ColumnProperty, 0);
                combo.SetValue(Grid.RowProperty, j + 1);
                combo.Margin = new Thickness(10, 2, 2, 2);
                grid.Children.Add(combo);
                comboBoxList.Add(combo);

                for (int i = 1; i < 8; i++)
                {
                    CheckBox checkBox = new CheckBox();
                    string s = "Checkbox" + ((j * 7) + (i));
                    checkBox.Name = s;
                    checkBox.IsChecked = dt.Rows[j]["Days"].ToString().Contains(days[i - 1]);
                    checkBox.SetValue(Grid.ColumnProperty, i);
                    checkBox.SetValue(Grid.RowProperty, j+1);
                    grid.Children.Add(checkBox);
                    checkBoxList.Add(checkBox);
                }

                TextBlock txt4 = new TextBlock();
                txt4.Text = dt.Rows[j]["Name"].ToString();
                txt4.SetValue(Grid.ColumnProperty, 8);
                txt4.SetValue(Grid.RowProperty, j+1);
                txt4.Margin = new Thickness(10, 2, 2, 2);
                grid.Children.Add(txt4);

                txt4 = new TextBlock();
                txt4.Text = dt.Rows[j]["Address"].ToString();
                txt4.SetValue(Grid.ColumnProperty, 9);
                txt4.SetValue(Grid.RowProperty, j + 1);
                txt4.Margin = new Thickness(10, 2, 2, 2);
                grid.Children.Add(txt4);

                txt4 = new TextBlock();
                txt4.Text = dt.Rows[j]["City"].ToString();
                txt4.SetValue(Grid.ColumnProperty, 10);
                txt4.SetValue(Grid.RowProperty, j + 1);
                txt4.Margin = new Thickness(10, 2, 2, 2);
                grid.Children.Add(txt4);

                txt4 = new TextBlock();
                txt4.Text = dt.Rows[j]["State"].ToString();
                txt4.SetValue(Grid.ColumnProperty, 11);
                txt4.SetValue(Grid.RowProperty, j + 1);
                txt4.Margin = new Thickness(10, 2, 2, 2);
                grid.Children.Add(txt4);

                txt4 = new TextBlock();
                txt4.Text = dt.Rows[j]["Zip"].ToString();
                txt4.SetValue(Grid.ColumnProperty, 12);
                txt4.SetValue(Grid.RowProperty, j + 1);
                txt4.Margin = new Thickness(10, 2, 2, 2);
                grid.Children.Add(txt4);

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

        private void Plugin_importPath_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        public static DataTable ReadCSV(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            string full = Path.GetFullPath(filePath);
            string file = Path.GetFileName(full);
            string dir = Path.GetDirectoryName(full);


            string connectionStr = "Provider=Microsoft.Jet.OLEDB.4.0;"
              + "Data Source=\"" + dir + "\\\";"
              + "Extended Properties=\"text;HDR=Yes;FMT=Delimited;IMEX=1\"";

            string queryStr = "SELECT * FROM [" + file + "]";

            DataTable dataTable = new DataTable();

            OleDbDataAdapter oledbDataAdapter = new OleDbDataAdapter(queryStr, connectionStr);

            try
            {
                //fill the DataTable
                oledbDataAdapter.Fill(dataTable);
            }
            catch (InvalidOperationException e)
            {
                string s = e.Message;
            }

            oledbDataAdapter.Dispose();

            return dataTable;
        }


    }
}

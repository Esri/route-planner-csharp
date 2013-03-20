using System;
using System.Collections;
using System.Windows.Forms;
using DataDynamics.ActiveReports;
using DataDynamics.ActiveReports.Design;
using DataDynamics.ActiveReports.Design.Toolbox;

namespace ESRI.ArcLogistics.App.Dialogs
{
    /// <summary>
    /// Form for the End User Designer application
    /// </summary>
    internal class EndUserDesignerForm : System.Windows.Forms.Form
    {
        private Designer arDesigner;
        private System.Windows.Forms.PropertyGrid arPropertyGrid;
        private Toolbox arToolbox;
        private CommandBarManager commandBarManager;
        private Panel pnlToolbox;
        private Splitter splitterToolboxDesigner;
        private Splitter splitterDesignerProperties;
        private Panel pnlProperties;
        private Splitter splitterReportExplorerPropertyGrid;
        private StatusBar arStatus;

        private string templatePath = null;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public EndUserDesignerForm(string reportName, string reportTemplatePath)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(reportTemplatePath));

            InitializeComponent();

            // Create new report instance and assign to Report Explorer
            this.arDesigner.Toolbox = this.arToolbox;
            this.arDesigner.PropertyGrid = this.arPropertyGrid;

            // Add Menu and CommandBar to Form
            this.commandBarManager = this.arDesigner.CommandBarManager;

            // Edit CommandBar
            // NOTE: need check after each used version of ActiveReports - can be changed
            this.commandBarManager.CommandBars.RemoveAt(0); // NOTE: remove menu
            CommandBarItem item = this.commandBarManager.CommandBars[0].Items[2]; // NOTE: get SaveAs button
            this.commandBarManager.CommandBars[0].Items.Clear(); // NOTE: remove New, Open, SaveAs buttons
            this.commandBarManager.CommandBars[0].Items.AddButton(item.Image, item.Text, new CommandEventHandler(OnSaveNew), 0); // NOTE: set customize Save routine

            this.Controls.Add(this.commandBarManager);

            // Fill Toolbox
            LoadTools(this.arToolbox);
            // Activate default group on the toolbox
            this.arToolbox.SelectedCategory = "ActiveReports 3.0";

            // Setup Status Bar
            this.arStatus.Panels.Add(new StatusBarPanel());
            this.arStatus.Panels.Add(new StatusBarPanel());
            this.arStatus.Panels[0].AutoSize = StatusBarPanelAutoSize.Spring;
            this.arStatus.Panels[1].AutoSize = StatusBarPanelAutoSize.Spring;
            this.arStatus.ShowPanels = true;

            ActiveReport3 rpt = new ActiveReport3();
            rpt.LoadLayout(reportTemplatePath);
            arDesigner.Report = rpt;
            if (!string.IsNullOrEmpty(reportName))
                this.Text = reportName;
            templatePath = reportTemplatePath;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                if(this.arDesigner != null)
                {
                    this.arDesigner.Dispose();
                    this.arDesigner = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.arToolbox = new DataDynamics.ActiveReports.Design.Toolbox.Toolbox();
            this.pnlToolbox = new System.Windows.Forms.Panel();
            this.splitterToolboxDesigner = new System.Windows.Forms.Splitter();
            this.arDesigner = new DataDynamics.ActiveReports.Design.Designer();
            this.arPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.splitterDesignerProperties = new System.Windows.Forms.Splitter();
            this.pnlProperties = new System.Windows.Forms.Panel();
            this.splitterReportExplorerPropertyGrid = new System.Windows.Forms.Splitter();
            this.arStatus = new System.Windows.Forms.StatusBar();
            this.commandBarManager = new DataDynamics.ActiveReports.Design.CommandBarManager();
            ((System.ComponentModel.ISupportInitialize)(this.arToolbox)).BeginInit();
            this.pnlToolbox.SuspendLayout();
            this.pnlProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // arToolbox
            // 
            this.arToolbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.arToolbox.LargeImages = null;
            this.arToolbox.Location = new System.Drawing.Point(0, 0);
            this.arToolbox.Name = "arToolbox";
            this.arToolbox.Selected = null;
            this.arToolbox.Size = new System.Drawing.Size(200, 578);
            this.arToolbox.SmallImages = null;
            this.arToolbox.TabIndex = 0;
            // 
            // pnlToolbox
            // 
            this.pnlToolbox.Controls.Add(this.arToolbox);
            this.pnlToolbox.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlToolbox.Location = new System.Drawing.Point(0, 0);
            this.pnlToolbox.Name = "pnlToolbox";
            this.pnlToolbox.Size = new System.Drawing.Size(200, 578);
            this.pnlToolbox.TabIndex = 0;
            // 
            // splitterToolboxDesigner
            // 
            this.splitterToolboxDesigner.BackColor = System.Drawing.SystemColors.Control;
            this.splitterToolboxDesigner.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitterToolboxDesigner.Location = new System.Drawing.Point(200, 0);
            this.splitterToolboxDesigner.Name = "splitterToolboxDesigner";
            this.splitterToolboxDesigner.Size = new System.Drawing.Size(3, 578);
            this.splitterToolboxDesigner.TabIndex = 1;
            this.splitterToolboxDesigner.TabStop = false;
            // 
            // arDesigner
            // 
            this.arDesigner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.arDesigner.IsDirty = false;
            this.arDesigner.Location = new System.Drawing.Point(203, 0);
            this.arDesigner.LockControls = false;
            this.arDesigner.Name = "arDesigner";
            this.arDesigner.PropertyGrid = this.arPropertyGrid;
            this.arDesigner.ReportTabsVisible = true;
            this.arDesigner.ShowDataSourceIcon = true;
            this.arDesigner.Size = new System.Drawing.Size(397, 578);
            this.arDesigner.TabIndex = 2;
            this.arDesigner.Toolbox = null;
            this.arDesigner.ToolBoxItem = null;
            this.arDesigner.SelectionChanged += new DataDynamics.ActiveReports.Design.SelectionChangedEventHandler(this.arDesigner_SelectionChanged);
            // 
            // arPropertyGrid
            // 
            this.arPropertyGrid.CommandsVisibleIfAvailable = true;
            this.arPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.arPropertyGrid.LargeButtons = false;
            this.arPropertyGrid.LineColor = System.Drawing.SystemColors.ScrollBar;
            this.arPropertyGrid.Location = new System.Drawing.Point(0, 231);
            this.arPropertyGrid.Name = "arPropertyGrid";
            this.arPropertyGrid.Size = new System.Drawing.Size(200, 347);
            this.arPropertyGrid.TabIndex = 2;
            this.arPropertyGrid.Text = "propertyGrid1";
            this.arPropertyGrid.ViewBackColor = System.Drawing.SystemColors.Window;
            this.arPropertyGrid.ViewForeColor = System.Drawing.SystemColors.WindowText;
            // 
            // splitterDesignerProperties
            // 
            this.splitterDesignerProperties.BackColor = System.Drawing.SystemColors.Control;
            this.splitterDesignerProperties.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitterDesignerProperties.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitterDesignerProperties.Location = new System.Drawing.Point(597, 0);
            this.splitterDesignerProperties.Name = "splitterDesignerProperties";
            this.splitterDesignerProperties.Size = new System.Drawing.Size(3, 578);
            this.splitterDesignerProperties.TabIndex = 3;
            this.splitterDesignerProperties.TabStop = false;
            // 
            // pnlProperties
            // 
            this.pnlProperties.Controls.Add(this.arPropertyGrid);
            this.pnlProperties.Controls.Add(this.splitterReportExplorerPropertyGrid);
            this.pnlProperties.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlProperties.Location = new System.Drawing.Point(600, 0);
            this.pnlProperties.Name = "pnlProperties";
            this.pnlProperties.Size = new System.Drawing.Size(200, 578);
            this.pnlProperties.TabIndex = 4;
            // 
            // splitterReportExplorerPropertyGrid
            // 
            this.splitterReportExplorerPropertyGrid.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitterReportExplorerPropertyGrid.Location = new System.Drawing.Point(0, 228);
            this.splitterReportExplorerPropertyGrid.Name = "splitterReportExplorerPropertyGrid";
            this.splitterReportExplorerPropertyGrid.Size = new System.Drawing.Size(200, 3);
            this.splitterReportExplorerPropertyGrid.TabIndex = 1;
            this.splitterReportExplorerPropertyGrid.TabStop = false;
            // 
            // arStatus
            // 
            this.arStatus.Location = new System.Drawing.Point(0, 578);
            this.arStatus.Name = "arStatus";
            this.arStatus.Size = new System.Drawing.Size(800, 22);
            this.arStatus.TabIndex = 5;
            // 
            // commandBarManager
            // 
            this.commandBarManager.Dock = System.Windows.Forms.DockStyle.Top;
            this.commandBarManager.Location = new System.Drawing.Point(0, 0);
            this.commandBarManager.Name = "commandBarManager";
            this.commandBarManager.Size = new System.Drawing.Size(800, 0);
            this.commandBarManager.TabIndex = 6;
            this.commandBarManager.TabStop = false;
            // 
            // EndUserDesignerMainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.splitterDesignerProperties);
            this.Controls.Add(this.arDesigner);
            this.Controls.Add(this.splitterToolboxDesigner);
            this.Controls.Add(this.pnlToolbox);
            this.Controls.Add(this.pnlProperties);
            this.Controls.Add(this.arStatus);
            this.Controls.Add(this.commandBarManager);
            this.Name = "EndUserDesignerMainForm";
            this.Text = (string)App.Current.FindResource("ActiveReportsTitle");
            ((System.ComponentModel.ISupportInitialize)(this.arToolbox)).EndInit();
            this.pnlToolbox.ResumeLayout(false);
            this.pnlProperties.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private void LoadTools(DataDynamics.ActiveReports.Design.Toolbox.Toolbox toolbox)
        {
            //Add Data Providers
            toolbox.AddToolboxItem(new System.Drawing.Design.ToolboxItem(typeof(System.Data.DataSet)), "Data");
            toolbox.AddToolboxItem(new System.Drawing.Design.ToolboxItem(typeof(System.Data.DataView)), "Data");
            toolbox.AddToolboxItem(new System.Drawing.Design.ToolboxItem(typeof(System.Data.OleDb.OleDbConnection)), "Data");
            toolbox.AddToolboxItem(new System.Drawing.Design.ToolboxItem(typeof(System.Data.OleDb.OleDbDataAdapter)), "Data");
            toolbox.AddToolboxItem(new System.Drawing.Design.ToolboxItem(typeof(System.Data.Odbc.OdbcConnection)), "Data");
            toolbox.AddToolboxItem(new System.Drawing.Design.ToolboxItem(typeof(System.Data.Odbc.OdbcDataAdapter)), "Data");
            toolbox.AddToolboxItem(new System.Drawing.Design.ToolboxItem(typeof(System.Data.SqlClient.SqlConnection)), "Data");
            toolbox.AddToolboxItem(new System.Drawing.Design.ToolboxItem(typeof(System.Data.SqlClient.SqlDataAdapter)), "Data");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (this.arDesigner.CommandBarManager.PreProcessMessage(ref msg))
                return true;
            return base.ProcessCmdKey (ref msg, keyData);
        }

        #region "Designer Events"
        private void arDesigner_SelectionChanged()
        {
            string curSelection = "";
            IEnumerator selectionEnum = null;
            if(arDesigner.Selection != null)
            selectionEnum = arDesigner.Selection.GetEnumerator();
            while(selectionEnum != null && selectionEnum.MoveNext())
            {
                if(selectionEnum.Current is Section)
                    curSelection = curSelection + (selectionEnum.Current as Section).Name + ", ";
                if(selectionEnum.Current is ARControl)
                    curSelection = curSelection + (selectionEnum.Current as ARControl).Name + ", ";
                if(selectionEnum.Current is Field)
                    curSelection = curSelection + (selectionEnum.Current as Field).Name + ", ";
                if(selectionEnum.Current is Parameter)
                    curSelection = curSelection + (selectionEnum.Current as Parameter).Key + ", ";
                if(selectionEnum.Current is ActiveReport3)
                    curSelection = curSelection + (selectionEnum.Current as ActiveReport3).Document.Name + ", ";
            }

            if(this.arStatus.Created && this.arStatus.Panels[1] != null)
            {
                this.arStatus.Panels[1].Text = (string.IsNullOrEmpty(curSelection))?
                    (string)App.Current.FindResource("ActiveReportsStatusNoSelection") :
                    string.Format((string)App.Current.FindResource("ActiveReportsStatusSelectionFormat"), curSelection.Substring(0, curSelection.Length - 2));
            }
        }
        #endregion

        private void OnSaveNew(object sender, CommandEventArgs e)
        {
            arDesigner.Report.SaveLayout(templatePath);
        }

        /// <summary>
        /// OnExit
        /// </summary>
        private void OnExit(object sender, CommandEventArgs e)
        {
            this.Close();
        }
    }
}

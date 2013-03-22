/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;
using DataDynamics.ActiveReports.Viewer;

using DataDynamics.ActiveReports;
using ESRI.ArcLogistics.App.Reports;

namespace ESRI.ArcLogistics.App.Dialogs
{
    /// <summary>
    /// PreviewForm - child MDI form that loads up the ActiveReports Viewer to view a report
    /// and provides options to export, save and print the generated report
    /// </summary>
    internal class PreviewForm : System.Windows.Forms.Form
    {
        private DataDynamics.ActiveReports.Viewer.Viewer arvMain =  null;
        private ReportStateDescription _description = null;
        private bool _disposeResponsible = false;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public PreviewForm(Form parentForm, ReportStateDescription description, bool disposeResponsible)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.MdiParent = parentForm;
            if (null == description.Report)
            {
                ReportsGenerator generator = App.Current.ReportGenerator;
                generator.RunReport(description);
            }
            description.IsLocked = true;

            arvMain.Document = description.Report.Document;
            this.Text = description.Report.Document.Name;

            _description = description;
            _disposeResponsible = disposeResponsible;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (null != components)
                    components.Dispose();
            }

            base.Dispose(disposing);

            if (_disposeResponsible)
            {
                ReportsGenerator generator = App.Current.ReportGenerator;
                generator.DisposeReport(_description);
            }

            if (null != _description)
                _description.IsLocked = false;
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.arvMain = new DataDynamics.ActiveReports.Viewer.Viewer();
            this.SuspendLayout();
            //
            // arvMain
            //
            this.arvMain.BackColor = System.Drawing.SystemColors.Control;
            this.arvMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.arvMain.Name = "arvMain";
            this.arvMain.ReportViewer.CurrentPage = 0;
            this.arvMain.ReportViewer.MultiplePageCols = 3;
            this.arvMain.ReportViewer.MultiplePageRows = 2;
            this.arvMain.Size = new System.Drawing.Size(624, 661);
            this.arvMain.TabIndex = 0;
            this.arvMain.TableOfContents.Text = (string)App.Current.FindResource("ActiveReportsContents");
            this.arvMain.TableOfContents.Width = 200;
            this.arvMain.Toolbar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            // 
            // PreviewForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(624, 661);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {this.arvMain});
            this.Name = "PreviewForm";
            this.Text = (string)App.Current.FindResource("ActiveReportsViewTitle");
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);
        }
        #endregion
    }
}

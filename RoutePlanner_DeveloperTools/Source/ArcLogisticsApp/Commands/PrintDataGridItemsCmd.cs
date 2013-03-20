using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.DataGrid;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Class contains logic to print content of DataGridControl. Implements ISupportContext and IDisposable.
    /// </summary>
    internal class PrintDataGridItemsCmd : CommandBase, ISupportContext, IDisposable
    {
        #region CommandBase Members

        /// <summary>
        /// Gets command name.
        /// </summary>
        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        /// <summary>
        /// Gets command title.
        /// </summary>
        public override string Title
        {
            get { return (string)App.Current.FindResource(PRINT_TITLE); }
        }

        /// <summary>
        /// Gets/sets command tooltip.
        /// NOTE : not defined yet.
        /// </summary>
        public override string TooltipText
        {
            get;
            protected set;
        }

        /// <summary>
        /// Prints items of necessary data grid control.
        /// </summary>
        /// <param name="args">Command args.</param>
        /// <exception cref="Exception">Throws when any print error occured.</exception>
        protected override void _Execute(params object[] args)
        {
            Debug.Assert(_dataGridControl != null);

            try
            {
                _dataGridControl.Print(false);
            }
            catch
            {
                throw;
                // NOTE : print error
            }
        }

        #endregion

        #region ISupportContext Members

        /// <summary>
        /// Gets/sets command context. In this command it should be DataGridControl. 
        /// </summary>
        public object Context
        {
            get
            {
                return _dataGridControl;
            }
            set
            {
                Debug.Assert(value != null);
                Debug.Assert(value is DataGridControl);
                _dataGridControl = (DataGridControl)value;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Not implemented there because used resource is DataGridControl and it's can be not disposed.
        /// </summary>
        public void Dispose()
        {
            // Not implemented there because context is and can be not disposed DataGridControl.
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Command name.
        /// </summary>
        private const string COMMAND_NAME = "ArcLogistics.Commands.PrintDataGridItems";

        /// <summary>
        /// "Print" text resource name.
        /// </summary>
        private const string PRINT_TITLE = "PrintButtonHeader";

        #endregion

        #region Private Fields

        /// <summary>
        /// Data grid control which should be printed.
        /// </summary>
        private DataGridControl _dataGridControl = null;

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Class for storing selection on dates.
    /// </summary>
    internal class DateSelectionKeeper
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public DateSelectionKeeper()
        {
            _InitEventHandlers();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Store selection for date.
        /// </summary>
        /// <param name="date">Date to store.</param>
        /// <param name="selectedItems">Selected items to store.</param>
        public void StoreSelection(DateTime date, IList selectedItems)
        {
            Debug.Assert(selectedItems != null);
            Debug.Assert(_dateSelections != null);

            // save selection
            IList<object> selection = new List<object>();
            foreach (Object obj in selectedItems)
            {
                if (obj is Route || obj is Order)
                {
                    selection.Add(obj);
                }
                else
                {
                    Stop stop = obj as Stop;
                    if (stop != null && stop.AssociatedObject is Order)
                    {
                        selection.Add(stop.AssociatedObject);
                    }
                }
            }

            _dateSelections[date] = selection;
        }

        /// <summary>
        /// Restore selection for date.
        /// </summary>
        /// <param name="date">Date to restore.</param>
        /// <returns>Selected items to restore.</returns>
        public List<object> RestoreSelection(DateTime date)
        {
            Debug.Assert(_dateSelections != null);

            List<object> selection = null;

            if (_dateSelections.ContainsKey(date))
            {
                selection = _dateSelections[date] as List<object>;
                _dateSelections.Remove(date);
            }

            return selection;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Creates all common event handlers (loaded/unloaded, collection changed etc.).
        /// </summary>
        private void _InitEventHandlers()
        {
            App.Current.ProjectLoaded += new EventHandler(_OnProjectLoaded);
        }

        /// <summary>
        /// React on project loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _OnProjectLoaded(object sender, EventArgs e)
        {
            _dateSelections.Clear();
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Collection of pairs : collection of selected items + dete where these items should be selected. 
        /// </summary>
        private Dictionary<DateTime, IEnumerable> _dateSelections = new Dictionary<DateTime, IEnumerable>();
        
        #endregion
    }
}

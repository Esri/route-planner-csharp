using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace ESRI.ArcLogistics.App.Pages
{
    public interface ISupportSelection
    {
        /// <summary>
        /// Returns collection of selected items
        /// </summary>
        IList SelectedItems { get; }

        /// <summary>
        /// Set selection to the input collection of items.
        /// </summary>
        /// <param name="items"></param>
        void Select(IEnumerable items);

        /// <summary>
        /// Try to save edited object. Returns "true" if successfully, "false" otherwise.
        /// </summary>
        /// <returns></returns>
        bool SaveEditedItem();
    }
}

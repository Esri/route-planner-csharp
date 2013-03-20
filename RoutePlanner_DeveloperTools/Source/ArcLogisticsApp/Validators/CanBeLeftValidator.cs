using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Validators
{
    /// <summary>
    /// Validates that all items in the panel is valid.
    /// </summary>
    internal static class CanBeLeftValidator<T>
        where T:DataObject
    {
        #region public static methods

        /// <summary>
        /// Validator for IEnumerable of <c>DataObject</c>.
        /// </summary>
        /// <param name="objects">IEnumerable of <c>DataObject</c> to validate.</param>
        /// <returns>True if 'objects' are valid and false otherwise.</returns>
        public static bool IsValid(IEnumerable<T> collection)
        {
            // If any item is invalid - return false.
            foreach (var item in collection)
                if (!string.IsNullOrEmpty(item.Error))
                    return false;

            return true;
        }

        /// <summary>
        /// Validator for <c>Breaks</c>.
        /// </summary>
        /// <param name="objects"><c>Breaks</c> to validate.</param>
        /// <returns>True if all breaks in collection are valid and false otherwise.</returns>
        public static bool IsValid(Breaks collection)
        {
            // If any item is invalid - return false.
            foreach (var item in collection)
                if (!string.IsNullOrEmpty(item.Error))
                    return false;

            return true;
        }

        /// <summary>
        /// If breaks are not valid - show message in MessageWindow.
        /// </summary>
        /// <param name="breaks"><c>Breaks</c> to validate.</param>
        public static void ShowErrorMessagesInMessageWindow(Breaks breaks)
        {
            List<MessageDetail> details = new List<MessageDetail>();
            string invalidPropertyFormat = ((string)App.Current.
                FindResource("SolveValidationPropertyInvalidFormat"));

            // Check that break has errors.
            foreach (var breakObject in breaks)
            {
                Debug.Assert(breakObject as IDataErrorInfo != null);

                string errorString = (breakObject as IDataErrorInfo).Error;

                // If it has - add new MessageDetail.
                if (!string.IsNullOrEmpty(errorString))
                    details.Add(new MessageDetail(MessageType.Warning, errorString));
            }

            // If we have MessageDetails add new Message to message window.
            if (details.Count > 0)
            {
                string errorMessage = ((string)App.Current.
                    FindResource("SetupPanelValidationError"));
                App.Current.Messenger.AddMessage(MessageType.Warning, errorMessage, details);
            }
        }

        /// <summary>
        /// If DataObjects are not valid - show message in MessageWindow.
        /// </summary>
        /// <param name="objects">IEnumerable of <c>DataObject</c> to validate.</param>
        public static void ShowErrorMessagesInMessageWindow(IEnumerable<T> objects)
        {
            List<MessageDetail> details = new List<MessageDetail>();
            string invalidPropertyFormat = ((string)App.Current.
                FindResource("SolveValidationPropertyInvalidFormat"));

            // Check that DataObject has errors.
            foreach (var obj in objects)
            {
                Debug.Assert(obj as DataObject != null);

                string errorString = (obj as IDataErrorInfo).Error;

                // If it has - add new MessageDetail.
                if (!string.IsNullOrEmpty(errorString))
                {
                    // Format error string.
                    string text = string.Format(INVALID_STRING_FORMAT,
                        (obj as DataObject).TypeTitle,
                        invalidPropertyFormat);
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(text);
                    sb.Append(errorString);
                    details.Add(new MessageDetail(MessageType.Warning, sb.ToString(),
                        obj as DataObject));
                }
            }

            // If we have MessageDetails add new Message to Message Window.
            if (details.Count > 0)
            {
                string errorMessage = ((string)App.Current.
                    FindResource("SetupPanelValidationError"));
                App.Current.Messenger.AddMessage(MessageType.Warning, errorMessage, details);
            }
        } 

        #endregion

        #region Private Const

        private const string INVALID_STRING_FORMAT = "{0} {1}";

        #endregion
    }
}

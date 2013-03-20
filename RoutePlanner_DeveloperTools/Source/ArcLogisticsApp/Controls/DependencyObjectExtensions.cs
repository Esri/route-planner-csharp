using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Provides extension methods for <see cref="T:System.Windows.DependencyObject"/>
    /// instances.
    /// </summary>
    internal static class DependencyObjectExtensions
    {
        #region public static methods
        /// <summary>
        /// Enumerates all immediate visual children of the specified
        /// <see cref="T:System.Windows.DependencyObject"/>.
        /// </summary>
        /// <param name="root">The <see cref="T:System.Windows.DependencyObject"/>
        /// to enumerate visual children for.</param>
        /// <returns>A collection of immediate visual children of the specified
        /// <see cref="T:System.Windows.DependencyObject"/>.</returns>
        public static IEnumerable<DependencyObject> EnumerateVisualChildren(
            this DependencyObject root)
        {
            Debug.Assert(root != null);

            var count = VisualTreeHelper.GetChildrenCount(root);
            var children = Enumerable.Range(0, count)
                .Select(index => VisualTreeHelper.GetChild(root, index));

            return children;
        }

        /// <summary>
        /// Enumerates all descendants of the specified <see cref="T:System.Windows.DependencyObject"/>
        /// in it's visual tree.
        /// </summary>
        /// <param name="root">The <see cref="T:System.Windows.DependencyObject"/>
        /// to enumerate visual children for.</param>
        /// <returns>A collection of all visual children of the specified
        /// <see cref="T:System.Windows.DependencyObject"/>.</returns>
        public static IEnumerable<DependencyObject> EnumerateVisualChildrenRecursively(
            this DependencyObject root)
        {
            Debug.Assert(root != null);

            return _EnumerateVisualChildrenRecursively(root);
        }

        /// <summary>
        /// Finds node with the specified name searching both visual and logical
        /// trees of the specified <see cref="T:System.Windows.DependencyObject"/>.
        /// </summary>
        /// <param name="root">The <see cref="T:System.Windows.DependencyObject"/>
        /// to look for named element at.</param>
        /// <param name="name">The name of the element to find.</param>
        /// <returns>A <see cref="T:System.Windows.DependencyObject"/> with the
        /// specified name or null reference if no such element was found.</returns>
        public static DependencyObject FindNode(
            this DependencyObject root,
            string name)
        {
            Debug.Assert(root != null);
            Debug.Assert(!string.IsNullOrEmpty(name));

            var elements = EnumerableEx.Return(root)
                .Concat(root.EnumerateVisualChildrenRecursively());

            foreach (var element in elements)
            {
                var result = LogicalTreeHelper.FindLogicalNode(element, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Implements <see cref="M:ESRI.ArcLogistics.App.Controls.EnumerateVisualChildrenRecursively"/>
        /// using an iterator block allowing the original method to perform
        /// eager validation of arguments.
        /// </summary>
        /// <param name="root">The <see cref="T:System.Windows.DependencyObject"/>
        /// to enumerate visual children for.</param>
        /// <returns>A collection of all visual children of the specified
        /// <see cref="T:System.Windows.DependencyObject"/>.</returns>
        private static IEnumerable<DependencyObject> _EnumerateVisualChildrenRecursively(
            DependencyObject root)
        {
            foreach (var child in root.EnumerateVisualChildren())
            {
                yield return child;

                foreach (var item in _EnumerateVisualChildrenRecursively(child))
                {
                    yield return item;
                }
            }
        }
        #endregion
    }
}

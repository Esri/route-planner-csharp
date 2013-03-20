using System.Diagnostics;
namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Stores custom order property information.
    /// </summary>
    public sealed class OrderPropertyInfo
    {
        /// <summary>
        /// Creates a new instance of the <see cref="OrderPropertyInfo"/> class with the
        /// specified name and title.
        /// </summary>
        /// <param name="name">The name of the custom property.</param>
        /// <param name="title">The title of the custom property.</param>
        /// <returns>A new <see cref="OrderPropertyInfo"/> object with the specified
        /// name and title.</returns>
        public static OrderPropertyInfo Create(string name, string title)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(name));
            Debug.Assert(!string.IsNullOrWhiteSpace(title));

            return new OrderPropertyInfo
            {
                Name = name,
                Title = title,
            };
        }

        /// <summary>
        /// Gets or sets custom order property name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets custom order property title.
        /// </summary>
        public string Title { get; private set; }
    }
}

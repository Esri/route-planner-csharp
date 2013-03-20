using System;

namespace ESRI.ArcLogistics.DomainObjects.Attributes
{
    /// <summary>
    /// This attribute is applied to any domain object property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class DomainPropertyAttribute : Attribute
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public DomainPropertyAttribute() : this(null) { }
        public DomainPropertyAttribute(string resourceName) : this(resourceName, false) { }
        public DomainPropertyAttribute(string resourceName, bool isMandatory)
        {
            Title = (string.IsNullOrEmpty(resourceName)) ? string.Empty : Properties.Resources.ResourceManager.GetString(resourceName);
            IsMandatory = isMandatory;
        }
        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Property title for showing in UI.
        /// </summary>
        public readonly string Title = null;

        /// <summary>
        /// Indicates either property is mandatory.
        /// </summary>
        public readonly bool IsMandatory = false;
        
        #endregion // Public properties

        #region Override functions
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            return string.Format("[DomainPropertyAttribute] Title: {0} IsMandatory: {1}", Title, IsMandatory.ToString());
        }
        #endregion // Override functions
    }
}

using System;

namespace ESRI.ArcLogistics.DomainObjects.Attributes
{
    /// <summary>
    /// This attribute is applied to any property that affects routing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class AffectsRoutingPropertyAttribute : Attribute
    {
    }
}

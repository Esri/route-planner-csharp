using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Geocode
{
    /// <summary>
    /// Name/address pair for geocode.
    /// </summary>
    class NameAddress
    {
        /// <summary>
        /// Address.
        /// </summary>
        public Address Address
        {
            get;
            set;
        }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }
    }
}

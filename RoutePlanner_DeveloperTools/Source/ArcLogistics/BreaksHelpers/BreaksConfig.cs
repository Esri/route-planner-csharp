using System.Runtime.Serialization;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.BreaksHelpers
{
    /// <summary>
    /// Breaks settings configuration.
    /// </summary>
    [DataContract]
    internal class BreaksConfig
    {
        /// <summary>
        /// Breaks type.
        /// </summary>
        [DataMember]
        public BreakType? BreaksType
        {
            get;
            set;
        }

        /// <summary>
        /// Default Breaks in string format. That field is used to save to XML.
        /// </summary>
        [DataMember]
        public string DefaultBreaksString
        {
            get { return Breaks.AssemblyDBString(DefaultBreaks); }
            set
            {
                DefaultBreaks = Breaks.CreateFromDBString(value);
            }
        }
        
        /// <summary>
        /// Default Breaks.
        /// </summary>
        [IgnoreDataMember]
        public Breaks DefaultBreaks
        {
            get;
            set;
       }
    }
}

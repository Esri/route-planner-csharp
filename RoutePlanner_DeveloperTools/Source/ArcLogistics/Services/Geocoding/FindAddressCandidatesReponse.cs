using System.Runtime.Serialization;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Services.Geocoding
{
    /// <summary>
    /// Response of the FindAddressCandidates operation.
    /// </summary>
    [DataContract]
    internal sealed class FindAddressCandidatesReponse : GPResponse
    {
        /// <summary>
        /// Gets or sets a reference to the address candidates collection.
        /// </summary>
        [DataMember(Name = "candidates")]
        public FindAddressCandidateResponse[] Candidates
        {
            get;
            set;
        }
    }
}

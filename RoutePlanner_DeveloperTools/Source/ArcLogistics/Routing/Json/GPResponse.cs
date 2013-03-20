using System;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// GPResponse class.
    /// </summary>
    [DataContract]
    internal class GPResponse : IFaultInfo
    {
        #region IFaultInfo interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public bool IsFault
        {
            get { return _isFault; }
        }

        [DataMember(Name = "error")]
        public GPError FaultInfo
        {
            get { return _error; }
            set
            {
                _error = value;
                _isFault = true;
            }
        }

        #endregion IFaultInfo interface members

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private bool _isFault = false;
        private GPError _error;

        #endregion private fields
    }
}
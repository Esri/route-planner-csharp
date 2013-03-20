using System;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// QueryParameterAttribute class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    internal sealed class QueryParameterAttribute : Attribute
    {
        #region constructors

        public QueryParameterAttribute() {
        }

        public QueryParameterAttribute(string paramName) {
            _name = paramName;
        }

        #endregion

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _name;
    }
}

using System;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// SpecFields class.
    /// </summary>
    internal sealed class SpecFields
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public SpecFields(string keyFieldName)
        {
            this.KeyFieldName = keyFieldName;
        }

        public SpecFields(string keyFieldName, string deletionFieldName)
        {
            this.KeyFieldName = keyFieldName;
            this.DeletionFieldName = deletionFieldName;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public string KeyFieldName { get; set; }

        public string DeletionFieldName { get; set; }

        #endregion public properties
    }
}

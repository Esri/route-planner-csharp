using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// GPDate class.
    /// </summary>
    [DataContract]
    internal class GPDate
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // default format pattern
        private const string DEFAULT_FORMAT = "dd.MM.yyyy HH:mm:ss";

        #endregion constants

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public GPDate()
            : this(DateTime.Now)
        {
        }

        public GPDate(DateTime date)
        {
            _dateStr = date.ToString(DEFAULT_FORMAT,
                CultureInfo.InvariantCulture);

            _format = DEFAULT_FORMAT;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        [IgnoreDataMember]
        public DateTime? Date
        {
            get
            {
                if (_dateStr == null)
                    return null;

                DateTime date;
                if (_format != null)
                {
                    // convert using specified format
                    date = DateTime.ParseExact(_dateStr, _format,
                        CultureInfo.InvariantCulture);
                }
                else
                {
                    // try to convert using standard formats
                    date = DateTime.Parse(_dateStr,
                        CultureInfo.InvariantCulture);
                }

                return date;
            }
        }

        [DataMember(Name = "date")]
        public string DateString
        {
            get { return _dateStr; }
            set { _dateStr = value; }
        }


        [DataMember(Name = "format")]
        public string Format
        {
            get { return _format; }
            set { _format = value; }
        }

        #endregion public properties

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _dateStr;
        private string _format;

        #endregion private fields
    }
}

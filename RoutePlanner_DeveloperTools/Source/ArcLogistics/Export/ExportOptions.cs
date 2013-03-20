namespace ESRI.ArcLogistics.Export
{
    /// <summary>
    /// Class that reperesents exporter options keeper.
    /// </summary>
    public class ExportOptions
    {
        /// <summary>
        /// Creates a new instance of the <c>ExportOptions</c> class.
        /// </summary>
        public ExportOptions()
        { }

        /// <summary>
        /// Show leading stem time flag.
        /// </summary>
        public bool ShowLeadingStemTime
        {
            get
            {
                return _showLeadingStemTime;
            }
            set
            {
                _showLeadingStemTime = value;
            }
        }

        /// <summary>
        /// Show trailing stem time flag.
        /// </summary>
        public bool ShowTrailingStemTime
        {
            get
            {
                return _showTrailingStemTime;
            }
            set
            {
                _showTrailingStemTime = value;
            }
        }

        /// <summary>
        /// Show leading stem time flag.
        /// </summary>
        private bool _showLeadingStemTime = true;
        /// <summary>
        /// Show trailing stem time flag.
        /// </summary>
        private bool _showTrailingStemTime = true;
    }
}

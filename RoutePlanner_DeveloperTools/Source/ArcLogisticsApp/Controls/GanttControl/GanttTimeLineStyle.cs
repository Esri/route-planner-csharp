using System;
using System.Windows;
using System.Windows.Media;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Gantt TimeLine draw style class
    /// </summary>
    internal class GanttTimeLineStyle
    {
        #region Constructor
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public GanttTimeLineStyle()
        {
            _Init();
        }

        #endregion // Constructor

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Hairlines pen
        /// </summary>
        public Pen HairlinePen
        {
            get { return (Pen)_hairlinePen; }
        }

        /// <summary>
        /// Range boundaryes pen
        /// </summary>
        public Pen RangeBoundaryPen
        {
            get { return (Pen)_rangeLinePen; }
        }

        /// <summary>
        /// Hairline height
        /// </summary>
        public int HairlineHeight
        {
            get { return HAIRLINE_HEIGHT; }
        }

        /// <summary>
        /// TimeLine label font
        /// </summary>
        public Typeface FontTypeface
        {
            get { return _typeFace; }
        }

        /// <summary>
        /// TimeLine label font brush
        /// </summary>
        public SolidColorBrush FontBrush
        {
            get { return _fontBrush; }
        }

        /// <summary>
        /// TimeLine label font size
        /// </summary>
        public int FontSize
        {
            get { return _fontSize; }
        }

        /// <summary>
        /// TimeLine label margin
        /// </summary>
        public Size LabelMargin
        {
            get { return _labelMargin; }
        }

        /// <summary>
        /// Part subranges per hour
        /// </summary>
        public int PartPerHour
        {
            get { return PART_PER_HOUR; }
        }

        #endregion // Public properties

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _Init()
        {
            _hairlinePen = new Pen((SolidColorBrush)Application.Current.FindResource("GanttVerticalLinesBrushColor"), PEN_THICKNESS);
            _hairlinePen.Freeze();
            _typeFace = new Typeface((FontFamily)Application.Current.FindResource("DefaultApplicationFont"),
                                     FontStyles.Normal, FontWeights.Normal, FontStretches.Condensed);
            _fontBrush = (SolidColorBrush)Application.Current.FindResource("GanttCaptionBrush");
            _fontBrush.Freeze();
            _fontSize = (int)((double)Application.Current.FindResource("SmallFontSize"));
            _labelMargin = new Size(LABEL_MARGIN_X, LABEL_MARGIN_Y);

            _rangeLinePen = new Pen((SolidColorBrush)Application.Current.FindResource("GanttVerticalLinesBrushColor"), PEN_THICKNESS);
            _rangeLinePen.DashStyle = new DashStyle(new double[] {4}, 8);
            _rangeLinePen.Freeze();
        }

        #endregion // Private helpers

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Pen _hairlinePen = null;
        private Pen _rangeLinePen = null;
        private Typeface _typeFace = null;
        private Size _labelMargin;
        private int _fontSize = 0;
        private SolidColorBrush _fontBrush = null;

        #endregion // Private members

        #region Const definitions
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const int HAIRLINE_HEIGHT = 6;
        private const double PEN_THICKNESS = 1.0f;
        private const int LABEL_MARGIN_X = 2;
        private const int LABEL_MARGIN_Y = 0;

        private const int PART_PER_HOUR = 4;

        #endregion // Const definitions
    }
}

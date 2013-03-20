using System;
using System.Xml.Serialization;

// APIREV: rename file to the class name
namespace ESRI.ArcLogistics.Geometry
{
    /// <summary>
    /// Structure that represents an envelope.
    /// </summary>
    public struct Envelope
    {
        #region public fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // APIREV: convert fields to properties that starts with the capital letter.

        /// <summary>
        /// Left coordinate of the envelope.
        /// </summary>
        [XmlAttribute("xmin")]
        public double left;

        /// <summary>
        /// Top coordinate of the envelope.
        /// </summary>
        [XmlAttribute("ymax")]
        public double top;

        /// <summary>
        /// Right coordinate of the envelope.
        /// </summary>
        [XmlAttribute("xmax")]
        public double right;

        /// <summary>
        /// Bottom coordinate of the envelope.
        /// </summary>
        [XmlAttribute("ymin")]
        public double bottom;

        #endregion public fields

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>Envelope</c> structure.
        /// </summary>
        /// <param name="left">Left coordinate of the envelope.</param>
        /// <param name="top">Top coordinate of the envelope.</param>
        /// <param name="right">Right coordinate of the envelope.</param>
        /// <param name="bottom">Bottom coordinate of the envelope.</param>
        public Envelope(double left, double top, double right, double bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the envelope width.
        /// </summary>
        public double Width
        {
            get { return Math.Abs(right - left); }
        }

        /// <summary>
        /// Gets the envelope height.
        /// </summary>
        public double Height
        {
            get { return Math.Abs(top - bottom); }
        }

        // APIREV: rename to IsValid and correct logic

        /// <summary>
        /// Returns <c>true</c> if the envelope left is more than right or top is less than bottom and <c>false</c> otherwise.
        /// </summary>
        public bool IsEmpty
        {
            get { return (left > right || top < bottom); }
        }

        // APIREV: remove IsNull
        public bool IsNull
        {
            get { return (left == 0.0 && top == 0.0 && right == 0.0 && bottom == 0.0); }
        }

        #endregion public properties

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // APIREV: remove
        public void SetEmpty()
        {
            left = bottom = 1.0;
            right = top = 0.0;
        }

        /// <summary>
        /// Adjusts to include <c>pt</c>. 
        /// </summary>
        /// <param name="pt">Point to include in envelope.</param>
        public void Union(Point pt)
        {
            if (IsEmpty)
            {
                left = right = pt.X;
                top = bottom = pt.Y;
            }
            else
            {
                left = Math.Min(left, pt.X);
                top = Math.Max(top, pt.Y);
                right = Math.Max(right, pt.X);
                bottom = Math.Min(bottom, pt.Y);
            }
        }

        /// <summary>
        /// Indicates whether input point is inside of the envelope.
        /// </summary>
        /// <param name="point">Point to check.</param>
        /// <returns>Returns <c>true</c> if the envelope <c>pt</c> resides in the envelope and <c>false</c> otherwise.</returns>
        public bool IsPointIn(Point point)
        {
            bool result = false;

            if (left <= point.X && right >= point.X && top >= point.Y && bottom <= point.Y)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Gets geometric center of the envelope.
        /// </summary>
        public Point Center
        {
            get
            {
                double x = (left + right) / 2;
                double y = (top + bottom) / 2;

                return new Point(x, y);
            }
        }

        /// <summary>
        /// Moves the envelope.
        /// </summary>
        /// <param name="deltaX">Value that will be added to <c>left</c> and <c>right</c> coordinates.</param>
        /// <param name="deltaY">Value that will be added to <c>top</c> and <c>bottom</c> coordinates.</param>
        public void Move(double deltaX, double deltaY)
        {
            left += deltaX;
            right += deltaX;
            top += deltaY;
            bottom += deltaY;
        }

        #endregion public methods
    }
}

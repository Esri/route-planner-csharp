using System;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

using ESRI.ArcLogistics.Data;
using DataModel = ESRI.ArcLogistics.Data.DataModel;
using System.Globalization;
using ESRI.ArcLogistics.DomainObjects.Attributes;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents a barrier type.
    /// </summary>
    public class BarrierEffect : ICloneable, INotifyPropertyChanged
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>BarrierEffect</c> class.
        /// </summary>
        public BarrierEffect()
        {
            // init default state
            _blockTravel = true;
        }

        #endregion // Constructors

        #region Public static properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets name of the BlockTravel property.
        /// </summary>
        public static string PropertyNameBlockTravel
        {
            get { return PROP_NAME_BLOCKTRAVEL; }
        }

        /// <summary>
        /// Gets name of the DelayTime property.
        /// </summary>
        public static string PropertyNameDelayTime
        {
            get { return PROP_NAME_DELAYTIME; }
        }

        /// <summary>
        /// Gets name of the SpeedFactor property.
        /// </summary>
        public static string PropertyNameSpeedFactor
        {
            get { return PROP_NAME_SPEEDFACTOR; }
        }

        #endregion // Public static properties

        #region Static methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Puts barrier's type to string.
        /// </summary>
        /// <param name="obj">Break values.</param>
        /// <returns>Barrier's type values string.</returns>
        internal static string AssemblyDBString(BarrierEffect obj)
        {
            var result = new StringBuilder();

            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(CommonHelpers.STORAGE_CULTURE);
            // 0. Current version
            result.Append(VERSION_CURR.ToString());
            // 1. BlockTravel
            result.Append(CommonHelpers.SEPARATOR);
            result.Append(obj.BlockTravel);
            // 2. DelayTime
            result.Append(CommonHelpers.SEPARATOR);
            result.Append(obj.DelayTime.ToString(cultureInfo));
            // 3. SpeedFactor
            result.Append(CommonHelpers.SEPARATOR);
            result.Append(obj.SpeedFactorInPercent.ToString(cultureInfo));

            return result.ToString();
        }

        /// <summary>
        /// Parses string and split it to properties values.
        /// </summary>
        /// <param name="value">DB barrier's type properties string.</param>
        /// <returns>Parsed barrier's type.</returns>
        internal static BarrierEffect CreateFromDBString(string value)
        {
            var obj = new BarrierEffect();
            if (null != value)
            {
                char[] valuesSeparator = new char[1] { CommonHelpers.SEPARATOR };
                string[] values;
                values = value.Split(valuesSeparator, StringSplitOptions.None);
                System.Diagnostics.Debug.Assert(4 == values.Length);

                CultureInfo cultureInfo = CultureInfo.GetCultureInfo(CommonHelpers.STORAGE_CULTURE);
                // 0. Current version - Now ignored
                int index = 0;
                // 1. BlockTravel
                obj.BlockTravel = bool.Parse(values[++index]);
                // 2. DelayTime
                obj.DelayTime = double.Parse(values[++index], cultureInfo);
                // 3. SpeedFactor
                obj.SpeedFactorInPercent = double.Parse(values[++index], cultureInfo);
            }

            return obj;
        }

        #endregion // Static methods

        #region Public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Block travel flag.
        /// </summary>
        /// <remarks>If this property is true this means no way to drive through the barrier.
        /// Otherwise DelayTime or SpeedFactor properties make sense.</remarks>
        [DomainProperty("DomainPropertyNameBlockTravel")]
        public bool BlockTravel
        {
            get { return _blockTravel; }
            set
            {
                if (value != _blockTravel)
                {
                    _blockTravel = value;

                    _NotifyPropertyChanged(PROP_NAME_BLOCKTRAVEL);
                }
            }
        }

        /// <summary>
        /// Delay time at barrier point in minutes.
        /// </summary>
        /// <remarks>It makes sense only for Point barriers.</remarks>
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        [DomainProperty("DomainPropertyNameDelayTime")]
        public double DelayTime
        {
            get { return _delayTime; }
            set
            {
                if (value != _delayTime)
                {
                    _delayTime = value;
                    _NotifyPropertyChanged(PROP_NAME_DELAYTIME);
                }
            }
        }

        /// <summary>
        /// Speed scales factor the times of traveling the covered streets.
        /// </summary>
        /// <remarks>It makes sense only for Polyline and Polygon barriers.
        /// Assigning a factor of 0.5 would mean travel is expected to be twice as fast as normal.
        /// A factor of 2.0 would mean it is expected to take twice as long as normal.</remarks>
        [DomainProperty("DomainPropertyNameSpeedFactorInPercent")]
        public double SpeedFactorInPercent
        {
            get { return _speedFactor; }
            set
            {
                if (value != _speedFactor)
                {
                    _speedFactor = value;
                    _NotifyPropertyChanged(PROP_NAME_SPEEDFACTOR);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns a string representation of the barrier type information.
        /// </summary>
        /// <returns>Barrier type string.</returns>
        public override string ToString()
        {
            return AssemblyDBString(this);
        }

        #endregion // Public members

        #region ICloneable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clones the <c>BarierType</c> object.
        /// </summary>
        /// <returns>Cloned object.</returns>
        public object Clone()
        {
            var obj = new BarrierEffect();
            obj.BlockTravel = this.BlockTravel;
            obj.DelayTime = this.DelayTime;
            obj.SpeedFactorInPercent = this.SpeedFactorInPercent;

            return obj;
        }

        #endregion // ICloneable members

        #region INotifyPropertyChanged members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Event which is invoked when any of the object's properties change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion // INotifyPropertyChanged members

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Notifies about change of the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        private void _NotifyPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the BlockTravel property.
        /// </summary>
        private const string PROP_NAME_BLOCKTRAVEL = "BlockTravel";

        /// <summary>
        /// Name of the DelayTime property.
        /// </summary>
        private const string PROP_NAME_DELAYTIME = "DelayTime";

        /// <summary>
        /// Name of the BlockTravel property.
        /// </summary>
        private const string PROP_NAME_SPEEDFACTOR = "SpeedFactorInPercent";

        /// <summary>
        /// Storage schema version 1.
        /// </summary>
        private const int VERSION_1 = 0x00000001;
        /// <summary>
        /// Storage schema current version.
        /// </summary>
        private const int VERSION_CURR = VERSION_1;

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Block travel flag.
        /// </summary>
        private bool _blockTravel;
        /// <summary>
        /// Delay time (in minutes).
        /// </summary>
        private double _delayTime;
        /// <summary>
        /// Speed scale factor.
        /// </summary>
        private double _speedFactor;

        #endregion // Private members
    }
}

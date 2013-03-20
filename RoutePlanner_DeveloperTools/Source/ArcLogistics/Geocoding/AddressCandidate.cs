using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.DomainObjects;
using System.ComponentModel;

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Class that represents an address candidate.
    /// </summary>
    public class AddressCandidate : INotifyPropertyChanged
    {
        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// returns the full address as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Address.FullAddress;
        }

        #endregion public methods

        #region public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Candidate address.
        /// </summary>
        public Address Address
        {
            get { return _address; }
            set { _address = value; }
        }

        /// <summary>
        /// Candidate location.
        /// </summary>
        public Point GeoLocation
        {
            get { return _mapLocation; }
            set 
            { 
                _mapLocation = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(""));
            }
        }

        /// <summary>
        /// Candidate score.
        /// </summary>
        /// <remarks>This is a value in range from 0 to 100. 100 means the best match.</remarks>
        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }

        /// <summary>
        /// Gets or sets a reference to the locator information object for the locator used for
        /// retrieving current address candidate.
        /// </summary>
        public LocatorInfo Locator
        {
            get
            {
                return _locatorInfo;
            }

            set
            {
                if (_locatorInfo != value)
                {
                    _locatorInfo = value;
                    _NotifyPropertyChanged(PROPERTY_NAME_LOCATOR);
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Fired when property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion

        #region Internal members

        /// <summary>
        /// Candidate address type.
        /// </summary>
        internal string AddressType { get; set; }

        #endregion

        #region protected methods
        /// <summary>
        /// Raises <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
        /// event.
        /// </summary>
        /// <param name="e">The arguments for the event.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged(this, e);
        }
        #endregion

        #region private methods
        /// <summary>
        /// Notifies about change of the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        private void _NotifyPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region private constants
        /// <summary>
        /// Stores name of the <see cref="P:ESRI.ArcLogistics.Geocoding.AddressCandidate.Locator"/>
        /// property.
        /// </summary>
        private const string PROPERTY_NAME_LOCATOR = "Locator";
        #endregion

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Address _address;
        private Point _mapLocation;
        private int _score;

        /// <summary>
        /// Stores value of the <see cref="P:ESRI.ArcLogistics.Geocoding.AddressCandidate.Locator"/>
        /// property.
        /// </summary>
        private LocatorInfo _locatorInfo;
        #endregion private members
    }
}

using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ComponentModel;
using ESRI.ArcLogistics.GeocodeService;
using System.ServiceModel.Channels;

namespace ESRI.ArcLogistics.GeocodeService
{
    internal partial class GeocodeServerPortClient : ESRI.ArcLogistics.Services.IAsyncState
    {
        public int RefCount
        {
            get { return _asyncRefCount; }
            set { _asyncRefCount = value; }
        }
        private int _asyncRefCount = 0;

    }
}

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// GeocodeServiceClient class.
    /// </summary>
    internal class GeocodeServiceClient
        : ServiceClientWrap<GeocodeServerPortClient, GeocodeServerPort>
    {
        #region constructors
        public GeocodeServiceClient(string url, AgsServerConnection connection)
            : base(url, connection)
        {
        }
        #endregion

        #region public events
        public event EventHandler<ReverseGeocodeCompletedEventArgs> ReverseGeocodeCompleted;
        #endregion

        #region public methods
        public PropertySet GeocodeAddress(PropertySet Address, PropertySet PropMods)
        {
            ServiceMethod<PropertySet> method = (client) =>
            {
                using (var contextScope = new OperationContextScope(client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] =
                        _CreateRequestMessageProperty();

                    return client.GeocodeAddress(Address, PropMods);
                }
            };

            return Invoke<PropertySet>(method);
        }

        public RecordSet GeocodeAddresses(RecordSet AddressTable, PropertySet AddressFieldMapping, PropertySet PropMods)
        {
            ServiceMethod<RecordSet> method = (client) =>
            {
                using (var contextScope = new OperationContextScope(client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] =
                        _CreateRequestMessageProperty();

                    return client.GeocodeAddresses(AddressTable, AddressFieldMapping, PropMods);
                }
            };

            return Invoke<RecordSet>(method);
        }
        
        public PropertySet ReverseGeocode(Point Location, bool ReturnIntersection, PropertySet PropMods)
        {

            ServiceMethod<PropertySet> method = (client) =>
            {
                using (var contextScope = new OperationContextScope(client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] =
                        _CreateRequestMessageProperty();

                    return client.ReverseGeocode(Location, ReturnIntersection, PropMods);
                }
            };

            return Invoke<PropertySet>(method);
        }

        public RecordSet FindAddressCandidates(PropertySet Address, PropertySet PropMods)
        {
            ServiceMethod<RecordSet> method = (client) =>
            {
                using (var contextScope = new OperationContextScope(client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] =
                        _CreateRequestMessageProperty();

                    return client.FindAddressCandidates(Address, PropMods);
                }
            };

            return Invoke<RecordSet>(method);
        }

        public Fields GetResultFields(PropertySet PropMods)
        {
            ServiceMethod<Fields> method = (client) =>
            {
                using (var contextScope = new OperationContextScope(client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] =
                        _CreateRequestMessageProperty();

                    return client.GetResultFields(PropMods);
                }
            };

            return Invoke<Fields>(method);
        }

        public Fields GetAddressFields()
        {
            ServiceMethod<Fields> method = (client) =>
            {
                using (var contextScope = new OperationContextScope(client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] =
                        _CreateRequestMessageProperty();

                    return client.GetAddressFields();
                }
            };

            return Invoke<Fields>(method);
        }

        public PropertySet GetLocatorProperties()
        {
            ServiceMethod<PropertySet> method = (client) =>
            {
                using (var contextScope = new OperationContextScope(client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] =
                        _CreateRequestMessageProperty();

                    return client.GetLocatorProperties();
                }
            };

            return Invoke<PropertySet>(method);
        }

        public void ReverseGeocodeAsync(Point Location, bool ReturnIntersection, PropertySet PropMods)
        {
            this.ReverseGeocodeAsync(Location, ReturnIntersection, PropMods, null);
        }

        public void ReverseGeocodeAsync(Point Location, bool ReturnIntersection, PropertySet PropMods, object userState)
        {
            Guid id = Guid.NewGuid();
            ServiceMethod method = (client) =>
            {
                using (var contextScope = new OperationContextScope(client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = 
                        _CreateRequestMessageProperty();

                    client.ReverseGeocodeAsync(Location, ReturnIntersection, PropMods, id);
                }
            };

            InvokeAsync(method, id, userState);
        }
        #endregion

        #region protected overrided methods

        protected override GeocodeServerPortClient CreateInnerClient(
            string url)
        {
            Debug.Assert(url != null);

            var client = ServiceHelper.CreateServiceClient<GeocodeServerPortClient>(
                "GeocodeServiceBinding",
                url);

            client.ReverseGeocodeCompleted += new EventHandler<ReverseGeocodeCompletedEventArgs>(_ReverseGeocodeCompleted);

            return client;
        }

        protected override void OnCloseInnerClient(GeocodeServerPortClient client)
        {
            client.ReverseGeocodeCompleted -= new EventHandler<ReverseGeocodeCompletedEventArgs>(_ReverseGeocodeCompleted);
            base.OnCloseInnerClient(client);
        }
        #endregion

        #region private methods

        /// <summary>
        /// Create message property which add "referer" header to HTTP request.
        /// </summary>
        /// <returns>HttpRequestMessageProperty.</returns>
        private HttpRequestMessageProperty _CreateRequestMessageProperty()
        {
            var httpRequestProperty = new HttpRequestMessageProperty();
            httpRequestProperty.Headers.Add(AgsHelper.RefererParameterName, AgsHelper.RefererValue);

            return httpRequestProperty;
        }

        private void _ReverseGeocodeCompleted(object sender, ReverseGeocodeCompletedEventArgs e)
        {
            AsyncCompletedEventArgs outArgs = null;
            if (IsAsyncCompleted(sender, e, out outArgs))
            {
                PropertySet result = null;
                if (!outArgs.Cancelled && outArgs.Error == null)
                    result = e.Result;

                if (ReverseGeocodeCompleted != null)
                {
                    ReverseGeocodeCompleted(this, new ReverseGeocodeCompletedEventArgs(
                        new object[1] { result },
                        outArgs.Error,
                        outArgs.Cancelled,
                        outArgs.UserState));
                }
            }
        }
        #endregion
    }
}

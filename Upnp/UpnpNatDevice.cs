using System;
using System.Net;

namespace Vinchuca.Upnp
{
    internal static class UpnpConstants
    {
        public const int InvalidArguments = 402;
        public const int ActionFailed = 501;
        public const int Unathorized = 606;
        public const int SpecifiedArrayIndexInvalid = 713;
        public const int NoSuchEntryInArray = 714;
        public const int WildCardNotPermittedInSourceIp = 715;
        public const int WildCardNotPermittedInExternalPort = 716;
        public const int ConflictInMappingEntry = 718;
        public const int SamePortValuesRequired = 724;
        public const int OnlyPermanentLeasesSupported = 725;
        public const int RemoteHostOnlySupportsWildcard = 726;
        public const int ExternalPortOnlySupportsWildcard = 727;
        public const int NoPortMapsAvailable = 728;
        public const int ConflictWithOtherMechanisms = 729;
        public const int WildCardNotPermittedInIntPort = 732;
    }

    internal class UpnpNatDeviceInfo
    {
        public UpnpNatDeviceInfo(IPAddress localAddress, Uri locationUri, string serviceControlUrl, string serviceType)
        {
            LocalAddress = localAddress;
            ServiceType = serviceType;
            HostEndPoint = new IPEndPoint(IPAddress.Parse(locationUri.Host), locationUri.Port);

            if (Uri.IsWellFormedUriString(serviceControlUrl, UriKind.Absolute))
            {
                var u = new Uri(serviceControlUrl);
                serviceControlUrl = u.PathAndQuery;
            }

            var builder = new UriBuilder("http", locationUri.Host, locationUri.Port);
            ServiceControlUri = new Uri(builder.Uri, serviceControlUrl);
        }

        public IPEndPoint HostEndPoint { get; private set; }
        public IPAddress LocalAddress { get; private set; }
        public string ServiceType { get; private set; }
        public Uri ServiceControlUri { get; private set; }
    }

    public sealed class UpnpNatDevice
    {
        internal readonly UpnpNatDeviceInfo DeviceInfo;
        private readonly SoapClient _soapClient;

        internal UpnpNatDevice(UpnpNatDeviceInfo deviceInfo)
        {
            DeviceInfo = deviceInfo;
            _soapClient = new SoapClient(DeviceInfo.ServiceControlUri, DeviceInfo.ServiceType);
        }

        public IPAddress GetExternalIP()
        {
            var message = new GetExternalIPAddressRequestMessage();
            var soapResponse = _soapClient.Invoke("GetExternalIPAddress", message.ToXml());
            var response = new GetExternalIPAddressResponseMessage(soapResponse, DeviceInfo.ServiceType);
            return response.ExternalIPAddress;
        }

        public void CreatePortMap(Mapping mapping)
        {
            if (mapping.PrivateIP.Equals(IPAddress.None)) mapping.PrivateIP = DeviceInfo.LocalAddress;

            var message = new CreatePortMappingRequestMessage(mapping);
            try
            {
                _soapClient.Invoke("AddPortMapping", message.ToXml());
            }
            catch (MappingException me)
            {

                switch (me.ErrorCode)
                {
                    case UpnpConstants.OnlyPermanentLeasesSupported:
                        mapping.Lifetime = 0;
                        // We create the mapping anyway. It must be released on shutdown.
                        mapping.LifetimeType = MappingLifetime.ForcedSession;
                        CreatePortMap(mapping);
                        break;
                    case UpnpConstants.SamePortValuesRequired:
                        mapping.PublicPort = mapping.PrivatePort;
                        CreatePortMap(mapping);
                        break;
                    case UpnpConstants.RemoteHostOnlySupportsWildcard:
                        mapping.PublicIP = IPAddress.None;
                        CreatePortMap(mapping);
                        break;
                    default:
                        throw;
                }
            }
        }
    }
}
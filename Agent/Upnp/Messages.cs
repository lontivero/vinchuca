using System;
using System.Collections.Generic;
using System.Net;
using System.Xml;

namespace Vinchuca.Upnp
{
    internal abstract class RequestMessageBase
    {
        public abstract IDictionary<string, object> ToXml();
    }

    internal abstract class ResponseMessageBase
    {
        private readonly XmlDocument _document;
        protected string ServiceType;
        private readonly string _typeName;

        protected ResponseMessageBase(XmlDocument response, string serviceType, string typeName)
        {
            _document = response;
            ServiceType = serviceType;
            _typeName = typeName;
        }

        protected XmlNode GetNode()
        {
            var nsm = new XmlNamespaceManager(_document.NameTable);
            nsm.AddNamespace("responseNs", ServiceType);

            string typeName = _typeName;
            string messageName = typeName.Substring(0, typeName.Length - "Message".Length);
            XmlNode node = _document.SelectSingleNode("//responseNs:" + messageName, nsm);
            if (node == null) throw new InvalidOperationException("The response is invalid: " + messageName);

            return node;
        }
    }

    internal class GetExternalIPAddressRequestMessage : RequestMessageBase
	{
		public override IDictionary<string, object> ToXml()
		{
			return new Dictionary<string, object>();
		}
	}

    internal class GetExternalIPAddressResponseMessage : ResponseMessageBase
    {
        public GetExternalIPAddressResponseMessage(XmlDocument response, string serviceType)
            : base(response, serviceType, "GetExternalIPAddressResponseMessage")
        {
            var ipNode = GetNode();
            var element = ipNode["NewExternalIPAddress"];
            var ip = element != null ? element.InnerText : string.Empty;

            ExternalIPAddress = IPAddress.Parse(ip);
        }

        public IPAddress ExternalIPAddress { get; private set; }
    }
}
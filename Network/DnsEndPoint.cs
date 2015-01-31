using System;
using System.Net;

namespace DreamBot.Network
{
	public class DnsEndPoint : EndPoint
	{
	    public string Host { get; private set; }
        public int Port { get; private set; }


        public DnsEndPoint(string host, int port)
		{
			Host = host;
			Port = port;
		}
		
        public override bool Equals(object comparand)
		{
			var dnsEndPoint = comparand as DnsEndPoint;
			return dnsEndPoint != null && (Port == dnsEndPoint.Port) && Host == dnsEndPoint.Host;
		}
		
		public override int GetHashCode()
		{
			return StringComparer.InvariantCultureIgnoreCase.GetHashCode(ToString());
		}
		
		public override string ToString()
		{
			return string.Concat(new object[]
			{
				Host,
				":",
				Port
			});
		}
	}
}
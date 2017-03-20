using System;
using System.Net;

namespace Vinchuca.Upnp
{
	enum MappingLifetime
	{
		Permanent,
		Session,
		Manual,
		ForcedSession
	}

    public enum Protocol
    {
        Tcp,
        Udp
    }

    public class Mapping
    {
        private DateTime _expiration;
        private int _lifetime;
        internal MappingLifetime LifetimeType { get; set; }

        public string Description { get; internal set; }
        public IPAddress PrivateIP { get; internal set; }
        public Protocol Protocol { get; internal set; }
        public int PrivatePort { get; internal set; }
        public IPAddress PublicIP { get; internal set; }
        public int PublicPort { get; internal set; }

        public int Lifetime
        {
            get { return _lifetime; }
            internal set
            {
                switch (value)
                {
                    case int.MaxValue:
                        LifetimeType = MappingLifetime.Session;
                        _lifetime = 10*60; // ten minutes
                        _expiration = DateTime.UtcNow.AddSeconds(_lifetime);
                        ;
                        break;
                    case 0:
                        LifetimeType = MappingLifetime.Permanent;
                        _lifetime = 0;
                        _expiration = DateTime.UtcNow;
                        break;
                    default:
                        LifetimeType = MappingLifetime.Manual;
                        _lifetime = value;
                        _expiration = DateTime.UtcNow.AddSeconds(_lifetime);
                        break;
                }
            }
        }

        public DateTime Expiration
        {
            get { return _expiration; }
            internal set
            {
                _expiration = value;
                _lifetime = (int) (_expiration - DateTime.UtcNow).TotalSeconds;
            }
        }

        internal Mapping(Protocol protocol, IPAddress privateIP, int privatePort, int publicPort)
            : this(protocol, privateIP, privatePort, publicPort, 0, "Open.Nat")
        {
        }

        public Mapping(Protocol protocol, IPAddress privateIP, int privatePort, int publicPort, int lifetime,
            string description)
        {
            Protocol = protocol;
            PrivateIP = privateIP;
            PrivatePort = privatePort;
            PublicIP = IPAddress.None;
            PublicPort = publicPort;
            Lifetime = lifetime;
            Description = description;
        }

        public Mapping(Protocol protocol, int privatePort, int publicPort)
            : this(protocol, IPAddress.None, privatePort, publicPort, 0, "Open.NAT")
        {
        }

        public Mapping(Protocol protocol, int privatePort, int publicPort, string description)
            : this(protocol, IPAddress.None, privatePort, publicPort, int.MaxValue, description)
        {
        }

        public Mapping(Protocol protocol, int privatePort, int publicPort, int lifetime, string description)
            : this(protocol, IPAddress.None, privatePort, publicPort, lifetime, description)
        {
        }

        internal Mapping(Mapping mapping)
        {
            PrivateIP = mapping.PrivateIP;
            PrivatePort = mapping.PrivatePort;
            Protocol = mapping.Protocol;
            PublicIP = mapping.PublicIP;
            PublicPort = mapping.PublicPort;
            LifetimeType = mapping.LifetimeType;
            Description = mapping.Description;
            _lifetime = mapping._lifetime;
            _expiration = mapping._expiration;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var m = obj as Mapping;
            if (ReferenceEquals(null, m)) return false;
            return PublicPort == m.PublicPort && PrivatePort == m.PrivatePort;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = PublicPort;
                hashCode = (hashCode*397) ^ (PrivateIP != null ? PrivateIP.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ PrivatePort;
                return hashCode;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Xml.Serialization;

namespace SimpleP2P
{
    public class Peer
    {
        public Peer()
        {
            //only for serialization
        }
        public Peer(string identifier, IPAddress host, int port)
        {
            Identifier = identifier;
            Host = host;
            Port = port;
            Discovered = DateTime.Now;
            LastContact = DateTime.MinValue;
            LastHandshake = DateTime.MinValue;
        }

        public override string ToString()
        {
            return $"{Identifier} {Host}:{Port}";
        }
        public override bool Equals(object obj)
        { //don't recall why implemented this way instead of the generated Equals (similar to the generated GetHashCode)
            if (obj == null || this.GetType() != obj.GetType()) return false;

            var otherPeer = (Peer)obj;
            //Shared.Log($"\t\tHost {Host.ToString()} == {otherPeer.Host.ToString()} = {Host.ToString() == otherPeer.Host.ToString()}");
            //Shared.Log($"\t\tPort {Port} == {otherPeer.Port} = {Port == otherPeer.Port}");
            return Host.ToString() == otherPeer.Host.ToString() && Port == otherPeer.Port;
        }
        public override int GetHashCode()
        { //generated
            var hashCode = 995452845;
            hashCode = hashCode * -1521134295 + EqualityComparer<IPAddress>.Default.GetHashCode(Host);
            hashCode = hashCode * -1521134295 + Port.GetHashCode();
            return hashCode;
        }

        public string Identifier { get; set; }
        [XmlIgnore] public IPAddress Host { get; set; }
        [XmlElement("Host")] public string SerializableHost
        {
            get { return Host.ToString(); }
            set { Host = string.IsNullOrEmpty(value) ? null : IPAddress.Parse(value); }
        }
        public int Port { get; set; }
        [XmlIgnore] public DateTime Discovered { get; set; }
        [XmlIgnore] public DateTime LastHandshake { get; set; }
        [XmlIgnore] public DateTime LastContact { get; set; }
        [XmlIgnore] public int ConnectionFailures { get; set; }
        [XmlIgnore] public bool IsMaster { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

namespace SimpleP2P
{
    public class Network
    {
        public Network(Peer masterpeer, Peer self)
        {
            Masterpeer = masterpeer;
            Masterpeer.IsMaster = true;
            Self = self;
            Peers = new List<Peer>();
        }

        public void AddPeer(Peer peer)
        {
            if (Peers.Where(p => p.Equals(peer)).Count() == 0 && !peer.Equals(Masterpeer) && !peer.Equals(Self)) Peers.Add(peer);
        }
        public void RemovePeer(Peer peer)
        {
            Peers = Peers.Where(p => !p.Equals(peer)).ToList();
        }
        public void AddPeers(IEnumerable<Peer> peers)
        {
            peers.ToList().ForEach(p => AddPeer(p));
        }

        public void LogPeerList()
        {
            Shared.Log($"Internal peer list:");
            foreach (var peer in Peers)
            {
                var newIndicator = peer.LastHandshake == DateTime.MinValue ? " (new)" : "";
                Shared.Log($"\t{peer.ToString()}{newIndicator}");
            }
        }

        public Peer Masterpeer { get; private set; }
        public Peer Self { get; private set; }
        public List<Peer> Peers { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;

namespace SimpleP2P
{
    public class Networker
    {
        public Networker(ref Network network)
        {
            Network = network;
        }

        public void StartNetworking(int handshakeInterval = 1)
        {
            var sleepTime = 5000; //lower value at first to speed up discovery on first start
            do
            {
                if (!Network.Self.IsMaster &&
                        (
                            Network.Masterpeer.LastHandshake < DateTime.Now.AddMinutes(handshakeInterval * -2) ||
                            (Network.Masterpeer.LastHandshake < DateTime.Now.AddMinutes(handshakeInterval * -1) && Network.Peers.Count == 0)
                        )
                   )
                {
                    //if not master
                    //if last handshake with master was X minutes ago
                    //or if last handshake with master was X minute ago and I have no other peers
                    Shared.Log($"Announcing to Master");
                    Handshake(Network.Masterpeer);
                }

                //Network.LogPeerList();

                var peersCopy = Network.Peers.ToArray(); //needed to prevent Unhandled Exception: System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
                foreach (var peer in peersCopy)
                {
                    if (peer.LastHandshake == DateTime.MinValue ||
                        peer.LastHandshake < DateTime.Now.AddMinutes(handshakeInterval * -5))
                    {
                        //if master
                        //handshake with new peers
                        //also handshake with known peers every now and then to see if they're still alive
                        //this way the network can contain peers that have been unreachable for about 5 mins, but that's okay, whenever the network is needed and a peer is unreachable it can be removed at that time
                        Shared.Log($"Announcing to peer");
                        Handshake(peer);
                    }
                }
                Thread.Sleep(sleepTime);
                if (Network.Peers.Count > 0 && !Network.Self.IsMaster) sleepTime = 30000;
            } while (true);
        }

        private void Handshake(Peer peer)
        {
            try
            {
                Shared.Log($"{peer} connecting");
                using (var client = new TcpClient())
                {
                    client.Connect(new IPEndPoint(peer.Host, peer.Port));
                    Shared.Log($"{peer} connected");
                    using (var stream = client.GetStream())
                    {
                        using (var writer = new StreamWriter(stream, Encoding.ASCII))
                        {
                            Shared.Log($"{peer} sending");
                            var peerlistPacket = new PeerlistPacket()
                            {
                                Sender = Network.Self,
                                Peerlist = Network.Self.IsMaster ? Network.Peers.Where(p => p.ConnectionFailures == 0 && !p.Equals(peer)).ToList()
                                                                 : Network.Peers.Where(p => p.ConnectionFailures == 0 && !p.Equals(peer) && p.LastHandshake > DateTime.MinValue).ToList()
                                //will only send peers it connected to itself to prevent propagating unavailable peers
                                //except if master to make the network grow as fast as possible on first start
                            };
                            writer.Write(peerlistPacket.ToXml());
                            writer.Flush();
                        }
                    }
                }
                peer.LastContact = DateTime.Now;
                peer.LastHandshake = DateTime.Now;
                Shared.Log($"{peer} ready");
            }
            catch (SocketException ex)
            {
                peer.ConnectionFailures += 1;
                var counter = " (" + peer.ConnectionFailures.ToString() + (!peer.IsMaster ? "/3)" : ")");
                Shared.Log(ex.Message + counter);
                if (peer.ConnectionFailures >= 3)//&& !peer.IsMaster)
                {
                    Network.RemovePeer(peer); //remove unreachable peer
                    Shared.Log($"{peer} removed");
                }
            }
        }

        private Network Network { get; set; } //passed by ref from outside so does not need to be available to outside from here
    }
}

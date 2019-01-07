using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimpleP2P
{
    public class Listener
    {
        public Listener(ref Network network, IPAddress listenAddress, int listenPort)
        {
            Network = network; //the Listener will need to update what the application knows as its network
            ListenAddress = listenAddress;
            ListenPort = listenPort;
        }
        public void Listen()
        {
            try
            {
                //listen for other peers
                var listener = new TcpListener(new IPEndPoint(ListenAddress, ListenPort));
                listener.Start();
                Shared.Log($"Listening on {ListenAddress}:{ListenPort}...");
                do
                {
                    try
                    {
                        using (var socket = listener.AcceptSocket())
                        {
                            Shared.Log($"{socket.RemoteEndPoint} connected");
                            using (var stream = new NetworkStream(socket))
                            {
                                using (var reader = new StreamReader(stream, Encoding.ASCII))
                                {
                                    Shared.Log($"{socket.RemoteEndPoint} receiving");
                                    var data = reader.ReadToEnd();
                                    //Shared.Log(data);
                                    var peerlistPacket = data.FromXml<PeerlistPacket>();
                                    peerlistPacket.Sender.LastContact = DateTime.Now;
                                    Network.AddPeer(peerlistPacket.Sender);
                                    Shared.Log($"{socket.RemoteEndPoint} is {peerlistPacket.Sender}");

                                    if (peerlistPacket.Peerlist != null && peerlistPacket.Peerlist.Count > 0)
                                    {
                                        Shared.Log($"{peerlistPacket.Sender} received {peerlistPacket.Peerlist.Count} peers");
                                        peerlistPacket.Peerlist.ForEach(p => Shared.Log($"\tReceived peer {p.ToString()}"));
                                        Network.AddPeers(peerlistPacket.Peerlist); //add received peers to network
                                    }
                                    Shared.Log($"{peerlistPacket.Sender} closed");
                                }
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        Shared.Log($"Disconnected: {ex.Message}");
                    }

                    Network.LogPeerList();

                    Thread.Sleep(50);
                } while (true);
            }
            catch (SocketException ex)
            {
                Shared.Log($"Unable to start listener: {ex.Message}");
            }
        }
        private Network Network { get; set; } //passed by ref from outside do does need to be available to outside from here
        public IPAddress ListenAddress { get; set; }
        public int ListenPort { get; set; }
    }
}

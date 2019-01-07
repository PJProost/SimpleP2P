using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleP2P
{
    public class PeerlistPacket
    {
        public Peer Sender { get; set; }
        public List<Peer> Peerlist { get; set; }
    }
}

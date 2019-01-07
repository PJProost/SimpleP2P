using System;
using System.Net;
//using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimpleP2P
{
    public class Program
    {
        static void Main(string[] args)
        {
            //in the current application there is a single point of failure when the master node is down
            //network would stay up between existing nodes but no new nodes could join the network as long as it is down

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("configuration.json", true, true)
                .Build();
            bool isMaster = config["IsMaster"] == null ? false : bool.Parse(config["IsMaster"]); //if this is the first peer of the network
            //
            string masterAddress = config["Masteraddress"]; //first peer to connect to as client
            int masterPort = config["Masterport"] == null ? 1337 : int.Parse(config["Masterport"]); //first peer to connect to as client
            string listenAddress = config["Listenaddress"] ?? Shared.GetPublicIPAddress(); //to accept connections
            int listenPort = config["Listenport"] == null ? new Random().Next(1338, 1999) : int.Parse(config["Listenport"]); //to accept connections
            string identifier = config["Identifier"] == null ? Environment.MachineName + listenPort : config["Identifier"];
            int handshakeInterval = config["HandshakeInterval"] == null ? 1 : int.Parse(config["HandshakeInterval"]); //in minutes, master will use double this value

            var masterpeer = new Peer("Master", IPAddress.Parse(masterAddress), masterPort);
            var self = new Peer(identifier, IPAddress.Parse(listenAddress), listenPort);

            if (isMaster)
            {
                self.IsMaster = isMaster;
                self.Identifier = "Master";
                self.Port = masterPort;
                identifier = "Master";
                //listenAddress = masterAddress; not needed as we should listen for any connection regardeless of if it was a loopback connection or a remote connection, using dns or ip address, or whatever
                listenPort = masterPort;
            } else
            {
                Shared.Log($"Masterpeer: {masterpeer}");
            }
            Shared.Log($"I am: {self}");
            Console.Title = self.ToString();

            var network = new Network(masterpeer, self);

            var listener = new Listener(ref network, IPAddress.Parse(listenAddress), listenPort);
            Thread listenThread = new Thread(() => listener.Listen())
            {
                IsBackground = true
            };
            listenThread.Start();

            Thread.Sleep(100);

            var networker = new Networker(ref network);
            Thread networkerThread = new Thread(() => networker.StartNetworking(handshakeInterval))
            {
                IsBackground = false
            };
            networkerThread.Start();

            //Shared.Log($"Press enter to exit...");
            //Console.ReadLine();
        }
    }
}

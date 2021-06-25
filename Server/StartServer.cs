using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class StartServer
    {
        private Socket listener;
        private IPHostEntry host;
        private IPAddress ipAddress;
        IPEndPoint localEndPoint;

        public StartServer()
        {
            Set();
            Create();
        }

        void Set()
        {
            host = Dns.GetHostEntry("127.0.0.1");
            ipAddress = host.AddressList[0];
            localEndPoint = new IPEndPoint(ipAddress, 11000);
        }
        void Create()
        {
            listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (listener == null)
            {
                Console.WriteLine("Socket creation error");
            }
            Console.WriteLine("Socket created");
        }
    }
}

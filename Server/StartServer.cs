using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class StartServer
    {
        private Socket socket, handler;
        private IPHostEntry host;
        private IPAddress ipAddress;
        private IPEndPoint localEndPoint;

        public StartServer()
        {
            Set();
            Create();
            Bind();
            Listen();
            Accept();
            Clean();
            
        }

        void Set()
        {
            host = Dns.GetHostEntry("127.0.0.1");
            ipAddress = host.AddressList[0];
            localEndPoint = new IPEndPoint(ipAddress, 7777);
        }
        void Create()
        {
            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (socket == null)
            {
                Console.WriteLine("Socket creation error");
            }
            Console.WriteLine("Socket created");
        }
        void Bind()
        {
            socket.Bind(localEndPoint);
           Console.WriteLine("Socket bound");
        }
        void Listen()
        {
            socket.Listen(10);
            Console.WriteLine("Socket listening");
        }
        void Accept()
        {
            Console.WriteLine("Waiting for connection");
            handler = socket.Accept();
            Console.WriteLine("Conected");
        }
        void Clean()
        {
            socket.Shutdown(SocketShutdown.Receive);
            socket.Close();
            handler.Shutdown(SocketShutdown.Receive);
            handler.Close();
        }
    }
}

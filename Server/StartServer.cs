using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Server
{
    class StartServer
    {
        private Socket socket, handler;
        private IPHostEntry host;
        private IPAddress ipAddress;
        private IPEndPoint localEndPoint;
        private long connectId = 0;

        Hashtable socketHolder = new Hashtable();
        Hashtable threadHolder = new Hashtable();

        Thread thread;
        Thread read;
        


        public StartServer()
        {
            Set();
            Create();
            Bind();
            Listen();
            thread = new Thread(new ThreadStart(WaitingForClient));
            threadHolder.Add(connectId, thread);
            thread.Start();            
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
        void Receive()
        {

            long realId = connectId;

            string data = null;
            byte[] buffer;
            buffer = new byte[1024];
            int bytesRec = ((Socket)socketHolder[realId]).Receive(buffer);
            data = Encoding.ASCII.GetString(buffer, 0, bytesRec);
            Computer computer = JsonSerializer.Deserialize<Computer>(data);
            Console.WriteLine(computer.name);
            CloseTheThread(connectId);
        }
        void CleanSocket(long realId)
        {
            
            Socket sct = (Socket)socketHolder[realId];
            sct.Shutdown(SocketShutdown.Both);
            sct.Close();

        }
        void WaitingForClient()
        {
            while (true)
            {
                Accept();

                if (connectId < 10000)
                    Interlocked.Increment(ref connectId);
                else
                    connectId = 1;
                
                if (socketHolder.Count < 10)
                {
                    while (socketHolder.Contains(connectId))
                    {
                        Interlocked.Increment(ref connectId);
                    }
                    socketHolder.Add(connectId, handler);
                    read = new Thread(new ThreadStart(Receive));
                    threadHolder.Add(connectId,read);
                    read.Start();               
                }
            }
        }

        private void CloseTheThread(long realId)
        {
            CleanSocket(realId);
            socketHolder.Remove(realId);         
            threadHolder.Remove(realId);
        }
    }
}

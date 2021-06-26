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
        Thread identification;

        private string data = null;
        private byte[] buffer;
        private int identificator;
        private int amountOfBytes;




        public StartServer()
        {
            Set();
            Create();
            Bind();
            Listen();
            /*
            thread = new Thread(new ThreadStart(WaitingForClient));
            threadHolder.Add(connectId, thread);
            thread.Start();
            */
            WaitingForClient();
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
            Console.WriteLine("Waiting for connection THREAD[{0}]",Thread.CurrentThread.ManagedThreadId);
            handler = socket.Accept();           
        }
        void ReceiveJson(long realId)
        {              
            buffer = new byte[1024];
            Socket temporallySocket = ((Socket)socketHolder[realId]);

            amountOfBytes = temporallySocket.Send(Encoding.ASCII.GetBytes("Send_Json", 0, 9));

            amountOfBytes = temporallySocket.Receive(buffer);
            data = Encoding.ASCII.GetString(buffer, 0, amountOfBytes);
            Computer computer = JsonSerializer.Deserialize<Computer>(data);
            Console.WriteLine("Json received THREAD[{0}]", Thread.CurrentThread.ManagedThreadId);

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
                    identification = new Thread(new ThreadStart(ClientIdentify));
                    threadHolder.Add(connectId, identification);
                    identification.Start();             
                }
            }
        }

        private void CloseTheThread(long realId)
        {
            CleanSocket(realId);
            socketHolder.Remove(realId);         
            threadHolder.Remove(realId);
        }
        void ClientIdentify()
        {
            long realId = connectId;
            buffer = new byte[30];
            amountOfBytes = ((Socket)socketHolder[realId]).Receive(buffer);
            identificator = BitConverter.ToInt32(buffer,0);

            Console.WriteLine("Client {0}: Connected! THREAD[{1}]", identificator, Thread.CurrentThread.ManagedThreadId);
            if (identificator == 1) ReceiveJson(realId);
        }
    }
}

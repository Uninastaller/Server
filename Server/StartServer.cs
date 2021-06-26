using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Server
{
    class StartServer
    {
        private static Socket socket, handler;
        private static IPHostEntry host;
        private static IPAddress ipAddress;
        private static IPEndPoint localEndPoint;
        private static long connectId;

        private static Hashtable socketHolder = new Hashtable();
        private static Hashtable threadHolder = new Hashtable();

        private static Thread thread;
        private static Thread identification;

        private static string data = null;
        private static byte[] buffer;
        private static byte[] msg;
        private static int identificator;
        private static int amountOfBytes;

        private static Computer computer;

        private static Semaphore semaphore;




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
            semaphore = new Semaphore(1, 1);

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
        void ReceiveJson(Socket s)
        {              
            buffer = new byte[1024];

            amountOfBytes = s.Send(Encoding.ASCII.GetBytes("Send_Json", 0, 9));

            amountOfBytes = s.Receive(buffer);
            data = Encoding.ASCII.GetString(buffer, 0, amountOfBytes);
            computer = JsonSerializer.Deserialize<Computer>(data);
            Console.WriteLine("Json received THREAD[{0}]", Thread.CurrentThread.ManagedThreadId);

            Console.WriteLine(computer.name);
            //CloseTheThread(connectId);
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
                semaphore.WaitOne();
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
                    semaphore.Release();
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
            Socket s = ((Socket)socketHolder[realId]);
            amountOfBytes = s.Receive(buffer);
            identificator = BitConverter.ToInt32(buffer,0);

            Console.WriteLine("Client {0}: Connected! THREAD[{1}]", identificator, Thread.CurrentThread.ManagedThreadId);
            if (identificator == 1)
            {
                semaphore.WaitOne();
                ReceiveJson(s);
                semaphore.Release();
            }
            if (identificator == 2)
            {
                semaphore.WaitOne();
                SendJson(s);                
            }
        }
        void SendJson(Socket s)
        {
            
            string stringjson = JsonSerializer.Serialize(computer);
            msg = Encoding.ASCII.GetBytes(stringjson);
            amountOfBytes = s.Send(msg);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
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
        private static Socket ServerSocket, handler;
        private static IPHostEntry host;
        private static IPAddress ipAddress;
        private static IPEndPoint localEndPoint;
        private static long connectId;

        private static Hashtable socketHolder = new Hashtable();
        private static Hashtable threadHolder = new Hashtable();

        private static List<Socket> clientSockets = new List<Socket>();

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
            _Listen();
            Console.ReadLine();
        }

        void Set()
        {

            host = Dns.GetHostEntry("127.0.0.1");
            ipAddress = host.AddressList[0];
            localEndPoint = new IPEndPoint(ipAddress, 7777);
        }
        void Create()
        {
            ServerSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("Socket created");
        }
        void Bind()
        {
            ServerSocket.Bind(localEndPoint);
            Console.WriteLine("Socket bound");
        }
        void _Listen()
        {
            ServerSocket.Listen(10);
            ServerSocket.BeginAccept(new AsyncCallback(_AcceptCallback), null);
            Console.WriteLine("Socket listening");
        }
        private static void _AcceptCallback(IAsyncResult AR)
        {
            Socket socket = ServerSocket.EndAccept(AR);
            clientSockets.Add(socket);
            buffer = new byte[1024];
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(_ReceiveCallback), socket);
            ServerSocket.BeginAccept(new AsyncCallback(_AcceptCallback), null);

        }
        private static void _ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            amountOfBytes = socket.EndReceive(AR);
            msg = new byte[amountOfBytes];
            Array.Copy(buffer,msg,amountOfBytes);
            data = Encoding.ASCII.GetString(msg);
            Console.WriteLine("[Client] " + data);
            if (JsonValidation(data))
            {
                Console.WriteLine("[SERVER] Valid json of Computer object was sent.");
            }
            if (data.ToLower() == "request json")
            {
                SendJson(socket);
                Console.WriteLine("[SERVER] Sending Json.");
            }
            else Send("Invalid request", socket);

        }
        void CleanSocket(long realId)
        {
            
            Socket sct = (Socket)socketHolder[realId];
            sct.Shutdown(SocketShutdown.Both);
            sct.Close();

        }

        static void SendJson(Socket socket)
        {
            if (computer != null)
            {
                string stringjson = JsonSerializer.Serialize(computer);
                Send(stringjson, socket);
            }
            else Console.WriteLine("I dont have any Json yet.");
        }
        static void Send(String data, Socket socket)
        {
            msg = Encoding.ASCII.GetBytes(data);
            socket.BeginSend(msg, 0, msg.Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket);
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(_ReceiveCallback), socket);
        }
        static void SendCallBack(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }
        static bool JsonValidation(String data) 
        {
            try
            {
                computer = JsonSerializer.Deserialize<Computer>(data);
            }
            catch(System.Text.Json.JsonException)
            {
                return false;
            }
            return true;
        }
    }
}

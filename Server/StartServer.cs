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
        private static Socket ServerSocket;
        private static IPHostEntry host;
        private static IPAddress ipAddress;
        private static IPEndPoint localEndPoint;

        private static List<Socket> clientSockets = new List<Socket>();

        private const int BUFFER_SIZE = 2048;
        private static byte[] buffer = new byte[BUFFER_SIZE];

        private static Computer computer;




        public StartServer()
        {
            Set();
            Create();
            Bind();
            Listen();            
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
        void Listen()
        {
            ServerSocket.Listen(10);
            ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            Console.WriteLine("Socket listening");
            Console.ReadLine();
        }
        void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = ServerSocket.EndAccept(AR);

            clientSockets.Add(socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            Console.WriteLine("Client{0} connected",socket.Handle);
            ServerSocket.BeginAccept(AcceptCallback, null);

        }
        void ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            int amountOfBytes;

            try
            {
                amountOfBytes = socket.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                socket.Close();
                clientSockets.Remove(socket);
                return;
            }

            byte[] msg = new byte[amountOfBytes];
            Array.Copy(buffer,msg,amountOfBytes);
            string data = Encoding.ASCII.GetString(msg);
            Console.WriteLine("[Client] " + data);
            if (JsonValidation(data))
            {
                Send("Valid json of the Computer object was sent.", socket);
            }
            else if (data.ToLower() == "request json")
            {
                SendJson(socket);               
            }
            else if (data.ToLower() == "exit")
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                clientSockets.Remove(socket);
                Console.WriteLine("Client disconnected");
                return;
            }
            else Send("Invalid request", socket);

        }

        void SendJson(Socket socket)
        {
            if (computer != null)
            {
                string stringjson = JsonSerializer.Serialize(computer);
                Console.WriteLine("[SERVER] Sending Json.");
                Send(stringjson, socket);
            }
            else{
                Send("I dont have any Json yet.", socket);
                Console.WriteLine("I dont have any Json yet."); 
                }
        }
        void Send(String data, Socket socket)
        {   
            byte[] msg = Encoding.ASCII.GetBytes(data);
            socket.Send(msg);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        bool JsonValidation(String data) 
        {
            try
            {
                computer = JsonSerializer.Deserialize<Computer>(data);
                Console.WriteLine("[SERVER] Valid json of Computer object was sent.");
            }
            catch(JsonException)
            {
                return false;
            }
            return true;
        }
    }
}

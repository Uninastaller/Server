﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server
{
    class Server
    {
        private Socket serverSocket;
        private IPHostEntry host;
        private IPAddress ipAddress;
        private IPEndPoint localEndPoint;

        private List<Socket> clientSockets = new List<Socket>();

        private const int BUFFER_SIZE = 2048;
        private byte[] buffer = new byte[BUFFER_SIZE];

        private Computer computer;




        public Server(int port)
        {
            start(port);
        }

        void start(int port)
        {
            Set(port);
            Create();
            Bind();
            Listen();
            CloseAllSockets();
        }

        void Set(int port)
        {
            host = Dns.GetHostEntry("127.0.0.1");
            ipAddress = host.AddressList[0];
            localEndPoint = new IPEndPoint(ipAddress, port);
        }
        void Create()
        {
            serverSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("Socket created");
        }
        void Bind()
        {
            serverSocket.Bind(localEndPoint);
            Console.WriteLine("Socket bound");
        }
        void Listen()
        {
            serverSocket.Listen(10);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            Console.WriteLine("Socket listening");
            Console.ReadLine();
        }
        void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;
            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            clientSockets.Add(socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            Console.WriteLine("Client {0} - connected", socket.Handle);
            serverSocket.BeginAccept(AcceptCallback, null);

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
                Console.WriteLine("Client {0} - forcefully disconnected", ((Socket)AR.AsyncState).Handle);
                socket.Close();
                clientSockets.Remove(socket);
                return;
            }

            byte[] msg = new byte[amountOfBytes];
            Array.Copy(buffer, msg, amountOfBytes);
            Array.Clear(buffer, 0, BUFFER_SIZE);

            string data = Encoding.ASCII.GetString(msg);
            Console.WriteLine("[Client " + ((Socket)AR.AsyncState).Handle + "] " + data);
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
                Console.WriteLine("Client {0} - disconnected", ((Socket)AR.AsyncState).Handle);
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
            else
            {
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
            catch (JsonException)
            {
                return false;
            }
            return true;
        }
        void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            serverSocket.Close();
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;


namespace Server
{
    class Server
    {
        private Socket serverSocket;
        private IPHostEntry host;
        private IPAddress ipAddress;
        private IPEndPoint localEndPoint;

        private Hashtable bufferAndSocketHolder = new Hashtable();

        private const int BUFFER_SIZE = 2048;
        
        private Computer computer;

        public Server(string ipAddress, int port, char c)
        {
            GetIPAddress(ipAddress);
            Start(port);
            LoopCheckingForStopChar(c);
        }

        void Start(int port)
        {
            Set(port);
            Create();
            Bind();
            Listen();
        }
        void GetIPAddress(String ipAddress)
        {
            try
            {
                host = Dns.GetHostEntry(ipAddress);
                this.ipAddress = host.AddressList[0];
            }
            catch (SocketException)
            {
                ErrorWithStarting("Invalid IP address",false);
            }
        }

        void Set(int port)
        {
            localEndPoint = new IPEndPoint(ipAddress, port);
        }
        void Create()
        {
            try
            {
                serverSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }catch (SocketException)
            {
                ErrorWithStarting("Failed to create socket",false);     
            }
            Console.WriteLine("Socket created");
        }
        void Bind()
        {
            try
            {
                serverSocket.Bind(localEndPoint);
            }catch (SocketException)
            {
                ErrorWithStarting("Error with binding", true);
            }
            Console.WriteLine("Socket bound");
        }
        void Listen()
        {
            serverSocket.Listen(10);
            Console.WriteLine("Socket listening");
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
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

            bufferAndSocketHolder.Add(socket, new byte[BUFFER_SIZE]);
  
            socket.BeginReceive((byte[])bufferAndSocketHolder[socket], 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            Console.WriteLine("Client {0} - connected", socket.Handle);
            serverSocket.BeginAccept(AcceptCallback, null);

        }


        void ReceiveCallback(IAsyncResult AR)
        {
            Console.Write("[THREAD {0}] ",Thread.CurrentThread.ManagedThreadId);
            Socket socket = (Socket)AR.AsyncState;
            int amountOfBytes;

            try
            {
                amountOfBytes = socket.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client {0} - forcefully disconnected", ((Socket)AR.AsyncState).Handle);
                Clean(socket);
                return;
            }         

            byte[] msg = new byte[amountOfBytes];
            Array.Copy((byte[])bufferAndSocketHolder[socket], msg, amountOfBytes);

            string data = Encoding.ASCII.GetString(msg);
            Console.WriteLine("[Client " + ((Socket)AR.AsyncState).Handle + "] " + data);
            EvaluateOfReceivedData(data,socket);

        }

        void EvaluateOfReceivedData(String data, Socket socket)
        {
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
                Clean(socket);

                Console.WriteLine("Client - disconnected");
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
            socket.BeginReceive((byte[])bufferAndSocketHolder[socket], 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);

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
            foreach (Socket socket in bufferAndSocketHolder.Keys)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }


        void Clean(Socket socket)
        {
            socket.Close();              
            bufferAndSocketHolder.Remove(socket);
        }
        void LoopCheckingForStopChar(char c)
        {
            while (true)
            {
                string x = Console.ReadLine();
                if ((x != "")&&(c == x[0]))
                {
                    CloseAllSockets();
                    break;
                }

            }
        }
        void ErrorWithStarting(String message, bool socketClose)
        {
            if(socketClose)serverSocket.Close();
            Console.WriteLine(message);
            Environment.Exit(0);
        }
    }
}

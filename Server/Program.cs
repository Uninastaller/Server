using System;

namespace Server
{
    class Program
    {
        const int PORT = 7777;
        const char CLOSIN_CHAR = 'q';
        const string IP_ADDRESS = "127.0.0.1";
        static void Main(string[] args)
        {
            new Server(IP_ADDRESS, PORT, CLOSIN_CHAR);
        }
    }
}

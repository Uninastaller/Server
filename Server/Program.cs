using System;

namespace Server
{
    class Program
    {
        const int PORT = 7777;
        const char CLOSIN_CHAR = 'q';
        static void Main(string[] args)
        {
            new Server(PORT,CLOSIN_CHAR);
        }
    }
}

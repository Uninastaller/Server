using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Computer
    {
        public string name { get; set; }
        public string version { get; set; }
        public int price { get; set; }



        public Computer(string name, string version, int price)
        {
            this.name = name;
            this.version = version;
            this.price = price;
        }
    }
}

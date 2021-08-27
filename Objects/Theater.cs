using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movieez
{
    public class Theater
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public Theater() { }

        public Theater(string name, string address = "")
        {
            Name = name;
            Address = address;
        }
    }
}

public enum ScreeningType
{
    regular,
    threeD,
    Imax,
    Vip,
    Business
}

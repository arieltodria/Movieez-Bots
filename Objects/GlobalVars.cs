using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movieez
{
    class GlobalVars
    {
        public string[] SUPPORTED_BOTS = { "CinemeCity", "YesPlanet", "HotCinema" };
        public const int ACTION_RETRY_COUNTER = 3;
        public const int DEFAULT_WAIT_TIME = 10;
        public const int WAIT_FOR_ELEMENT = 10;
    }
}
using System;
using System.Globalization;
using System.Text;
using System.IO;
using System.Threading;

namespace Movieez
{
    class Program
    {
        public static string ResourcesPath = Path.GetFullPath(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "..\\Resources"));
        static void Main(string[] args)
        {
            initConfig();
            Console.WriteLine("Hello World!");
            BotsManager bots = new BotsManager();
            bots.LaunchHotCinemaBot();
            //bots.LaunchCinemaCityBot();
            //bots.LaunchYesPlanetBot();

            //MyScheduler.IntervalInHours(9, 44, 1, bots.LaunchYesPlanetBot());
        }

        static void initConfig()
        {
            CultureInfo ci = new CultureInfo("he-IL");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }
    }
}



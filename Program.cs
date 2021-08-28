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
            init();
            BotsManager bots = new BotsManager();

            //MyScheduler.IntervalInHours(9, 44, 1, bots.LaunchYesPlanetBot());
        }

        static void init()
        {
            initLogger();
            initConfig();
        }

        static void initConfig()
        {
            CultureInfo ci = new CultureInfo("he-IL");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        static void initLogger()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory + $"movieezLogs"));
        }
    }
}



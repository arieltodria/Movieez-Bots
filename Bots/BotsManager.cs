using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Movieez
{
    public class BotsManager
    {
        // Logger
        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public BotsManager(bool runOnStart = true)
        {
            logger.Info("Starting BotsManager");
            if (runOnStart)
                run();
        }
        [SetUp]
        public void SetUp()
        {
        }

        public void run()
        {
            logger.Info("Running BotsManager");
            LaunchCinemaCityBot();
            LaunchYesPlanetBot();
            LaunchHotCinemaBot();
        }

        [Test]
        public void LaunchYesPlanetBot()
        {
            logger.Info("Launching Yes Planet bot");
            YesPlanetBot yesPlanet = new YesPlanetBot();
            yesPlanet.run();
        }

        [Test]
        public void LaunchCinemaCityBot()
        {
            logger.Info("Launching Cinema City bot");
            CinemaCityBot cinemaCity = new CinemaCityBot();
            cinemaCity.run();
        }

        [Test]
        public void LaunchHotCinemaBot()
        {
            logger.Info("Launching Hot Cinema bot");
            HotCinema hotCinema = new HotCinema();
            hotCinema.run();
        }
    }
}

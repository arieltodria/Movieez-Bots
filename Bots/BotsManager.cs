using NUnit.Framework;
using System;
using System.Threading;

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

        public void run()
        {
            System.Threading.Tasks.Task task;
            System.Action[] bots = { LanchLevBot, LaunchHotCinemaBot, LaunchYesPlanetBot, LaunchCinemaCityBot };
            while (true)
            {
                foreach (System.Action bot in bots)
                {
                    logger.Info("Running bot");
                    task = new System.Threading.Tasks.Task(bot);
                    task.RunSynchronously();
                    logger.Info("Finished running bot ");
                }
                // Run bots every 8 hours
                logger.Info("Going to sleep.....");
                Thread.Sleep(new TimeSpan(8, 0, 0));
                logger.Info("Waking up from sleep.....");
            }
        }
        public void LaunchYesPlanetBot()
        {
            logger.Info("Launching Yes Planet bot");
            YesPlanetBot yesPlanet = new YesPlanetBot();
            yesPlanet.run();
        }
        public void LanchLevBot()
        {
            logger.Info("Launching Lev bot");
            LevBot lev = new LevBot();
            lev.run();
        }
        public void LaunchCinemaCityBot()
        {
            logger.Info("Launching Cinema City bot");
            CinemaCityBot cinemaCity = new CinemaCityBot();
            cinemaCity.run();
        }

        public void LaunchHotCinemaBot()
        {
            logger.Info("Launching Hot Cinema bot");
            HotCinema hotCinema = new HotCinema();
            hotCinema.run();
        }
    }
}

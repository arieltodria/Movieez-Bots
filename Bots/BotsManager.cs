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
        private IWebDriver driver;
        [SetUp]
        public void SetUp()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
        }

        [Test]
        public void LaunchYesPlanetBot()
        {
            YesPlanet yesPlanet = new YesPlanet();
            yesPlanet.run();
        } 

        public void LaunchCinemaCityBot()
        {
            CinemaCityBot cinemaCity = new CinemaCityBot();
            cinemaCity.run();
        }

        public void LaunchHotCinemaBot()
        {
            HotCinema hotCinema = new HotCinema();
            hotCinema.run();
        }

        [TearDown]
        public void TearDown()
        {
            //driver.Close();
        }
    }
}

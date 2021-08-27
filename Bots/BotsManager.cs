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
            YesPlanetBot yesPlanet = new YesPlanetBot();
            yesPlanet.run();
        } 

        public void LaunchCinemaCityBot()
        {
            CinemaCityBot cinemaCity = new CinemaCityBot();
            cinemaCity.run();
        }

        [TearDown]
        public void TearDown()
        {
            //driver.Close();
        }
    }
}

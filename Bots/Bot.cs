using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OpenQA.Selenium.Support.UI;
using System.IO;
using Movieez.Bots;

namespace Movieez
{
    public class Bot 
    {
        public IWebDriver driver;
        // Movieez DB API
        public MovieezApiUtils _movieezApiUtils;

        public void initDriver(string botUrl)
        {
            //var chromeOptions = new ChromeOptions();
            //chromeOptions.AddArgument("--headless");
            driver = new ChromeDriver(Movieez.Program.ResourcesPath);
            goToUrl(botUrl);
            driver.Manage().Window.Maximize();
            wait();
        }

        // Waits default time
        public void wait(int time = GlobalVars.DEFAULT_WAIT_TIME)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(GlobalVars.DEFAULT_WAIT_TIME);
        }

        public IWebElement FindElementByDriver(By by, bool runScrollToLoadAllElements = false)
        {
            if (runScrollToLoadAllElements)
                scrollToLoadAllElements();
            wait();
            for (int j=0; j < GlobalVars.ACTION_RETRY_COUNTER; j++)
            {
                if (driver.FindElements(by).Count > 0)
                    return driver.FindElement(by);
                wait();
            }
            return null;
        }

        public IWebElement FindElementByFather(By by, IWebElement father, bool runScrollToLoadAllElements = false)
        {
            if (runScrollToLoadAllElements)
                scrollToLoadAllElements();
            wait();
            for (int j = 0; j < GlobalVars.ACTION_RETRY_COUNTER; j++)
            {
                if (father.FindElements(by).Count > 0)
                    return father.FindElement(by);
                wait();
            }
            return null;
        }

        public IReadOnlyCollection<IWebElement> FindElementsByDriver(By by, bool runScrollToLoadAllElements = false)
        {
            if (runScrollToLoadAllElements)
                scrollToLoadAllElements();
            wait();
            for (int j = 0; j < GlobalVars.ACTION_RETRY_COUNTER; j++)
            {
                if (driver.FindElements(by).Count > 0)
                    return driver.FindElements(by);
                wait();
            }
            return null;
        }

        public IReadOnlyCollection<IWebElement> FindElementsByFather(By by, IWebElement father, bool runScrollToLoadAllElements = false)
        {
            if (runScrollToLoadAllElements)
                scrollToLoadAllElements();
            wait();
            for (int j = 0; j < GlobalVars.ACTION_RETRY_COUNTER; j++)
            {
                if (father.FindElements(by).Count > 0)
                    return father.FindElements(by);
                wait();
            }
            return null;

        }

        public void Click(IWebElement element, bool runGoToElement = false, bool ruHover = true)
        {
            if (runGoToElement)
            {
                goToElement(element);
                wait();
            }
            if (ruHover)
            {
                hover(element, false);
                wait();
            }
            try
            {
                element.Click();
            }
            catch (Exception) { }
            wait(15);

        }

        public bool isElementVisible(By by)
        {
            if (driver.FindElements(by).Count > 0)
                return true;
            return false;
        }

        public void goToElement(IWebElement element)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", element);
            wait();
        }

        public void scrollToLoadAllElements()
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scroll(0,0)");
            int heigh = System.Convert.ToInt32(((IJavaScriptExecutor)driver).ExecuteScript("return document.body.scrollHeight;"));
            int pixeslsToScroll = 300;
            int numOfScrolls = heigh / pixeslsToScroll;
            for (int i=0; i< numOfScrolls; i++)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollBy(0, {pixeslsToScroll})");
                wait();
            }
        }

        public void hover(IWebElement el, bool runScroll = false)
        {
            if (runScroll)
                scrollToLoadAllElements();
            for(int i=0; i < GlobalVars.ACTION_RETRY_COUNTER; i++)
            {
                try
                {
                    Actions action = new Actions(driver);
                    wait();
                    action.MoveToElement(el).Perform();
                }
                catch { Console.WriteLine("Failed to hover"); }
            }
            wait();
        }
        public string parseMovieGenre(string type)
        {
            string res = "";
            if (type.IndexOf("קומדיה") != -1||type.IndexOf("Comedy") != -1)
                res+=",Comedy";
            if (type.IndexOf("אקשן") != -1|| type.IndexOf("Action") != -1)
                res += ",Action";
            if (type.IndexOf("מותחן") != -1 || type.IndexOf("מתח") != -1|| type.IndexOf("Thriller")!= -1)
                res += ",Thriller";
            if (type.IndexOf("דרמה") != -1|| type.IndexOf("Drama")!=-1)
                res += ",Drama";
            if (type.IndexOf("אימה") != -1 || type.IndexOf("Horror") != -1)
                res += ",Horror";
            if (type.IndexOf("מדע בדיוני") != -1 || type.IndexOf("SciFi") != -1|| type.IndexOf("Science") != -1)
                res += ",SciFi";
            if (type.IndexOf("מיוזיקל") != -1||type.IndexOf("Musical") != -1)
                res += ",Musical";
            if (type.IndexOf("ילדים") != -1 || type.IndexOf("Kids") != -1)
                res += "Kids";
            if (type.IndexOf("משפחה") != -1 || type.IndexOf("Family") != -1)
                res += ",Family";
            if (type.IndexOf("פשע") != -1 || type.IndexOf("Crime") != -1)
                res += ",Crime";
            if (type.IndexOf("הרפתקאות") != -1 || type.IndexOf("Adventures") != -1)
                res += ",Adventures";
            if (type.IndexOf("אנימציה") != -1 || type.IndexOf("Animation") != -1)
                res += ",Animation"; 

            return res.Substring(1);
        }
        public string ParseGenreToString(List<Genre> genres)
        {
            string genreOfMovie = "";
            for (int i = 0; i < genres.Count; i++)
            {
                genreOfMovie += ' ';
                genreOfMovie += genres[i];
            }
            return genreOfMovie;
        }
        public Rating parseMovieRating(string rating)
        {
            if (rating.IndexOf("13") != -1 || rating.IndexOf("12") != -1 || rating.IndexOf("14") != -1)
                return Rating.pg13Rated;
            if (rating.IndexOf("כל הגילאים") != -1 || rating.IndexOf("מותר לכל") != -1)
                return Rating.gRated;
            if (rating.IndexOf("16") != -1)
                return Rating.nc17Rated;
            return Rating.Unknown;
        }

        // Parses movie duraion (in min) from string
        // For example "משך הסרט 99 דקןת" returns 99
        public string parseMovieDuration(string durtion)
        {
            Regex re = new Regex(@"(\d)+");
            Match m = re.Match(durtion);
            if (m.Success)
                return m.Value;
            else
            {
                Console.WriteLine("Failef to parse duration string: " + durtion);
                return "";
            }
        }

        public void goToUrl(string url)
        {
            driver.Url = url;
            wait();
        }
    }

}

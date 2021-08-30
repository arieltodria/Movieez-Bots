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
        // Logger
        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        string debugDataPath = AppDomain.CurrentDomain.BaseDirectory + $"movieezLogs\\Data\\";

        public void initDriver(string botUrl)
        {
            logger.Debug("Initializing driver");
            driver = new ChromeDriver(Movieez.Program.ResourcesPath);
            goToUrl(botUrl);
            driver.Manage().Window.Maximize();
            wait();
        }

        // Waits default time
        public void wait(int time = GlobalVars.DEFAULT_WAIT_TIME)
        {
            //logger.Debug($"Waiting {time} seconds..");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(GlobalVars.DEFAULT_WAIT_TIME);
        }

        public IWebElement FindElementByDriver(By by, bool runScrollToLoadAllElements = false)
        {
            logger.Trace($"Finding element by driver");
            if (runScrollToLoadAllElements)
                scrollToLoadAllElements();
            wait();
            for (int j = 0; j < GlobalVars.ACTION_RETRY_COUNTER; j++)
            {
                if (driver.FindElements(by).Count > 0)
                    return driver.FindElement(by);
                wait();
            }
            return null;
        }

        public IWebElement FindElementByFather(By by, IWebElement father, bool runScrollToLoadAllElements = false)
        {
            logger.Trace($"Finding element by father");
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
            logger.Trace($"Finding elements by driver");
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
            logger.Trace($"Finding elements by father");
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
            logger.Trace("Clicking on element");
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
            logger.Debug("Going to element");
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", element);
            wait();
        }

        public void scrollToLoadAllElements()
        {
            logger.Debug("Scrolling from top to buttom to load all elements");
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scroll(0,0)");
            int heigh = System.Convert.ToInt32(((IJavaScriptExecutor)driver).ExecuteScript("return document.body.scrollHeight;"));
            int pixeslsToScroll = 300;
            int numOfScrolls = heigh / pixeslsToScroll;
            for (int i = 0; i < numOfScrolls; i++)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollBy(0, {pixeslsToScroll})");
                wait();
            }
        }

        public void hover(IWebElement el, bool runScroll = false)
        {
            logger.Trace("Hovering on element");
            if (runScroll)
                scrollToLoadAllElements();
            for (int i = 0; i < GlobalVars.ACTION_RETRY_COUNTER; i++)
            {
                try
                {
                    Actions action = new Actions(driver);
                    wait();
                    action.MoveToElement(el).Perform();
                }
                catch { logger.Debug("Failed to hover"); }
            }
            wait();
        }
        public Genre parseMovieGenre(string type)
        {
            logger.Debug("Parsing movie genre");
            if (type.IndexOf("קומדיה") != -1)
                return Genre.Comedy;
            if (type.IndexOf("אקשן") != -1)
                return Genre.Action;
            if (type.IndexOf("מותחן") != -1 || type.IndexOf("מתח") != -1)
                return Genre.Thriller;
            if (type.IndexOf("דרמה") != -1)
                return Genre.Drama;
            if (type.IndexOf("אימה") != -1)
                return Genre.Horror;
            if (type.IndexOf("מדע בדיוני") != -1)
                return Genre.SciFi;
            if (type.IndexOf("מיוזיקל") != -1)
                return Genre.Musical;
            if (type.IndexOf("ילדים") != -1)
                return Genre.Kids;
            if (type.IndexOf("משפחה") != -1)
                return Genre.Family;
            return Genre.Unknown;
        }

        public Rating parseMovieRating(string rating)
        {
            logger.Debug("Parsing movie rating");
            if (rating.IndexOf("13") != -1 || rating.IndexOf("12") != -1 || rating.IndexOf("14") != -1)
                return Rating.pg13Rated;
            if (rating.IndexOf("כל הגילאים") != -1 || rating.IndexOf("מותר לכל") != -1)
                return Rating.gRated;
            if (rating.IndexOf("16") != -1)
                return Rating.nc17Rated;
            return Rating.Unknown;
        }

        public string fixMovieName(string name)
        {
            logger.Debug("Fixing movie's hebrew name");
            string[] stringsToRemove = { "עברית עם כתוביות", "עברית", "אנגלית"};
            foreach (string str in stringsToRemove)
            {
                if (name.Contains(str))
                {
                    name.Remove(name.IndexOf(str));
                }
            }
            return name;
        }

        // Parses movie duraion (in min) from string
        // For example "משך הסרט 99 דקןת" returns 99
        public string parseMovieDuration(string durtion)
        {
            logger.Debug("Parsing movie's duration");
            Regex re = new Regex(@"(\d)+");
            Match m = re.Match(durtion);
            if (m.Success)
                return m.Value;
            else
            {
                logger.Error("Failed to parse movie's duration");
                return "";
            }
        }

        public void goToUrl(string url)
        {
            logger.Info($"Going to url {url}");
            driver.Url = url;
            wait();
        }

        public void closeBrowser()
        {
            logger.Info("Closing browser...");
            driver.Close();
        }

        public void saveDebugData()
        {
            logger.Info("Saving debug data of bot run");
            Directory.CreateDirectory(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory + $"movieezLogs\\Data\\"));
            var timestamp = DateTime.Now.ToString("dd.MM.yyyy_HH-mm");
            savePageHtml(timestamp);
            takeScreenshot(timestamp);
        }

        // Saves HTML page source of current chrome driver url in debug data location with current date
        public void savePageHtml(string timestamp)
        {
            logger.Debug("Saving page source to file");
            try
            {
                var pageSource = driver.PageSource;
                File.WriteAllText(debugDataPath + $"pageSource_{timestamp}.txt", pageSource);
            }
            catch(Exception e)
            {
                logger.Error(e);
            }
        }

        // Saves screenshot of current chrome driver url in debug data location with current date
        public void takeScreenshot(string timestamp)
        {
            logger.Debug("Saving screenshot");
            try
            {
                //Take the screenshot
                Screenshot image = ((ITakesScreenshot)driver).GetScreenshot();
                //Save the screenshot
                image.SaveAsFile(debugDataPath + $"screenshot_{timestamp}.png", ScreenshotImageFormat.Png);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }
    }
}

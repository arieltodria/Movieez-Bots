using Movieez.Bots;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Movieez
{
    public class LevBot : Bot
    {
        string Name = "LevCinema";
        string MainUrl = "https://www.lev.co.il/";
        public List<Movie> MoviesList;
        public List<Theater> TheatersList;
        public List<Showtime> ScreeningsList;
        public List<string> moviesUrlList;
        public List<string> TheatersUrlList;
        string theatersUrl = "https://www.lev.co.il/cinemas/";
        string moviesUrl = "https://www.lev.co.il/movie/";
        // Logger
        public static new NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public LevBot()
        {
            initDriver(MainUrl);
            MoviesList = new List<Movie>();
            TheatersList = new List<Theater>();
            ScreeningsList = new List<Showtime>();
            moviesUrlList = new List<string>();
            _movieezApiUtils = new MovieezApiUtils(MovieezApiUtils.e_Theaters.HotCinema);
            TheatersUrlList = new List<string>();
        }
        public void run()
        {
           // this.parseTheaters();
            this.ParseAllMovies();
           // this.ParseScreening();
           // this.printResults();
            this.closeBrowser();
        }
        void parseTheaters()
        {//
            goToUrl(theatersUrl);
            var elem = driver.FindElements(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/article/section/div/p[8]"));
            var theaters = FindElementsByFather(By.XPath(".//*"), elem.ToList()[0]);
            for (int i = 0; i < theaters.Count; i += 2)
            {
                string nv = theaters.ToList()[i].GetAttribute("href");
                TheatersUrlList.Add(nv);

            }                                       ///html/body/div/div[2]/div[2]/div/div[1]/div/section/article/section/div/p[9]
            var elem2 = driver.FindElements(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/article/section/div/p[9]"));
            var theaters2 = FindElementsByFather(By.XPath(".//*"), elem2.ToList()[0]);
            for (int i = 0; i < theaters2.Count; i += 2)
            {
                string nv = theaters2.ToList()[i].GetAttribute("href");
                TheatersUrlList.Add(nv);
            }
            for (int i = 0; i < TheatersUrlList.Count; i ++)
            {
                string adressOfTheater = null;
                goToUrl(TheatersUrlList[i]);
                var elem3 = driver.FindElements(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div"));
                var theatername = FindElementsByFather(By.XPath(".//*"), elem3.ToList()[0]);
                string nameOfTheater = theatername.ToList()[0].GetAttribute("innerText").ToString().Trim();
                var elem4 = driver.FindElements(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/div[2]/div"));
                if (elem4.Count == 0)
                {
                    elem4 = driver.FindElements(By.ClassName("exdetail"));
                    var theaterAdress = FindElementsByFather(By.XPath(".//*"), elem4.ToList()[0]);
                    string theaterForSubs2 = theaterAdress.ToList()[0].GetAttribute("innerHTML").ToString().Trim();
                    string theaterForSubs = elem4.ToList()[0].GetAttribute("innerHTML").ToString().Trim();
                    if (theaterForSubs.IndexOf("ברחוב") != -1)
                    {
                        int j = theaterForSubs.IndexOf("ברחוב");
                        while(theaterForSubs[j]!='.')
                        {
                            adressOfTheater += theaterForSubs[j];
                            j++;
                        }    

                    }
                    else
                    {
                        string[] subs = theaterForSubs.Split(" ");
                        for (int j = 1; j < subs.Length; j++)
                        {
                            adressOfTheater += subs[j];
                        }
                    }
                }
                else
                {
                    var theaterAdress = FindElementsByFather(By.XPath(".//*"), elem4.ToList()[0]);

                    string theaterForSubs = theaterAdress.ToList()[0].GetAttribute("innerHTML").ToString().Trim();
                    string[] subs = theaterForSubs.Split("<br>\r\n");
                    for (int k = 0; k < subs.Length; k++)
                    {
                        adressOfTheater += subs[k];
                        adressOfTheater += " ";
                    }
                }
                Theater theater = new Theater(nameOfTheater, adressOfTheater.Trim());
                TheatersList.Add(theater);

            }

        }
        void ParseAllMovies()
        {
            goToUrl(moviesUrl);

            var elem = driver.FindElements(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/article/section/div/div"));
            var movieList2 = FindElementsByFather(By.XPath(".//*"), elem.ToList()[0]);
            var movieList = movieList2.ToList()[0].FindElements(By.ClassName("featureItem poster cat_0"));

        }




        // var theathersList = FindElementsByFather(By.CssSelector("li"), elem.ToList()[0]);
        //  for (int i = 0; i < theathersList.Count; i++)

    }
        
    }




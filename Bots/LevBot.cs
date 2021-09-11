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
            this.parseTheaters();
            this.ParseAllMovies();
            this.ParseScreening();
            // this.printResults();
            this.closeBrowser();
        }
        void ParseScreening()
        {
            for (int i = 0; i < TheatersUrlList.Count; i++)
            {
                goToUrl(TheatersUrlList[i]);
                var elem1 = FindElementsByDriver(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/aside/div[1]/div/div[2]/div"));
                var theaters2 = FindElementsByFather(By.XPath(".//*"), elem1.ToList()[0]);
                var Screenings = FindElementsByFather(By.CssSelector("tr"), theaters2.ToList()[0]);
                for (int j = 0; j < Screenings.Count; j++)
                {
                    try
                    {
                        //AddScreeningToList(Screenings.ToList()[j], TheatersList[i]);
                    }
                    catch (Exception e)
                    {
                        logger.Error("Failed to parse a single screening");
                        logger.Error(e);
                        saveDebugData();
                    }
                }

            }
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
            for (int i = 0; i < TheatersUrlList.Count; i++)
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
                        while (theaterForSubs[j] != '.')
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
            // / html / body / div / div[2] / div[2] / div / div[1] / div / section / article / section / div / div / div[1] / ul
            var elem = driver.FindElements(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/article/section/div/div/div[1]/ul"));
            //  var movieList2 = FindElementsByFather(By.XPath(".//*"), elem.ToList()[0]);
            var movieList = elem.ToList()[0].FindElements(By.CssSelector("li"));
            for (int i = 0; i < movieList.Count; i++)
            {
                try
                {
                    Movie movie = new Movie();
                    var elem2 = FindElementsByFather(By.XPath(".//*"), movieList.ToList()[i]);
                    var movieToParse = FindElementsByFather(By.XPath(".//*"), elem2.ToList()[0]);

                    AddMovieToMovieList(movie, movieToParse.ToList()[0]);
                }
                catch (Exception e)
                {
                    logger.Error("Failed to parse a single movie");
                    logger.Error(e);
                    saveDebugData();
                }

            }
            for (int i = 0; i < MoviesList.Count; i++)
            {
                parseMovieMetadata(MoviesList[i], moviesUrlList[i]);
                // _movieezApiUtils.PostMovie(MoviesList[i]);
                logger.Debug(MoviesList[i].ToString());
            }
            var vz = elem.ToList()[0].FindElements(By.ClassName("item"));
            //for(int i=0;i< movieList)
            //var 

        }
        void parseMovieMetadata(Movie movie, string urlOfMovie)
        {
            goToUrl(urlOfMovie);
            AddNameOfMovie(movie);
            AddPlotOfMovie(movie);
            AddEnglishName(movie);
            AddDurationOfMovie(movie);
            AddMovieGenre(movie);
            //AddMovieDirectorAndCast(movie);
            AddMovieTrailer(movie);
            //AddMovieRating(movie);
            //AddMovieReleaseDate(movie);
            AddMoviePicture(movie);
            movie.OriginalLanguage = null;
        }
        //void AddMovieDirectorAndCast(Movie movie)
        //{
        //    var directorVar = FindElementsByDriver(By.ClassName("movie_casts"));

        //    string director = directorVar.ToList()[0].GetAttribute("innerText").ToString().Trim();

        //    // director= director.Substring()
        //    string[] subs = director.Split(':');
        //    movie.Director = subs[1].Trim();
        //    var castVar = driver.FindElements(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/div[1]/div[2]/div[2]/div[3]"));
        //    var elem2 = FindElementsByFather(By.XPath(".//*"), directorVar.ToList()[0]);
        //    var cast2 = elem2.ToList()[0].GetAttribute("<br>");

        //  //  string cast = cast2.ToList()[0].GetAttribute("innerText").ToString().Trim();
        //    // director= director.Substring()
        //    string[] subs2 = director.Split(':');
        //    movie.Cast = subs2[1].Trim();
        //    // movie.EnglishName = englishNameString;
        //}
        void AddMoviePicture(Movie movie)
        {
            var moviePic = FindElementsByDriver(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/div[1]/div[1]/img"));
            string movieImgString = moviePic.ToList()[0].GetAttribute("src");
            if (movieImgString != "https://www.lev.co.il/wp-content/themes/lev/images/dmovie.jpg")
            {
                movie.MainImage = movieImgString;
                movie.PosterImage = movieImgString;
            }
            else
            {
                movie.MainImage = null;
                movie.PosterImage = null;
            }
        }
        void AddMovieTrailer(Movie movie)
        {
            var MovieTrailer = FindElementsByDriver(By.XPath("/html/body/div/div/a"));
            if (MovieTrailer.Count != 0)
            {
                string MovieTrailerString = MovieTrailer.ToList()[0].GetAttribute("href");
                movie.TrailerUrl = MovieTrailerString;
            }
        }
        void AddDurationOfMovie(Movie movie)
        {
            var durationOfMovie = FindElementsByDriver(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/div[1]/div[2]/div[2]/div[1]/div[3]"));
            string durationOfMovieString = durationOfMovie.ToList()[0].GetAttribute("innerHTML").ToString();
            List<char> tmp = new List<char>();
            for (int i = 0; i < durationOfMovieString.Length; i++)
            { //delete all letters
                if (durationOfMovieString[i] <= '9' && durationOfMovieString[i] >= '0')
                    tmp.Add(durationOfMovieString[i]);
            }
            for (int i = 0; i < tmp.Count; i++)
                movie.Duration += tmp[i];
        }
        void AddMovieGenre(Movie movie)
        {
            movie.Genre = parseMovieGenre(movie.Plot);
        }

        void AddEnglishName(Movie movie)
        {
            var englishName = FindElementsByDriver(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/div[1]/div[2]/div[1]/h2"));
            string englishNameString = englishName.ToList()[0].GetAttribute("innerHTML").ToString();
            movie.EnglishName = englishNameString;
        }
        void AddNameOfMovie(Movie movie)
        {
            var nameOfMovie = FindElementsByDriver(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/div[1]/div[2]/div[1]/h1"));
            string nameString = nameOfMovie.ToList()[0].GetAttribute("innerHTML").ToString();
            movie.Name = nameString;
        }
        void AddPlotOfMovie(Movie movie)
        {
            try
            {
                var plot = FindElementsByDriver(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/div[3]/p[1]/span[1]"));
                string plotString = plot.ToList()[0].GetAttribute("innerHTML").ToString().Trim();
                movie.Plot = plotString;
            }
            catch
            {
                string catchstring = "";
                catchstring += movie.Name;
            }
        }

        void AddMovieToMovieList(Movie movie, IWebElement movieToParse)
        {
            string urlOfMovie = movieToParse.GetAttribute("href").ToString();
            movie.Urls.Add(Name, urlOfMovie);
            MoviesList.Add(movie);
            moviesUrlList.Add(urlOfMovie);
        }




        // var theathersList = FindElementsByFather(By.CssSelector("li"), elem.ToList()[0]);
        //  for (int i = 0; i < theathersList.Count; i++)

    }

}
using Movieez.Bots;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Movieez
{
    public class HotCinema : Bot
    {
        string Name = "HotCinema";
        string MainUrl = "https://hotcinema.co.il/";
        public List<Movie> MoviesList;
        public List<Theater> TheatersList;
        public List<Showtime> ScreeningsList;
        public List<string> moviesUrlList;
		public List<string> TheatersUrlList;
        // Logger
        public static new NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public HotCinema()
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
            this.printResults();
            this.closeBrowser();
        }

        public void printResults()
        {
            logger.Info($"Total results: movies={MoviesList.Count} screenings={ScreeningsList.Count}");
        }

        void ParseAllMovies()
        {
            try
            {
                var elem2 = FindElementsByDriver(By.XPath("/html/body/div[2]/div[1]/div/div/div[1]/div[3]/div[1]/div[2]/div[2]"));
                var movies = FindElementsByFather(By.XPath(".//*"), elem2.ToList()[0]);
                for (int i = 0; i < movies.Count; i++)
                {
                    try
                    {
                        Movie movie = new Movie();
                        AddMovieToMovieList(movie, movies.ToList()[i]);
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
                    _movieezApiUtils.PostMovie(MoviesList[i]);
                    logger.Debug(MoviesList[i].ToString());
                }
            }
            catch (Exception e)
            {
                logger.Info("Failed to parse Movies");
                logger.Info(e);
                saveDebugData();
            }
        }

        void AddMovieToMovieList(Movie movieToAdd, IWebElement movieToParse)
        {
            string urlOfMovie = movieToParse.GetAttribute("href").ToString();
            movieToAdd.Urls.Add(Name, urlOfMovie);
            MoviesList.Add(movieToAdd);
            moviesUrlList.Add(urlOfMovie);
        }

        void ParseScreening()
        {
            try
            {
                for (int i = 0; i < TheatersList.Count; i++)
                {
                    goToUrl(TheatersUrlList[i]);
                    var elem1 = FindElementsByDriver(By.XPath("/html/body/div[2]/div[4]/div[2]/div/div[2]/div/div/div/div/table/tbody"));
                    var Screenings = FindElementsByFather(By.CssSelector("tr"), elem1.ToList()[0]);
                    for (int j = 0; j < Screenings.Count; j++)
                    {
                        try
                        {
                            AddScreeningToList(Screenings.ToList()[j], TheatersList[i]);
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
            catch (Exception e)
            {
                logger.Error("Failed to parse all screening");
                logger.Error(e);
                saveDebugData();
            }
        }

        void AddScreeningToList(IWebElement Screenings, Theater theater)
        {
            var elem = FindElementsByFather(By.CssSelector("td"), Screenings);
            string nameOfMovieToday = elem.ToList()[0].GetAttribute("innerHTML").ToString().Trim();
            string language = elem.ToList()[1].GetAttribute("innerHTML").ToString().Trim();
            if (language.IndexOf("מדובב") != -1)
                language = "עברית";
            else
                language = "שפת המקור עם כתוביות";
            string type = elem.ToList()[2].GetAttribute("innerHTML").ToString().Trim();
            if (type == "<!---->")
                type = "2D";
            for (int k = 0; k < MoviesList.Count; k++)
            {
                if (nameOfMovieToday == MoviesList[k].Name)
                {
                    var times = FindElementsByFather(By.ClassName("dates"), Screenings);
                    var newTimes = FindElementsByFather(By.XPath(".//*"), times.ToList()[0]);
                    for (int z = 0; z < newTimes.Count; z++)
                    {
                        string timeOfMovie = newTimes.ToList()[z].GetAttribute("innerHTML").ToString().Trim();
                        Theater theaterForScreening = new Theater(Name, theater.Address, theater.Name);
                        DateTime time = parseMovieTime(timeOfMovie);
                        Showtime screening = new Showtime(MoviesList[k], time, theaterForScreening, moviesUrlList[k], type, language);
                        ScreeningsList.Add(screening);
                        var movieFromApi = _movieezApiUtils.GetMovie(MoviesList[k]).Result;
                        List<Movieez.API.Model.Models.ShowTime> showTimesFromApi = null;
                        if (movieFromApi != null)
                        {
                            showTimesFromApi = _movieezApiUtils.GetShowTimesByMovieId(movieFromApi.ID).Result;
                        }

                        if (movieFromApi != null)
                        {
                            var showTimeExists = showTimesFromApi.Any(st =>
                            st.Day == screening.Time.ToString("dd/MM/yyyy") &&
                            st.Time == screening.Time.ToString("hh:mm"));
                            if (!showTimeExists)
                            {
                                _movieezApiUtils.PostShowTime(screening, movieFromApi.ID);
                            }
                        }

                    }
                    break;
                }
            }
        }

        void parseMovieMetadata(Movie movie, string urlOfMovie)
        {
            goToUrl(urlOfMovie);
            AddPlotOfMovie(movie);
            AddNameOfMovie(movie);
            AddEnglishName(movie);
            AddDurationOfMovie(movie);
            AddMovieGenre(movie);
            AddMovieDirectorAndCast(movie);
            AddMovieTrailer(movie);
            AddMovieRating(movie);
            AddMovieReleaseDate(movie);
            //AddMoviePicture(movie);
        }
        //void AddMoviePicture(Movie movie)
        //{
        //    var MovieTrailer = driver.FindElements(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[1]/img"));
        //    var theater = MovieTrailer[0].FindElement(By.XPath(".//*"));
        //     movie.TrailerUrl = MovieTrailerString;
        //}
        void AddMovieRating(Movie movie)
        {
            var movieRating = FindElementsByDriver(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[3]/div[2]/div[2]/span"));
            string movieRatingString = movieRating.ToList()[0].GetAttribute("innerHTML").ToString();
            movie.Rating = parseMovieRating(movieRatingString);
        }

        void AddMovieReleaseDate(Movie movie)
        {
            var ReleaseDateOfMovie = FindElementsByDriver(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[3]/div[2]/div[1]/span"));
            string ReleaseDateOfMovieString = ReleaseDateOfMovie.ToList()[0].GetAttribute("innerHTML").ToString();
            movie.ReleaseDate = parseMovieReleaseDate(ReleaseDateOfMovieString);
        }

        void AddMovieTrailer(Movie movie)
        {                                                 
            var MovieTrailer = FindElementsByDriver(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[2]/div[2]/iframe"));
            string MovieTrailerString = MovieTrailer.ToList()[0].GetAttribute("src");
            movie.TrailerUrl = MovieTrailerString;
        }

        void AddMovieDirectorAndCast(Movie movie)
        {
            movie.Director = null;
            movie.Cast = null;
           var movieDirectorAndCast = FindElementsByDriver(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[2]"));
            string movieDirectorAndCastString = movieDirectorAndCast.ToList()[0].GetAttribute("innerHTML").ToString();
            string[] subs = movieDirectorAndCastString.Split(' ');
            int k = 0;
            for (int i = 0; i < subs.Length; i++)
            {
                if (subs[i] == "במאי:")
                {
                    k = i+1;
                    while(subs[k]!="")
                    {
                        movie.Director += subs[k];
                        movie.Director += " ";
                        k ++;
                    }
                    if(movie.Director!=null)
                    movie.Director = movie.Director.Trim();
                    i = k;
                }

                if (subs[i] == "שחקנים:")
                {
                    k = i + 1;
                    while (subs[k] != "")
                    {
                        movie.Cast += subs[k];
                        movie.Cast += " ";
                        k++;
                    }
                    if (movie.Cast != null)
                        movie.Cast = movie.Cast.Trim();
                    break;
                }

            }
        }

        void AddDurationOfMovie(Movie movie)
        {
            var durationOfMovie = FindElementsByDriver(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[3]/div[1]/div[2]/span"));
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
            var movieGenre = FindElementsByDriver(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[3]/div[1]/div[1]/span"));
            string movieGenreString = movieGenre.ToList()[0].GetAttribute("innerHTML").ToString().Trim();
            movie.Genre = parseMovieGenre(movieGenreString);
        }

        void AddEnglishName(Movie movie)
        {
            var englishName = FindElementsByDriver(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[1]/div[2]/h2"));
            string englishNameString = englishName.ToList()[0].GetAttribute("innerHTML").ToString();
            movie.EnglishName = englishNameString;
        }

        void AddNameOfMovie(Movie movie)
        {
            var nameOfMovie = FindElementsByDriver(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[1]/div[1]/h1"));
            string nameString = nameOfMovie.ToList()[0].GetAttribute("innerHTML").ToString();
            movie.Name = fixMovieName(nameString);
        }

        void AddPlotOfMovie(Movie movie)
        {
            var plot = FindElementsByDriver(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[1]"));
            string plotString = plot.ToList()[0].GetAttribute("innerHTML").ToString().Trim();
            movie.Plot = plotString;
        }

        void parseTheaters()
        {
            try
            {
                var elem2 = FindElementsByDriver(By.XPath("/html/body/div[2]/div[1]/div/div/div[1]/div[3]/div[2]/div[2]"));
                var theater = FindElementsByFather(By.XPath(".//*"), elem2.ToList()[0]);
                for (int i = 0; i < theater.Count; i++)
                {
                    try
                    {
                        string theaterPageLink = theater.ToList()[i].GetAttribute("href");
                        TheatersUrlList.Add(theaterPageLink);
                    }
                    catch (Exception e)
                    {
                        logger.Error("Failed to parse a single Theater");
                        logger.Error(e);
                        saveDebugData();
                    }
                }
                for (int i = 0; i < theater.Count; i++)
                {
                    try
                    {
                        goToUrl(TheatersUrlList[i]);
                        var theatherName = driver.FindElements(By.XPath("/html/body/div[2]/div[4]/div[1]/div/div/div/div[2]/h1"));
                        string nameOFtheaterName = theatherName[0].GetAttribute("innerHTML").ToString().Trim();

                        var elem = FindElementsByDriver(By.XPath("/html/body/div[2]/div[4]/div[2]/div/div[4]/div[2]/div"));
                        string theatherAdress = elem.ToList()[0].GetAttribute("innerText").ToString().Trim();
                        string[] subs = theatherAdress.Split("\r\n");
                        theatherAdress = subs[1];
                        string[] subs2 = nameOFtheaterName.Split(" ");
                        theatherAdress += " ,";
                        theatherAdress += subs2[2];
                        TheatersList.Add(new Theater(nameOFtheaterName, theatherAdress));
                    }
                    catch (Exception e)
                    {
                        logger.Error("Failed to parse a single Theater");
                        logger.Error(e);
                        saveDebugData();
                    }

                }
            }
            catch (Exception e)
            {
                logger.Error("Failed to parse Theaters");
                logger.Error(e);
                saveDebugData();
            }
            goToUrl(MainUrl);
        }

        DateTime parseMovieReleaseDate(string date)
        {
            date = date.Remove(0, " :בכורה".Length);
            DateTime oDate = DateTime.ParseExact(date, "dd/MM/yyyy", null);
            return oDate;
        }

        DateTime parseMovieTime(string Time)
        {
            DateTime oDate = DateTime.Parse(Time);
            return oDate;
        }

            //var movieFromApi = _movieezApiUtils.GetMovie(movie.Name).Result;
            //List<Movieez.API.Model.Models.ShowTime> showTimesFromApi = null;
            //if (movieFromApi != null)
            //{
            //    showTimesFromApi = _movieezApiUtils.GetShowTimesByMovieId(movieFromApi.ID).Result;
            //}
        

    }
}

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
            _movieezApiUtils = new MovieezApiUtils(MovieezApiUtils.e_Theaters.Lev);
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
        void ParseScreening()
        {
            try
            {
                for (int i = 0; i < TheatersUrlList.Count; i++)
                {
                    goToUrl(TheatersUrlList[i]);
                    var elem1 = FindElementsByDriver(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/aside/div[1]/div/div[2]/div/ul"));
                    var Screenings = FindElementsByFather(By.CssSelector("li"), elem1.ToList()[0]);
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
        void parseTheaters()
        {//
            goToUrl(theatersUrl);
            var elem = driver.FindElements(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/article/section/div/p[8]"));
            var theaters = FindElementsByFather(By.XPath(".//*"), elem.ToList()[0]);
            for (int i = 0; i < theaters.Count; i += 2)
            {
                string nv = theaters.ToList()[i].GetAttribute("href");
                TheatersUrlList.Add(nv);

            }
            var elem2 = driver.FindElements(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/article/section/div/p[9]"));
            var theaters2 = FindElementsByFather(By.XPath(".//*"), elem2.ToList()[0]);
            for (int i = 0; i < theaters2.Count; i += 2)
            {
                string nv = theaters2.ToList()[i].GetAttribute("href");
                TheatersUrlList.Add(nv);
            }
            for (int i = 0; i < TheatersUrlList.Count; i++)
            {
                //string adressOfTheater = null;
                goToUrl(TheatersUrlList[i]);
                // var elem7 = FindElementsByDriver(By.ClassName("exdetail"));
                //string nameOfTheater7 = elem7.ToList()[0].GetAttribute("innerText").ToString().Trim();

                var elem3 = driver.FindElements(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div"));
                var theatername = FindElementsByFather(By.XPath(".//*"), elem3.ToList()[0]);
                string nameOfTheater = theatername.ToList()[0].GetAttribute("innerText").ToString().Trim();
                var elem4 = FindElementsByDriver(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/div[4]/div/div/div/div[1]/div/div/a"));
                if (elem4 == null)
                {
                    elem4 = FindElementsByDriver(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/div[6]/div/div/div/div[1]/div/div/a"));
                    if (elem4 == null)
                    {
                        elem4 = FindElementsByDriver(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/div[3]/div/div/div/div[1]/div/div/a/span[2]"));
                        if (elem4 == null)
                        {
                            elem4 = FindElementsByDriver(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/div[5]/div/div/div/div[1]/div/div/a/span[2]"));

                        }
                    }

                }
                string adressOfTheater = elem4.ToList()[0].GetAttribute("innerText").ToString().Trim();
                if (adressOfTheater.IndexOf("אל") != -1)
                {
                    adressOfTheater = adressOfTheater.Substring(adressOfTheater.IndexOf("אל") + 2).Trim();
                }
                Theater theater = new Theater(nameOfTheater, adressOfTheater.Trim());

                TheatersList.Add(theater);
                
            }

        }
        void ParseAllMovies()
        {
            try
            {
                goToUrl(moviesUrl);
                var elem = driver.FindElements(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/article/section/div/div/div[1]/ul"));
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
                    try
                    {
                        parseMovieMetadata(MoviesList[i], moviesUrlList[i]);
                        _movieezApiUtils.PostMovie(MoviesList[i]);
                        logger.Debug(MoviesList[i].ToString());
                    }
                    catch (Exception e)
                    {
                        logger.Error("Failed to parse a single movie");
                        logger.Error(e);
                        saveDebugData();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Info("Failed to parse Movies");
                logger.Info(e);
                saveDebugData();
            }
        }



        void parseMovieMetadata(Movie movie, string urlOfMovie)
        {
            goToUrl(urlOfMovie);
            AddNameOfMovie(movie);
            if (movie.Name.IndexOf("פסטיבל ירושלים") == -1)
            {
                parseMovieMetadataOfNotPastival(movie);
            }
            else
            {
                parseMovieMetadataOfPastival(movie);

            }
        }
        void parseMovieMetadataOfPastival(Movie movie)
        {
            AddPlotOfMovie(movie);
            AddEnglishName(movie);
            AddDurationOfMovie(movie);
            AddMovieGenre(movie);
            AddMovieDirectorAndCast(movie);
            AddMovieTrailer(movie);
            AddMovieRating(movie);
            AddMovieReleaseDate(movie);
            AddMoviePicture(movie);
            AddMovieOriginalLanguage(movie);
        }
        void AddMovieReleaseDate(Movie movie)
        {
            string tmp = "";
            var movieRelease = FindElementsByDriver(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/div[1]/div[2]/div[2]/div[1]/div[1]"));
            string movieReleaseString = movieRelease.ToList()[0].GetAttribute("innerText");
            for (int i = 0; i < movieReleaseString.Length; i++)
            { //delete all letters
                if (movieReleaseString[i] <= '9' && movieReleaseString[i] >= '0')
                    tmp += movieReleaseString[i];
            }
            //for (int i = 0; i < tmp.Count; i++)
            DateTime oDate = DateTime.ParseExact(tmp, "dd/MM/yyyy", null);

            movie.ReleaseDate = oDate;

        }
        void AddMovieOriginalLanguage(Movie movie)
        {
            var OriginalLanguageVar = FindElementsByDriver(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/div[1]/div[2]/div[2]/div[1]/div[2]"));
            string OriginalLanguage = OriginalLanguageVar.ToList()[0].GetAttribute("innerText");
            string[] subs = OriginalLanguage.Split('|');
            movie.OriginalLanguage = subs[0].Trim();
        }
        void AddMovieDirectorAndCast(Movie movie)
        {
            var directorVar = FindElementsByDriver(By.ClassName("movie_casts"));
            if (directorVar == null)
            {
                movie.Director = null;
                movie.Cast = null;
                return;
            }
            string director = directorVar.ToList()[0].GetAttribute("innerText").ToString().Trim();
            string[] subs = director.Split('\n');
            if (subs[0].Length > 6)
                movie.Director = subs[0].Substring(6).Trim();
            else
                movie.Director = null;
            if (subs.Length > 1 && subs[1].Length > 5)
                movie.Cast = subs[1].Substring(5).Trim();
            else
                movie.Cast = null;
        }
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
            var MovieTrailer = FindElementsByDriver(By.XPath("/html/body/div/div[2]/div[2]/div/div[1]/div/section/div[3]/iframe"));
            if (MovieTrailer != null)
            {
                string MovieTrailerString = MovieTrailer.ToList()[0].GetAttribute("src");
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
        void parseMovieMetadataOfNotPastival(Movie movie)
        {
            AddEnglishName(movie);
            AddDurationOfMovie(movie);
            AddMovieRating(movie);
            AddPlotOfMovie2(movie);
            AddMoviePicture(movie);
            AddMovieOriginalLanguage(movie);
            AddMovieDirectorAndCast(movie);
            AddMovieTrailer(movie);
            AddMovieReleaseDate(movie);
            AddMovieGenre(movie);
        }
        void AddMovieToMovieList(Movie movie, IWebElement movieToParse)
        {
            string urlOfMovie = movieToParse.GetAttribute("href").ToString();
            movie.Urls.Add(Name, urlOfMovie);
            MoviesList.Add(movie);
            moviesUrlList.Add(urlOfMovie);
        }
        void AddPlotOfMovie2(Movie movie)
        {
            string plotString2 = "";
            var directorVar = FindElementsByDriver(By.ClassName("movie_content"));
            for (int i = 0; i < directorVar.Count; i++)
            {
                plotString2 += directorVar.ToList()[i].GetAttribute("innerText").ToString().Trim();
            }
            movie.Plot = plotString2.Substring(6).Trim();
        }
        void AddMovieRating(Movie movie)
        {
            var movieRating = FindElementsByDriver(By.ClassName("movie_age"));
            if (movieRating == null)
                movie.Rating = Rating.Unknown;
            string movieRatingString = movieRating.ToList()[0].GetAttribute("innerHTML").ToString();
            movie.Rating = parseMovieRating(movieRatingString);
        }
        void AddScreeningToList(IWebElement Screenings, Theater theater)
        {
            bool isDub = false;
            string movienameandtime = Screenings.GetAttribute("innerText").ToString().Trim();
            string[] subs = movienameandtime.Split("\n");
            string movieName = subs[0].Trim();
            if (movieName.IndexOf("מדובב") != -1)
            {
                movieName = movieName.Substring(0, movieName.IndexOf("מדובב")).Trim();
                isDub = true;
            }
            string language;
            string movieTime = subs[1];
            DateTime time = parseMovieTime(movieTime);
            for (int i = 0; i < MoviesList.Count; i++)
            {
                if (movieName == MoviesList[i].Name)
                {
                    if (isDub)
                        language = "מדובב לעברית";
                    else
                        language = MoviesList[i].OriginalLanguage;
                    Showtime screening = new Showtime(MoviesList[i], time, theater, moviesUrlList[i], "2D", language);
                    ScreeningsList.Add(screening);
                    var movieFromApi = _movieezApiUtils.GetMovie(MoviesList[i]).Result;
                    List<Movieez.API.Model.Models.ShowTime> showTimesFromApi = null;
                    if (movieFromApi != null)
                    {
                        showTimesFromApi = _movieezApiUtils.GetShowTimesByMovieId(movieFromApi.ID).Result;
                    }

                    if (movieFromApi != null)
                    {
                        var showTimeExists = showTimesFromApi.Any(st =>
                        st.Day == screening.Time.ToString("dd/MM/yyyy") &&
                        st.Time == screening.Time.ToString("HH:mm"));
                        if (!showTimeExists)
                        {
                            _movieezApiUtils.PostShowTime(screening, movieFromApi.ID);
                        }
                    }
                    return;
                }
            }
        }
        DateTime parseMovieTime(string Time)
        {
            DateTime oDate = DateTime.Parse(Time);
            return oDate;
        }
    }

}






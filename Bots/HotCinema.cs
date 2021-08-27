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
        public List<Screening> ScreeningsList;
        public List<string> moviesUrlList;
        public HotCinema()
        {
            initDriver(MainUrl);
            MoviesList = new List<Movie>();
            TheatersList = new List<Theater>();
            ScreeningsList = new List<Screening>();
            moviesUrlList = new List<string>();
            _movieezApiUtils = new MovieezApiUtils(MovieezApiUtils.e_Theaters.HotCinema);

        }
        public void run()
        {
            this.parseTheaters();
            this.ParseAllMovies();
            this.ParseScreening();
        }

        void ParseAllMovies()
        {
            try
            {
                var elem2 = driver.FindElements(By.XPath("/html/body/div[2]/div[1]/div/div/div[1]/div[3]/div[1]/div[2]/div[2]"));
                var movies = elem2[0].FindElements(By.XPath(".//*"));
                for (int i = 0; i < movies.Count; i++)
                {
                    try
                    {
                        Movie movie = new Movie();
                        AddMovieToMovieList(movie, movies[i]);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failed to parse a single movie");
                    }
                }
                for (int i = 0; i < MoviesList.Count; i++)
                {
                    parseMovieMetadata(MoviesList[i], moviesUrlList[i]);
                    _movieezApiUtils.PostMovie(MoviesList[i]);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to parse Movies");
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
                    goToUrl(TheatersList[i].Address);
                    var elem1 = driver.FindElements(By.XPath("/html/body/div[2]/div[4]/div[2]/div/div[2]/div/div/div/div/table/tbody"));
                    var Screenings = elem1[0].FindElements(By.CssSelector("tr"));
                    for (int j = 0; j < Screenings.Count; j++)
                    {
                        try
                        {
                            AddScreeningToList(Screenings[j], TheatersList[i]);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Failed to parse a single screening");
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to parse all screening");
            }
        }

        void AddScreeningToList(IWebElement Screenings, Theater theater)
        {
            var elem = Screenings.FindElements(By.CssSelector("td"));
            string nameOfMovieToday = elem[0].GetAttribute("innerHTML").ToString().Trim();
            for (int k = 0; k < MoviesList.Count; k++)
            {
                if (nameOfMovieToday == MoviesList[k].Name)
                {
                    var times = Screenings.FindElements(By.ClassName("dates"));
                    times = times[0].FindElements(By.XPath(".//*"));
                    for (int z = 0; z < times.Count; z++)
                    {
                        string timeOfMovie = times[z].GetAttribute("innerHTML").ToString().Trim();
                        Theater theaterForScreening = new Theater(theater.Name, theater.Address);
                        DateTime time = parseMovieTime(timeOfMovie);
                        Screening screening = new Screening(MoviesList[k], time, theaterForScreening);
                        ScreeningsList.Add(screening);
                        // post showtime
                        /*var movieFromApi = _movieezApiUtils.GetMovie(MoviesList[k].Name).Result;
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
                        }*/
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
        }

        void AddMovieRating(Movie movie)
        {
            var movieRating = driver.FindElements(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[3]/div[2]/div[2]/span"));
            string movieRatingString = movieRating[0].GetAttribute("innerHTML").ToString();
            movie.Rating = parseMovieRating(movieRatingString);
        }

        void AddMovieReleaseDate(Movie movie)
        {
            var ReleaseDateOfMovie = driver.FindElements(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[3]/div[2]/div[1]/span"));
            string ReleaseDateOfMovieString = ReleaseDateOfMovie[0].GetAttribute("innerHTML").ToString();
            movie.ReleaseDate = parseMovieReleaseDate(ReleaseDateOfMovieString);
        }

        void AddMovieTrailer(Movie movie)
        {
            var MovieTrailer = driver.FindElements(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[2]/div[2]/iframe"));
            string MovieTrailerString = MovieTrailer[0].GetAttribute("src");
            movie.TrailerUrl = MovieTrailerString;
        }

        void AddMovieDirectorAndCast(Movie movie)
        {
            var movieDirectorAndCast = driver.FindElements(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[2]"));
            string movieDirectorAndCastString = movieDirectorAndCast[0].GetAttribute("innerHTML").ToString();
            string[] subs = movieDirectorAndCastString.Split(' ');
            movie.Director += subs[1];
            movie.Director += " ";
            movie.Director += subs[2];
            for (int i = 7; i < subs.Length - 5; i++)
            { // add cast
                movie.Cast += subs[i];
                movie.Cast += " ";
            }
        }

        void AddDurationOfMovie(Movie movie)
        {
            var durationOfMovie = driver.FindElements(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[3]/div[1]/div[2]/span"));
            string durationOfMovieString = durationOfMovie[0].GetAttribute("innerHTML").ToString();
            List<char> tmp = new List<char>();
            for (int i = 0; i < durationOfMovieString.Length; i++)
            { //delete all letters
                if (durationOfMovieString[i] <= '9' && durationOfMovieString[i] >= '1')
                    tmp.Add(durationOfMovieString[i]);
            }
            for (int i = 0; i < tmp.Count; i++)
                movie.Duration += tmp[i];
        }

        void AddMovieGenre(Movie movie)
        {
            var movieGenre = driver.FindElements(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[3]/div[1]/div[1]/span"));
            string movieGenreString = movieGenre[0].GetAttribute("innerHTML").ToString();
            movie.Genre = movieGenreString;
        }

        void AddEnglishName(Movie movie)
        {
            var englishName = driver.FindElements(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[1]/div[2]/h2"));
            string englishNameString = englishName[0].GetAttribute("innerHTML").ToString();
            movie.EnglishName = englishNameString;
        }

        void AddNameOfMovie(Movie movie)
        {
            var nameOfMovie = driver.FindElements(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[1]/div[1]/h1"));
            string nameString = nameOfMovie[0].GetAttribute("innerHTML").ToString();
            movie.Name = nameString;
        }

        void AddPlotOfMovie(Movie movie)
        {
            var plot = driver.FindElements(By.XPath("/html/body/div[2]/div[4]/div[2]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[1]"));
            string plotString = plot[0].GetAttribute("innerHTML").ToString();
            movie.Plot = plotString;
        }

        void parseTheaters()
        {
            try
            {
                var elem2 = driver.FindElements(By.XPath("/html/body/div[2]/div[1]/div/div/div[1]/div[3]/div[2]/div[2]"));
                var theater = elem2[0].FindElements(By.XPath(".//*"));
                for (int i = 0; i < theater.Count; i++)
                {
                    try
                    {
                        string nameOFtheaterName = theater[i].GetAttribute("innerHTML").ToString();
                        string theaterPageLink = theater[i].GetAttribute("href");
                        TheatersList.Add(new Theater(nameOFtheaterName, theaterPageLink));
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failed to parse a single Theater");
                    }
                }
            }
            catch (Exception)
            {

                Console.WriteLine("Failed to parse Theaters");
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


    }
}

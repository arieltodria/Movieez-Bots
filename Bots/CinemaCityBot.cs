using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System.Text.RegularExpressions;
using Movieez.Resources;
using Movieez.Bots;

namespace Movieez
{
    class CinemaCityBot : Bot
    {
        string Name = "CinemaCity";
        string MainUrl = "https://www.cinema-city.co.il/";
        //string TheatersPageUrl = "https://www.cinema-city.co.il/locations";

        // Web elements
        private ReadOnlyCollection<IWebElement> movies; // Movies' web elements list
        // Data obj
        public List<Movie> MoviesList;
        public List<Theater> TheatersList;
        public List<Showtime> ScreeningsList;

        public CinemaCityBot()
        {

            initDriver(MainUrl);
            MoviesList = new List<Movie>();
            TheatersList = new List<Theater>();
            ScreeningsList = new List<Showtime>();
            _movieezApiUtils = new MovieezApiUtils(MovieezApiUtils.e_Theaters.CinemaCity);
        }

        public void run()
        {
            parseAllMovies();
            printResults();
            closeBrowser();
        }

        public void parseAllMovies()
        {
            loadAllMovies();
            initMoviesElements();
            int totalMoviesToParse = movies.Count;
            for(int i = 0; i < totalMoviesToParse; i++)
            {
                initMoviesElements();
                Movie movie = parseMovie(movies.ToList()[i]);
                logger.Debug(movie.ToString());
                this.loadAllMovies();
            }
        }

        void initMoviesElements()
        {
            logger.Debug("Finding movie elements");
            movies = this.driver.FindElements(By.ClassName("flipper"));
        }

        Movie parseMovie(IWebElement movieElement)
        {
            Movie movie = new Movie();

            parseMoviePage(movie, movieElement);
            parseMovieMetadata(movie);
            parseMovieScreenings(movie);

            return movie;
        }

        void parseMovieScreenings(Movie movie)
        {
            // Init search boxes
            IWebElement theater_search_box = initTheatersSearchBox();
            IWebElement date_search_box = initDateSearchBox();
            IWebElement time_search_box = initTimeSearchBox();
            // Init search boxes lists
            IWebElement theater_search_box_list = initTheatersSearchBoxList();
            IWebElement date_box_search_list = initDateSearchBoxList();
            IWebElement time_search_box_list = initTimeSearchBoxList();

            IReadOnlyCollection<IWebElement> theather_names = initTheatersNames();
            IReadOnlyCollection<IWebElement> screeningTimes;
            IWebElement date;

            int theatersCount = theather_names.Count;
            var movieFromApi = _movieezApiUtils.GetMovie(movie).Result;
            List<Movieez.API.Model.Models.ShowTime> showTimesFromApi = null;
            if (movieFromApi != null)
            {
                showTimesFromApi = _movieezApiUtils.GetShowTimesByMovieId(movieFromApi.ID).Result;
            }

            // Run on all theaters
            for (int i = 0; i < theatersCount; i++)
            {
                IWebElement theater;
                try
                {
                    theater = theather_names.ToList()[i];
                }
                catch(Exception e)
                {
                    logger.Error("Failed to parse theaters");
                    logger.Error(e);
                    saveDebugData();
                    break;
                }
                closeChatPopUp();
                Click(theater_search_box, true, true);
                try
                {
                    Click(theater, false, true);
                }
                catch (Exception e)
                {
                    logger.Error("Failed to click on theater");
                    logger.Error(e);
                    saveDebugData();
                }
                // Running only on 1st day's screenings
                closeChatPopUp();
                Click(date_search_box, true, true);
                try
                {
                    date = getDateElement();
                    Click(date, false, true);
                }
                catch (Exception e)
                {
                    logger.Error("Failed to click on date");
                    logger.Error(e);
                    saveDebugData();
                }

                screeningTimes = initScreeningTimes();
                try
                {
                    foreach (IWebElement time in screeningTimes)
                    {
                        closeChatPopUp();
                        Click(time_search_box, true, true);
                        try
                        {
                            Showtime screening = parseScreeningMetadata(movie, theater_search_box, date_search_box, time);
                            ScreeningsList.Add(screening);
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
                        catch (Exception e)
                        {
                            logger.Error("Failed to click on time");
                            logger.Error(e);
                            saveDebugData();
                        }
                    }
                    theather_names = initTheatersNames();
                }
                catch(Exception e)
                {
                    logger.Error("Failed to click on screening time");
                    logger.Error(e);
                    saveDebugData();
                }
            }
            goToUrl(MainUrl); // Back to main
        }

        
        IReadOnlyCollection<IWebElement> initSearchBoxes()
        {
            logger.Debug("Finding search boxes");
            return FindElementsByDriver(By.CssSelector(CinemaCity_QueryStrings.searchBoxes));
        }

        IWebElement initTheatersSearchBox()
        {
            return initSearchBoxes().ToList()[3];
        }
        IWebElement initDateSearchBox()
        {
            return initSearchBoxes().ToList()[4];
        }
        IWebElement initTimeSearchBox()
        {
            return initSearchBoxes().ToList()[5];
        }

        IReadOnlyCollection<IWebElement> initSearchBoxesLists()
        {
            logger.Debug("Finding search boxes lists");
            return FindElementsByDriver(By.CssSelector(CinemaCity_QueryStrings.boxList));
        }

        IWebElement initTheatersSearchBoxList()
        {
            return initSearchBoxesLists().ToList()[0];
        }
        
        IWebElement initDateSearchBoxList()
        {
            return initSearchBoxesLists().ToList()[1];
        }
        
        IWebElement initTimeSearchBoxList()
        {
            return initSearchBoxesLists().ToList()[2];
        }

        IReadOnlyCollection<IWebElement> initTheatersNames()
        {
            logger.Debug("Finding theater names");
            return FindElementsByFather(By.CssSelector(CinemaCity_QueryStrings.theaterSearchBoxList), initTheatersSearchBoxList());
        }

        IWebElement getDateElement()
        {
            logger.Debug("Finding day element of 1st day");
            // returns 1st day from days list
            return FindElementsByFather(By.CssSelector(CinemaCity_QueryStrings.dateBoxSearchList), initDateSearchBoxList()).ToList()[0];
        }

        IReadOnlyCollection<IWebElement> initScreeningTimes()
        {
            logger.Debug("Finding screening times");
            return FindElementsByFather(By.CssSelector(CinemaCity_QueryStrings.screeningTimes), initTimeSearchBoxList());
        }

        Showtime parseScreeningMetadata(Movie movie, IWebElement theater, IWebElement date, IWebElement time)
        {
            logger.Debug("Parsing screening metadata");
            Showtime screening = new Showtime();
            try
            {
                screening.Movie = movie;
                screening.MovieUrl = movie.Urls[Name];
                screening.Theater = new Theater(parseScreeningLocation(theater.GetAttribute("innerText")), "");
                screening.Time = DateTime.Parse(date.GetAttribute("innerText") + " " + time.GetAttribute("innerText"));
                screening.Type = parseScreeningType(theater.GetAttribute("innerText"));
                logger.Debug("New screening added " + screening.Time);
                return screening;
            }
            catch (Exception e)
            {
                logger.Error(e);
                saveDebugData();
                return null;
            }
        }

        string parseScreeningType(string location)
        {
            var result = Regex.Matches(location, @"\((.*)\)");
            if (result.Count != 0)
                return result[0].Groups[1].ToString();
            return "2D";
        }

        string parseScreeningLocation(string location)
        {
            return Regex.Replace(location, @"\((.*)\)", "");
        }

        // Parse movie's url from main cinema city page, url is taken from poster
        void parseMoviePage(Movie movie, IWebElement movieElement)
        {
            logger.Debug("Finding movie's url");
            goToUrl(movieElement.FindElement(By.CssSelector(CinemaCity_QueryStrings.movieUrl)).GetAttribute("href").ToString());
            movie.Urls.Add(Name, driver.Url);
        }

        // Driver already in movie url when method is called.
        // Recieves Movie obj to extract metadata into
        void parseMovieMetadata(Movie movie)
        {
            int INNER_METADATA_GENRE_INDEX = 0;
            int INNER_METADATA_DURATION_INDEX = 1;
            int INNER_METADATA_RELEASE_DATE_INDEX = 2;
            int INNER_METADATA_RATING_INDEX = 3;

            logger.Debug("Finding info_div element");
            IWebElement info_div = FindElementByDriver(By.CssSelector(CinemaCity_QueryStrings.info_div));
            logger.Debug("Finding movie's plot element");
            movie.Plot = FindElementByFather(By.CssSelector(CinemaCity_QueryStrings.plot), info_div).GetAttribute("innerText");
            logger.Debug("Finding movie's plot element");
            IWebElement info_div_inner = FindElementByFather(By.CssSelector(CinemaCity_QueryStrings.info_div_inner), info_div);
            logger.Debug("Finding inner_metadata element");
            IReadOnlyCollection<IWebElement> inner_metadata = FindElementsByFather(By.CssSelector(CinemaCity_QueryStrings.inner_metadata), info_div_inner);

            movie.Genre += parseMovieGenre(inner_metadata.ToList()[INNER_METADATA_GENRE_INDEX].Text);
            movie.Duration = parseMovieDuration(inner_metadata.ToList()[INNER_METADATA_DURATION_INDEX].Text);
            movie.ReleaseDate = parseMovieReleaseDate(inner_metadata.ToList()[INNER_METADATA_RELEASE_DATE_INDEX].Text);
            movie.Rating = parseMovieRating(inner_metadata.ToList()[INNER_METADATA_RATING_INDEX].Text);
            logger.Debug("Finding movie TrailerUrl element");
            movie.TrailerUrl = FindElementByDriver(By.Id("fullpagevideo")).GetAttribute("src");
            logger.Debug("Finding MainImage element");
            movie.MainImage = FindElementByDriver(By.CssSelector(CinemaCity_QueryStrings.MainImage)).GetAttribute("src");
            movie.PosterImage = movie.MainImage;
            parseMovieName(movie);

            // Movie obj is ready
            MoviesList.Add(movie); // Add parsed movies to movies list
            _movieezApiUtils.PostMovie(movie); // Post movie to movieez API
        }

        // remove "תאריך בכורה" from string
        DateTime parseMovieReleaseDate(string date)
        {
            date = date.Remove(0, "תאריך בכורה".Length);
            return DateTime.Parse(date);
        }

        void parseMovieName(Movie movie)
        {
            logger.Debug("Finding MovieName element");
            string tmpName = driver.FindElement(By.CssSelector(CinemaCity_QueryStrings.tmpMovieName)).GetAttribute("innerText");
            if (tmpName.Contains('/'))
            {
                movie.EnglishName = tmpName.Substring(tmpName.IndexOf('/') + 1);
                movie.Name = fixMovieName(tmpName.Substring(0, tmpName.IndexOf('/')));
            }
            else
            {
                logger.Debug("English name is not found");
                movie.Name = fixMovieName(tmpName);
            }
        }

        void loadAllMovies()
        {
            logger.Debug("Finding load_button element");
            IWebElement load_button = this.driver.FindElement(By.ClassName("loadmoreposts"));
            while(!isLoadingOver())
            {
                Click(load_button, true, false);
            }
        }

        /* returns true if all movies loaded in cinema city's main page */
        bool isLoadingOver()
        {
            logger.Debug("Finding Movies elements");
            IReadOnlyCollection<IWebElement> loadedMovies = FindElementsByDriver(By.CssSelector(CinemaCity_QueryStrings.loadedMovies));
            if (loadedMovies != null)
            {
                IWebElement lastMovie = loadedMovies.Reverse().ToList()[0];
                if (FindElementsByFather(By.CssSelector("div"), lastMovie) != null)
                    return false;
                return true;
            }
            return false;
        }

        /* Close the chat bot popup*/
        void closeChatPopUp()
        {
            logger.Debug("Finding openBotButton element");
            if (FindElementsByDriver(By.CssSelector(CinemaCity_QueryStrings.openBotButton)) != null)
            {
                IWebElement openBotButton = FindElementByDriver(By.CssSelector(CinemaCity_QueryStrings.openBotButton));
                try
                {
                    Click(openBotButton, false, true);
                }
                catch (Exception)
                {
                }
            }
            wait();
            logger.Debug("Finding closeBotButton element");
            IWebElement closeBotButton = FindElementByDriver(By.Id("lblCloseChat")).FindElement(By.CssSelector("span span a"));
            try
            {
                Click(closeBotButton, false, true);
            }
            catch (Exception) {
            }
        }

        public void printResults()
        {
            logger.Info($"Total results: movies={MoviesList.Count} screenings={ScreeningsList.Count}");
        }
    }
}




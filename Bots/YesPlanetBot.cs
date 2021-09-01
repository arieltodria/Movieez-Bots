using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System.Text.RegularExpressions;
using Movieez.Bots;
using Movieez.Resources;

namespace Movieez
{
    class YesPlanetBot : Bot
    {
        int i = 1;
        string Name = "YesPlanet";
        string MainUrl = "https://www.yesplanet.co.il/?lang=iw_IL#/";
        // WebElements
        IReadOnlyCollection<IWebElement> allMoviesElements;
        // Data obj
        public List<Movie> MoviesList;
        public List<Theater> TheatersList;
        public List<Showtime> ScreeningsList;
        // Logger
        public static new NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public YesPlanetBot()
        {
            initDriver(MainUrl);
            MoviesList = new List<Movie>();
            TheatersList = new List<Theater>();
            ScreeningsList = new List<Showtime>();
            _movieezApiUtils = new MovieezApiUtils(MovieezApiUtils.e_Theaters.YesPlanet);
        }

        public void run()
        {
            logger.Debug("Running Yes Planet bot");
            this.parseAllMovies();
            printResults();
            closeBrowser();
        }

        // Print lists of all parsed movies and showtimes
        public void printResults()
        {
            logger.Info($"Total results: movies={MoviesList.Count} screenings={ScreeningsList.Count}");
        }

        // Load all movies in YesPlanet main page
        public void loadAllMovies()
        {
            logger.Debug("Loading all movies in Yes Planet main page");
            int i = 0;
            wait();
            scrollToLoadAllElements();
            IWebElement loadMoreMoviesButton = getLoadMoreMoviesButton();
            while (isLoadMoreMoviesButtonVisible())
            {
                wait();
                try
                {
                    Click(loadMoreMoviesButton, false, false);
                }
                catch (Exception e)
                {
                    logger.Error("Failed to click on loadMoreMoviesButton");
                    logger.Error(e);
                    saveDebugData();
                }
            }
            logger.Debug("Loaded all movies in Yes Planet main page");
        }

        // returns true if load more movies button is enabled
        public bool isLoadMoreMoviesButtonVisible()
        {
            logger.Debug("Checking if LoadMoreMoviesButton is visible");
            IReadOnlyCollection<IWebElement> loadMoreMovieBottons = FindElementsByDriver(By.CssSelector(YesPlanet_QueryStrings.loadMoreMoviesButton), true);
            return (loadMoreMovieBottons.Count == 2);
        }

        // finds and returns load more movies button
        IWebElement getLoadMoreMoviesButton()
        {
            logger.Debug("Getting LoadMoreMoviesButton");
            IReadOnlyCollection<IWebElement> loadMoreMoviesButton = FindElementsByDriver(By.CssSelector(YesPlanet_QueryStrings.loadMoreMoviesButton), true);
            return loadMoreMoviesButton.ToList()[0];
        }

        // Find all movie poster elements in YesPlanet home page
        void initMoviesElementsLists(bool sort = false, bool loadAllMovies = true)
        {
            logger.Debug("Initializing all movies elements from YesPlanet home page");
            allMoviesElements = null;
            if (loadAllMovies)
                this.loadAllMovies();
            allMoviesElements = FindElementsByDriver(By.CssSelector(YesPlanet_QueryStrings.allMoviesElements));
        }

        // Parses all movies in YesPlanet home page
        void parseAllMovies()
        {
            logger.Debug("Parsing all movies");
            int totalMovies;
            List<string> moviesUrls = new List<string>();

            initMoviesElementsLists();
            totalMovies = allMoviesElements.Count;
            logger.Debug($"Total movies to parse: {totalMovies}");
            for (int counter = 0; counter < totalMovies; counter++)
            {
                Movie movie = new Movie();
                try
                {
                    movie.Urls.Add("YesPlanet", parseMovieUrl(allMoviesElements.ToList().ElementAt(counter)));
                    parseMovieMetadata(allMoviesElements.ToList().ElementAt(counter), movie);
                    // init movies list in new loaded home page
                    initMoviesElementsLists();
                }
                catch (Exception e)
                {
                    logger.Error("Failed to click on current movie");
                    logger.Error(e);
                    saveDebugData();
                }
            }
        }

        // Receives movie element of poster link. Goes to movie url and exctracts metadata.
        void parseMovieMetadata(IWebElement movieElement, Movie movie)
        {
            logger.Info("Parsing movie's metadata");
            goToUrl(movie.Urls[Name]);
            closeCookiesPopUp();
            scrollToLoadAllElements();
            movie.Name = fixMovieName(parseMovieName());
            movie.EnglishName = parseEnglishNameFromMovieUrl(movie.Urls[Name]);
            movie.Duration = parseMovieDuraion();
            movie.ReleaseDate = parseMovieReleaseDate();
            if (movie.ReleaseDate.Year < 2020)
                return;
            movie.Plot = parseMoviePlot();
            movie.TrailerUrl = parseMovieTrailerUrl();
            // Parse inner metadata web element
            IReadOnlyCollection<IWebElement> innerMetadataContainer = FindElementsByDriver(By.CssSelector(YesPlanet_QueryStrings.innerMetadataContainer));
            goToElement(innerMetadataContainer.ToList()[0]);
            int english_name_index = 0;
            int genre_index = 1;
            int cast_index = 2;
            int director_index = 3;
            int original_language_index = 5;
            int rating_index = 6;
            movie.EnglishName = innerMetadataContainer.ToList()[english_name_index].GetAttribute("innerText");
            movie.Genre = parseMovieGenre(innerMetadataContainer.ToList()[genre_index].GetAttribute("innerText"));
            movie.Cast = innerMetadataContainer.ToList()[cast_index].GetAttribute("innerText");
            movie.Director = innerMetadataContainer.ToList()[director_index].GetAttribute("innerText");
            movie.OriginalLanguage = innerMetadataContainer.ToList()[original_language_index].GetAttribute("innerText");
            movie.Rating = parseMovieRating(innerMetadataContainer.ToList()[rating_index].GetAttribute("innerText"));
            movie.PosterImage = parseMoviePoster();
            movie.MainImage = movie.PosterImage;

            MoviesList.Add(movie);
            _movieezApiUtils.PostMovie(movie);
            logger.Debug(movie.ToString());
            parseScreenings(movie);
            //Console.WriteLine(MoviesList.Count + "/" + (i++));
            goToUrl(MainUrl);
            closeCookiesPopUp();
        }

        /************************************* Parse Movies' metadata helpers *************************************/
        string parseMovieName()
        {
            logger.Debug("Parsing movie's name");
            IWebElement movieNameElement = FindElementByDriver(By.CssSelector(YesPlanet_QueryStrings.movieNameElement));
            if (movieNameElement != null)
                return movieNameElement.GetAttribute("innerText");
            return "";
        }

        string parseMoviePoster()
        {
            IWebElement posterElment = FindElementByDriver(By.CssSelector(YesPlanet_QueryStrings.PosterImage), true);
            var poster = posterElment.GetAttribute("src");
            if (poster.Contains("film.placeholder.poster.jpg"))
                return null;
            return poster;
        }

        string parseMoviePlot()
        {
            logger.Debug("Parsing movie's plot");
            IWebElement moviePlotElement = FindElementByDriver(By.ClassName(YesPlanet_QueryStrings.moviePlotElement));
            if (moviePlotElement != null)
                return moviePlotElement.GetAttribute("innerText");
            return "";
        }

        string parseMovieTrailerUrl()
        {
            logger.Debug("Parsing movie's trailer url");
            IWebElement movieTrailerElement = FindElementByDriver(By.CssSelector(YesPlanet_QueryStrings.movieTrailerElement));
            if (movieTrailerElement != null)
                return movieTrailerElement.GetAttribute("href");
            return "";
        }
        string parseMovieUrl(IWebElement movieElement)
        {
            logger.Debug("Parsing movie's url");
            string movieUrl = movieElement.GetAttribute("href");
            return movieUrl;
        }

        string parseEnglishNameFromMovieUrl(string url)
        {
            logger.Debug("Parsing movie's english name");
            Regex re = new Regex(@".*\/(.*)\/.*");
            Match m = re.Match(url);
            if (m.Success)
                return m.Value;
            else
            {
                logger.Error("Failed to parse url string: " + driver.Url);
                return "";
            }
        }

        string parseMovieDuraion()
        {
            logger.Debug("Parsing movie's duration");
            IReadOnlyCollection<IWebElement> movieDurationElement = FindElementsByDriver(By.CssSelector(YesPlanet_QueryStrings.movieDurationElement));
            return parseMovieDuration(movieDurationElement.ToList()[1].GetAttribute("innerText").ToString());
        }

        DateTime parseMovieReleaseDate()
        {
            logger.Debug("Parsing movie's release date");
            IReadOnlyCollection<IWebElement> movieReleaseDateElement = FindElementsByDriver(By.CssSelector(YesPlanet_QueryStrings.movieReleaseDateElement));
            string date = movieReleaseDateElement.ToList()[0].GetAttribute("innerText").ToString();
            var dateTime = DateTime.Parse(date);
            return dateTime;
        }

        /************************************* Parse Movies' screenings *************************************/

        void parseScreenings(Movie movie)
        {
            logger.Info($"Parsing movie's {movie.EnglishName} screenings");
            IWebElement theaterFilterBoxElement;
            IWebElement screeningTypeFilterBoxElement;
            IWebElement allScreeningTypesButton;
            IReadOnlyCollection<IWebElement> screeningsFilterElements;
            IReadOnlyCollection<IWebElement> theaterFilterBoxListElements;
            IReadOnlyCollection<IWebElement> daysFilterButtons;
            IReadOnlyCollection<IWebElement> screeningTimeInfoContainer;
            IReadOnlyCollection<IWebElement> screeningTimes;

            var movieFromApi = _movieezApiUtils.GetMovie(movie).Result;
            List<Movieez.API.Model.Models.ShowTime> showTimesFromApi = null;
            if (movieFromApi != null)
            {
                showTimesFromApi = _movieezApiUtils.GetShowTimesByMovieId(movieFromApi.ID).Result;
            }

            screeningsFilterElements = FindElementsByDriver(By.CssSelector(YesPlanet_QueryStrings.screeningsFilterElements), true);
            theaterFilterBoxElement = FindElementByDriver(By.CssSelector(YesPlanet_QueryStrings.theaterFilterBoxElement));
            screeningTypeFilterBoxElement = FindElementByDriver(By.CssSelector(YesPlanet_QueryStrings.screeningTypeFilterBoxElement));

            int teaterCount = 1; // debug
            try
            {
                Click(theaterFilterBoxElement, true, true); // Click on theater filter box
            }
            catch (Exception e)
            {
                logger.Error("Failed to click on theaterFilterBoxElement");
                logger.Error(e);
                saveDebugData();
                return;
            }
            theaterFilterBoxListElements = getTheaterFilterBoxListElements();
            int tCount = theaterFilterBoxListElements.Count;
            for (int i = 0; i < tCount; i++)
            {
                logger.Debug($"Parsing new theater #{teaterCount++}");
                try
                {
                    Click(theaterFilterBoxListElements.ToList()[i], true, true); // Filter by theater
                }
                catch (Exception e)
                {
                    logger.Error("Failed to click on theaterFilterBoxListElements");
                    logger.Error(e);
                    saveDebugData();
                    break;
                }
                try
                {
                    Click(screeningTypeFilterBoxElement, true, true); // Click on screening filter box
                }
                catch (Exception e)
                {
                    logger.Error("Failed to click on screeningTypeFilterBoxElement");
                    logger.Error(e);
                    saveDebugData();
                    break;
                }
                allScreeningTypesButton = getAllScreeningTypesButton();
                if (i == 0)
                {
                    try
                    {
                        Click(allScreeningTypesButton, true, true); // Filter by all screening types 
                    }
                    catch (Exception e)
                    {
                        logger.Error("Failed to click on allScreeningTypesButton");
                        logger.Error(e);
                        saveDebugData();
                        break;
                    }
                }
                daysFilterButtons = getDaysFilterButtons();
                int dayCount = 1; // debug
                // Filter only today's screenings
                if (daysFilterButtons == null)
                    break;
                IWebElement dayFilterButton = daysFilterButtons.ToList()[0];
                try
                {
                    Click(dayFilterButton, true, true); // Filter by specific day
                }
                catch (Exception e)
                {
                    logger.Error("Failed to click on dayFilterButton");
                    logger.Error(e);
                    saveDebugData();
                    break;
                }
                logger.Debug("Parsing new day #" + (dayCount++)); // Debug
                if (isMoviePlaying(movie))
                {
                    Theater theater = parseTheaterFromScreenings();
                    Showtime screening = new Showtime(movie, theater);

                    screeningTimeInfoContainer = getScreeningContainer(); // Screening containers ordered by screening type
                    foreach (IWebElement screeningTimeInfo in screeningTimeInfoContainer)
                    {
                        screening.MovieUrl = movie.Urls[Name];
                        screening.Type = parseScreeningType(screeningTimeInfo);
                        screening.Language = parseScreeningLanguage(screeningTimeInfo);
                        screeningTimes = getScreeningTimesElements(screeningTimeInfo);
                        foreach (IWebElement time in screeningTimes)
                        {
                            screening.Time = parseScreeningTime(time);
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

                            logger.Debug("Added new screening, time=" + screening.Time); // Debug
                        }
                    }
                }
                else
                    logger.Error("Movie screening is missing on this day");

                try
                {
                    Click(theaterFilterBoxElement, true, true); // Click on theater filter box
                }
                catch (Exception e)
                {
                    logger.Error("Failed to click on theaterFilterBoxElement");
                    logger.Error(e);
                    return;
                }
                theaterFilterBoxListElements = getTheaterFilterBoxListElements();
            }
            logger.Debug("Total screenings in list: " + ScreeningsList.Count);
        }

        // returns true if movie is playing in current selected day, theater and type
        bool isMoviePlaying(Movie movie)
        {
            if (FindElementByDriver(By.CssSelector(YesPlanet_QueryStrings.isMoviePlaying)) != null)
                return true;
            if ((movie.ReleaseDate - DateTime.Now).Days > 0)
            {
                logger.Debug("Movie is not released yet. Showtimes are missing");
                return true;
            }
            return false;
        }

        // Find and returns screening days filter buttons from movie's page
        IReadOnlyCollection<IWebElement> getDaysFilterButtons()
        {
            logger.Debug("Gettings daysFilterButtons");
            return FindElementsByDriver(By.CssSelector(YesPlanet_QueryStrings.daysFilterButtons));
        }

        // Find and returns all screening type filter button from movie's page
        IWebElement getAllScreeningTypesButton()
        {
            logger.Debug("Gettings allScreeningTypesButton");
            IReadOnlyCollection<IWebElement> screeningTypeFilterBoxListElements = FindElementsByDriver(By.CssSelector(YesPlanet_QueryStrings.screeningTypeFilterBoxListElements), true);
            if (screeningTypeFilterBoxListElements != null)
                return screeningTypeFilterBoxListElements.ToList()[0];
            return null;
        }

        IReadOnlyCollection<IWebElement> getScreeningContainer()
        {
            logger.Debug("Gettings screeningsContainer");
            IReadOnlyCollection<IWebElement> screeningsContainer = FindElementsByDriver(By.CssSelector(YesPlanet_QueryStrings.screeningsContainer));
            return screeningsContainer;
        }

        Theater parseTheaterFromScreenings()
        {
            logger.Debug("Parsing theater from screenings");
            Theater theater = new Theater();
            theater.Location = FindElementByDriver(By.ClassName(YesPlanet_QueryStrings.theaterName)).GetAttribute("innerText");
            theater.Name = Name;
            theater.Address = FindElementByDriver(By.CssSelector(YesPlanet_QueryStrings.theaterAddress)).GetAttribute("innerText");
            return theater;
        }

        string parseScreeningType(IWebElement screeningInfo)
        {
            logger.Debug("Parsing screening type");
            string type = "";
            IReadOnlyCollection<IWebElement> screeningTypeElements = FindElementsByFather(By.CssSelector(YesPlanet_QueryStrings.screeningTypeElements), screeningInfo);
            foreach (IWebElement screeningType in screeningTypeElements)
            {
                string tmp = screeningType.GetAttribute("innerText");
                if (!type.Contains(tmp))
                    type += $"{tmp} ";
            }
            return type;
        }

        public string parseScreeningLanguage(IWebElement screeningInfo)
        {
            logger.Debug("Parsing screening language");
            string lang = "";
            IReadOnlyCollection<IWebElement> langElements = FindElementsByFather(By.CssSelector(YesPlanet_QueryStrings.langElements), screeningInfo);
            foreach (IWebElement langElement in langElements)
            {
                string tmp = langElement.GetAttribute("innerText");
                if (!lang.Contains(tmp))
                    lang += $"{tmp} ";
            }
            return lang;
        }

        DateTime parseScreeningTime(IWebElement screeningTimeInfo)
        {
            logger.Debug("Parsing screening time");
            return DateTime.Parse(getDayDate() + " " + screeningTimeInfo.GetAttribute("innerText"));
        }

        IReadOnlyCollection<IWebElement> getTheaterFilterBoxListElements()
        {
            logger.Debug("Getting theaterFilterBoxListElements");
            return FindElementsByDriver(By.CssSelector(YesPlanet_QueryStrings.theaterFilterBoxListElements));
        }

        IReadOnlyCollection<IWebElement> getScreeningTimesElements(IWebElement screeningContainer)
        {
            logger.Debug("Getting screeningTimesElements");
            return FindElementsByFather(By.CssSelector(YesPlanet_QueryStrings.screeningTimesElements), screeningContainer);
        }

        public void closeCookiesPopUp()
        {
            logger.Debug("Closing cookies popup");
            IWebElement closeButton = FindElementByDriver(By.ClassName("close_btn_thick"));
            try { Click(closeButton, false, false); }
            catch (Exception) { }
        }

        public string getDayDate()
        {
            Regex re = new Regex(@"[0-9]{1,2}(\/|-)[0-9]{1,2}(\/|-)[0-9]{4}");
            IWebElement el = FindElementByDriver(By.CssSelector(YesPlanet_QueryStrings.screeningDate));
            return re.Match(el.GetAttribute("innerText")).ToString();
        }
    }
}

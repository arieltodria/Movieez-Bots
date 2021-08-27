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
        string TheatersPageUrl = "https://www.cinema-city.co.il/locations";

        // Web elements
        private ReadOnlyCollection<IWebElement> movies; // Movies' web elements list
        // Data obj
        public List<Movie> MoviesList;
        public List<Theater> TheatersList;
        public List<Screening> ScreeningsList;
        /*// CSS query strings for elements
        string queryString_searchBoxes = "dl dt a";
        string queryString_boxList = "dd[role='menuitem']";
        string queryString_theaterSearchBoxList = "ul li a";*/

        public CinemaCityBot()
        {
            initDriver(MainUrl);
            MoviesList = new List<Movie>();
            TheatersList = new List<Theater>();
            ScreeningsList = new List<Screening>();
            _movieezApiUtils = new MovieezApiUtils(MovieezApiUtils.e_Theaters.CinemaCity);

        }

        public void run()
        {
            this.parseAllMovies();
            printResults();
        }

        public void parseAllMovies()
        {
            this.loadAllMovies();
            initMoviesElements();
            int totalMoviesToParse = movies.Count;
            for(int i = 0; i < totalMoviesToParse; i++)
            {
                initMoviesElements();
                Movie movie = parseMovie(movies.ToList()[i]);
                MoviesList.Add(movie);
                _movieezApiUtils.PostMovie(movie);
                this.loadAllMovies();
            }
        }

        void initMoviesElements()
        {
            movies = this.driver.FindElements(By.ClassName("flipper"));
        }

        Movie parseMovie(IWebElement movieElement)
        {
            Movie movie = new Movie();

            parseMoviePage(movie, movieElement);
            parseMovieMetadata(movie);
            Console.WriteLine("Parsed " + movie.EnglishName);
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
            IReadOnlyCollection<IWebElement> screeningDays;
            IReadOnlyCollection<IWebElement> screeningTimes;
            IWebElement date;
            int tCount = 1; // debug
            int theatersCount = theather_names.Count;

            var movieFromApi = _movieezApiUtils.GetMovie(movie.Name).Result;
            List<Movieez.API.Model.Models.ShowTime> showTimesFromApi = null;
            if (movieFromApi != null)
            {
                showTimesFromApi = _movieezApiUtils.GetShowTimesByMovieId(movieFromApi.ID).Result;
            }

            // Run on all theaters
            for (int i = 0; i < theatersCount; i++)
            {
                IWebElement theater = theather_names.ToList()[i];
                closeChatPopUp();
                Click(theater_search_box, true, true);
                try
                {
                    Click(theater, false, true);
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to click on theater");
                }
                // Running only on 1st day's screenings
                closeChatPopUp();
                Click(date_search_box, true, true);
                try
                {
                    date = getDateElement();
                    Click(date, false, true);
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to click on date");
                }

                screeningTimes = initScreeningTimes();
                foreach (IWebElement time in screeningTimes)
                {
                    closeChatPopUp();
                    Click(time_search_box, true, true);
                    try
                    {
                        Screening screening = parseScreeningMetadata(movie, theater_search_box, date_search_box, time);
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
                    catch (Exception)
                    {
                        Console.WriteLine("Failed to click on time");
                    }
                }
                theather_names = initTheatersNames();
            }
            goToUrl(MainUrl); // Back to main
        }

        
        IReadOnlyCollection<IWebElement> initSearchBoxes()
        {
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
            return FindElementsByFather(By.CssSelector(CinemaCity_QueryStrings.theaterSearchBoxList), initTheatersSearchBoxList());
        }

        IWebElement getDateElement()
        {
            // returns 1st day from days list
            return FindElementsByFather(By.CssSelector(CinemaCity_QueryStrings.dateBoxSearchList), initDateSearchBoxList()).ToList()[0];
        }

        IReadOnlyCollection<IWebElement> initScreeningTimes()
        {
            return FindElementsByFather(By.CssSelector(CinemaCity_QueryStrings.screeningTimes), initTimeSearchBoxList());
        }

        Screening parseScreeningMetadata(Movie movie, IWebElement theater, IWebElement date, IWebElement time)
        {
            Screening screening = new Screening();
            screening.Movie = movie;
            screening.Theater = new Theater(theater.GetAttribute("innerText"), "");
            screening.Time = DateTime.Parse(date.GetAttribute("innerText") + " " + time.GetAttribute("innerText"));
            screening.Type = "2D";
            Console.WriteLine("New screening added " + screening.Time);
            return screening;
        }

        string parseScreeningType(string type)
        {
            return Regex.Replace(type, @"(\w?)\ (\d?)", "").ToString();
        }

        /**void parseTheaters()
        {
            goToUrl(TheatersPageUrl);
            try {
                IReadOnlyCollection<IWebElement> theaters = driver.FindElements(By.CssSelector(CinemaCity_QueryStrings.theaters));
                foreach (IWebElement theater in theaters)
                {
                    string theaterPageLink = theater.FindElement(By.CssSelector(CinemaCity_QueryStrings.theaterPageLink)).GetAttribute("href");
                    string theaterName = theater.FindElement(By.ClassName("theatre-name")).GetAttribute("innerText");
                    goToUrl(theaterPageLink);
                    List<IWebElement> addressElement = driver.FindElements(By.ClassName("all-info")).ToList();
                    string theaterAddress = addressElement[2].GetAttribute("innerText");
                    TheatersList.Add(new Theater(theaterName, theaterAddress));
                    goToUrl(TheatersPageUrl);
                }
            }
            catch{
                Console.WriteLine("Failed to parse Theaters");
            }
            goToUrl(MainUrl);
        } **/

        // Parse movie's url from main cinema city page, url is taken from poster
        void parseMoviePage(Movie movie, IWebElement movieElement)
        {
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

            IWebElement info_div = FindElementByDriver(By.CssSelector(CinemaCity_QueryStrings.info_div));
            movie.Plot = FindElementByFather(By.CssSelector(CinemaCity_QueryStrings.plot), info_div).GetAttribute("innerText");
            IWebElement info_div_inner = FindElementByFather(By.CssSelector(CinemaCity_QueryStrings.info_div_inner), info_div);
            IReadOnlyCollection<IWebElement> inner_metadata = FindElementsByFather(By.CssSelector(CinemaCity_QueryStrings.inner_metadata), info_div_inner);

            movie.Genre += parseMovieGenre(inner_metadata.ToList()[INNER_METADATA_GENRE_INDEX].Text);
            movie.Duration = parseMovieDuration(inner_metadata.ToList()[INNER_METADATA_DURATION_INDEX].Text);
            movie.ReleaseDate = parseMovieReleaseDate(inner_metadata.ToList()[INNER_METADATA_RELEASE_DATE_INDEX].Text);
            movie.Rating = parseMovieRating(inner_metadata.ToList()[INNER_METADATA_RATING_INDEX].Text);
            movie.TrailerUrl = FindElementByDriver(By.Id("fullpagevideo")).GetAttribute("src");
            movie.MainImage = FindElementByDriver(By.CssSelector(CinemaCity_QueryStrings.MainImage)).GetAttribute("src");
            parseMovieName(movie);
        }

        DateTime parseMovieReleaseDate(string date)
        {
            // remove "תאריך בכורה" from string
            date = date.Remove(0, "תאריך בכורה".Length);
            return DateTime.Parse(date);
        }

        void parseMovieName(Movie movie)
        {
            string tmpName = driver.FindElement(By.CssSelector(CinemaCity_QueryStrings.tmpMovieName)).GetAttribute("innerText");
            if (tmpName.Contains('/'))
            {
                movie.EnglishName = tmpName.Substring(tmpName.IndexOf('/') + 1);
                movie.Name = tmpName.Substring(0, tmpName.IndexOf('/'));
            }
            else
                movie.Name = tmpName;
        }

        void loadAllMovies()
        {
            /** int n = 1;
            while (loadMoreMovies(n)) {
                n++;
            }
            Console.WriteLine("Finished loading all movies"); **/
            IWebElement load_button = this.driver.FindElement(By.ClassName("loadmoreposts"));
            Click(load_button);            
            Click(load_button);
            Click(load_button);
            Click(load_button);
            Click(load_button);
            Click(load_button);
            Click(load_button);
        }

        void closeChatPopUp()
        {
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
            Console.WriteLine("####################################################");
            Console.WriteLine("Total movies: " + MoviesList.Count);
            Console.WriteLine("Total screenings: " + ScreeningsList.Count);
            Console.WriteLine("####################################################");
        }
    }
}




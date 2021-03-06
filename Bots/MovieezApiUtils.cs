using Movieez.API.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime;
using System.ComponentModel;
using System.Xml.Linq;

namespace Movieez.Bots
{
    public class MovieezApiUtils
    {
        public string ApiUrl { get; set; } = "http://localhost:5000/";
        private readonly HttpClient _client;
        private readonly e_Theaters _theater;
        // Logger
        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public MovieezApiUtils(e_Theaters theater)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(ApiUrl);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _theater = theater;
        }

        public void PostMovie(Movie movieToAdd)
        {
            var movieModel = mapMovieToApiModel(movieToAdd);
            string movieJson = JsonSerializer.Serialize(movieModel);
            var content = new StringContent(movieJson.ToString(), Encoding.UTF8, "application/json");
            var existingMovie = GetMovie(movieToAdd).Result;
            
            if (existingMovie != null)
            {
                logger.Debug($"Movie {(movieToAdd.EnglishName != null ? movieToAdd.EnglishName : movieToAdd.Name)} already exists");
            }

            HttpResponseMessage postMovieResponse = _client.PostAsync("api/Movies", content).Result;

            if (postMovieResponse.IsSuccessStatusCode)
            {
                logger.Debug($"New Movie '{(movieToAdd.EnglishName != null ? movieToAdd.EnglishName : movieToAdd.Name)}' posted successfully");
            }
            else
            {
                logger.Error("{0} ({1})", (int)postMovieResponse.StatusCode, postMovieResponse.ReasonPhrase);
            }
        }

        public void PostShowTime(Showtime showTime, int movieId)
        {
            var showTimeModel = mapShowTimeToApiModel(showTime);
            showTimeModel.MovieId = movieId;
            string showTimeJson = JsonSerializer.Serialize(showTimeModel);
            var content = new StringContent(showTimeJson.ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage postMovieResponse = _client.PostAsync("api/ShowTimes", content).Result;
            if (postMovieResponse.IsSuccessStatusCode)
            {
                logger.Debug($"New Showtime {showTime.Time} posted successfully");
                //Console.WriteLine("Request Message Information:- \n\n" + postMovieResponse.RequestMessage + "\n");
                //Console.WriteLine("Response Message Header \n\n" + postMovieResponse.Content.Headers + "\n");
            }
            else
            {
                logger.Error("{0} ({1})", (int)postMovieResponse.StatusCode, postMovieResponse.ReasonPhrase);
            }
        }

        public async Task<API.Model.Models.Movie> GetMovie(Movie movie)
        {
            API.Model.Models.Movie fetchedMovieObject = null;
            //HttpResponseMessage getMovieResponseEnglish = _client.GetAsync($"api/Movies/{movie.EnglishName}").Result;
            //HttpResponseMessage getMovieResponseHebrew = _client.GetAsync($"api/Movies/{movie.Name}").Result;
            if (movie.EnglishName != null)
            {
                HttpResponseMessage getMovieResponseEnglish = _client.GetAsync($"api/Movies/{movie.EnglishName}").Result;
                if (getMovieResponseEnglish.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseContent = await getMovieResponseEnglish.Content.ReadAsStringAsync();
                    fetchedMovieObject = Newtonsoft.Json.JsonConvert.DeserializeObject<API.Model.Models.Movie>(responseContent);
                    return fetchedMovieObject;
                }
            }
            if(movie.Name != null)
            {
                HttpResponseMessage getMovieResponseHebrew = _client.GetAsync($"api/Movies/{movie.Name}").Result;
                if (getMovieResponseHebrew.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseContent = await getMovieResponseHebrew.Content.ReadAsStringAsync();
                    fetchedMovieObject = Newtonsoft.Json.JsonConvert.DeserializeObject<API.Model.Models.Movie>(responseContent);
                    return fetchedMovieObject;
                }
            }
            return fetchedMovieObject;
        }

        public async Task<List<API.Model.Models.ShowTime>> GetShowTimesByMovieId(int movieId)
        {
            IEnumerable<API.Model.Models.ShowTime> fetchedShowTimes = null;
            HttpResponseMessage getMovieResponse = _client.GetAsync($"api/showtimes/{movieId}").Result;
            if (getMovieResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseContent = await getMovieResponse.Content.ReadAsStringAsync();
                fetchedShowTimes = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<API.Model.Models.ShowTime>>(responseContent);
                fetchedShowTimes = fetchedShowTimes.Where(st => st.TheaterId == (int)_theater);
            }

            return fetchedShowTimes.ToList();
        }

        private API.Model.Models.Movie mapMovieToApiModel(Movie movie)
        {
            return new API.Model.Models.Movie
            {
                Name = movie.Name,
                EnglishName = movie.EnglishName,
                Plot = movie.Plot,
                OriginalLanguage = movie.OriginalLanguage,
                Duration = int.Parse(movie.Duration),
                Genre = movie.Genre,
                Cast = movie.Cast,
                Director = movie.Director,
                Rating = movie.Rating,
                PosterImage = movie.PosterImage,
                MainImage = movie.MainImage,
                TrailerUrl = movie.TrailerUrl,
                ReleaseDate = movie.ReleaseDate.ToShortDateString(),
                IsActive = DateTime.Now > movie.ReleaseDate
            };
        }

        private API.Model.Models.ShowTime mapShowTimeToApiModel(Showtime showTime)
        {
            return new API.Model.Models.ShowTime
            {
                TheaterId = (int)_theater,
                TheaterName = showTime.Theater.Name,
                TheaterLocation = showTime.Theater.Location,
                TheaterAddress = showTime.Theater.Address,
                MovieName = showTime.Movie.Name,
                MovieUrl = showTime.MovieUrl,
                Day = showTime.Time.ToString("dd/MM/yyyy"),
                Time = showTime.Time.ToString("HH:mm"),
                Type = showTime.Type,
                Language = showTime.Language
            };
        }

        public enum e_Theaters
        {
            YesPlanet = 2,
            HotCinema = 4,
            CinemaCity = 5,
            Lev = 6
        }
    }
}

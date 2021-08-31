using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movieez
{
    public class Showtime
    {
        public Movie Movie { get; set; }
        public string MovieUrl { get; set; }
        public DateTime Time { get; set; }
        public Theater Theater { get; set; }
        public string Type { get; set; }
        public string Language { get; set; }
        public Showtime() { }
        public Showtime(Movie movie, DateTime time, Theater theater, string movieUrl="", string type = "", string language = "")
        {
            Movie = movie;
            MovieUrl = movieUrl;
            Time = time;
            Theater = theater;
            Type = type;
            Language = language;
        }
        public Showtime(Movie movie, Theater theater)
        {
            Movie = movie;
            Theater = theater;
        }
        public Showtime(Movie movie)
        {
            Movie = movie;
        }
    }
}

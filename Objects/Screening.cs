using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movieez
{
    public class Screening
    {
        public Movie Movie { get; set; }
        public DateTime Time { get; set; }
        public Theater Theater { get; set; }
        public string Type { get; set; }
        public string Language { get; set; }
        public Screening() { }
        public Screening(Movie movie, DateTime time, Theater theater, string type, string language)
        {
            Movie = movie;
            Time = time;
            Theater = theater;
            Type = type;
            Language = language;
        }
        public Screening(Movie movie, Theater theater)
        {
            Movie = movie;
            Theater = theater;
        }
        public Screening(Movie movie)
        {
            Movie = movie;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movieez
{
    public class Movie
    {
        public Dictionary<string, string> Urls { get; set; }
        public string Name { get; set; }
        public string EnglishName { get; set; }
        public string Plot { get; set; }
        public string OriginalLanguage { get; set; }
        public string TrailerUrl { get; set; }
        public string Duration { get; set; }
        public string Genre {get; set; }
        public string Rating {get; set; }
        public string Cast { get; set; }
        public string Director { get; set; }
        public string PosterImage { get; set; }
        public string MainImage { get; set; }
        public DateTime ReleaseDate { get; set; }

        public Movie() 
        {
            Urls = new Dictionary<string, string>();
            Genre = "";
        }
        public override string ToString()
        {
            return $"Name={Name} EnglishName={EnglishName} Plot={Plot} OriginalLanguage={OriginalLanguage} Trailer_link={TrailerUrl} Duration={Duration} Genre={Genre} Rating={Rating} Cast={Cast} Director={Director} PosterImage={PosterImage} MainImage={MainImage} ReleaseDate={ReleaseDate}";
        }
    }
}

public enum Genre
{
    Comedy,
    Action,
    Thriller,
    Drama,
    Horror,
    SciFi,
    Musical,
    Kids,
    Family,
    Unknown
}

public static class Rating
{
    public static readonly string gRated = "לכל הגלאים";
    public static readonly string pgRated = "13+";
    public static readonly string pg13Rated = "13+";
    public static readonly string rRated = "17+";
    public static readonly string nc17Rated = "17+";
    public static readonly string Unknown = "אחר";
}
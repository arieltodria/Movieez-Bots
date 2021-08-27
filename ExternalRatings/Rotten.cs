using System;
using System.Collections.Generic;
using System.Text;
using RottenTomatoes.Api;

namespace Movieez.ExternalRatings
{
    class Rotten
    {
        RottenTomatoes.Api.RottenTomatoesRestClient rotten;
        public Rotten(bool run)
        {
            // init with API key
            if (run)
                Run();
        }
        void Run()
        {
            var res = rotten.MoviesSearch("The Matrix 1999");
        }
    }
}

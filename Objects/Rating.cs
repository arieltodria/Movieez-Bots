using System;
using System.Collections.Generic;
using System.Text;

namespace Movieez.Objects
{
    public class Rating
    {
        public string Id;
        public string Name;
        public string Reviewer;
        public double AvgRating;
        public int NumOfVotes;
        public string Text;

        public Rating()
        {

        }
        public Rating(string id, string name, string reviewer, double avgRating, int numOfVotes, string text)
        {
            Id = id;
            Name = name;
            Reviewer = reviewer;
            AvgRating = avgRating;
            NumOfVotes = numOfVotes;
            Text = text;
        }
    }
}

using MovieAPI.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieAPI.Models;


namespace MovieServiceAPITests_NUnit
{
    class MovieServiceFilterTest : MovieAPI.Services.MovieSearchFilters
    {
        private static List<Movie> data = new List<Movie>()
        {
            new Movie()
            {
                  ID =1,
                  Name  ="God Man Dog",
                  Language = 1,
                  FilmingStarted = new DateTime(2010, 10, 10),
                  FilmingEnded = new DateTime(2011, 10, 10),
                  SecretField = "secret"
            }
        };
        //SearchOptions.Is, SearchOptions.DoesNotContain
        [TestCase("Ape", SearchOptions.Contains, ExpectedResult = false, Description = "Contains, string not present in name")]
        [TestCase("Man", SearchOptions.Contains, ExpectedResult = true, Description = "Contains, string present in name")]
        [TestCase("mAN", SearchOptions.Contains, ExpectedResult = true, Description = "Contains, string present in name, diff case")]
        [TestCase("mAN", SearchOptions.Is, ExpectedResult = false, Description = "Is, false")]
        [TestCase("God Man Dog", SearchOptions.Is, ExpectedResult = true, Description = "Is, case same")]
        [TestCase("gOD mAN dOG", SearchOptions.Is, ExpectedResult = true, Description = "Is, case different")]
        public bool NameFilterTest(string text, SearchOptions options)
        {
            var dto = new NameSearchFilterDTO()
            {
                SearchString = text,
                Option = options
            };
            return NameFilter(dto,data.AsQueryable()).Any();
        }
    }

}

using MovieAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieAPI.Services
{
    public class MovieService
    {
        protected static SortedDictionary<long, string> LanguageMap =
        new ()
        {
            { 1, "English" },
            {2, "Mandarin" },
            {3, "French" },
            {4, "German"}
        };

        public static string ValidateNewMovie(MovieDTO toValidate, IEnumerable<Movie> existingData)
        {
            return ValidationItems
                     .Where(x => (!x.Validate(toValidate, existingData)))
                     .Aggregate("", (x, y) => string.IsNullOrWhiteSpace(x) ? y.ValidationMessage : x + Environment.NewLine + y.ValidationMessage);
        }

        public static IQueryable<Movie> FilterMovies(Services.MovieSearchFilters.MovieSearchFiltersDTO filterDTO, IQueryable<Movie> query)
        {
            return query;
        }


        protected class ValidationItem
        {
            public Func<MovieDTO, IEnumerable<Movie>, bool> Validate;
            public string ValidationMessage;
        }

        protected static ValidationItem NameValidationItem = new()
        {
             Validate = (tv, set) => { return !string.IsNullOrWhiteSpace(tv.Name); },
             ValidationMessage = "A Name value must be provided!"
         };
        protected static ValidationItem LanguageValidationItem = new()
        {
            Validate = (tv, set) => { return LanguageMap.ContainsKey(tv.Language); },
            ValidationMessage = "A valid Language value must be provided!"
        };
        protected static ValidationItem FilmingDateValidationItem = new()
        {
            Validate = (tv, set) => { return tv.FilmingStarted == null 
                                             || tv.FilmingEnded == null 
                                             || tv.FilmingEnded >= tv.FilmingStarted; },
            ValidationMessage = "FilmingEnded must not be before FilmingStarted!"
        };
        protected static ValidationItem UniqueRecordValidationItem = new()
        {
            Validate = (tv, set) => {
                return !set.Any(x => x.ID != tv.ID && x.Name == tv.Name && x.Language == tv.Language);
            },
            ValidationMessage = "A record with this name and language exists!"
        };

        private static List<ValidationItem> ValidationItems = new()
        {
            NameValidationItem,
            LanguageValidationItem,
            FilmingDateValidationItem,
            UniqueRecordValidationItem
        };
    }
}

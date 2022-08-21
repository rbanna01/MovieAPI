using MovieAPI.Models;
using System;
using System.Linq;

namespace MovieAPI.Services
{
    public class MovieSearchFilters
    {
        public class MovieSearchFiltersDTO
        {
            public NameSearchFilterDTO Name { get; set; }
        }

        public enum SearchOptions
        {
            Contains,
            DoesNotContain,
            Is
        }

        public class NameSearchFilterDTO
        {
            public string SearchString { get; set; }
            public SearchOptions Option { get; set; }
        }

        public static IQueryable<Movie> NameFilter(NameSearchFilterDTO dto, IQueryable<Movie> toQuery)
        {
            switch (dto.Option)
            {
                case SearchOptions.Is:
                    return toQuery.Where(movie => string.Equals(movie.Name, dto.SearchString, StringComparison.CurrentCultureIgnoreCase));
                case SearchOptions.Contains:
                    return toQuery.Where(movie => movie.Name.Contains(dto.SearchString, StringComparison.CurrentCultureIgnoreCase));
                case SearchOptions.DoesNotContain:
                    return toQuery.Where(movie => !movie.Name.Contains(dto.SearchString, StringComparison.CurrentCultureIgnoreCase));
                default:
                    return toQuery;
            }
        }

    }
}

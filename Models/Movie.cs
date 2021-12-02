using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MovieAPI.Models
{
    public class Movie
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public long Language { get; set; }
        public DateTime? FilmingStarted { get; set; }
        public DateTime? FilmingEnded { get; set; }
        public string SecretField { get; set; }

    }

    public static class MovieService
    {
        public enum Languages
        {
            English = 1,
            Mandarin = 2,
            French = 3,
            German = 4
        }


        private static SortedDictionary<int, string> LanguageMap =
        new SortedDictionary<int, string>
        {
            { 1, "English" },
            {2, "Mandarin" },
            {3, "French" },
            {4, "German"}
        };

        public static string ValidateNewMovie(MovieDTO toValidate, DbSet<Movie> existingData)
        {
            var output = new System.Text.StringBuilder();

            if (toValidate.FilmingStarted != null
                && toValidate.FilmingEnded != null
                && toValidate.FilmingEnded < toValidate.FilmingStarted)
            {
                output.Append("Filming Ended must not be before Filming Started!");
            }
            
            if (!LanguageMap.ContainsKey((int) toValidate.Language))
            {
                output.Append("Invalid Language value!");
            }

            if ( toValidate.ID == 0 //New item being created
                && existingData.Any(x => x.Name == toValidate.Name && x.Language == toValidate.Language))
            {
                output.Append($"A movie with the Name '{toValidate.Name}' and Language '{toValidate.Language}' already exists!");
            }
            return output.ToString();
        }
    }

    public class MovieDTO
    {
        public long ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public long Language { get; set; }
        public DateTime? FilmingStarted { get; set; }
        public DateTime? FilmingEnded { get; set; }
    }
}

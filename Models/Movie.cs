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

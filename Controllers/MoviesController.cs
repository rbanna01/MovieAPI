using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPI.Models;

namespace MovieAPI.Controllers
{
    [Route("api/Movies")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly MovieContext _context;

        public MoviesController(MovieContext context)
        {
            _context = context;
#if DEBUG
            long putTestHappyPathItemID = 5000;

            if(!context.Movies.Any(x => x.ID == putTestHappyPathItemID))
            {
                context.Movies.Add(new Movie()
                { //Also used in GET test happyPath
                    ID = putTestHappyPathItemID,
                    Name = "Put test happyPath",
                    Language = 1,
                    FilmingStarted = new DateTime(),
                    FilmingEnded = new DateTime()
                }); 
            }
            if(!context.Movies.Any(x => x.Name =="Duplicate Test"))
            {
                context.Movies.Add(new Movie()
                {//Duplicate in POST unit tests
                    Name = "Duplicate test",
                    Language = 1,
                    FilmingStarted = new DateTime(2007, 01, 01),
                    FilmingEnded = new DateTime(2007, 12, 21)
                });
            }
            //Happy path in POST unit tests
            var toDelete = context.Movies.Where(x => x.Name == "God Man Dog" && x.Language == 2).FirstOrDefault();
            if (toDelete != null)
            {
                context.Movies.Remove(toDelete);
            }

            //Happy path in DELETE unit tests
            if (!context.Movies.Any(x => x.Name == "Deletion Test"))
            {
                context.Movies.Add(new Movie()
                {//Duplicate in POST unit tests
                    ID = -8,
                    Name = "Deletion Test",
                    Language = 1,
                    FilmingStarted = new DateTime(2007, 01, 01),
                    FilmingEnded = new DateTime(2007, 12, 21)
                });
            }
            context.SaveChanges();
#endif 

        }

        // GET: api/Movies
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MovieDTO>>> GetMovies()
        {
            return await _context.Movies
                .Select(movie => MovieToDTO(movie))
                .ToListAsync();
        }

        // GET: api/Movies/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<MovieDTO>> GetMovie(long id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }
            
            return MovieToDTO(movie);
        }

        // PUT: api/Movies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovie(long id, MovieDTO movieDTO)
        {
            if (id != movieDTO.ID)
            {
                return BadRequest();
            }

            var movieItem = await _context.Movies.FindAsync(id);

            var validationMessages = MovieAPI.Models.MovieService.ValidateNewMovie(movieDTO, _context.Movies);
            if(validationMessages.Any())
            {
                return Problem(validationMessages, statusCode: (int)System.Net.HttpStatusCode.BadRequest);
            }

            if (movieItem == null)
            {
                return NotFound();
            }

            movieItem.Name = movieDTO.Name;
            movieItem.Language = movieDTO.Language;
            movieItem.FilmingStarted = movieDTO.FilmingStarted;
            movieItem.FilmingEnded = movieDTO.FilmingEnded;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException) when (!_context.Movies.Any(e => e.ID == id))
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Movies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Movie>> PostMovie(MovieDTO movieDTO)
        {
            var errorMessages = MovieAPI.Models.MovieService.ValidateNewMovie(movieDTO, _context.Movies);
            if (errorMessages.Length == 0)
            {
                var newMovie = new Movie
                {
                    Name = movieDTO.Name,
                    Language = movieDTO.Language,
                    FilmingStarted = movieDTO.FilmingStarted,
                    FilmingEnded = movieDTO.FilmingEnded
                };
                _context.Movies.Add(newMovie);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetMovie)
                    , new { ID = newMovie.ID }
                    , MovieToDTO(newMovie));
            }
            else
            {
                return Problem(errorMessages, statusCode: (int)System.Net.HttpStatusCode.BadRequest);
            }
        }

        // DELETE: api/Movies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(long id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        private static MovieDTO MovieToDTO(Movie input) =>
            new MovieDTO
            {
                ID = input.ID,
                Name = input.Name,
                FilmingStarted = input.FilmingStarted,
                FilmingEnded = input.FilmingEnded
            };
    }
}

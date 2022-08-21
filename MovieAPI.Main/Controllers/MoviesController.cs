using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPI.Models;
using MovieAPI.Services;
using static MovieAPI.Services.MovieSearchFilters;

namespace MovieAPI.Controllers
{
    [Route("api/Movies")]
    [ApiController]
    // [Produces("application/json")]
    public class MoviesController : ControllerBase
    {
        private readonly MovieContext _context;

        public MoviesController(MovieContext context)
        {
            _context = context;
#if DEBUG
            long putTestHappyPathItemID = 5000;

            if (!context.Movies.Any(x => x.ID == putTestHappyPathItemID))
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
            if (!context.Movies.Any(x => x.Name == "Duplicate Test"))
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

        /// <summary>
        /// Gets all movie records
        /// </summary>
        /// <returns>Array of MovieDTO objects</returns>
        /// <response code="200">All records returned as list of MovieDTOs in json</response>
        // GET: api/Movies
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //[Route("api/movies")]
        public async Task<ActionResult<IEnumerable<MovieDTO>>> GetMovies()
        {
            return await _context.Movies
                .Select(movie => MovieToDTO(movie))
                .ToListAsync();
        }

        /// <summary>
        /// Gets all movie records, filtered by criteria passed in filters arg
        /// </summary>
        /// <returns>Array of MovieDTO objects</returns>
        /// <response code="200">All records returned as list of MovieDTOs in json</response>
        // GET: api/Movies 
        [HttpPost("byNameFilter")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MovieDTO>>> GetFilteredMovies(NameSearchFilterDTO filterIn)
        {
            if(filterIn != null)
            {
            
            var filteredMovies = MovieSearchFilters.NameFilter(filterIn, _context.Movies);
            return  await filteredMovies
                   .Select(movie => MovieToDTO(movie))
                   .ToListAsync();
            }
            else
            {
                return await _context.Movies
                   .Select(movie => MovieToDTO(movie))
                   .ToListAsync();
            }
        }

        /// <summary>
        /// Returns existing movie record with ID passed in, if any
        /// </summary>
        /// <param name="id">ID of existing record to be updated.</param>
        /// <returns>Nothing</returns>
        /// <response code="200">A MovieDTO with ID equal to id parameter returned</response>
        /// <response code="400">ID parameter passed in invalid</response>
        /// <response code="404">No movie with ID equal to id parameter found </response>
        // GET: api/Movies/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MovieDTO>> GetMovie(long id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }
            
            return MovieToDTO(movie);
        }

        /// <summary>
        /// Overwrites the existing movie with ID in URL with data passed in as MovieDTO
        /// </summary>
        /// <param name="id">ID of existing record to be updated.</param>
        /// <param name="movieDTO">MovieDTO containing data to be used to update existing record.</param>
        /// <returns>Nothing</returns>
        /// <response code="204">A movie with ID equal to id parameter found and updated, nothing returned</response>
        /// <response code="400">Input invalid: ID in URL did not match that in MovieDTO passed in, or data in MovieDTO not valid</response>
        /// <response code="404">No movie with ID equal to id parameter found </response>
        /// 
        // PUT: api/Movies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovie(long id, MovieDTO movieDTO)
        {
            if (id != movieDTO.ID)
            {
                return BadRequest();
            }

            var movieItem = await _context.Movies.FindAsync(id);
            if (movieItem == null)
            {
                return NotFound();
            }


            var validationMessages = MovieAPI.Services.MovieService.ValidateNewMovie(movieDTO, _context.Movies.AsQueryable<Movie>());
            if(validationMessages.Any())
            {
                return Problem(validationMessages, statusCode: (int)System.Net.HttpStatusCode.BadRequest);
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

        /// <summary>
        /// Creates a new movie record using MovieDTO parameter passed in
        /// </summary>
        /// <param name="movieDTO">MovieDTO containing data to be used to create new record.</param>
        /// <returns>MovieDTO containing record created </returns>
        /// <response code="204">A movie with ID equal to id parameter found and updated, nothing returned</response>
        /// <response code="400">Input invalid: data in MovieDTO not valid</response>
        // POST: api/Movies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<Movie>> PostMovie(MovieDTO movieDTO)
        {
            var errorMessages = MovieAPI.Services.MovieService.ValidateNewMovie(movieDTO, _context.Movies.AsQueryable<Movie>());
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


        /// <summary>
        /// Deletes the movie specified in the id parameter, if found
        /// </summary>
        /// <param name="id">ID of the movie to delete</param>
        /// <returns>Nothing</returns>
        /// <response code="204">A movie with ID equal to id parameter found and deleted, no content returned</response>
        /// <response code="404">No movie with ID equal to id parameter found </response>
        /// 
        // DELETE: api/Movies/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

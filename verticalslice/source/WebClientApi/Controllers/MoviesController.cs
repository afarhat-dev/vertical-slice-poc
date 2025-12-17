using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebClientApi.Features.Movies;

namespace WebClientApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MoviesController> _logger;

    public MoviesController(IMediator mediator, ILogger<MoviesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all movies
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<GetAllMovies.MovieDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GetAllMovies.MovieDto>>> GetAll()
    {
        var query = new GetAllMovies.Query();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific movie by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetMovieById.MovieDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetMovieById.MovieDto>> GetById(int id)
    {
        var query = new GetMovieById.Query(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = $"Movie with Id {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Search movies by various criteria
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<SearchMovies.MovieDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SearchMovies.MovieDto>>> Search(
        [FromQuery] string? title,
        [FromQuery] string? director,
        [FromQuery] string? genre,
        [FromQuery] int? minYear,
        [FromQuery] int? maxYear,
        [FromQuery] decimal? minRating)
    {
        var query = new SearchMovies.Query(title, director, genre, minYear, maxYear, minRating);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Add a new movie
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AddMovie.Result), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AddMovie.Result>> Add([FromBody] AddMovieRequest request)
    {
        try
        {
            var command = new AddMovie.Command(
                request.Title,
                request.Director,
                request.ReleaseYear,
                request.Genre,
                request.Rating,
                request.Description
            );

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            return BadRequest(new { errors });
        }
    }

    /// <summary>
    /// Update an existing movie
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UpdateMovie.Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateMovie.Result>> Update(int id, [FromBody] UpdateMovieRequest request)
    {
        try
        {
            var command = new UpdateMovie.Command(
                id,
                request.Title,
                request.Director,
                request.ReleaseYear,
                request.Genre,
                request.Rating,
                request.Description
            );

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            return BadRequest(new { errors });
        }
    }

    /// <summary>
    /// Delete a movie
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(DeleteMovie.Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteMovie.Result>> Delete(int id)
    {
        var command = new DeleteMovie.Command(id);
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return NotFound(new { message = result.Message });
        }

        return Ok(result);
    }

    public record AddMovieRequest(
        string Title,
        string? Director,
        int? ReleaseYear,
        string? Genre,
        decimal? Rating,
        string? Description
    );

    public record UpdateMovieRequest(
        string Title,
        string? Director,
        int? ReleaseYear,
        string? Genre,
        decimal? Rating,
        string? Description
    );
}

using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebClientApi.Features.Movies;
using static WebClientApi.Features.Movies.AddMovie;
using static WebClientApi.Features.Movies.DeleteMovie;
using static WebClientApi.Features.Movies.UpdateMovie;

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
    [HttpGet(Name ="GetAll")]
    [ProducesResponseType(typeof(List<MovieDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MovieDto>>> GetAll()
    {
        var query = new GetAllMovies.Query();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific movie by ID
    /// </summary>
    [HttpGet("{id}", Name = "GetById")]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDto>> GetById(int id)
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
    [HttpGet("search",Name ="Search")]
    [ProducesResponseType(typeof(List<MovieDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MovieDto>>> Search(
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
    [ProducesResponseType(typeof(AddResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AddResult>> Add([FromBody] AddMovieRequest request)
    {
        try
        {
            var command = new AddCommand(
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
    [HttpPut("{id}",Name ="UpdateMovie")]
    [ProducesResponseType(typeof(UpdateResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateResult>> Update(int id, [FromBody] UpdateMovieRequest request)
    {
        try
        {
            var command = new UpdateCommand(
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
    [ProducesResponseType(typeof(DeleteResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteResult>> Delete(int id)
    {
        var command = new DeleteMovie.Command(id);
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return NotFound(new { message = result.Message });
        }

        return Ok(result);
    }

    //TODO: Should be in contract folder
    public record AddMovieRequest(
        string Title,
        string? Director,
        int? ReleaseYear,
        string? Genre,
        decimal? Rating,
        string? Description
    );

    //TODO: Should be in contract folder
    public record UpdateMovieRequest(
        string Title,
        string? Director,
        int? ReleaseYear,
        string? Genre,
        decimal? Rating,
        string? Description
    );
}

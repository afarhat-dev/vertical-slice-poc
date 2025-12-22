using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieLibrary.Features.Rentals;
using static MovieLibrary.Features.Rentals.CreateRental;
using static MovieLibrary.Features.Rentals.ReturnRental;

namespace WebClientApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RentalsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RentalsController> _logger;

    public RentalsController(IMediator mediator, ILogger<RentalsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all rentals
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RentalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RentalDto>>> GetAll()
    {
        var query = new GetAllRentals.Query();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific rental by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RentalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalDto>> GetById(int id)
    {
        var query = new GetRentalById.Query(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = $"Rental with Id {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new rental
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateRental.Result), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateRental.Result>> Create([FromBody] CreateRental.Command command)
    {
        try
        {
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
    /// Mark a rental as returned and calculate total cost
    /// </summary>
    [HttpPut("{id}/return")]
    [ProducesResponseType(typeof(ReturnRental.Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReturnRental.Result>> Return(int id, [FromBody] ReturnRentalRequest request)
    {
        try
        {
            var command = new ReturnRental.Command(id, request.ReturnDate);
            var result = await _mediator.Send(command);

            if (result == null)
            {
                return NotFound(new { message = $"Rental with Id {id} not found" });
            }

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            return BadRequest(new { errors });
        }
    }
}

public record ReturnRentalRequest(DateTime ReturnDate);

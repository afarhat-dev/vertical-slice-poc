using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebClientApi.Data;

namespace WebClientApi.Features.Movies;

public static class UpdateMovie
{
    public record UpdateCommand(
        int Id,
        string Title,
        string? Director,
        int? ReleaseYear,
        string? Genre,
        decimal? Rating,
        string? Description
    ) : IRequest<UpdateResult>;

    public record UpdateResult(bool Success, string Message);

    public class Validator : AbstractValidator<UpdateCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

            RuleFor(x => x.Director)
                .MaximumLength(100).WithMessage("Director name cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Director));

            RuleFor(x => x.Genre)
                .MaximumLength(50).WithMessage("Genre cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.Genre));

            RuleFor(x => x.ReleaseYear)
                .GreaterThan(1800).WithMessage("Release year must be after 1800")
                .LessThanOrEqualTo(DateTime.UtcNow.Year + 5).WithMessage("Release year cannot be more than 5 years in the future")
                .When(x => x.ReleaseYear.HasValue);

            RuleFor(x => x.Rating)
                .GreaterThanOrEqualTo(0).WithMessage("Rating must be at least 0")
                .LessThanOrEqualTo(10).WithMessage("Rating cannot exceed 10")
                .When(x => x.Rating.HasValue);

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }

    public class Handler : IRequestHandler<UpdateCommand, UpdateResult>
    {
        private readonly MovieDbContext _context;
        private readonly IValidator<UpdateCommand> _validator;

        public Handler(MovieDbContext context, IValidator<UpdateCommand> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<UpdateResult> Handle(UpdateCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (movie == null)
            {
                return new UpdateResult(false, $"Movie with Id {request.Id} not found");
            }

            movie.Title = request.Title;
            movie.Director = request.Director;
            movie.ReleaseYear = request.ReleaseYear;
            movie.Genre = request.Genre;
            movie.Rating = request.Rating;
            movie.Description = request.Description;
            movie.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new UpdateResult(true, "Movie updated successfully");
        }
    }
}

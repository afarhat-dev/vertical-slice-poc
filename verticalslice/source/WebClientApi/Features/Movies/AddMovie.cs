using FluentValidation;
using MediatR;
using WebClientApi.Data;

namespace WebClientApi.Features.Movies;

public static class AddMovie
{
    public record Command(
        string Title,
        string? Director,
        int? ReleaseYear,
        string? Genre,
        decimal? Rating,
        string? Description
    ) : IRequest<Result>;

    public record Result(int Id, string Title, string Message);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
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

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly MovieDbContext _context;
        private readonly IValidator<Command> _validator;

        public Handler(MovieDbContext context, IValidator<Command> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var movie = new Movie
            {
                Title = request.Title,
                Director = request.Director,
                ReleaseYear = request.ReleaseYear,
                Genre = request.Genre,
                Rating = request.Rating,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync(cancellationToken);

            return new Result(movie.Id, movie.Title, "Movie added successfully");
        }
    }
}

using FluentValidation;
using MediatR;
using MovieLibrary.Data;
using MovieLibrary.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Features.Movies;

public static class AddMovie
{
    public record AddCommand(
        string Title,
        string? Director,
        int? ReleaseYear,
        string? Genre,
        decimal? Rating,
        string? Description
    ) : IRequest<AddResult>;

    public record AddResult(Guid Id, string Title, string Message);

    public class Validator : AbstractValidator<AddCommand>
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

    public class Handler : IRequestHandler<AddCommand, AddResult>
    {
        private readonly IMovieRepository _repository;
        private readonly IValidator<AddCommand> _validator;

        public Handler(IMovieRepository repository, IValidator<AddCommand> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<AddResult> Handle(AddCommand request, CancellationToken cancellationToken)
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

            var addedMovie = await _repository.AddAsync(movie, cancellationToken);

            return new AddResult(addedMovie.Id, addedMovie.Title, "Movie added successfully");
        }
    }
}

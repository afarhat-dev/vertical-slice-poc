# Movie Library API - Vertical Slice Architecture with MediatR

This is a .NET 10.0 Web API project implementing a movie library management system using **Vertical Slice Architecture** and the **Mediator pattern** with MediatR.

## Architecture Overview

### Vertical Slice Architecture
Instead of organizing code by technical layers (Controllers, Services, Repositories), this project organizes code by **features**. Each feature is a self-contained vertical slice that includes:
- Command/Query (request)
- Handler (business logic)
- Validator (validation rules)
- DTOs (data transfer objects)

### Mediator Pattern
Uses MediatR library to implement the mediator pattern, which:
- Decouples request senders from handlers
- Promotes single responsibility principle
- Makes testing easier
- Reduces tight coupling between components

## Project Structure

```
WebClientApi/
├── Controllers/
│   └── MoviesController.cs          # REST API endpoints
├── Data/
│   ├── Movie.cs                      # Movie entity
│   └── MovieDbContext.cs             # EF Core DbContext
├── Features/
│   └── Movies/                       # Movie feature slices
│       ├── AddMovie.cs               # Command to add a movie
│       ├── GetAllMovies.cs           # Query to get all movies
│       ├── GetMovieById.cs           # Query to get movie by ID
│       ├── UpdateMovie.cs            # Command to update a movie
│       ├── DeleteMovie.cs            # Command to delete a movie
│       └── SearchMovies.cs           # Query to search movies
└── Program.cs                        # App configuration
```

## Features

### 1. Add Movie (Command)
- **Endpoint**: `POST /api/movies`
- **Validation**: Title required, rating 0-10, year validation
- **Handler**: Creates new movie in database

### 2. Get All Movies (Query)
- **Endpoint**: `GET /api/movies`
- **Returns**: List of all movies ordered by creation date

### 3. Get Movie By ID (Query)
- **Endpoint**: `GET /api/movies/{id}`
- **Returns**: Single movie or 404 if not found

### 4. Update Movie (Command)
- **Endpoint**: `PUT /api/movies/{id}`
- **Validation**: Same as Add Movie
- **Handler**: Updates existing movie or returns 404

### 5. Delete Movie (Command)
- **Endpoint**: `DELETE /api/movies/{id}`
- **Handler**: Removes movie or returns 404

### 6. Search Movies (Query)
- **Endpoint**: `GET /api/movies/search`
- **Query Parameters**:
  - `title` - Search by title (partial match)
  - `director` - Search by director (partial match)
  - `genre` - Search by genre (partial match)
  - `minYear` - Minimum release year
  - `maxYear` - Maximum release year
  - `minRating` - Minimum rating

## Technologies Used

- **.NET 10.0** - Web API framework
- **MediatR 12.4.1** - Mediator pattern implementation
- **Entity Framework Core 9.0.0** - ORM with In-Memory database
- **FluentValidation 11.11.0** - Input validation
- **OpenAPI** - API documentation

## Benefits of This Architecture

### 1. Vertical Slice Benefits
- **Easy to locate code**: All related code for a feature is in one place
- **Easy to add features**: Just add a new slice without modifying existing code
- **Easy to test**: Each slice can be tested independently
- **Reduced merge conflicts**: Teams can work on different slices simultaneously

### 2. Mediator Pattern Benefits
- **Loose coupling**: Controllers don't directly depend on business logic
- **Single responsibility**: Each handler does one thing
- **Easy to extend**: New handlers don't affect existing ones
- **Testability**: Mock the mediator to test controllers

### 3. CQRS-lite Approach
- **Commands** for writes (Add, Update, Delete)
- **Queries** for reads (Get, Search)
- Clear separation of concerns

## Running the Application

```bash
cd verticalslice/source/WebClientApi
dotnet restore
dotnet build
dotnet run
```

## API Endpoints

### Add a Movie
```http
POST /api/movies
Content-Type: application/json

{
  "title": "The Matrix",
  "director": "Wachowski Sisters",
  "releaseYear": 1999,
  "genre": "Sci-Fi",
  "rating": 8.7,
  "description": "A computer hacker learns about the true nature of reality."
}
```

### Get All Movies
```http
GET /api/movies
```

### Get Movie by ID
```http
GET /api/movies/1
```

### Search Movies
```http
GET /api/movies/search?genre=Sci-Fi&minRating=8.0&minYear=1990
```

### Update Movie
```http
PUT /api/movies/1
Content-Type: application/json

{
  "title": "The Matrix",
  "director": "Wachowski Sisters",
  "releaseYear": 1999,
  "genre": "Science Fiction",
  "rating": 9.0,
  "description": "Updated description"
}
```

### Delete Movie
```http
DELETE /api/movies/1
```

## Example Vertical Slice Structure

Each feature slice (e.g., `AddMovie.cs`) contains:

```csharp
public static class AddMovie
{
    // Request (Command or Query)
    public record Command(...) : IRequest<Result>;

    // Response
    public record Result(...);

    // Validator (optional)
    public class Validator : AbstractValidator<Command> { }

    // Handler (business logic)
    public class Handler : IRequestHandler<Command, Result> { }
}
```

## Validation

FluentValidation is used for input validation:
- Title: Required, max 200 characters
- Director: Max 100 characters
- Genre: Max 50 characters
- Release Year: Between 1800 and current year + 5
- Rating: Between 0 and 10
- Description: Max 1000 characters

## Database

Uses Entity Framework Core with **In-Memory database** for simplicity. For production, replace with:
- SQL Server
- PostgreSQL
- MySQL
- etc.

## Testing

To test the API:
1. Run the application
2. Navigate to the OpenAPI/Swagger UI (typically at `/swagger` or `/openapi`)
3. Use the interactive API documentation to test endpoints
4. Or use tools like Postman, curl, or http clients

## Future Enhancements

- Add pagination to list queries
- Add sorting options
- Implement caching
- Add authentication/authorization
- Add logging and monitoring
- Add integration tests
- Replace in-memory database with persistent storage
- Add API versioning
- Add rate limiting

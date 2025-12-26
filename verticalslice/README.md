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
MovieLibrary/
├── Data/
│   ├── Movie.cs                      # Movie entity
│   ├── Rental.cs                     # Rental entity
│   ├── RentalStatus.cs               # Rental status constants
│   └── MovieDbContext.cs             # EF Core DbContext
├── Features/
│   ├── Movies/                       # Movie feature slices
│   │   ├── AddMovie.cs               # Command to add a movie
│   │   ├── GetAllMovies.cs           # Query to get all movies
│   │   ├── GetMovieById.cs           # Query to get movie by ID
│   │   ├── UpdateMovie.cs            # Command to update a movie
│   │   ├── DeleteMovie.cs            # Command to delete a movie
│   │   └── SearchMovies.cs           # Query to search movies
│   └── Rentals/                      # Rental feature slices
│       ├── CreateRental.cs           # Command to create a rental
│       ├── GetAllRentals.cs          # Query to get all rentals
│       ├── GetRentalById.cs          # Query to get rental by ID
│       ├── ReturnRental.cs           # Command to return a rental
│       └── RentalDto.cs              # Rental data transfer object
└── Migrations/                       # EF Core database migrations

WebClientApi/
├── Controllers/
│   ├── MoviesController.cs           # Movie REST API endpoints
│   └── RentalsController.cs          # Rental REST API endpoints
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

### 7. Create Rental (Command)
- **Endpoint**: `POST /api/rentals`
- **Validation**: Customer name required, movie must exist, valid rental date and daily rate
- **Handler**: Creates a new rental, fetches movie title, validates movie exists
- **Business Rules**: Rental date cannot be too far in future or past, daily rate must be positive

### 8. Get All Rentals (Query)
- **Endpoint**: `GET /api/rentals`
- **Returns**: List of all rentals with customer info, rental dates, and status

### 9. Get Rental By ID (Query)
- **Endpoint**: `GET /api/rentals/{id}`
- **Returns**: Single rental or 404 if not found

### 10. Return Rental (Command)
- **Endpoint**: `PUT /api/rentals/{id}/return`
- **Validation**: Return date required and valid, rental must be active
- **Handler**: Marks rental as returned, calculates total cost based on days rented
- **Business Rules**: Cannot return already-returned rental, return date must be after rental date

## Technologies Used

- **.NET 10.0** - Web API framework
- **MediatR 14.0.0** - Mediator pattern implementation
- **Entity Framework Core 10.0.1** - ORM with SQL Server
- **FluentValidation 12.1.1** - Input validation
- **OpenAPI** - API documentation
- **SQL Server** - Relational database with migrations support

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

### Prerequisites
- .NET 10.0 SDK
- SQL Server (LocalDB, Express, or full version)

### Setup Steps

1. **Update Connection String** (if needed)
   Edit `appsettings.json` to match your SQL Server configuration:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=.;Database=MovieRentalDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
   }
   ```

2. **Apply Database Migrations**
   ```bash
   cd verticalslice/source/WebClientApi
   dotnet ef database update --project ../MovieLibrary
   ```

3. **Build and Run**
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

4. **Access the API**
   - Navigate to `https://localhost:5001/swagger` for API documentation
   - Or use the OpenAPI endpoint

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

### Create a Rental
```http
POST /api/rentals
Content-Type: application/json

{
  "customerName": "John Doe",
  "movieId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "rentalDate": "2025-12-24T10:00:00Z",
  "dailyRate": 3.99
}
```

### Get All Rentals
```http
GET /api/rentals
```

### Get Rental by ID
```http
GET /api/rentals/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

### Return a Rental
```http
PUT /api/rentals/3fa85f64-5717-4562-b3fc-2c963f66afa6/return
Content-Type: application/json

{
  "returnDate": "2025-12-26T10:00:00Z"
}
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

Uses Entity Framework Core with **SQL Server** and full migration support.

### Database Schema

**Movies Table:**
- Id (uniqueidentifier, PK)
- Title (nvarchar(200), required)
- Director (nvarchar(100))
- ReleaseYear (int)
- Genre (nvarchar(50))
- Rating (decimal(3,1))
- Description (nvarchar(1000))
- CreatedAt (datetime2)
- UpdatedAt (datetime2)
- RowVersion (rowversion, for concurrency control)

**Rentals Table:**
- Id (uniqueidentifier, PK)
- MovieId (uniqueidentifier, FK to Movies, required)
- CustomerName (nvarchar(200), required)
- ItemName (nvarchar(200), required)
- RentalDate (datetime2, required)
- ReturnDate (datetime2, nullable)
- DailyRate (decimal(18,2), required)
- Status (nvarchar(50), required - "Active" or "Returned")
- RowVersion (rowversion, for concurrency control)

### Migrations

To create a new migration after schema changes:
```bash
dotnet ef migrations add MigrationName --project MovieLibrary --startup-project WebClientApi
```

To update the database:
```bash
dotnet ef database update --project MovieLibrary --startup-project WebClientApi
```

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
- Add API versioning
- Add rate limiting
- Add database indexes for frequently queried fields (MovieId, Status, CustomerName)
- Implement soft delete for audit trail
- Add late fee calculation logic
- Implement movie inventory/availability tracking
- Add maximum rental period enforcement

# Rental Management API

This is a .NET 10 Web API implementing a rental management system using **Vertical Slice Architecture** and **MS SQL Server** database.

## Features

### Rental Feature
The rental feature allows you to manage equipment/item rentals with the following capabilities:
- Create new rentals
- View all rentals
- Get rental by ID
- Mark rentals as returned with automatic cost calculation

## Architecture

### Vertical Slice Architecture
Each feature is organized as a self-contained slice with all related code:
```
Features/
  Rentals/
    Rental.cs                  # Entity
    CreateRental.cs           # Create rental handler
    GetRentals.cs            # Get all rentals handler
    GetRentalById.cs         # Get rental by ID handler
    ReturnRental.cs          # Return rental handler
    RentalsEndpoints.cs      # Endpoint mappings
```

## Database Setup

### Connection String
The application uses MS SQL Server. Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RentalDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

### Running Migrations

To apply the database migrations, run:

```bash
dotnet ef database update
```

To create a new migration after model changes:

```bash
dotnet ef migrations add MigrationName
```

### Migration Files
The initial migration creates the `Rentals` table with the following schema:
- `Id` (int, Primary Key, Identity)
- `CustomerName` (nvarchar(200), required)
- `ItemName` (nvarchar(200), required)
- `RentalDate` (datetime2, required)
- `ReturnDate` (datetime2, nullable)
- `DailyRate` (decimal(18,2), required)
- `Status` (nvarchar(50), required)

## API Endpoints

### Rentals API

**Base URL:** `/api/rentals`

#### Create Rental
```http
POST /api/rentals
Content-Type: application/json

{
  "customerName": "John Doe",
  "itemName": "Power Drill",
  "rentalDate": "2025-12-22T10:00:00Z",
  "dailyRate": 25.00
}
```

#### Get All Rentals
```http
GET /api/rentals
```

#### Get Rental by ID
```http
GET /api/rentals/{id}
```

#### Return Rental
```http
PUT /api/rentals/{id}/return
Content-Type: application/json

{
  "returnDate": "2025-12-25T10:00:00Z"
}
```

Returns the rental details with calculated total cost based on rental duration.

## Running the Application

1. Ensure SQL Server is running
2. Update the connection string in `appsettings.json`
3. Apply migrations: `dotnet ef database update`
4. Run the application: `dotnet run`
5. Access OpenAPI documentation at: `https://localhost:{port}/openapi/v1.json`

## Technologies Used

- **.NET 10**
- **ASP.NET Core Minimal APIs**
- **Entity Framework Core 10**
- **MS SQL Server**
- **OpenAPI/Swagger**

## Project Structure

```
WebClientApi/
├── Controllers/           # Traditional controllers (WeatherForecast)
├── Data/                 # Database context
│   └── RentalDbContext.cs
├── Features/             # Vertical slices
│   └── Rentals/         # Rental feature
├── Migrations/          # EF Core migrations
├── Properties/          # Launch settings
├── Program.cs           # Application entry point
├── appsettings.json     # Configuration
└── README.md           # This file
```

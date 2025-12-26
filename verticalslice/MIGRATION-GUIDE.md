# Database Migration Guide

This guide will help you create and apply the EF Core migrations for the Movie Rental database.

## Prerequisites

1. **.NET 10.0 SDK** installed
2. **SQL Server** running (LocalDB, Express, or full version)
3. **EF Core Tools** installed globally

### Install EF Core Tools

```bash
dotnet tool install --global dotnet-ef
```

Or update if already installed:

```bash
dotnet tool update --global dotnet-ef
```

## Quick Start (Recommended)

### Option 1: Use the Migration Script

From the `verticalslice` directory, run:

```bash
./create-migration.sh
```

This script will:
- Build both projects
- Create the InitialCreate migration
- Apply it to the database

### Option 2: Manual Commands

If you prefer to run commands manually:

```bash
# Navigate to the source directory
cd verticalslice/source

# Build the projects
dotnet build WebClientApi/WebClientApi.csproj
dotnet build MovieLibrary/MovieLibrary.csproj

# Create the migration
cd WebClientApi
dotnet ef migrations add InitialCreate --project ../MovieLibrary --startup-project .

# Apply the migration
dotnet ef database update --project ../MovieLibrary --startup-project .
```

## Connection String Configuration

The default connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MovieRentalDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

### Alternative Connection Strings

**For SQL Server LocalDB:**
```json
"Server=(localdb)\\mssqllocaldb;Database=MovieRentalDb;Trusted_Connection=true;MultipleActiveResultSets=true"
```

**For SQL Server with Username/Password:**
```json
"Server=localhost;Database=MovieRentalDb;User Id=yourUsername;Password=yourPassword;MultipleActiveResultSets=true;TrustServerCertificate=true"
```

**For SQL Server Express:**
```json
"Server=.\\SQLEXPRESS;Database=MovieRentalDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
```

## Verify Migration

After running the migration, verify it was created:

```bash
ls -la source/MovieLibrary/Migrations/
```

You should see:
- `YYYYMMDDHHMMSS_InitialCreate.cs`
- `YYYYMMDDHHMMSS_InitialCreate.Designer.cs`
- `MovieDbContextModelSnapshot.cs`

## Database Schema

The migration creates two tables:

### Movies Table
- `Id` (uniqueidentifier, PK)
- `Title` (nvarchar(200), required)
- `Director` (nvarchar(100))
- `ReleaseYear` (int)
- `Genre` (nvarchar(50))
- `Rating` (decimal(3,1))
- `Description` (nvarchar(1000))
- `CreatedAt` (datetime2)
- `UpdatedAt` (datetime2)
- `RowVersion` (rowversion)

### Rentals Table
- `Id` (uniqueidentifier, PK)
- `MovieId` (uniqueidentifier, FK → Movies.Id, required)
- `CustomerName` (nvarchar(200), required)
- `ItemName` (nvarchar(200), required)
- `RentalDate` (datetime2, required)
- `ReturnDate` (datetime2, nullable)
- `DailyRate` (decimal(18,2), required)
- `Status` (nvarchar(50), required)
- `RowVersion` (rowversion)

**Foreign Key:** `Rentals.MovieId` → `Movies.Id` with RESTRICT delete behavior

## Troubleshooting

### Build Errors

If you get build errors:

```bash
cd source/MovieLibrary
dotnet restore
dotnet build

cd ../WebClientApi
dotnet restore
dotnet build
```

### Connection Errors

1. Verify SQL Server is running
2. Test connection using SQL Server Management Studio or Azure Data Studio
3. Check the connection string in `appsettings.json`

### Migration Already Exists

To remove an existing migration:

```bash
cd source/WebClientApi
dotnet ef migrations remove --project ../MovieLibrary --startup-project .
```

### Drop and Recreate Database

```bash
cd source/WebClientApi
dotnet ef database drop --project ../MovieLibrary --startup-project .
dotnet ef database update --project ../MovieLibrary --startup-project .
```

## Useful Commands

### List all migrations
```bash
dotnet ef migrations list --project ../MovieLibrary --startup-project .
```

### Check database connection
```bash
dotnet ef database info --project ../MovieLibrary --startup-project .
```

### Generate SQL script (without applying)
```bash
dotnet ef migrations script --project ../MovieLibrary --startup-project . --output migration.sql
```

## Next Steps

After successfully creating the database:

1. Run the application:
   ```bash
   cd source/WebClientApi
   dotnet run
   ```

2. Access the Swagger UI at: `https://localhost:5001/swagger`

3. Test the API endpoints using Swagger or your preferred API client

## Support

For issues or questions, refer to:
- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- Project README.md

#!/bin/bash

# EF Core Migration Script for Movie Rental Database
# Run this script from the verticalslice directory

set -e  # Exit on any error

echo "========================================="
echo "Creating EF Core Migration"
echo "========================================="

# Navigate to the source directory
cd "$(dirname "$0")/source"

echo ""
echo "Step 1: Building projects..."
dotnet build WebClientApi/WebClientApi.csproj
dotnet build MovieLibrary/MovieLibrary.csproj

echo ""
echo "Step 2: Creating migration 'InitialCreate'..."
cd WebClientApi
dotnet ef migrations add InitialCreate --project ../MovieLibrary --startup-project .

echo ""
echo "Step 3: Applying migration to database..."
dotnet ef database update --project ../MovieLibrary --startup-project .

echo ""
echo "========================================="
echo "✅ Migration completed successfully!"
echo "========================================="
echo ""
echo "Database 'MovieRentalDb' has been created with:"
echo "  - Movies table"
echo "  - Rentals table"
echo "  - Foreign key relationship from Rentals to Movies"
echo ""
echo "You can now run the application with:"
echo "  cd WebClientApi"
echo "  dotnet run"
echo ""

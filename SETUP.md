# Concurrent Update Feature - Setup Guide

This guide covers the manual steps needed to complete the concurrent update feature implementation.

## What's Been Implemented

✅ Optimistic locking using RowVersion for Movies and Rentals
✅ Request/Response logging middleware with correlation ID tracking
✅ IP address tracking (source and host)
✅ Complete removal of MediatR - direct handler injection
✅ Serilog integration with Seq sink
✅ Database migration for RequestLog table
✅ Rental contracts (CreateRentalRequest, ReturnRentalRequest)
✅ BaseMovieRequest class for code reuse

## Manual Steps Required

### 1. Apply Database Migration

Run the migration to create the RequestLog table:

```bash
cd verticalslice/source/WebClientApi
dotnet ef database update --project ../MovieLibrary
```

This will create the `RequestLogs` table with indexes on `CorrelationId` and `Timestamp`.

### 2. Install and Run Seq

Seq is required for centralized logging. You have two options:

#### Option A: Docker (Recommended)

```bash
docker run --name seq -d --restart unless-stopped \
  -e ACCEPT_EULA=Y \
  -p 5341:80 \
  datalust/seq:latest
```

Access Seq UI at: http://localhost:5341

#### Option B: Download and Install

1. Download Seq from https://datalust.co/download
2. Install and run
3. Default runs on http://localhost:5341

### 3. Update appsettings.json (Optional)

If you need to change the Seq server URL, update `Program.cs`:

```csharp
.WriteTo.Seq("http://your-seq-server:5341")
```

### 4. Run the Application

```bash
cd verticalslice/source/WebClientApi
dotnet run
```

### 5. Test Concurrent Updates

The implementation includes optimistic locking for both Movies and Rentals. To test:

1. Get a movie: `GET /api/movies/{id}` - note the RowVersion in response
2. Update with correct RowVersion: `PUT /api/movies/{id}` - should succeed
3. Update with stale RowVersion: `PUT /api/movies/{id}` - should return 409 Conflict

Same pattern applies to rental returns: `PUT /api/rentals/{id}/return`

### 6. Verify Request Logging

All API requests are logged with:
- **Correlation ID**: X-Correlation-ID header (auto-generated if not provided)
- **Source IP**: Client IP (supports X-Forwarded-For for proxies)
- **Host IP**: Server IP where the API is running
- **Request/Response bodies**: Full payload logging
- **Timing**: Response time in milliseconds

Check:
- Database: Query `RequestLogs` table
- Seq UI: http://localhost:5341 (filter by CorrelationId)
- Console: Structured log output

## Architecture Changes Summary

### Before
- MediatR for request handling
- Handlers implemented IRequestHandler<TRequest, TResponse>
- Controllers injected IMediator
- Commands/Queries implemented IRequest<TResponse>

### After
- Direct handler injection
- Handlers have ExecuteAsync(request, cancellationToken) method
- Controllers inject specific handlers
- Commands/Queries are simple records (no interfaces)

### Example Controller Change

**Before:**
```csharp
public MoviesController(IMediator mediator) { ... }
var result = await _mediator.Send(command);
```

**After:**
```csharp
public MoviesController(AddMovie.Handler addHandler, UpdateMovie.Handler updateHandler, ...) { ... }
var result = await _addMovieHandler.ExecuteAsync(command);
```

## Testing

Run unit tests:
```bash
cd verticalslice/source/MovieLibrary.Tests
dotnet test
```

All tests have been updated to use `ExecuteAsync` instead of `Handle`.

## Troubleshooting

### Concurrency Exception Not Caught
- Ensure RowVersion is included in UpdateMovieRequest/ReturnRentalRequest
- Verify the controller has DbUpdateConcurrencyException handling

### Seq Not Receiving Logs
- Check Seq is running: http://localhost:5341
- Verify Serilog.Sinks.Seq package is installed
- Check Program.cs has `.WriteTo.Seq("http://localhost:5341")`

### RequestLog Not Saving
- Run the database migration
- Check MovieDbContext includes RequestLogs DbSet
- Verify middleware is registered in correct order

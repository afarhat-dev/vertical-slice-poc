using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using WebClientApi.Middleware;
using WebClientApi.Services;
using MovieLibrary.Data;
using MovieLibrary.Features.Movies;
using MovieLibrary.Features.Rentals;
using MovieLibrary.Repositories;
using static MovieLibrary.Features.Movies.AddMovie;
using static MovieLibrary.Features.Movies.UpdateMovie;
using static MovieLibrary.Features.Rentals.CreateRental;
using static MovieLibrary.Features.Rentals.ReturnRental;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MovieLibraryAPI")
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341") // Seq server URL
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// Add DbContext with SQL Server
builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IRentalRepository, RentalRepository>();

// Add Encryption Service
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();

// Register all handlers
builder.Services.AddScoped<AddMovie.Handler>();
builder.Services.AddScoped<UpdateMovie.Handler>();
builder.Services.AddScoped<DeleteMovie.Handler>();
builder.Services.AddScoped<GetMovieById.Handler>();
builder.Services.AddScoped<GetAllMovies.Handler>();
builder.Services.AddScoped<SearchMovies.Handler>();
builder.Services.AddScoped<CreateRental.Handler>();
builder.Services.AddScoped<ReturnRental.Handler>();
builder.Services.AddScoped<GetRentalById.Handler>();
builder.Services.AddScoped<GetAllRentals.Handler>();

// Add FluentValidation validators
builder.Services.AddScoped<IValidator<AddCommand>, AddMovie.Validator>();
builder.Services.AddScoped<IValidator<UpdateCommand>, UpdateMovie.Validator>();
builder.Services.AddScoped<IValidator<CreateRental.CreateRentalCommand>, CreateRental.Validator>();
builder.Services.AddScoped<IValidator<ReturnRental.Command>, ReturnRental.Validator>();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add middleware - ORDER MATTERS!
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting Movie Library API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

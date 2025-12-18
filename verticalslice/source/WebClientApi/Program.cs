using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Data;
using MovieLibrary.Features.Movies;
using static MovieLibrary.Features.Movies.AddMovie;
using static MovieLibrary.Features.Movies.UpdateMovie;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add DbContext with In-Memory database
builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseInMemoryDatabase("MovieLibraryDb"));

// Add MediatR - Register handlers from MovieLibrary
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MovieDbContext).Assembly));

// Add FluentValidation validators
builder.Services.AddScoped<IValidator<AddCommand>, AddMovie.Validator>();
builder.Services.AddScoped<IValidator<UpdateCommand>, UpdateMovie.Validator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

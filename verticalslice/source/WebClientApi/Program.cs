using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WebClientApi.Data;
using WebClientApi.Features.Movies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add DbContext with In-Memory database
builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseInMemoryDatabase("MovieLibraryDb"));

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add FluentValidation validators
builder.Services.AddScoped<IValidator<AddMovie.Command>, AddMovie.Validator>();
builder.Services.AddScoped<IValidator<UpdateMovie.Command>, UpdateMovie.Validator>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

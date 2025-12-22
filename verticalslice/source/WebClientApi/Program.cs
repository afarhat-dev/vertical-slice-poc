using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Data;
using MovieLibrary.Features.Movies;
using MovieLibrary.Features.Rentals;
using static MovieLibrary.Features.Movies.AddMovie;
using static MovieLibrary.Features.Movies.UpdateMovie;
using static MovieLibrary.Features.Rentals.CreateRental;
using static MovieLibrary.Features.Rentals.ReturnRental;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add DbContext with SQL Server
builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add MediatR - Register handlers from MovieLibrary
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MovieDbContext).Assembly));

// Add FluentValidation validators
builder.Services.AddScoped<IValidator<AddCommand>, AddMovie.Validator>();
builder.Services.AddScoped<IValidator<UpdateCommand>, UpdateMovie.Validator>();
builder.Services.AddScoped<IValidator<CreateRental.Command>, CreateRental.Validator>();
builder.Services.AddScoped<IValidator<ReturnRental.Command>, ReturnRental.Validator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

app.Run();

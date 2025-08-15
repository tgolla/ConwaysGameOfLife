using ConwaysGameOfLife.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddDatabaseDeveloperPageExceptionFilter(); // Only necessary with Microsoft.EntityFrameworkCore https://github.com/aspnet/Announcements/issues/432
builder.Services.AddDbContext<ConwaysGameOfLifeApiDbContext>(options =>
     options.UseSqlServer(configuration.GetConnectionString("ConwaysGameOfLifeApiDb")));

builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

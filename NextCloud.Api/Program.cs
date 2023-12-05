using NextCloud.Api.Extensions;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

configuration
    .AddJsonFile($"appsettings.{env}.json", true, true);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen().AddSettings(configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

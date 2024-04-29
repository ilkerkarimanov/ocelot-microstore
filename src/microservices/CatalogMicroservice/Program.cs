using Carter;
using CatalogMicroservice;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Middleware;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddMongoDb(configuration);
builder.Services.AddJwtAuthentication(configuration);
builder.Services.AddScoped<ICatalogRepository>(sp => new CatalogRepository(sp.GetService<IMongoDatabase>() ??
    throw new Exception("Mongo database cannot be initialized.")));
builder.Services.AddOpenApi();

builder.Services.AddHealthChecks();

builder.Services.AddCarter();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapCarter();

app.Map("/", () => { return "Hello from Catalog"; });

app.Run();

using CartMicroservice;
using Middleware;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddMongoDb(configuration);

builder.Services.AddJwtAuthentication(configuration);

builder.Services.AddScoped<ICartRepository>(sp => new CartRepository(sp.GetService<IMongoDatabase>() ??
    throw new Exception("Mongo database cannot be initialized.")));

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapCartEndpoints();

app.Map("/", () => { return "Hello from Cart!"; });

app.Run();

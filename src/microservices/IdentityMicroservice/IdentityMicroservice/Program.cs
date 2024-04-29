using IdentityMicroservice;
using Middleware;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMongoDb(configuration);
builder.Services.AddJwt(configuration);
builder.Services.AddTransient<IEncryptor, Encryptor>();
builder.Services.AddScoped<IUserRepository>(sp =>
    new UserRepository(sp.GetService<IMongoDatabase>() ??
        throw new Exception("IMongoDatabase not found"))
);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Map("/", () => { return "Hello from Identity"; });

app.Run();

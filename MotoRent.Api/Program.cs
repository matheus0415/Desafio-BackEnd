

using Microsoft.EntityFrameworkCore;
using MotoRent.Infrastructure.Persistence;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MotoRent API",
        Version = "v1",
        Description = "API para gerenciamento de aluguel de motos"
    });
    c.EnableAnnotations();
});
builder.Services.AddDbContext<MotoRent.Infrastructure.Persistence.MotoRentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Port=5432;Database=motorrent;Username=admin;Password=admin"));


var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};


app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();


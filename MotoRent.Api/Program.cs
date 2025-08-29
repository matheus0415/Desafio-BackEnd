

using Microsoft.EntityFrameworkCore;
using MotoRent.Domain.Services;
using MotoRent.Infrastructure.Persistence;
using MotoRent.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MotoRent API",
        Version = "v1",
        Description = "API for motorcycle rental system"
    });
});

// Configure Entity Framework
builder.Services.AddDbContext<MotoRentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure domain services
builder.Services.AddScoped<IRentalCalculationService, RentalCalculationService>();

// Configure infrastructure services
builder.Services.AddScoped<IMessagingService, RabbitMQService>();
builder.Services.AddScoped<IStorageService, LocalStorageService>();

// Configure background services
builder.Services.AddHostedService<MessageConsumerService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MotoRent API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MotoRentDbContext>();
    context.Database.Migrate();
}

app.Run();


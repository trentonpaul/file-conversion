using FileConversion.Api.Interfaces;
using FileConversion.Api.Services;
using FileConversion.Infrastructure;
using FileConversion.Infrastructure.Settings;
using FileConversion.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Enable CORS - allow all origins, methods and headers (for development / testing)
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()));

builder.Services.Configure<StorageSettings>(
    builder.Configuration.GetSection("Storage"));
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
builder.Services.AddScoped<IMessagePublisher, RabbitMqPublisher>();

// Singleton - one shared RabbitMQ connection per application lifetime
builder.Services.AddSingleton<RabbitMqConnectionFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (args.Contains("--migrate-only"))
    {
        db.Database.Migrate();
        return;
    }
}

app.UseExceptionHandler();
app.UseCors("AllowAll");
app.MapControllers();

// For health check
app.MapGet("/ping", () => Results.Ok("pong"));

app.Run();
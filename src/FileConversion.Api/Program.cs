
using FileConversion.Api.Services;
using FileConversion.Infrastructure;
using FileConversion.Infrastructure.Settings;
using FileConversion.Shared.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.Configure<StorageSettings>(
    builder.Configuration.GetSection("Storage"));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
    builder.Services.AddScoped<IJobRepository, SqliteJobRepository>();
    builder.Services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
}

builder.Services.AddScoped<IJobService, JobService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

app.MapControllers();

app.UseExceptionHandler();

app.Run();
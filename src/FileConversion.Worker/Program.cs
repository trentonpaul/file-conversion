using FileConversion.Infrastructure;
using Microsoft.EntityFrameworkCore;
using FileConversion.Infrastructure.Settings;
using FileConversion.Shared.Interfaces;
using FileConversion.Worker;
using FileConversion.Worker.Interfaces;
using FileConversion.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.Configure<StorageSettings>(
    builder.Configuration.GetSection("Storage"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IImageConverter, MagickImageConverter>();

builder.Services.AddSingleton<RabbitMqConnectionFactory>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
builder.Services.AddScoped<IConversionService, ConversionService>();

builder.Services.AddHostedService<ConversionWorker>();

var host = builder.Build();
host.Run();
using FileConversion.Worker;
using FileConversion.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IImageConverter, MagickImageConverter>();

var host = builder.Build();
host.Run();

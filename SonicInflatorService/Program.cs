using Serilog;
using SonicInflatorService;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/inflator_logs.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();
try
{
    Log.Information("Starting up...THE INFLATOR");
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddHostedService<Worker>();
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();

    var host = builder.Build();
    host.Run();
}
finally
{
    Log.CloseAndFlush();
}




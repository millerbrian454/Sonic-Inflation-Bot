using Serilog;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using SonicInflatorService.DependencyInjection;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/inflator_logs.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();
try
{
    Log.Information("Starting up...THE INFLATOR");

    IHost host = Host.CreateDefaultBuilder(args)
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .UseSerilog()
        .ConfigureAppConfiguration((context, config) =>
        {
            config
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
        })
        .ConfigureContainer<ContainerBuilder>(cb =>
        {
            cb.RegisterModule<BotModule>();
            cb.RegisterModule<EventHandlersModule>();
            cb.RegisterModule<ServicesModule>();
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Bot terminated.");
}
finally
{
    Log.CloseAndFlush();
}




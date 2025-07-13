using Serilog;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using SonicInflatorService.DependencyInjection;
using SonicInflatorService.Infrastructure.Data;

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

    // Seed database with configuration data
    using (var scope = host.Services.CreateScope())
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<SonicInflatorDbContext>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            Log.Information("Seeding database with configuration data...");
            await ConfigurationSeeder.SeedFromAppSettingsAsync(context, configuration);
            Log.Information("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred during database seeding");
        }
    }

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




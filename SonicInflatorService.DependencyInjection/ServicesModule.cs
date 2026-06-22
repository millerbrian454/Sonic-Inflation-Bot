using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SonicInflatorService.Core.Interfaces;
using SonicInflatorService.Infrastructure.Data;
using SonicInflatorService.Infrastructure.Services;
using Module = Autofac.Module;

namespace SonicInflatorService.DependencyInjection
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register DbContext
            builder.Register(c =>
            {
                var config = c.Resolve<IConfiguration>();
                var connectionString = config.GetConnectionString("DefaultConnection");
                var optionsBuilder = new DbContextOptionsBuilder<SonicInflatorDbContext>();
                optionsBuilder.UseSqlServer(connectionString);
                return new SonicInflatorDbContext(optionsBuilder.Options);
            }).InstancePerLifetimeScope();

            // Register Database Configuration Service
            builder
                .RegisterType<DatabaseConfigurationService>()
                .AsSelf()
                .InstancePerLifetimeScope();

            // Register Fallback Configuration Service as the main IConfigurationService
            builder
                .RegisterType<FallbackConfigurationService>()
                .As<IConfigurationService>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<OpenAiLlmService>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<OllamaService>()
                .AsSelf()
                .SingleInstance();

            builder
                .Register<ILlmService>(c =>
                {
                    var config = c.Resolve<IConfiguration>();
                    var useOllama = config.GetValue<bool>("OllamaConfig:UseOllama");

                    return useOllama
                        ? c.Resolve<OllamaService>()
                        : c.Resolve<OpenAiLlmService>();
                })
                .SingleInstance();

            builder
                .RegisterType<MessageHistoryService>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
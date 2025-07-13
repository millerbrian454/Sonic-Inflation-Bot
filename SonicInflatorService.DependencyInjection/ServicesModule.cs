using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SonicInflatorService.Core.Interfaces;
using SonicInflatorService.Infrastructure;
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

            // Register Configuration Service
            builder
                .RegisterType<DatabaseConfigurationService>()
                .As<IConfigurationService>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<OpenAiLlmService>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder
                .RegisterType<MessageHistoryService>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}

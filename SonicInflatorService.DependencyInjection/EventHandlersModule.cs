using Autofac;
using SonicInflatorService.Core;
using SonicInflatorService.Handlers;
using Module = Autofac.Module;

namespace SonicInflatorService.DependencyInjection
{
    public class EventHandlersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ReadyHandler>()
                   .As<IEventBinding>()
                   .As<IClientWatcher>()
                   .InstancePerLifetimeScope();

            builder.RegisterType<LogHandler>()
                   .As<IEventBinding>()
                   .InstancePerLifetimeScope();

            builder.RegisterType<MessageReceivedHandler>()
                   .As<IEventBinding>()
                   .InstancePerLifetimeScope();
        }
    }
}

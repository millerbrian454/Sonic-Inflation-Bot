using Autofac;
using SonicInflatorService.Core;
using SonicInflatorService.Handlers;
using SonicInflatorService.Handlers.EventHandlers;
using SonicInflatorService.Handlers.MessageProcessors;
using Module = Autofac.Module;

namespace SonicInflatorService.DependencyInjection
{
    public class EventHandlersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ReadyHandler>()
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();

            builder.RegisterType<LogHandler>()
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();

            builder.RegisterType<MessageReceivedHandler>()
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();

            builder.RegisterType<SonicInflationMessageProcessor>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterType<SonicDeflationMessageProcessor>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterType<SonichuMessageProcessor>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<MentionMessageProcessor>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<ChannelTracker>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}

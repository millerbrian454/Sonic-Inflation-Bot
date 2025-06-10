using Autofac;
using SonicInflatorService.Core;
using SonicInflatorService.Handlers;
using SonicInflatorService.Handlers.EventHandlers;
using SonicInflatorService.Handlers.MessageProcessors;
using SonicInflatorService.Infrastructure;
using Module = Autofac.Module;

namespace SonicInflatorService.DependencyInjection
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
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

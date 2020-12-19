using Microsoft.Extensions.DependencyInjection;
using Trl_3D.OpenTk.RenderCommands;

namespace Trl_3D.OpenTk
{
    public static class ServiceCollectionExtensions
    {
        public static void AddOpenTk(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient(RenderWindowFactory.Create);
                        
            serviceCollection
                .AddTransient<IRenderCommand, ClearColorCommand>()
                .AddTransient<ClearColorCommand>();

            serviceCollection
                .AddTransient<IRenderCommand, RenderTestTriagleCommand>()
                .AddTransient<RenderTestTriagleCommand>();

            serviceCollection
                .AddTransient<IRenderCommand, GrabScreenshotCommand>()
                .AddTransient<GrabScreenshotCommand>();

            serviceCollection.AddSingleton((serviceProvider) => new RenderCommandFactory(serviceProvider));
        }
    }
}

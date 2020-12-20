using Microsoft.Extensions.DependencyInjection;

namespace Trl_3D.OpenTk
{
    public static class ServiceCollectionExtensions
    {
        public static void AddOpenTk(this IServiceCollection serviceCollection)
        {
            // This needs to be a singleton otherwise there will be a new render window poping up 
            // for each instance
            serviceCollection.AddSingleton(RenderWindowFactory.Create);
        }
    }
}

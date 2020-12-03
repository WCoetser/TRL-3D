using Microsoft.Extensions.DependencyInjection;

namespace Trl_3D.OpenTk
{
    public static class ServiceCollectionExtensions
    {
        public static void AddOpenTk(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient(RenderWindowFactory.Create);
        }
    }
}

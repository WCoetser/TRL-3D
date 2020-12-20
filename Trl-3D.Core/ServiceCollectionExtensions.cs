using Microsoft.Extensions.DependencyInjection;
using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTrl3DCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IScene, Scene.Scene>();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTrl3DCore(this IServiceCollection serviceCollection)
        {
            // This needs to be a singleton because scene loaders need to acces the Assertion Update Channel,
            // and the scene is linked to a render window which is also a singleton.
            serviceCollection.AddSingleton<IScene, Scene.Scene>();
        }
    }
}

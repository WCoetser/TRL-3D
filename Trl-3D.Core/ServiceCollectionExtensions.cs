using Microsoft.Extensions.DependencyInjection;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Scene;
using Trl_3D.Core.Threading;

namespace Trl_3D.Core
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTrl3DCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IScene, Scene.Scene>();
            serviceCollection.AddSingleton<ICancellationTokenManager, CancellationTokenManager>();
            serviceCollection.AddSingleton<AssertionProcessor>();
        }
    }
}

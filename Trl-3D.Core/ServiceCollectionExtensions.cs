using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Trl_3D.Core.Scene;

namespace Trl_3D.Core
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTrl3DCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<CancellationTokenSource>();         
            serviceCollection.AddSingleton<SceneGraph>();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Trl_3D.Core.Threading;
using Trl_3D.Core.Abstractions;
using Trl_3D.OpenTk.Shaders;

namespace Trl_3D.OpenTk
{
    public static class ServiceCollectionExtensions
    {
        public static void AddOpenTk(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(RenderWindowFactory.Create);
            serviceCollection.AddSingleton<OpenGLSceneProcessor>();
            serviceCollection.AddSingleton<ICancellationTokenManager, CancellationTokenManager>();
            serviceCollection.AddSingleton<IShaderCompiler, ShaderCompiler>();
        }
    }
}

﻿using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Trl_3D.Core.Abstractions;
using Trl_3D.OpenTk.Shaders;
using Trl_3D.OpenTk.Textures;

namespace Trl_3D.OpenTk
{
    public static class ServiceCollectionExtensions
    {
        public static void AddOpenTk(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(RenderWindowFactory.Create);
            serviceCollection.AddSingleton<OpenGLSceneProcessor>();
            serviceCollection.AddSingleton<CancellationTokenSource>();
            serviceCollection.AddSingleton<IShaderCompiler, ShaderCompiler>();
            serviceCollection.AddSingleton<ITextureLoader, TextureLoader>();
            serviceCollection.AddSingleton<IAssertionProcessor, AssertionProcessor.AssertionProcessor>();
            serviceCollection.AddSingleton<GeometryBuffers.BufferManager>();
        }
    }
}

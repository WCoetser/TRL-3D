using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.Desktop;
using System;
using Trl_3D.Core.Abstractions;

namespace Trl_3D.OpenTk
{
    internal class RenderWindowFactory
    {
        public static IRenderWindow Create(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<RenderWindow>>();

            var settings = new GameWindowSettings
            {
                IsMultiThreaded = true
            };

            var nativeWindowSettings = new NativeWindowSettings
            {
                Title = "Trl-3D",
                APIVersion = new Version(4, 5), // OpenGL 4.4, June 2014
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL
            };
            
            var renderWindow = new RenderWindow(settings, nativeWindowSettings);
            renderWindow.SetLogger(logger);

            return renderWindow;
        }
    }
}

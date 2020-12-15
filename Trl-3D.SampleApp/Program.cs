using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Trl_3D.Core.Abstractions;
using Trl_3D.OpenTk;

namespace Trl_3D.SampleApp
{
    class Program
    {
        private static IServiceProvider serviceProvider = null;
        
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            try
            {
                IHost host = Host.CreateDefaultBuilder(args)
                    .ConfigureServices(ConfigureServices)
                    .Build();

                using (var serviceScope = host.Services.CreateScope())
                {
                    serviceProvider = serviceScope.ServiceProvider;
                    var renderWindow = serviceProvider.GetRequiredService<IRenderWindow>();
                    renderWindow.Run();
                }
            }
            finally
            {
                AppDomain.CurrentDomain.UnhandledException -= UnhandledExceptionHandler;
            }
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Action<Exception> dumpError = delegate (Exception ex)
            {
                string err = $"Error: {ex.Message}{Environment.NewLine}{ex.StackTrace}";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(err);
                Console.ResetColor();

                System.Diagnostics.Trace.WriteLine(err);
            };

            try
            {
                var logger = serviceProvider?.GetService<ILogger<Program>>();
                if (logger != null)
                {
                    logger.LogError((Exception)e.ExceptionObject, "Unhandled exception");
                }
                else
                {
                    dumpError((Exception)e.ExceptionObject);
                }
            }
            catch (Exception ex)
            {
                dumpError(ex);
            }
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddOpenTk();
            services.AddLogging(config => config.AddConsole());
            services.AddSingleton<ISceneLoader, SceneLoader>();
            services.AddSingleton<IEventProcessor, EventProcessor>();
        }
    }
}

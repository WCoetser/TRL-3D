using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;

using System;
using System.Threading;
using System.Threading.Tasks;

using Trl_3D.Core;
using Trl_3D.Core.Abstractions;
using Trl_3D.OpenTk;

namespace Trl_3D.SampleApp
{
    class Program
    {
        private static IServiceProvider serviceProvider;

        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            try
            {
                IHost host = Host.CreateDefaultBuilder(args)
                    .ConfigureServices(ConfigureServices)
                    .Build();

                using var serviceScope = host.Services.CreateScope();
                serviceProvider = serviceScope.ServiceProvider;

                // Cancelation token to stop all producers and consumers when program completes
                using var cancellationTokenSource = new CancellationTokenSource();

                // Produce scene elements (assertions) on seperate thread using producer-consumer pattern
                var loader = serviceProvider.GetRequiredService<IAssertionLoader>();
                var sceneProducer = Task.Run(async () => await loader.StartAssertionProducer(cancellationTokenSource.Token));

                // Consume scene elements (assertions) on seperate thread using producer-consumer pattern
                var scene = serviceProvider.GetRequiredService<IScene>();
                var sceneConsumer = Task.Run(async () => await scene.StartAssertionConsumer(cancellationTokenSource.Token));

                // Main UI thread
                var renderWindow = serviceProvider.GetRequiredService<IRenderWindow>();
                renderWindow.Run();

                // Wait for producer and consumer threads to terminate
                cancellationTokenSource.Cancel();
                await Task.WhenAll(sceneProducer, sceneConsumer);
            }
            finally
            {
                AppDomain.CurrentDomain.UnhandledException -= UnhandledExceptionHandler;
            }
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            static void dumpErrorNoLogger(Exception ex)
            {
                string err = $"Error: {ex.Message}{Environment.NewLine}{ex.StackTrace}";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(err);
                Console.ResetColor();

                System.Diagnostics.Trace.WriteLine(err);
            }

            try
            {
                var logger = serviceProvider?.GetService<ILogger<Program>>();
                if (logger != null)
                {
                    logger.LogError((Exception)e.ExceptionObject, "Unhandled exception");
                }
                else
                {
                    dumpErrorNoLogger((Exception)e.ExceptionObject);
                }
            }
            catch (Exception ex)
            {
                dumpErrorNoLogger(ex);
            }
        }

        public static void ConfigureServices(IServiceCollection services)
        {            
            // Logging
            var logger = new LoggerConfiguration()
                              .WriteTo.Console(Serilog.Events.LogEventLevel.Verbose)
                              .CreateLogger();
            services.AddLogging(builder => {
                builder.ClearProviders();
                builder.AddSerilog(logger);
            });

            // Library dependencies
            services.AddTrl3DCore();
            services.AddOpenTk();

            services.AddSingleton<IAssertionLoader, SceneLoader>();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;

using System;
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

            ICancellationTokenManager cancellationTokenManager = null;
            try
            {
                IHost host = Host.CreateDefaultBuilder(args)
                    .ConfigureServices(ConfigureServices)
                    .Build();

                using var serviceScope = host.Services.CreateScope();
                serviceProvider = serviceScope.ServiceProvider;
                cancellationTokenManager = serviceProvider.GetRequiredService<ICancellationTokenManager>();

                // Produce scene elements (assertions) on seperate thread using producer-consumer pattern
                var loader = serviceProvider.GetRequiredService<IAssertionLoader>();
                var sceneProducerTask = Task.Run(async () => await loader.StartAssertionProducer());

                // Consume scene elements (assertions) on seperate thread using producer-consumer pattern
                var scene = serviceProvider.GetRequiredService<IScene>();
                var sceneConsumerTask = Task.Run(async () => await scene.StartAssertionConsumer());

                // Events are processed on it's own thread
                var eventProcessor = serviceProvider.GetRequiredService<IEventProcessor>();
                var eventProcessorTask = Task.Run(async () => await eventProcessor.StartEventProcessor());

                // Main UI thread
                var renderWindow = serviceProvider.GetRequiredService<IRenderWindow>();
                renderWindow.Run();

                // Terminate producer/consumer threads
                cancellationTokenManager.CancelToken();

                // Await tasks to catch any lingering unhandled exceptions
                // ---
                // NB: This is expected to generate OperationCanceledException
                // ---
                await Task.WhenAll(sceneProducerTask, sceneConsumerTask, eventProcessorTask);
            }
            catch (OperationCanceledException e) 
                when (e.CancellationToken == cancellationTokenManager.CancellationToken
                    && cancellationTokenManager.IsCancellationRequested)
            {
                // This will be thrown when producer/consumer threads terminate due to cancellation tokens
                // and can be safely ignored.
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
            services.AddSingleton<IEventProcessor, EventProcessor>();
            services.AddSingleton<IImageLoader, ImageLoader>();
        }
    }
}

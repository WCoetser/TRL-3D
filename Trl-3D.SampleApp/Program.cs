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

            CancellationTokenSource cancellationTokenManager = null;
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .Build();

            using var serviceScope = host.Services.CreateScope();
            serviceProvider = serviceScope.ServiceProvider;

            try
            {
                cancellationTokenManager = serviceProvider.GetRequiredService<CancellationTokenSource>();

                // Produce scene elements (assertions) on seperate thread using producer-consumer pattern
                var loader = serviceProvider.GetRequiredService<IAssertionLoader>();
                var sceneProducerTask = Task.Run(async () => await loader.StartAssertionProducer());

                // Consume scene elements (assertions) on seperate thread using producer-consumer pattern
                var scene = serviceProvider.GetRequiredService<IAssertionProcessor>();
                var sceneConsumerTask = Task.Run(async () => await scene.StartAssertionConsumer());

                // Events are processed on it's own thread
                var eventProcessor = serviceProvider.GetRequiredService<IEventProcessor>();
                var eventProcessorTask = Task.Run(async () => await eventProcessor.StartEventProcessor());

                // Start animation thread for non-user inputs
                var animations = serviceProvider.GetRequiredService<Animation>();
                var animationsTask = Task.Run(async () => await animations.Start());

                // Main UI thread
                var renderWindow = serviceProvider.GetRequiredService<IRenderWindow>();
                renderWindow.Run();

                // Terminate producer/consumer threads
                cancellationTokenManager.Cancel();

                // Await tasks to catch any lingering unhandled exceptions
                // ---
                // NB: This is expected to generate OperationCanceledException
                // ---
                await Task.WhenAll(sceneProducerTask, sceneConsumerTask, eventProcessorTask, animationsTask);
            }
            catch (OperationCanceledException e) 
                when (e.CancellationToken == cancellationTokenManager.Token
                    && cancellationTokenManager.IsCancellationRequested)
            {
                // This will be thrown when producer/consumer threads terminate due to cancellation tokens
                // and can be safely ignored.
            }
            catch (Exception ex)
            {
                // Note: Exceptions thrown in the try block will count as being handled in certain cases,
                // ex. when the OpenGL version is not supported by the installed driver, therefore this is needed
                UnhandledExceptionHandler(null, new UnhandledExceptionEventArgs(ex, true));
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
            catch (Exception)
            {
                dumpErrorNoLogger((Exception)e.ExceptionObject);
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

            // Test class using the rest of the dependencies to do animations
            // on a seperate thread
            services.AddSingleton<Animation>();
        }
    }
}

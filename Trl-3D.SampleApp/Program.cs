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
        static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                try
                {
                    var myService = services.GetRequiredService<IRenderWindow>();
                    myService.Run();                    
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddOpenTk();
            services.AddLogging(config => config.AddConsole());
        }
    }
}

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace WebApp
{
    class Program
    {
        static void Main()
        {
            string s = null;
            Console.WriteLine(s);
            var builder = new WebHostBuilder()
                .UseUrls("http://localhost:4324")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseMonitoring()
                .ConfigureAppConfiguration((hostBuilderContext, confBuilder) =>
                {
                    confBuilder
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();

                })
                .UseKestrel()
                .UseStartup<Startup>();

            builder.Build().Run();
        }
    }
}

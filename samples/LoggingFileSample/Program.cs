using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using QuadriPlus.Extensions.Logging;

namespace LoggingFileSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Build().Run();
        }

        public static IWebHostBuilder BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => builder.AddFile())
                .UseStartup<Startup>();
    }
}

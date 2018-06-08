using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using QuadriPlus.Extensions.Logging;

namespace LoggingFileSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => builder.AddFile())
                .UseStartup<Startup>()
                .Build();
    }
}

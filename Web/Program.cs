using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BillionSongs {
    public class Program {
        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string settingsFile = Environment.GetEnvironmentVariable("BILLION_SONGS_SETTINGS");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"{settingsFile}.json", optional: false)
                .AddJsonFile($"{settingsFile}.{environment}.json", optional: true)
                .AddCommandLine(args)
                .AddEnvironmentVariables()
                .Build();
            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(configuration)
                .UseStartup<Startup>();
        }
    }
}

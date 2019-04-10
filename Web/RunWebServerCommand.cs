namespace BillionSongs {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ManyConsole.CommandLineUtils;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    public class RunWebServerCommand: ConsoleCommand {
        public static int Main(string[] args) {
            return ConsoleCommandDispatcher.DispatchCommand(
                ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(RunWebServerCommand)),
                args, consoleOut: Console.Error);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddCommandLine(args)
                .AddEnvironmentVariables()
                .Build();
            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(configuration)
                .UseStartup<Startup>();
        }

        public string Generator { get; set; }

        public RunWebServerCommand() {
            this.IsCommand("web");
            this.AllowsAnyAdditionalArguments();
        }

        public override int Run(string[] remainingArguments) {
            CreateWebHostBuilder(remainingArguments).Build().Run();
            return 0;
        }
    }
}

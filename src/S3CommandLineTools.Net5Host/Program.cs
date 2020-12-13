using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace S3CommandLineTools.Net5Host
{
    class Program
    {
        static int Main(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            var configuration = configurationBuilder.Build();


            IServiceCollection services = new ServiceCollection();
            services
                .AddLogging()
                .AddS3CommandLineTools()
                .AddSingleton<IConsole>(PhysicalConsole.Singleton)
                .Configure<S3CommandLineOption>(configuration.GetSection("S3CommandLineOption"));

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.ConfigureS3CommandLineTools();

            var app = new CommandLineApplication<Program>()
            {
                Name = "s3-cli",
                Description = "s3-cli tool",
                ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
            };

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(serviceProvider);
            app.VersionOption("-v|--version", AppConsts.Version);
            app
                .InfoCommandOption(serviceProvider, configuration)
                .ConfigCommand(serviceProvider, configuration)
                .SpeedCommand(serviceProvider)
                .AclCommand(serviceProvider)
                .ListCommand(serviceProvider)
                .DownloadCommand(serviceProvider)
                .UploadCommand(serviceProvider)
                .DeleteCommand(serviceProvider)
                .CopyCommand(serviceProvider)
                .GenerateUrlCommand(serviceProvider);

            return app.Execute(args);
        }

        private IConsole _console;
        public Program(IConsole console)
        {
            _console = console;
        }

        private int OnExecute()
        {
            _console.WriteLine("Welcome s3-cli !");
            return 0;
        }
    }
}

using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace S3CommandLineTools
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
                ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated
            };

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(serviceProvider);
            app.HelpOption();

            app
                .ConfigCommand(configuration)
                .AclCommand(serviceProvider)
                .ListBucketCommand(serviceProvider)
                .ListCommand(serviceProvider)
                .DownloadCommand(serviceProvider)
                .UploadCommand(serviceProvider)
                .UploadDefaultCommand(serviceProvider)
                .DeleteCommand(serviceProvider)
                .CopyCommand(serviceProvider)
                .GenerateUrlCommand(serviceProvider);

            return app.Execute(args);
        }


        private void OnExecute()
        {
            Console.WriteLine("Execute!!!");
        }

    }
}

using Amazon.S3.Multiplex;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace S3CommandLineTools
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddS3CommandLineTools(this IServiceCollection services, Action<S3CommandLineOption> configure = null)
        {
            if (configure == null)
            {
                configure = c =>
                {
                };
            }

            services
                .AddS3Multiplex()
                .AddS3MultiplexKS3Builder()
                .Configure<S3CommandLineOption>(configure)
                .AddSingleton<IS3CommandLineService, S3CommandLineService>()
                ;

            return services;
        }

    }
}

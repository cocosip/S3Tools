using AutoS3;
using AutoS3.KS3;
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
                configure = new Action<S3CommandLineOption>(o => { });
            }

            services
                .AddAutoS3()
                .AddAutoKS3()
                .Configure<S3CommandLineOption>(configure)
                .AddSingleton<IS3CommandLineService, S3CommandLineService>()
                ;

            return services;
        }

    }
}

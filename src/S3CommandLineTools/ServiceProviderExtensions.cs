using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace S3CommandLineTools
{
    public static class ServiceProviderExtensions
    {
        public static IServiceProvider ConfigureS3CommandLineTools(this IServiceProvider provider)
        {
            var option = provider.GetService<IOptions<S3CommandLineOption>>().Value;

            if (string.IsNullOrWhiteSpace(option.TemporaryPath))
            {
                option.TemporaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            }

            if (!Directory.Exists(option.TemporaryPath))
            {
                Directory.CreateDirectory(option.TemporaryPath);
            }

            return provider;
        }
    }
}

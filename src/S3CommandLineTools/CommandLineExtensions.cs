using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace S3CommandLineTools
{
    public static class CommandLineExtensions
    {
        /// <summary>Config command
        /// </summary>
        public static CommandLineApplication InfoCommandOption(this CommandLineApplication app, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var console = serviceProvider.GetService<IConsole>();
            var infoOption = app.Option("-i|--info", $"Info", CommandOptionType.NoValue);
            app.OnExecute(() =>
            {
                if (infoOption.Values != null && infoOption.Values.Any())
                {
                    console.WriteLine("--- S3-CLI ---");
                    console.WriteLine("Version:");
                    console.WriteLine(AppConsts.Version);
                    console.WriteLine("");
                    console.WriteLine("Config:");
                    console.WriteLine("Vendor:{0}", configuration.GetSection("S3CommandLineOption")["Vendor"]);
                    console.WriteLine("AccessKeyId:{0}", configuration.GetSection("S3CommandLineOption")["AccessKeyId"]);
                    console.WriteLine("SecretAccessKey:{0}", configuration.GetSection("S3CommandLineOption")["SecretAccessKey"]);
                    console.WriteLine("ServerUrl:{0}", configuration.GetSection("S3CommandLineOption")["ServerUrl"]);
                    console.WriteLine("DefaultBucket:{0}", configuration.GetSection("S3CommandLineOption")["DefaultBucket"]);
                    console.WriteLine("ForcePathStyle:{0}", configuration.GetSection("S3CommandLineOption")["ForcePathStyle"]);
                    console.WriteLine("SignatureVersion:{0}", configuration.GetSection("S3CommandLineOption")["SignatureVersion"]);
                    console.WriteLine("TemporaryPath:{0}", configuration.GetSection("S3CommandLineOption")["TemporaryPath"]);
                }
            });
            return app;
        }

        /// <summary>Config command
        /// </summary>
        public static CommandLineApplication ConfigCommand(this CommandLineApplication app, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var console = serviceProvider.GetService<IConsole>();
            app.Command("config", commandLine =>
            {
                commandLine.ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated;
                commandLine.Description = "Config some global configuration,such as 'ak','sk'...";
                //set config value
                //s3-cli config info
                //commandLine.Command("info", configCommand =>
                //{
                //    configCommand.OnExecute(() =>
                //    {
                //        Console.WriteLine("--- s3-cli info ---");
                //        Console.WriteLine("Vendor:{0}", configuration.GetSection("S3CommandLineOption")["Vendor"]);
                //        Console.WriteLine("AccessKeyId:{0}", configuration.GetSection("S3CommandLineOption")["AccessKeyId"]);
                //        Console.WriteLine("SecretAccessKey:{0}", configuration.GetSection("S3CommandLineOption")["SecretAccessKey"]);
                //        Console.WriteLine("ServerUrl:{0}", configuration.GetSection("S3CommandLineOption")["ServerUrl"]);
                //        Console.WriteLine("DefaultBucket:{0}", configuration.GetSection("S3CommandLineOption")["DefaultBucket"]);
                //        Console.WriteLine("ForcePathStyle:{0}", configuration.GetSection("S3CommandLineOption")["ForcePathStyle"]);
                //        Console.WriteLine("SignatureVersion:{0}", configuration.GetSection("S3CommandLineOption")["SignatureVersion"]);
                //        Console.WriteLine("TemporaryPath:{0}", configuration.GetSection("S3CommandLineOption")["TemporaryPath"]);
                //    });
                //});

                //s3-cli config set -ak 123 -sk 232323 -s http://192.168.0.3 -f true -sv 2.0
                commandLine.Command("set", configCommand =>
                {
                    var vendorOption = configCommand.Option("-v|--vendor", "Set vendor(Amazon|KS3)", CommandOptionType.SingleOrNoValue);
                    var accessKeyIdOption = configCommand.Option("-ak|--accesskey_id", "Set AccessKeyId", CommandOptionType.SingleOrNoValue);
                    var secretAccessKeyOption = configCommand.Option("-sk|--secret_accesskey", "Set SecretAccessKey", CommandOptionType.SingleOrNoValue);
                    var serverUrlOption = configCommand.Option("-s|--server_url", "Set ServerUrl", CommandOptionType.SingleOrNoValue);
                    var defaultBucketOption = configCommand.Option("-b|--bucket", "Set DefaultBucket", CommandOptionType.SingleOrNoValue);
                    var forcePathStyleOption = configCommand.Option<bool>("-f|--force_path", "Set ForcePathStyle", CommandOptionType.SingleOrNoValue);
                    var signatureVersionOption = configCommand.Option("-sv|--sign_version", "Set SignatureVersion(2.0)", CommandOptionType.SingleOrNoValue);
                    var temporaryPathOption = configCommand.Option("-t|--temporary", "Set TemporaryPath", CommandOptionType.SingleOrNoValue);

                    configCommand.OnExecute(() =>
                    {
                        //读取配置信息

                        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                        var s3CommandLineOption = new S3CommandLineOption()
                        {
                            Vendor = vendorOption.HasValue() ? vendorOption.Value() : configuration.GetSection("S3CommandLineOption")["Vendor"],
                            AccessKeyId = accessKeyIdOption.HasValue() ? accessKeyIdOption.Value() : configuration.GetSection("S3CommandLineOption")["AccessKeyId"],
                            SecretAccessKey = secretAccessKeyOption.HasValue() ? secretAccessKeyOption.Value() : configuration.GetSection("S3CommandLineOption")["SecretAccessKey"],
                            ServerUrl = serverUrlOption.HasValue() ? serverUrlOption.Value() : configuration.GetSection("S3CommandLineOption")["ServerUrl"],
                            DefaultBucket = defaultBucketOption.HasValue() ? defaultBucketOption.Value() : configuration.GetSection("S3CommandLineOption")["DefaultBucket"],
                            ForcePathStyle = forcePathStyleOption.HasValue() ? forcePathStyleOption.ParsedValue : true,
                            SignatureVersion = signatureVersionOption.HasValue() ? signatureVersionOption.Value() : "",
                            TemporaryPath = temporaryPathOption.HasValue() ? temporaryPathOption.Value() : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data")
                        };

                        var serializeOption = new SerializeOption()
                        {
                            S3CommandLineOption = s3CommandLineOption
                        };

                        var json = JsonSerializer.Serialize(serializeOption);
                        File.WriteAllBytes(filePath, Encoding.UTF8.GetBytes(json));

                        console.WriteLine("update appsetting!");
                    });

                });
            });
            return app;
        }

        /// <summary>Config command
        /// </summary>
        public static CommandLineApplication SpeedCommand(this CommandLineApplication app, IServiceProvider serviceProvider)
        {
            var s3CommandLineService = serviceProvider.GetService<IS3CommandLineService>();
            var option = serviceProvider.GetService<IOptions<S3CommandLineOption>>().Value;

            app.Command("speed", commandLine =>
            {
                commandLine.ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated;
                commandLine.Description = "Test speed";

                var bucketOption = commandLine.Option("-b|--bucket", $"BucketName,default is '{option.DefaultBucket}'", CommandOptionType.SingleOrNoValue);
                var fileSizeOption = commandLine.Option<int>("-s|--size", "Test file size", CommandOptionType.SingleOrNoValue);
                var fileCountOption = commandLine.Option<int>("-c|--count", "Test file count", CommandOptionType.SingleOrNoValue);
                var autoDeleteOption = commandLine.Option<bool>("-a|--autodel", "Auto delete the upload files(default is true)", CommandOptionType.SingleOrNoValue);

                commandLine.OnExecuteAsync((cancellationToken) =>
                {
                    var bucket = bucketOption.HasValue() ? bucketOption.Value() : option.DefaultBucket;
                    var fileSize = fileSizeOption.HasValue() ? fileSizeOption.ParsedValue : 1024 * 512;
                    var fileCount = fileCountOption.HasValue() ? fileCountOption.ParsedValue : 100;
                    var autoDelete = autoDeleteOption.HasValue() ? autoDeleteOption.ParsedValue : true;
                    return s3CommandLineService.TestSpeedAsync(bucket, fileSize, fileCount, autoDelete);
                });

            });
            return app;
        }

        /// <summary>List
        /// </summary>
        public static CommandLineApplication ListCommand(this CommandLineApplication app, IServiceProvider serviceProvider)
        {
            var s3CommandLineService = serviceProvider.GetService<IS3CommandLineService>();
            var option = serviceProvider.GetService<IOptions<S3CommandLineOption>>().Value;

            //s3-cli list
            app.Command("list", commandLine =>
            {
                commandLine.ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated;
                commandLine.Description = "List bucket or object";

                //s3-cli list bucket 
                commandLine.Command("bucket", listCommand =>
                {
                    listCommand.ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated;
                    listCommand.Description = "List bucket";
                    listCommand.OnExecuteAsync((cancellationToken) =>
                    {
                        return s3CommandLineService.ListBucketAsync();
                    });
                });

                //s3-cli list object
                commandLine.Command("object", listCommand =>
                {
                    listCommand.ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated;
                    listCommand.Description = "List object ";

                    var bucketOption = listCommand.Option("-b|--bucket", $"BucketName,default is '{option.DefaultBucket}'", CommandOptionType.SingleOrNoValue);
                    var maxOption = listCommand.Option<int>("-m|--max", "Max objects count", CommandOptionType.SingleOrNoValue);
                    var prefixOption = listCommand.Option("-p|--prefix", "Prefix", CommandOptionType.SingleOrNoValue);
                    var delimiterOption = listCommand.Option("-d|--delimiter", "Delimiter", CommandOptionType.SingleOrNoValue);

                    listCommand.OnExecuteAsync((cancellationToken) =>
                    {
                        var bucket = bucketOption.HasValue() ? bucketOption.Value() : option.DefaultBucket;
                        var max = maxOption.HasValue() ? maxOption.ParsedValue : 10;
                        var prefix = prefixOption.HasValue() ? prefixOption.Value() : "";
                        var delimiter = delimiterOption.HasValue() ? delimiterOption.Value() : "";
                        return s3CommandLineService.ListObjectV2Async(bucket, max, prefix, delimiter);
                    });
                });

            });
            return app;
        }

        /// <summary>Acl
        /// </summary>
        public static CommandLineApplication AclCommand(this CommandLineApplication app, IServiceProvider serviceProvider)
        {
            var s3CommandLineService = serviceProvider.GetService<IS3CommandLineService>();
            var option = serviceProvider.GetService<IOptions<S3CommandLineOption>>().Value;

            //s3-cli acl
            app.Command("acl", commandLine =>
            {
                commandLine.ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated;
                commandLine.Description = "Get or set bucket acls";

                commandLine.Command("get", aclCommand =>
                {
                    var bucketOption = aclCommand.Option("-b|--bucket", $"BucketName,default is '{option.DefaultBucket}'", CommandOptionType.SingleOrNoValue);
                    var objectKeyOption = aclCommand.Option("-k|--key", $"Object key", CommandOptionType.SingleOrNoValue);

                    aclCommand.OnExecuteAsync((cancellationToken) =>
                    {
                        var bucket = bucketOption.HasValue() ? bucketOption.Value() : option.DefaultBucket;
                        var objectKey = objectKeyOption.HasValue() ? objectKeyOption.Value() : "";
                        return s3CommandLineService.GetAclAsync(bucket, objectKey);
                    });
                });

            });
            return app;
        }

        /// <summary>download
        /// </summary>
        public static CommandLineApplication DownloadCommand(this CommandLineApplication app, IServiceProvider serviceProvider)
        {
            var s3CommandLineService = serviceProvider.GetService<IS3CommandLineService>();
            var option = serviceProvider.GetService<IOptions<S3CommandLineOption>>().Value;

            //s3-cli download
            app.Command("download", commandLine =>
            {
                commandLine.ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated;
                commandLine.Description = "Download objects to temp path";

                var bucketOption = commandLine.Option("-b|--bucket", $"BucketName,default is '{option.DefaultBucket}'", CommandOptionType.SingleOrNoValue);
                var objectKeysOption = commandLine.Option("-k|--key <KEYS>", "Download key(s)", CommandOptionType.MultipleValue);

                commandLine.OnExecuteAsync((cancellationToken) =>
                {
                    var bucket = bucketOption.HasValue() ? bucketOption.Value() : option.DefaultBucket;
                    return s3CommandLineService.DownloadObjectAsync(bucket, objectKeysOption.Values.ToArray());
                });
            });
            return app;
        }

        /// <summary>upload
        /// </summary>
        public static CommandLineApplication UploadCommand(this CommandLineApplication app, IServiceProvider serviceProvider)
        {
            var s3CommandLineService = serviceProvider.GetService<IS3CommandLineService>();
            var option = serviceProvider.GetService<IOptions<S3CommandLineOption>>().Value;

            //s3-cli download
            app.Command("upload", commandLine =>
            {
                commandLine.ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated;
                commandLine.Description = "Upload objects from path or dynamic create objects and upload";

                commandLine.Command("file", uploadCommand =>
                {
                    uploadCommand.ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated;
                    uploadCommand.Description = "Upload objects from path or dir.";

                    var bucketOption = uploadCommand.Option("-b|--bucket", $"BucketName,default is '{option.DefaultBucket}'", CommandOptionType.SingleOrNoValue);

                    var filePathsOption = uploadCommand.Option("-p|--path <PATHS>", "Upload file path(s)", CommandOptionType.MultipleValue);

                    var autoDeleteOption = uploadCommand.Option<bool>("-a|--autodel", "Auto delete the upload files(default is true)", CommandOptionType.SingleOrNoValue);

                    var dirOption = uploadCommand.Option("-d|--dir", $"Directory path to upload.", CommandOptionType.SingleOrNoValue);

                    uploadCommand.OnExecuteAsync((cancellationToken) =>
                    {
                        var bucket = bucketOption.HasValue() ? bucketOption.Value() : option.DefaultBucket;
                        var autoDelete = autoDeleteOption.HasValue() ? autoDeleteOption.ParsedValue : true;

                        var filePaths = new string[] { };
                        if (filePathsOption.HasValue())
                        {
                            filePaths = filePathsOption.Values.ToArray();
                        }
                        else
                        {
                            if (dirOption.HasValue())
                            {
                                filePaths = Directory.GetFiles(dirOption.Value());
                            }
                        }

                        return s3CommandLineService.UploadObjectAsync(bucket, autoDelete, filePaths);
                    });
                });

                commandLine.Command("default", uploadCommand =>
                {
                    uploadCommand.ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated;
                    uploadCommand.Description = "Upload default object.";

                    var bucketOption = uploadCommand.Option("-b|--bucket", $"BucketName,default is '{option.DefaultBucket}'", CommandOptionType.SingleOrNoValue);
                    var autoDeleteOption = uploadCommand.Option<bool>("-a|--autodel", "Auto delete the upload files(default is true)", CommandOptionType.SingleOrNoValue);
                    var fileSizeOption = uploadCommand.Option<int>("-s|--size", "Upload file size(dynamic generate object use this size.)", CommandOptionType.SingleOrNoValue);

                    uploadCommand.OnExecuteAsync((cancellationToken) =>
                    {
                        var bucket = bucketOption.HasValue() ? bucketOption.Value() : option.DefaultBucket;
                        var autoDelete = autoDeleteOption.HasValue() ? autoDeleteOption.ParsedValue : true;
                        var fileSize = fileSizeOption.HasValue() ? fileSizeOption.ParsedValue : 1024 * 1024;
                        return s3CommandLineService.UploadDefaultObjectAsync(bucket, autoDelete, fileSize);
                    });
                });

            });
            return app;
        }

        /// <summary>delete
        /// </summary>
        public static CommandLineApplication DeleteCommand(this CommandLineApplication app, IServiceProvider serviceProvider)
        {
            var s3CommandLineService = serviceProvider.GetService<IS3CommandLineService>();
            var option = serviceProvider.GetService<IOptions<S3CommandLineOption>>().Value;

            //s3-cli del
            app.Command("del", commandLine =>
            {
                commandLine.ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated;
                commandLine.Description = "Delete objects";

                var bucketOption = commandLine.Option("-b|--bucket", $"BucketName,default is '{option.DefaultBucket}'", CommandOptionType.SingleOrNoValue);
                var objectKeysOption = commandLine.Option("-k|--key <KEYS>", "Delete key(s)", CommandOptionType.MultipleValue);

                commandLine.OnExecuteAsync((cancellationToken) =>
                {
                    var bucket = bucketOption.HasValue() ? bucketOption.Value() : option.DefaultBucket;
                    return s3CommandLineService.DeleteObjectAsync(bucket, objectKeysOption.Values.ToArray());
                });
            });
            return app;
        }

        /// <summary>copy
        /// </summary>
        public static CommandLineApplication CopyCommand(this CommandLineApplication app, IServiceProvider serviceProvider)
        {
            var s3CommandLineService = serviceProvider.GetService<IS3CommandLineService>();
            var option = serviceProvider.GetService<IOptions<S3CommandLineOption>>().Value;

            //s3-cli del
            app.Command("copy", commandLine =>
            {
                commandLine.ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated;
                commandLine.Description = "Copy object";

                var sourceBucketOption = commandLine.Option("-sb|--sourcebucket", $"Source bucket name,default is '{option.DefaultBucket}'", CommandOptionType.SingleOrNoValue);
                var destBucketOption = commandLine.Option("-db|--destbucket", $"Destination bucket name,default is '{option.DefaultBucket}'", CommandOptionType.SingleOrNoValue);

                var sourceKeyOption = commandLine.Option("-sk|--sourcekey", "Source key", CommandOptionType.SingleValue);
                var destKeyOption = commandLine.Option("-dk|--destkey", "Destination key", CommandOptionType.SingleValue);

                commandLine.OnExecuteAsync((cancellationToken) =>
                {
                    var sourceBucket = sourceBucketOption.HasValue() ? sourceBucketOption.Value() : option.DefaultBucket;
                    var destBucket = destBucketOption.HasValue() ? destBucketOption.Value() : option.DefaultBucket;

                    return s3CommandLineService.CopyObjectAsync(sourceBucket, destBucket, sourceKeyOption.Value(), destKeyOption.Value());
                });
            });
            return app;
        }

        /// <summary>GenerateUrl
        /// </summary>
        public static CommandLineApplication GenerateUrlCommand(this CommandLineApplication app, IServiceProvider serviceProvider)
        {
            var s3CommandLineService = serviceProvider.GetService<IS3CommandLineService>();
            var option = serviceProvider.GetService<IOptions<S3CommandLineOption>>().Value;

            //s3-cli gen 
            app.Command("gen", commandLine =>
            {
                commandLine.ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated;
                commandLine.Description = "Generate pre signed url";

                var bucketOption = commandLine.Option("-b|--bucket", $"BucketName,default is '{option.DefaultBucket}'", CommandOptionType.SingleOrNoValue);
                var objectKeyOption = commandLine.Option("-k|--key", "Object key", CommandOptionType.SingleValue);
                var expiresSecondOption = commandLine.Option<int>("-t|--expires", "Url expires seconds", CommandOptionType.SingleOrNoValue);

                commandLine.OnExecute(() =>
                {
                    var bucket = bucketOption.HasValue() ? bucketOption.Value() : option.DefaultBucket;
                    var objectKey = objectKeyOption.Value();
                    var expiresSecond = expiresSecondOption.HasValue() ? expiresSecondOption.ParsedValue : 60;
                    var expires = DateTime.Now.AddSeconds(expiresSecond);
                    s3CommandLineService.GeneratePreSignedURL(bucket, objectKey, expires);
                });
            });
            return app;

        }


    }
}

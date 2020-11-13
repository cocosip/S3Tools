using Amazon.S3;
using Amazon.S3.Model;
using AmazonKS3;
using AutoS3;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace S3CommandLineTools
{
    public class S3CommandLineService : IS3CommandLineService
    {
        private readonly S3CommandLineOption _option;
        private readonly IS3ClientFactory _s3ClientFactory;
        private readonly IConsole _console;
        public S3CommandLineService(IOptions<S3CommandLineOption> options, IS3ClientFactory s3ClientFactory, IConsole console)
        {
            _option = options.Value;
            _s3ClientFactory = s3ClientFactory;
            _console = console;
        }

        /// <summary>List buckets
        /// </summary>
        public async Task ListBucketAsync()
        {
            _console.WriteLine("--- List buckets ---");
            var client = GetClient();
            var listBucketsResponse = await client.ListBucketsAsync();
            foreach (var bucket in listBucketsResponse.Buckets)
            {
                _console.WriteLine("Bucket name:{0}.", bucket.BucketName);
            }
            _console.WriteLine("--- End of list buckets ---");
        }

        /// <summary>获取Bucket权限
        /// </summary>
        public async Task GetAclAsync(string bucket, string objectKey = "")
        {
            _console.WriteLine("--- Get acl ---");
            var client = GetClient();

            var getACLRequest = new GetACLRequest()
            {
                BucketName = bucket
            };

            if (!string.IsNullOrWhiteSpace(objectKey))
            {
                getACLRequest.Key = objectKey;
            }

            var getACLResponse = await client.GetACLAsync(getACLRequest);
            foreach (var grant in getACLResponse.AccessControlList.Grants)
            {
                Console.WriteLine("Current bucket acl:{0}", grant.Permission.Value);
            }
            _console.WriteLine("--- End of get acl ---");
        }

        //public async Task PutAclAsync(string bucket, string objectKey = "")
        //{
        //    _console.WriteLine("--- Set acl ---");
        //    var client = GetClient();

        //    var putACLRequest = new PutACLRequest()
        //    {
        //        BucketName = bucket,
        //        AccessControlList=new S3AccessControlList()
        //        {
        //            Grants=new List<S3Grant>()
        //            {

        //            }
        //        }
        //    };

        //    if (!string.IsNullOrWhiteSpace(objectKey))
        //    {
        //        putACLRequest.Key = objectKey;
        //    }

        //    var putACLResponse = await client.PutACLAsync(putACLRequest);
        //    _console.WriteLine("--- End of set acl ---");
        //}


        /// <summary>List objects
        /// </summary>
        public async Task ListObjectV2Async(string bucket, int max, string prefix = "", string delimiter = "")
        {
            _console.WriteLine("--- List objects ---");
            var client = GetClient();

            var listObjectsV2Request = new ListObjectsV2Request()
            {
                BucketName = bucket,
                MaxKeys = max,
                Prefix = prefix,
            };
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                listObjectsV2Request.Prefix = prefix;
            }
            if (!string.IsNullOrWhiteSpace(delimiter))
            {
                listObjectsV2Request.Delimiter = delimiter;
            }

            var listObjectsV2Response = await client.ListObjectsV2Async(listObjectsV2Request);
            foreach (var s3Object in listObjectsV2Response.S3Objects)
            {
                Console.WriteLine("Key:{0}", s3Object.Key);
            }
            _console.WriteLine("--- End of list objects ---");
        }

        /// <summary>Download object to temp path
        /// </summary>
        public async Task DownloadObjectAsync(string bucket, params string[] objectKeys)
        {
            _console.WriteLine("--- Download object ---");
            var client = GetClient();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                foreach (var objectKey in objectKeys)
                {
                    var getObjectRequest = new GetObjectRequest()
                    {
                        BucketName = bucket,
                        Key = objectKey
                    };
                    var getObjectResponse = await client.GetObjectAsync(getObjectRequest);
                    var ext = Util.GetPathExtension(objectKey);
                    var filePath = Path.Combine(_option.TemporaryPath, $"{Guid.NewGuid()}{ext}");
                    await getObjectResponse.WriteResponseStreamToFileAsync(filePath, false, CancellationToken.None);
                    _console.WriteLine("Download from '{0}' and save to '{1}' .", objectKey, filePath);
                }
            }
            finally
            {
                stopwatch.Stop();
            }

            Console.WriteLine("Download '{0}' files, it cost '{1}'ms, ", objectKeys.Length, stopwatch.Elapsed.TotalMilliseconds);
            _console.WriteLine("--- End of download object ---");
        }

        /// <summary>Upload objects
        /// </summary>
        public async Task UploadObjectAsync(string bucket, bool autoDelete = true, params string[] filePaths)
        {
            _console.WriteLine("--- Upload object ---");

            var client = GetClient();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var objectKeys = new List<string>();
            try
            {
                if (filePaths.Any())
                {
                    foreach (var filePath in filePaths)
                    {
                        var ext = Util.GetPathExtension(filePath);
                        var objectKey = $"S3CommandLineTest/{Guid.NewGuid()}{ext}";
                        var length = Util.GetFileLength(filePath);
                        if (length > 1024 * 1024 * 5)
                        {
                            await MultipartUploadAsync(bucket, objectKey, filePath: filePath);
                        }
                        else
                        {
                            await SimpleUploadAsync(bucket, objectKey, filePath: filePath);
                        }
                        objectKeys.Add(objectKey);
                    }
                }
            }
            finally
            {
                stopwatch.Stop();
            }

            if (autoDelete)
            {
                await DeleteObjectAsync(bucket, objectKeys.ToArray());
            }

            Console.WriteLine("Upload '{0}' files, it cost '{1}'ms, ", filePaths.Length, stopwatch.Elapsed.TotalMilliseconds);

            _console.WriteLine("--- End of upload object ---");
        }


        /// <summary>Upload default objects
        /// </summary>
        public async Task UploadDefaultObjectAsync(string bucket, bool autoDelete = true, int fileSize = 1024 * 1024)
        {
            _console.WriteLine("--- Upload default object ---");

            var client = GetClient();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var objectKey = "";
            try
            {
                objectKey = $"S3CommandLineTest/{Guid.NewGuid()}.txt";
                var buffer = new byte[fileSize];
                var stream = new MemoryStream(buffer);

                if (fileSize > 1024 * 1024 * 5)
                {
                    await MultipartUploadAsync(bucket, objectKey, stream: stream);
                }
                else
                {
                    await SimpleUploadAsync(bucket, objectKey, stream: stream);
                }
            }
            finally
            {
                stopwatch.Stop();
            }

            if (autoDelete)
            {
                await DeleteObjectAsync(bucket, objectKey);
            }
            Console.WriteLine("Upload default file '{0}', it cost '{1}'ms, ", objectKey, stopwatch.Elapsed.TotalMilliseconds);
            _console.WriteLine("--- End of upload default object ---");
        }

        /// <summary>Delete objects
        /// </summary>
        public async Task DeleteObjectAsync(string bucket, params string[] objectKeys)
        {
            _console.WriteLine("--- Delete object ---");
            var client = GetClient();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                foreach (var objectKey in objectKeys)
                {
                    var deleteObjectRequest = new DeleteObjectRequest()
                    {
                        BucketName = bucket,
                        Key = objectKey
                    };

                    var deleteObjectResponse = await client.DeleteObjectAsync(deleteObjectRequest);
                    _console.WriteLine("Delete object '{0}' .", objectKey);
                }
            }
            finally
            {
                stopwatch.Stop();
            }
            Console.WriteLine("Delete '{0}' files, it cost '{1}'ms, ", objectKeys.Length, stopwatch.Elapsed.TotalMilliseconds);
            _console.WriteLine("--- End of delete object ---");
        }

        /// <summary>Copy object
        /// </summary>
        public async Task CopyObjectAsync(string sourceBucket, string destinationBucket, string sourceKey, string destinationKey)
        {
            _console.WriteLine("--- Copy object ---");
            var copyObjectResponse = await GetClient().CopyObjectAsync(new CopyObjectRequest()
            {
                SourceBucket = sourceBucket,
                DestinationBucket = destinationBucket,
                SourceKey = sourceKey,
                DestinationKey = destinationKey
            });

            _console.WriteLine("Copy file success,sourcekey:{0},destinationKey", copyObjectResponse.ResponseMetadata.RequestId, destinationKey);

            _console.WriteLine("--- End of copy object ---");

        }

        /// <summary>GetPreSignedURL
        /// </summary>
        public void GetPreSignedURL(string bucket, string objectKey, DateTime expires)
        {
            _console.WriteLine("--- GetPreSignedURL ---");
            var client = GetClient();

            var request = new GetPreSignedUrlRequest()
            {
                BucketName = bucket,
                Key = objectKey,
                Expires = expires,
            };
            var url = client.GetPreSignedURL(request);
            _console.WriteLine("PresignedURL:[{0}]", url);
            _console.WriteLine("--- End of GetPreSignedURL ---");
        }

        /// <summary>GetPreSignedURL
        /// </summary>
        public void GeneratePreSignedURL(string bucket, string objectKey, DateTime expires)
        {
            _console.WriteLine("--- GeneratePreSignedURL ---");
            var client = GetClient();
            var url = client.GeneratePreSignedURL(_option.DefaultBucket, objectKey, expires, null);
            _console.WriteLine("PresignedURL:[{0}]", url);
            _console.WriteLine("--- End of GeneratePreSignedURL ---");
        }

        /// <summary>Test upload download speed
        /// </summary>
        public async Task TestSpeedAsync(string bucket, int fileSize = 1024 * 512, int fileCount = 100, bool autoDelete = true)
        {
            _console.WriteLine("--- Test speed ---");
            var client = GetClient();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var objectKeys = new List<string>();
            var totalSizeMB = (fileSize * fileCount * 1.0) / 1024 / 1024;

            _console.WriteLine("--- Upload speed begin ---");

            try
            {
                for (int i = 0; i < fileCount; i++)
                {
                    var objectKey = $"S3CommandLineTest/{Guid.NewGuid()}.txt";
                    using (var ms = new MemoryStream())
                    {
                        if (fileSize > 1024 * 1024 * 5)
                        {
                            await MultipartUploadAsync(bucket, objectKey, stream: ms);
                        }
                        else
                        {
                            await SimpleUploadAsync(bucket, objectKey, stream: ms);
                        }
                    }
                    objectKeys.Add(objectKey);
                }
            }
            finally
            {
                stopwatch.Stop();
            }

            _console.WriteLine("--- Upload speed end ---");

            var uploadSpeed = totalSizeMB / stopwatch.Elapsed.TotalSeconds;
            var uploadTotalMillSeconds = stopwatch.Elapsed.TotalMilliseconds;


            stopwatch.Reset();
            stopwatch.Restart();

            _console.WriteLine("--- Download speed begin ---");
            var tempFiles = new List<string>();
            try
            {
                foreach (var objectKey in objectKeys)
                {
                    var filePath = Path.Combine(_option.TemporaryPath, $"{Guid.NewGuid()}.txt");
                    await client.DownloadToFilePathAsync(bucket, objectKey, filePath, null);
                    tempFiles.Add(filePath);
                    _console.WriteLine("download '{0}' to '{1}'", objectKey, filePath);
                }
            }
            finally
            {
                stopwatch.Stop();
            }
            _console.WriteLine("--- Download speed end ---");

            _console.WriteLine("Upload speed,thread:{0},totalSize:'{1} MB',elapsed:'{2} ms', speed:'{3} MB/S' ", 1, totalSizeMB, uploadTotalMillSeconds, uploadSpeed);

            var downloadSpeed = totalSizeMB / stopwatch.Elapsed.TotalSeconds;
            var downloadTotalMillSeconds = stopwatch.Elapsed.TotalMilliseconds;


            if (autoDelete)
            {
                await DeleteObjectAsync(bucket, objectKeys.ToArray());
            }

            foreach (var tempFile in tempFiles)
            {
                Util.DeleteIfExists(tempFile);
            }

            _console.WriteLine("Upload speed,thread:{0},totalSize:'{1} MB',elapsed:'{2} ms', speed:'{3} MB/S' ", 1, totalSizeMB, uploadTotalMillSeconds, uploadSpeed);

            _console.WriteLine("Download speed,thread:{0},totalSize:'{1} MB',elapsed:'{2} ms', speed:'{3} MB/S' ", 1, totalSizeMB, downloadTotalMillSeconds, downloadSpeed);

            _console.WriteLine("--- End of test speed ---");
        }

        #region Private Methods
        private IAmazonS3 GetClient()
        {
            //_console.WriteLine("Current Client Info: Vendor:{0},Ak:{1},SK:{2}", _option.Vendor, _option.AccessKeyId, _option.SecretAccessKey);

            if (_option.Vendor == S3Vendor.KS3)
            {
                return _s3ClientFactory.GetOrAddClient(_option.AccessKeyId, _option.SecretAccessKey, () =>
                {
                    return new S3ClientConfiguration()
                    {
                        Vendor = S3VendorType.KS3,
                        MaxClient = 10,
                        AccessKeyId = _option.AccessKeyId,
                        SecretAccessKey = _option.SecretAccessKey,
                        Config = new AmazonKS3Config()
                        {
                            ServiceURL = _option.ServerUrl,
                            ForcePathStyle = _option.ForcePathStyle,
                            SignatureVersion = _option.SignatureVersion
                        }
                    };
                });
            }
            else
            {
                return _s3ClientFactory.GetOrAddClient(_option.AccessKeyId, _option.SecretAccessKey, () =>
                {
                    return new S3ClientConfiguration()
                    {
                        Vendor = S3VendorType.Amazon,
                        MaxClient = 10,
                        AccessKeyId = _option.AccessKeyId,
                        SecretAccessKey = _option.SecretAccessKey,
                        Config = new AmazonS3Config()
                        {
                            ServiceURL = _option.ServerUrl,
                            ForcePathStyle = _option.ForcePathStyle,
                            SignatureVersion = _option.SignatureVersion
                        }
                    };
                });
            }

        }


        /// <summary>MultipartUploadAsync
        /// </summary>
        private async Task<string> MultipartUploadAsync(string bucket, string objectKey, string filePath = "", Stream stream = null)
        {
            var client = GetClient();

            //初始化分片上传
            var initiateMultipartUploadResponse = await GetClient().InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest()
            {
                BucketName = _option.DefaultBucket,
                Key = objectKey,

            });

            //上传Id
            var uploadId = initiateMultipartUploadResponse.UploadId;
            // 计算分片总数。
            var partSize = 5 * 1024 * 1024;
            long fileSize = 0;
            Stream uploadStream = null;
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                var fi = new FileInfo(filePath);
                fileSize = fi.Length;
                uploadStream = File.Open(filePath, FileMode.Open);
            }
            if (stream != null)
            {
                fileSize = stream.Length;
                if (stream.CanSeek && stream.Position > 0)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
                uploadStream = stream;
            }


            var partCount = fileSize / partSize;
            if (fileSize % partSize != 0)
            {
                partCount++;
            }
            // 开始分片上传。partETags是保存partETag的列表，OSS收到用户提交的分片列表后，会逐一验证每个分片数据的有效性。 当所有的数据分片通过验证后，OSS会将这些分片组合成一个完整的文件。
            var partETags = new List<PartETag>();

            var uploadPartTasks = new List<Task>();

            using (uploadStream)
            {
                for (var i = 0; i < partCount; i++)
                {
                    var skipBytes = (long)partSize * i;
                    // 定位到本次上传起始位置。
                    //fs.Seek(skipBytes, 0);
                    // 计算本次上传的片大小，最后一片为剩余的数据大小。
                    var size = (int)((partSize < fileSize - skipBytes) ? partSize : (fileSize - skipBytes));

                    byte[] buffer = new byte[size];
                    uploadStream.Read(buffer, 0, size);

                    uploadPartTasks.Add(Task.Run<UploadPartResponse>(() =>
                    {
                        return client.UploadPartAsync(new UploadPartRequest()
                        {
                            BucketName = _option.DefaultBucket,
                            UploadId = uploadId,
                            Key = objectKey,
                            InputStream = new MemoryStream(buffer),
                            PartSize = size,
                            PartNumber = i + 1,
                            UseChunkEncoding = _option.UseChunkEncoding
                        });

                    }).ContinueWith(t =>
                    {
                        partETags.Add(new PartETag(t.Result.PartNumber, t.Result.ETag));
                        _console.WriteLine("finish {0}/{1}", partETags.Count, partCount);
                    }));
                }
            }

            Task.WaitAll(uploadPartTasks.ToArray());

            _console.WriteLine("Total '{0}' PartETags", partETags.Count);

            //列出所有分片
            _console.WriteLine("---List all parts,UploadId:{0}---", uploadId);

            var listPartsResponse = await client.ListPartsAsync(new ListPartsRequest()
            {
                BucketName = _option.DefaultBucket,
                Key = objectKey,
                UploadId = uploadId
            });
            foreach (var part in listPartsResponse.Parts)
            {
                _console.WriteLine("Part number:{0},part ETag:{1}", part.PartNumber, part.ETag);
            }


            var completeMultipartUploadResponse = await client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest()
            {
                BucketName = _option.DefaultBucket,
                Key = objectKey,
                UploadId = uploadId,
                PartETags = partETags
            });

            _console.WriteLine("MultipartUploadAsync complete,Key:{0}", completeMultipartUploadResponse.Key);
            return completeMultipartUploadResponse.Key;
        }

        /// <summary>简单上传文件
        /// </summary>
        private async Task<string> SimpleUploadAsync(string bucket, string objectKey, string filePath = "", Stream stream = null)
        {
            var client = GetClient();
            Console.WriteLine("---Simple upload key:{0}---", objectKey);
            var putObjectRequest = new PutObjectRequest()
            {
                BucketName = _option.DefaultBucket,
                AutoCloseStream = true,
                Key = objectKey,
                CannedACL = S3CannedACL.Private,
                UseChunkEncoding = false
            };

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                putObjectRequest.FilePath = filePath;
            }
            if (stream != null)
            {
                if (stream.CanSeek && stream.Position > 0)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
                putObjectRequest.InputStream = stream;
            }

            var putObjectResponse = await GetClient().PutObjectAsync(putObjectRequest);
            _console.WriteLine("Simple upload complete ,key:{0},Etag:{1}", objectKey, putObjectResponse.ETag);
            return objectKey;
        }
        #endregion

    }
}

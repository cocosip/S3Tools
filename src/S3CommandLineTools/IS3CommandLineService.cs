using System;
using System.Threading.Tasks;

namespace S3CommandLineTools
{
    public interface IS3CommandLineService
    {
        /// <summary>List buckets
        /// </summary>
        Task ListBucketAsync();

        /// <summary>获取Bucket权限
        /// </summary>
        Task GetAclAsync(string bucket);

        /// <summary>List objects
        /// </summary>
        Task ListObjectV2Async(string bucket, int max, string prefix = "", string delimiter = "");

        /// <summary>Download object to temp path
        /// </summary>
        Task DownloadObjectAsync(string bucket, params string[] objectKeys);

        /// <summary>Upload objects
        /// </summary>
        Task UploadObjectAsync(string bucket, bool autoDelete = true, params string[] filePaths);

        /// <summary>Upload default objects
        /// </summary>
        Task UploadDefaultObjectAsync(string bucket, bool autoDelete = true, int fileSize = 1024 * 1024);

        /// <summary>Delete objects
        /// </summary>
        Task DeleteObjectAsync(string bucket, params string[] objectKeys);

        /// <summary>Copy object
        /// </summary>
        Task CopyObjectAsync(string sourceBucket, string destinationBucket, string sourceKey, string destinationKey);

        /// <summary>GetPreSignedURL
        /// </summary>
        void GetPreSignedURL(string bucket, string objectKey, DateTime expires);

        /// <summary>GetPreSignedURL
        /// </summary>
        void GeneratePreSignedURL(string bucket, string objectKey, DateTime expires);
    }
}

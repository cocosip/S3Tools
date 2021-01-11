namespace S3CommandLineTools
{
    public class S3CommandLineOption
    {
        /// <summary>S3供应商
        /// </summary>
        public string Vendor { get; set; }

        /// <summary>AccessKeyId
        /// </summary>
        public string AccessKeyId { get; set; }

        /// <summary>SecretAccessKey
        /// </summary>
        public string SecretAccessKey { get; set; }

        /// <summary>ServerUrl
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>Default bucket
        /// </summary>
        public string DefaultBucket { get; set; }

        /// <summary>ForcePathStyle
        /// </summary>
        public bool ForcePathStyle { get; set; } = true;

        /// <summary>UseChunkEncoding
        /// </summary>
        public bool UseChunkEncoding { get; set; }

        /// <summary>SignatureVersion
        /// </summary>
        public string SignatureVersion { get; set; }

        /// <summary>临时目录
        /// </summary>
        public string TemporaryPath { get; set; }

        /// <summary>
        /// 分片上传多线程分片
        /// </summary>
        public bool MultipartMultiThread { get; set; } = false;

    }
}

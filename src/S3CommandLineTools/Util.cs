using System.IO;

namespace S3CommandLineTools
{
    public static class Util
    {
        public static string GetPathExtension(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && path.IndexOf('.') >= 0)
            {
                return path.Substring(path.LastIndexOf('.'));
            }
            return "";
        }

        public static long GetFileLength(string path)
        {
            if (File.Exists(path))
            {
                FileInfo f = new FileInfo(path);
                return f.Length;
            }
            return 0;
        }

    }
}

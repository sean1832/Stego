using System.IO.Compression;

namespace Stego.Core
{
    public static class Compression
    {
        public static byte[] CompressGz(byte[] data)
        {
            using MemoryStream memoryStream = new();
            using (GZipStream gzipStream = new(memoryStream, CompressionMode.Compress))
            {
                gzipStream.Write(data, 0, data.Length);
            }
            return memoryStream.ToArray();
        }
    }
}

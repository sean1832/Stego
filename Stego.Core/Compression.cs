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

        public static byte[] DecompressGz(byte[] compressedData)
        {
            using MemoryStream inputStream = new(compressedData);
            using MemoryStream outputStream = new();
            using (GZipStream gzipStream = new(inputStream, CompressionMode.Decompress))
            {
                gzipStream.CopyTo(outputStream);
            }

            return outputStream.ToArray();
        }
    }
}

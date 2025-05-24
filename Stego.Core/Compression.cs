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

        public static Task<byte[]> CompressGzAsync(byte[] data)
        {
            return Task.Run(() => CompressGz(data));
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

        public static Task<byte[]> DecompressGzAsync(byte[] compressedData)
        {
            return Task.Run(() => DecompressGz(compressedData));
        }

        public static bool IsCompressedGz(byte[] data)
        {
            // Check for GZIP magic number (0x1F, 0x8B)
            return data.Length >= 2 && data[0] == 0x1F && data[1] == 0x8B;
        }
    }
}

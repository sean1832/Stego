using SkiaSharp;

namespace Stego.Core
{
    /// <summary>
    /// Provides methods for encoding and decoding data using the Least Significant Bit (LSB) steganography technique.
    /// </summary>
    /// <remarks>
    /// This class supports encoding and decoding operations specifically for PNG image files.  The
    /// LSB steganography technique embeds data into the least significant bits of pixel values in an image.
    /// </remarks>
    public static class SteganographyLsb
    {
        /// <summary>
        /// Encodes a message into the least significant bits (LSB) of a PNG image and saves the result to a specified
        /// output file.
        /// </summary>
        /// <remarks>This method uses the least significant bit (LSB) steganography technique to embed the
        /// message into the pixel data of the cover image. The message is prefixed with its length (in bytes) as a
        /// 4-byte big-endian integer to facilitate decoding. The output image is saved in PNG format.</remarks>
        /// <param name="message">The message to encode, represented as a byte array. Must not be empty.</param>
        /// <param name="coverFilePath">The file path to the cover PNG image. Must point to an existing PNG file.</param>
        /// <param name="outputPath">The file path where the encoded PNG image will be saved. Must not be null or empty.</param>
        /// <param name="spacing">The spacing between pixels used for encoding. Determines the interval at which bits are embedded in the
        /// image.</param>
        /// <param name="lsbCount">The number of LSB to encode in to an image (1-8). Large means less stealthy, more data density</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="coverFilePath"/> is not a PNG file, if <paramref name="message"/> is empty, if
        /// <paramref name="outputPath"/> is null or empty, or if the image is too small to encode the message with the
        /// specified spacing.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file specified by <paramref name="coverFilePath"/> does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the cover file cannot be decoded as a valid image.</exception>
        public static void Encode(byte[] message, string coverFilePath, string outputPath, int spacing, short lsbCount)
        {
            // validate lsbCount
            if (lsbCount < 1 || lsbCount > 8)
                throw new ArgumentException("LSB count must be between 1 and 8.", nameof(lsbCount));

            string ext = Path.GetExtension(coverFilePath).ToLowerInvariant();
            if (ext != ".png")
            {
                throw new ArgumentException("Unsupported file format. LSB only support PNG");
            }
            if (!File.Exists(coverFilePath))
                throw new FileNotFoundException("Cover file not found.", coverFilePath);
            if (message.Length == 0)
                throw new ArgumentException("Payload cannot be empty.", nameof(message));
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));

            using FileStream inputStream = File.OpenRead(coverFilePath);
            using SKCodec codec = SKCodec.Create(inputStream) 
                ?? throw new InvalidOperationException("Failed to create SKCodec from the cover file.");
            SKImageInfo info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888);
            byte[] pixelBuffer = new byte[info.BytesSize];
            codec.GetPixels(info, pixelBuffer);

            // prepend 4 byte big endian length of the payload
            byte[] lengthHeader = BitConverter.GetBytes(message.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthHeader);
            byte[] payload = lengthHeader.Concat(message).ToArray();
            bool[] bits = ToBits(payload); // 0-1 byte array

            int totalBits = bits.Length;
            int groupCount = (totalBits + lsbCount - 1) / lsbCount;
            if (!IsValidSpacing(spacing, groupCount, pixelBuffer.Length))
                throw new ArgumentException(
                    $"Image too small: need at least {groupCount * spacing} bytes for encoding with {lsbCount} LSBs.");


            // mask to clear the lowest 'lsbCount' bits
            byte mask = (byte)(~((1 << lsbCount) - 1));
            for (int g = 0; g < groupCount; g++)
            {
                int idx = g * spacing;
                byte cleared = (byte)(pixelBuffer[idx] & mask);
                byte value = 0;
                for (int bitIndex = 0; bitIndex < lsbCount; bitIndex++)
                {
                    int bitPos = g * lsbCount + bitIndex;
                    if (bitPos >= totalBits) break;
                    if (bits[bitPos])
                        value |= (byte)(1 << bitIndex);
                }
                pixelBuffer[idx] = (byte)(cleared | value);
            }

            // write to output file
            using SKBitmap bitmap = new SKBitmap(info);
            bitmap.Pixels = pixelBuffer
                .Chunk(info.BytesPerPixel)
                .Select(chunk => new SKColor(chunk[0], chunk[1], chunk[2], chunk[3]))
                .ToArray();

            using SKImage image = SKImage.FromBitmap(bitmap);
            using FileStream outputStream = File.OpenWrite(outputPath);
            image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(outputStream);
        }

        public static async Task EncodeAsync(byte[] message, string coverFilePath, string outputPath, int spacing, short lsbCount)
        {
            await Task.Run(() => Encode(message, coverFilePath, outputPath, spacing, lsbCount));
        }

        /// <summary>
        /// Decodes a hidden payload from the least significant bits (LSB) of a PNG image.
        /// </summary>
        /// <remarks>This method extracts a payload embedded in the least significant bits of the pixel
        /// data in a PNG image. The first 32 bits (4 bytes) of the payload represent the length of the data in
        /// big-endian format. The method validates the file format and ensures the image is large enough to contain the
        /// declared payload.</remarks>
        /// <param name="coverFilePath">The file path to the PNG image containing the hidden payload.</param>
        /// <param name="spacing">The spacing, in pixels, between bits in the image. Must be a positive integer.</param>
        /// <returns>A byte array containing the decoded payload.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the specified <paramref name="coverFilePath"/> does not exist.</exception>
        /// <exception cref="ArgumentException">Thrown if the file specified by <paramref name="coverFilePath"/> is not a PNG image.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the image cannot be decoded, or if the image is too small to contain the declared payload length.</exception>
        public static byte[] Decode(string coverFilePath, int spacing, short lsbCount)
        {
            // validate lsbCount
            if (lsbCount < 1 || lsbCount > 8)
                throw new ArgumentException("LSB count must be between 1 and 8.", nameof(lsbCount));

            if (!File.Exists(coverFilePath))
                throw new FileNotFoundException("Cover file not found.", coverFilePath);
            if (Path.GetExtension(coverFilePath).ToLowerInvariant() != ".png")
            {
                throw new ArgumentException("Unsupported file format. LSB only support PNG.");
            }

            // load into RGBA byte buffer
            using FileStream inputStream = File.OpenRead(coverFilePath);
            using SKCodec codec = SKCodec.Create(inputStream) 
                ?? throw new InvalidOperationException("Failed to create SKCodec from the cover file.");
            SKImageInfo info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888);
            byte[] pixelBuffer = new byte[info.BytesSize];
            codec.GetPixels(info, pixelBuffer);

            // read 32 header bits (4 bytes) for length
            bool[] headerBits = new bool[32];
            int headerFilled = 0;
            for (int g = 0; headerFilled < headerBits.Length; g++)
            {
                int idx = g * spacing;
                for (int bitIndex = 0; bitIndex < lsbCount && headerFilled < headerBits.Length; bitIndex++)
                {
                    headerBits[headerFilled++] = ((pixelBuffer[idx] >> bitIndex) & 1) != 0;
                }
            }

            // reconstruct 4-byte big-endian length
            byte[] headerBytes = FromBits(headerBits);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(headerBytes);
            int payloadLength = BitConverter.ToInt32(headerBytes, 0);

            // read payload bits
            long totalBits = (long)payloadLength * 8;
            int groupCountPayload = (int)((totalBits + lsbCount - 1) / lsbCount);
            if (!IsValidSpacing(spacing, groupCountPayload, pixelBuffer.Length))
                throw new InvalidOperationException($"Image too small for declared payload length.");

            bool[] payloadBits = new bool[totalBits];
            int payloadFilled = 0;
            for (int g = 0; payloadFilled < totalBits; g++)    // <<< new
            {
                int idx = g * spacing;
                for (int bitIndex = 0; bitIndex < lsbCount && payloadFilled < totalBits; bitIndex++)
                {
                    payloadBits[payloadFilled++] = ((pixelBuffer[idx] >> bitIndex) & 1) != 0;
                }
            }

            return FromBits(payloadBits);
        }

        public static async Task<byte[]> DecodeAsync(string coverFilePath, int spacing, short lsbCount)
        {
            return await Task.Run(() => Decode(coverFilePath, spacing, lsbCount));
        }

        /// <summary>
        /// Converts an array of bytes into an array of bits, with each byte represented as 8 bits in most significant
        /// bit (MSB) order.
        /// </summary>
        private static bool[] ToBits(byte[] data)
        {
            bool[] bits = new bool[data.Length * 8];
            for (int b = 0; b < data.Length; b++)
            {
                for (int i = 0; i < 8; i++)
                {
                    // highest bit first
                    bits[b * 8 + i] = (data[b] & (1 << (7 - i))) != 0;
                }
            }
            return bits;
        }

        /// <summary>
        /// PackAll every 8 bit (MSB first) into a byte.
        /// </summary>
        private static byte[] FromBits(bool[] bits)
        {
            if (bits.Length % 8 != 0)
                throw new ArgumentException("Bit array length must be a multiple of 8.");
            int byteCount = bits.Length / 8;
            byte[] data = new byte[byteCount];
            for (int b = 0; b < byteCount; b++)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (bits[b * 8 + i])
                    {
                        data[b] |= (byte)(1 << (7 - i));
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Checks if the specified spacing and number of bits fit within the buffer size.
        /// </summary>
        private static bool IsValidSpacing(int spacing, int numBit, int bufferSize)
        {
            return (long)numBit * spacing <= bufferSize;
        }
    }
}

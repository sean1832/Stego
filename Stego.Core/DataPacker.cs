using System.Buffers.Binary;
using NSec.Cryptography;

namespace Stego.Core;

public static class DataPacker
{
    public static byte[] PackAll(
        ReadOnlySpan<byte> salt,
        ReadOnlySpan<byte> nonce,
        Argon2Parameters argon2Param,
        ReadOnlySpan<byte> data
    )
    {
        const int paramLength = 4 + 8 + 8; //<-- (int + long + long)
        int totalLength = salt.Length + nonce.Length + paramLength + data.Length;
        byte[] result = new byte[totalLength];

        int offset = 0;
        salt.CopyTo(result.AsSpan(offset, salt.Length)); // copy span directly
        offset += salt.Length;

        nonce.CopyTo(result.AsSpan(offset, nonce.Length)); 
        offset += nonce.Length;

        // write parameters in little‐endian explicitly
        BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(offset, 4),
            argon2Param.DegreeOfParallelism); //<-- Explicit LE
        offset += 4;

        BinaryPrimitives.WriteInt64LittleEndian(result.AsSpan(offset, 8),
            argon2Param.MemorySize); //<-- Explicit LE
        offset += 8;

        BinaryPrimitives.WriteInt64LittleEndian(result.AsSpan(offset, 8),
            argon2Param.NumberOfPasses); //<-- Explicit LE
        offset += 8;

        data.CopyTo(result.AsSpan(offset, data.Length));
        return result;
    }

    public static Dictionary<string, object> UnpackAll(ReadOnlySpan<byte> data)
    {
        const int saltLength = 16;
        const int nonceLength = 12;
        const int paramLength = 4 + 8 + 8;

        // bounds check
        if (data.Length < saltLength + nonceLength + paramLength)
            throw new ArgumentException(
                $"Data too short (min {saltLength + nonceLength + paramLength} bytes).",
                nameof(data));

        int offset = 0;
        byte[] salt = data.Slice(offset, saltLength).ToArray();
        offset += saltLength;

        byte[] nonce = data.Slice(offset, nonceLength).ToArray();
        offset += nonceLength;

        // read parameters in little‐endian explicitly
        int parallelism = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset, 4));  //<-- Explicit LE
        offset += 4;

        long memorySize = BinaryPrimitives.ReadInt64LittleEndian(data.Slice(offset, 8));  //<-- Explicit LE
        offset += 8;

        long passes = BinaryPrimitives.ReadInt64LittleEndian(data.Slice(offset, 8));  //<-- Explicit LE
        offset += 8;

        byte[] encryptedData = data.Slice(offset).ToArray();

        return new Dictionary<string, object>
        {
            ["saltByte"] = salt,
            ["nonceByte"] = nonce,
            ["parallelismInt"] = parallelism,
            ["memorySizeLong"] = memorySize,
            ["passesLong"] = passes,
            ["encryptedByte"] = encryptedData
        };
    }

    public static int PredictTotalBytes(byte[] data)
    {
        const int saltLength = 16;
        const int nonceLength = 12;
        const int paramLength = 4 + 8 + 8; //<-- (int + long + long)
        return saltLength + nonceLength + paramLength + data.Length;
    }
}
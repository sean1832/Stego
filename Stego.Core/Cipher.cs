using NSec.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Stego.Core
{
    public static class Cipher
    {
        public static byte[] EncryptAes256Gcm(ReadOnlySpan<byte> password, ReadOnlySpan<byte> data, Argon2Parameters param)
        {
            // Create a random salt
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            // create a random nonce
            byte[] nonce = new byte[12];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonce);
            }

            Argon2id kdf = PasswordBasedKeyDerivationAlgorithm.Argon2id(param);
            Aes256Gcm aes = new Aes256Gcm();
            Key key = kdf.DeriveKey(
                password,
                new ReadOnlySpan<byte>(salt),
                aes
            );

            // encrypt content using the key
            byte[] encryptedRawData = aes.Encrypt(key, new ReadOnlySpan<byte>(nonce), null, data);
            return DataPacker.PackAll(salt, nonce, param, encryptedRawData);
        }
    }
}

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace DecentraCloud.API.Helpers
{
    public class EncryptionHelper
    {
        private readonly byte[] _key;

        public EncryptionHelper(string key)
        {
            if (key.Length < 32)
            {
                throw new ArgumentException("Key must be at least 32 characters long");
            }

            _key = Encoding.UTF8.GetBytes(key);
        }

        public byte[] Encrypt(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = new byte[16]; // Initialization vector, can be customized
                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, encryptor);
                }
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = new byte[16]; // Initialization vector, must match the one used for encryption
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, decryptor);
                }
            }
        }

        private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
                {
                    using (var binaryWriter = new BinaryWriter(cryptoStream))
                    {
                        binaryWriter.Write(data);
                    }
                }
                return ms.ToArray();
            }
        }

        // Bcrypt hashing
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Bcrypt verification
        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}

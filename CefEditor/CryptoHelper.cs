using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CefEditor
{
    public static class CryptoHelper
    {
        public static string DecryptRawJson(string encryptedBase64, string privateKeyPem)
        {
            var data = Convert.FromBase64String(encryptedBase64);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);

            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            var encKeyLen = br.ReadInt32();
            var encryptedKey = br.ReadBytes(encKeyLen);
            var nonce = br.ReadBytes(12);
            var tag = br.ReadBytes(16);
            var ciphertext = br.ReadBytes((int)(ms.Length - ms.Position));

            var aesKey = rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);

            var plaintext = new byte[ciphertext.Length];
            using (var aesGcm = new AesGcm(aesKey, 16))
            {
                aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
            }

            return Encoding.UTF8.GetString(plaintext);
        }

        public static string EncryptRawJson(string jsonPayload, string pemKey)
        {
            var payload = Encoding.UTF8.GetBytes(jsonPayload);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(pemKey);

            var aesKey = RandomNumberGenerator.GetBytes(32); // AES-256
            var nonce = RandomNumberGenerator.GetBytes(12); // GCM nonce
            
            var ciphertext = new byte[payload.Length];
            var tag = new byte[16];
            using (var aesGcm = new AesGcm(aesKey, 16))
            {
                aesGcm.Encrypt(nonce, payload, ciphertext, tag);
            }
            
            // Encriptamos la llave AES con RSA (OaepSHA256)
            var encryptedAesKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);
            
            using var outMs = new MemoryStream();
            using var bw = new BinaryWriter(outMs);
            bw.Write(encryptedAesKey.Length);
            bw.Write(encryptedAesKey);
            bw.Write(nonce);
            bw.Write(tag);
            bw.Write(ciphertext);

            return Convert.ToBase64String(outMs.ToArray());
        }
    }
}
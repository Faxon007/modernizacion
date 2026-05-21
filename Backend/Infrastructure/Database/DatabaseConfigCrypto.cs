using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Infrastructure.Database
{
    public static class DatabaseConfigCrypto
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented          = false,
            PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public static DatabaseConfig Decrypt(string encryptedBase64, string privateKeyPem)
        {
            var data = Convert.FromBase64String(encryptedBase64);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);

            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            var encKeyLen    = br.ReadInt32();
            var encryptedKey = br.ReadBytes(encKeyLen);
            var nonce        = br.ReadBytes(12);
            var tag          = br.ReadBytes(16);
            var ciphertext   = br.ReadBytes((int)(ms.Length - ms.Position));

            var aesKey = rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);

            var plaintext = new byte[ciphertext.Length];
            using (var aesGcm = new AesGcm(aesKey, 16))
                aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);

            var json = Encoding.UTF8.GetString(plaintext);

            return JsonSerializer.Deserialize<DatabaseConfig>(json, JsonOptions)
                ?? throw new InvalidOperationException("El archivo .cef2 está vacío o tiene formato incorrecto.");
        }

        public static string Encrypt(DatabaseConfig config, string publicKeyPem)
        {
            var json    = JsonSerializer.Serialize(config, JsonOptions);
            var payload = Encoding.UTF8.GetBytes(json);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);

            var aesKey = RandomNumberGenerator.GetBytes(32); // AES-256
            var nonce  = RandomNumberGenerator.GetBytes(12); // GCM nonce
            
            var ciphertext = new byte[payload.Length];
            var tag        = new byte[16];
            using (var aesGcm = new AesGcm(aesKey, 16))
                aesGcm.Encrypt(nonce, payload, ciphertext, tag);
            
            var encryptedAesKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);
            
            using var outMs = new MemoryStream();
            using var bw    = new BinaryWriter(outMs);
            bw.Write(encryptedAesKey.Length);
            bw.Write(encryptedAesKey);
            bw.Write(nonce);
            bw.Write(tag);
            bw.Write(ciphertext);

            return Convert.ToBase64String(outMs.ToArray());
        }
    }
}

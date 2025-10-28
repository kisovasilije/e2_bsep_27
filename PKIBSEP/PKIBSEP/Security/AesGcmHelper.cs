using System.Security.Cryptography;
using System.Text;

namespace PKIBSEP.Security
{
    /// <summary>
    /// AES-GCM helper for encrypting/decrypting short secrets (e.g., PFX passwords).
    /// Output format: Base64( 12-byte nonce || 16-byte tag || ciphertext ).
    /// </summary>
    public static class AesGcmHelper
    {
        public static string EncryptToBase64(byte[] key32, string plaintext)
        {
            if (key32 is null || key32.Length != 32) throw new ArgumentException("key32 must be 32 bytes.");
            var plainBytes = Encoding.UTF8.GetBytes(plaintext);

            byte[] nonce = RandomNumberGenerator.GetBytes(12);
            byte[] tag = new byte[16];
            byte[] cipherBytes = new byte[plainBytes.Length];

            using var aes = new AesGcm(key32);
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

            var blob = new byte[nonce.Length + tag.Length + cipherBytes.Length];
            Buffer.BlockCopy(nonce, 0, blob, 0, 12);
            Buffer.BlockCopy(tag, 0, blob, 12, 16);
            Buffer.BlockCopy(cipherBytes, 0, blob, 28, cipherBytes.Length);

            return Convert.ToBase64String(blob);
        }

        public static string DecryptFromBase64(byte[] key32, string base64Blob)
        {
            if (key32 is null || key32.Length != 32) throw new ArgumentException("key32 must be 32 bytes.");
            var blob = Convert.FromBase64String(base64Blob);

            var nonce = blob.AsSpan(0, 12).ToArray();
            var tag = blob.AsSpan(12, 16).ToArray();
            var cipherBytes = blob.AsSpan(28).ToArray();

            var plainBytes = new byte[cipherBytes.Length];
            using var aes = new AesGcm(key32);
            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}

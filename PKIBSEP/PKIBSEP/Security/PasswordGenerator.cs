using System.Security.Cryptography;
using System.Text;

namespace PKIBSEP.Security
{
    /// <summary>
    /// Generates strong random passwords for keystore protection.
    /// </summary>
    public static class PasswordGenerator
    {
        /// <summary>
        /// Generates a random password from a mixed charset.
        /// Default length is 24, which is more than enough for PKCS#12/PFX password entropy.
        /// </summary>
        public static string Random(int length = 24)
        {
            const string charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+";
            var randomBytes = RandomNumberGenerator.GetBytes(length);
            var builder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
                builder.Append(charset[randomBytes[i] % charset.Length]);

            return builder.ToString();
        }
    }
}

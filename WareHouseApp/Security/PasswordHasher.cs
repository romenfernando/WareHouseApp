using System;
using System.Security.Cryptography;
using System.Text;

namespace WareHouseApp.Security
{
    /// <summary>
    /// Salted SHA-256 password hashing. Plain-text passwords are never stored or
    /// compared directly - only the hash (with a random salt per user) is kept.
    /// Stored format: "&lt;base64 salt&gt;:&lt;base64 hash&gt;".
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;

        public static string Hash(string plainTextPassword)
        {
            if (plainTextPassword == null)
                throw new ArgumentNullException(nameof(plainTextPassword));

            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = ComputeHash(plainTextPassword, salt);
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        public static bool Verify(string plainTextPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash) || !storedHash.Contains(":"))
                return false;

            string[] parts = storedHash.Split(':');
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expectedHash = Convert.FromBase64String(parts[1]);

            byte[] actualHash = ComputeHash(plainTextPassword, salt);

            return SlowEquals(expectedHash, actualHash);
        }

        private static byte[] ComputeHash(string plainTextPassword, byte[] salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(plainTextPassword);
                byte[] combined = new byte[salt.Length + passwordBytes.Length];
                Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
                Buffer.BlockCopy(passwordBytes, 0, combined, salt.Length, passwordBytes.Length);
                return sha256.ComputeHash(combined);
            }
        }

        // Constant-time comparison so an attacker can't learn how many bytes
        // matched from response timing.
        private static bool SlowEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}

using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LCrypt.Core.Crypto
{
    public static class Pbkdf2
    {
        public static Task<byte[]> DeriveKeyAsync(string password, byte[] salt, int iterations, int byteCount)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (salt == null)
                throw new ArgumentNullException(nameof(salt));

            return Task.Run(() =>
            {
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
                return pbkdf2.GetBytes(byteCount);
            });
        }
    }
}

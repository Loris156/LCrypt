using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LCrypt.Utility.Extensions
{
    public static class SymmetricAlgorithmExtensions
    {
        public static async Task<byte[]> EncryptStringAsync(this SymmetricAlgorithm algorithm, string input,
            Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (algorithm == null)
                throw new ArgumentNullException(nameof(algorithm));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            cancellationToken.ThrowIfCancellationRequested();
            encoding = encoding ?? Encoding.Unicode;

            var textBytes = encoding.GetBytes(input);

            using (var ms = new MemoryStream())
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var transform = algorithm.CreateEncryptor())
                {
                    using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                    {
                        await cs.WriteAsync(textBytes, 0, textBytes.Length, cancellationToken);
                        cs.FlushFinalBlock();
                    }
                }
                cancellationToken.ThrowIfCancellationRequested();
                return ms.ToArray();
            }
        }

        public static async Task<string> DecryptStringAsync(this SymmetricAlgorithm algorithm, byte[] cipher,
            Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (algorithm == null)
                throw new ArgumentNullException(nameof(algorithm));
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            cancellationToken.ThrowIfCancellationRequested();
            encoding = encoding ?? Encoding.Unicode;

            using (var ms = new MemoryStream())
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var transform = algorithm.CreateDecryptor())
                {
                    using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                    {
                        await cs.WriteAsync(cipher, 0, cipher.Length, cancellationToken);
                        cs.FlushFinalBlock();
                    }
                }
                cancellationToken.ThrowIfCancellationRequested();
                return encoding.GetString(ms.ToArray());
            }
        }
    }
}

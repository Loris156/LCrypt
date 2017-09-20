using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace LCrypt.Utility.Extensions
{
    public static class HashAlgorithmExtensions
    {
        public const int BufferSize = 131072;

        public static async Task<byte[]> ComputeHashAsync(this HashAlgorithm hashAlgorithm, Stream inputStream,
            CancellationToken cancellationToken)
        {
            if (hashAlgorithm == null)
                throw new ArgumentNullException(nameof(hashAlgorithm));
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));
            if (!inputStream.CanRead)
                throw new NotSupportedException("Input stream does not support reading.");
            if (!inputStream.CanSeek)
                throw new NotSupportedException("Input stream does not support seeking.");

            if (inputStream.Position != 0)
                inputStream.Seek(0, SeekOrigin.Begin);

            var buffer = new byte[BufferSize];
            int readBytes;

            hashAlgorithm.Initialize();

            while ((readBytes =
                       await inputStream.ReadAsync(buffer, 0, BufferSize, cancellationToken).ConfigureAwait(false)) > 0)
                // ReSharper disable once AssignNullToNotNullAttribute
                hashAlgorithm.TransformBlock(buffer, 0, readBytes, null, 0);

            hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
            return hashAlgorithm.Hash;
        }
    }
}
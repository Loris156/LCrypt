using LCrypt.Models;
using LCrypt.Utility;
using LCrypt.Utility.Extensions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace LCrypt.EncryptionAlgorithms
{
    public class FileEncryption
    {
        private const int FileBufferSize = 131072; // 128 KiB
        private const int EncryptionSaltLength = 32;
        private const int DeriveBytesIterations = 50000;

        public async Task EncryptFileAsync(FileEncryptionTask task, IProgress<long> progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var source = new FileStream(task.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: FileBufferSize, useAsync: true))
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var destination = new FileStream(task.DestinationPath, FileMode.Create, FileAccess.Write,
                    FileShare.None, bufferSize: FileBufferSize, useAsync: true))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using (var algorithm = task.Algorithm.Create())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var salt = Util.GenerateStrongRandomBytes(EncryptionSaltLength);
                        algorithm.IV = Util.GenerateStrongRandomBytes(algorithm.BlockSize / 8);
                        algorithm.Key = await task.Password.DeriveKeyAsync(salt, DeriveBytesIterations, algorithm.KeySize / 8, cancellationToken);

                        await destination.WriteAsync(salt, 0, EncryptionSaltLength, cancellationToken);
                        await destination.WriteAsync(algorithm.IV, 0, algorithm.BlockSize / 8, cancellationToken);

                        using (var transform = algorithm.CreateEncryptor())
                        {
                            using (var cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write))
                            {
                                await source.CopyToAsync(cryptoStream, progress, cancellationToken);
                            }
                        }
                    }
                }
            }
        }

        public async Task DecryptFileAsync(FileEncryptionTask task, IProgress<long> progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var source = new FileStream(task.FilePath, FileMode.Open, FileAccess.Read, FileShare.None,
                bufferSize: FileBufferSize, useAsync: true))
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var destination = new FileStream(task.DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: FileBufferSize, useAsync: true))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var salt = new byte[EncryptionSaltLength];
                    await source.ReadAsync(salt, 0, EncryptionSaltLength, cancellationToken);

                    using (var algorithm = task.Algorithm.Create())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var iv = new byte[algorithm.BlockSize / 8];
                        await source.ReadAsync(iv, 0, algorithm.BlockSize / 8, cancellationToken);
                        algorithm.IV = iv;

                        algorithm.Key = await task.Password.DeriveKeyAsync(salt, DeriveBytesIterations, algorithm.KeySize / 8, cancellationToken);

                        using (var transform = algorithm.CreateDecryptor())
                        {
                            using (var cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write))
                            {
                                await source.CopyToAsync(cryptoStream, progress, cancellationToken);
                            }
                        }
                    }
                }
            }
        }
    }
}

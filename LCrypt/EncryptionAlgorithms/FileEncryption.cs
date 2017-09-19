using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LCrypt.Models;
using LCrypt.Utility;
using LCrypt.Utility.Extensions;

namespace LCrypt.EncryptionAlgorithms
{
    public class FileEncryption
    {
        public const int EncryptionSaltLength = 32;
        public const int DeriveBytesIterations = 50000;

        public async Task EncryptFile(FileEncryptionTask task, IProgress<long> progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var source = new FileStream(task.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 131072, useAsync: true))
            {
                progress.Report(0);
                cancellationToken.ThrowIfCancellationRequested();

                using (var destination = new FileStream(task.DestinationPath, FileMode.Create, FileAccess.Write,
                    FileShare.None, bufferSize: 131072, useAsync: true))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using (var algorithm = task.Algorithm.Create())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var salt = Util.GenerateStrongRandomBytes(EncryptionSaltLength);
                        algorithm.IV = Util.GenerateStrongRandomBytes(algorithm.BlockSize / 8);
                        algorithm.Key = task.Password.DeriveKey(salt, DeriveBytesIterations, algorithm.KeySize / 8);

                        using (var transform = algorithm.CreateEncryptor())
                        {
                            using (var cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write))
                            {

                            }
                        }
                    }
                }
            }
        }

        public async Task DecryptFile(FileEncryptionTask task, IProgress<long> progress, CancellationToken cancellationToken)
        {
            
        }
    }
}

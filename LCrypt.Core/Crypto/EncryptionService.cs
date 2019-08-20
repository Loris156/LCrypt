using Be.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LCrypt.Core.Crypto
{
    public class EncryptionService
        : IEncryptionService
    {
        private readonly IAlgorithm _algorithm;
        private readonly Stream _sourceStream;
        private readonly Stream _destinationStream;
        private readonly string _password;
        private readonly IProgress<CryptoOperationProgress> _progress;

        private readonly Stopwatch _stopwatch;
        private readonly Stopwatch _reportStopwatch;

        public EncryptionService(IAlgorithm algorithm,
            Stream sourceStream,
            Stream destinationStream,
            string password,
            IProgress<CryptoOperationProgress> progress)
        {
            _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            _sourceStream = sourceStream ?? throw new ArgumentNullException(nameof(sourceStream));
            _destinationStream = destinationStream ?? throw new ArgumentNullException(nameof(destinationStream));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _progress = progress;

            _stopwatch = new Stopwatch();
            _reportStopwatch = new Stopwatch();
        }

        public async Task EncryptAsync()
        {
            var salt = GenerateSalt(Constants.SaltLength);

            // Perform CPU-intensive key derivation on own task
            await Task.Run(() =>
            {
                using (var pbkdf2 = new Rfc2898DeriveBytes(_password, salt, Constants.Pbkdf2Iterations))
                {
                    _algorithm.SymmetricAlgorithm.Key = pbkdf2.GetBytes(_algorithm.SymmetricAlgorithm.KeySize / 8);
                    _algorithm.SymmetricAlgorithm.GenerateIV();
                }
            }).ConfigureAwait(false);

            WriteHeaderV1(_destinationStream, new FileHeaderV1
            {
                Pbkdf2Iterations = Constants.Pbkdf2Iterations,
                Salt = salt,
                Iv = _algorithm.SymmetricAlgorithm.IV
            });

            using (var encryptor = _algorithm.SymmetricAlgorithm.CreateEncryptor())
            {
                _stopwatch.Start();
                _reportStopwatch.Start();

                using (var cryptoStream = new CryptoStream(_destinationStream, encryptor, CryptoStreamMode.Write))
                {
                    var buffer = new byte[Constants.FileBufferSize];

                    int readBytes;
                    var processedBytes = 0;
                    while ((readBytes = await _sourceStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
                    {
                        await cryptoStream.WriteAsync(buffer, 0, readBytes).ConfigureAwait(false);
                        processedBytes += readBytes;

                        if (_reportStopwatch.ElapsedMilliseconds >= Constants.ReportIntervalMs)
                        {
                            _progress?.Report(new CryptoOperationProgress
                            {
                                ProcessedBytes = processedBytes,
                                BytesPerSecond = processedBytes / _stopwatch.Elapsed.TotalSeconds
                            });

                            _reportStopwatch.Restart();
                        }
                    }

                    cryptoStream.FlushFinalBlock();
                }
            }
        }

        public void Dispose()
        {
            _algorithm.Dispose();
        }

        private void WriteHeaderV1(Stream stream, FileHeaderV1 header)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (header == null)
                throw new ArgumentNullException(nameof(header));

            using (var writer = new BeBinaryWriter(stream, new UTF8Encoding(false, true), leaveOpen: true))
            {
                writer.Write(Constants.MagicHeader); // Magic header
                writer.Write((byte)1); // Header version

                writer.Write(header.Pbkdf2Iterations);

                writer.Write(header.Salt.Length);
                writer.Write(header.Salt);

                writer.Write(_algorithm.Name);

                writer.Write(header.Iv.Length);
                writer.Write(header.Iv);
            }
        }

        private byte[] GenerateSalt(int length)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException(nameof(length));

            var salt = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }
    }
}

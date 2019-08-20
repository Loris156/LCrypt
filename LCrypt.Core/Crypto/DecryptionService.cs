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
    public class DecryptionService
        : IDecryptionService
    {
        private readonly Stream _sourceStream;
        private readonly Stream _destinationStream;
        private readonly string _password;
        private readonly IProgress<CryptoOperationProgress> _progress;

        private IAlgorithm _algorithm;

        private Stopwatch _stopwatch;
        private Stopwatch _reportStopwatch;

        public DecryptionService(Stream sourceStream, Stream destinationStream,
            string password, IProgress<CryptoOperationProgress> progress)
        {
            _sourceStream = sourceStream ?? throw new ArgumentNullException(nameof(sourceStream));
            _destinationStream = destinationStream ?? throw new ArgumentNullException(nameof(destinationStream));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _progress = progress;

            _stopwatch = new Stopwatch();
            _reportStopwatch = new Stopwatch();
        }

        public async Task DecryptAsync()
        {
            var version = await ReadCommonHeaderAsync(_sourceStream).ConfigureAwait(false);
            int pbkdf2Iterations;
            byte[] salt;

            switch (version)
            {
                case 1:
                    var v1Header = await ReadHeaderV1Async(_sourceStream).ConfigureAwait(false);
                    _algorithm = Algorithm.GetByName(v1Header.Algorithm);
                    _algorithm.SymmetricAlgorithm.IV = v1Header.Iv;

                    pbkdf2Iterations = v1Header.Pbkdf2Iterations;
                    salt = v1Header.Salt;
                    break;
                default:
                    throw new CryptographicException("invalid version");
            }

            _algorithm.SymmetricAlgorithm.Key =
                await Pbkdf2.DeriveKeyAsync(_password, salt, pbkdf2Iterations,
                _algorithm.SymmetricAlgorithm.KeySize / 8)
                .ConfigureAwait(false);

            using (var decryptor = _algorithm.SymmetricAlgorithm.CreateDecryptor())
            {
                using (var cryptoStream = new CryptoStream(_destinationStream, decryptor, CryptoStreamMode.Write))
                {
                    var buffer = new byte[Constants.FileBufferSize];

                    _stopwatch.Start();
                    _reportStopwatch.Start();

                    int readBytes;
                    var processedBytes = 0;
                    while ((readBytes = await _sourceStream.ReadAsync(buffer, 0, buffer.Length)
                        .ConfigureAwait(false)) > 0)
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
            _algorithm?.Dispose();
        }

        private Task<byte> ReadCommonHeaderAsync(Stream stream)
        {
            Debug.Assert(stream != null);

            using (var reader = new BeBinaryReader(stream, new UTF8Encoding(false, true), leaveOpen: true))
            {
                var header = reader.ReadBytes(Constants.MagicHeader.Length);
                if (!header.SequenceEqual(Constants.MagicHeader))
                    throw new CryptographicException("invalid magic header found");

                var version = reader.ReadByte();
                if (version <= 0)
                    throw new CryptographicException("invalid version");

                return Task.FromResult(version);
            }
        }

        private Task<FileHeaderV1> ReadHeaderV1Async(Stream stream)
        {
            Debug.Assert(stream != null);

            using (var reader = new BeBinaryReader(stream, new UTF8Encoding(false, true), leaveOpen: true))
            {
                var pbkdf2Iterations = reader.ReadInt32();
                if (pbkdf2Iterations <= 0)
                    throw new IOException("invalid PBKDF2 iterations");

                var saltLength = reader.ReadInt32();
                if (saltLength <= 0)
                    throw new IOException("invalid salt length");

                var salt = reader.ReadBytes(saltLength);

                var algorithm = reader.ReadString();

                var ivLength = reader.ReadInt32();
                if (ivLength <= 0)
                    throw new IOException("invalid IV");

                var iv = reader.ReadBytes(ivLength);

                return Task.FromResult(new FileHeaderV1
                {
                    Pbkdf2Iterations = pbkdf2Iterations,
                    Salt = salt,
                    Algorithm = algorithm,
                    Iv = iv
                });
            }
        }
    }
}

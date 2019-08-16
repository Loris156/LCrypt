using Be.IO;
using LCrypt.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LCrypt.Algorithms
{
    public class EncryptionService
    {
        private const int Pbkdf2Iterations = 120000;
        private const int SaltLength = 16;
        private const int FileBufferSize = 131072;

        private const int ReportIntervalMs = 250;

        ///////////////////////////////////////////////////////// L   C    r    y    p    t ///
        private static readonly byte[] MagicHeader = new byte[] { 76, 67, 114, 121, 112, 116 };

        private readonly SymmetricAlgorithm _algorithm;
        private readonly FileInfo _fileInfo;
        private readonly string _destination;
        private readonly string _password;
        private readonly IProgress<EncryptionServiceProgress> _progress;

        private readonly Stopwatch _stopwatch;
        private readonly Stopwatch _reportStopwatch;
        private long _processedBytes;

        public EncryptionService(SymmetricAlgorithm algorithm, FileInfo fileInfo, string destination, string password, IProgress<EncryptionServiceProgress> progress)
        {
            _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            _destination = destination ?? throw new ArgumentNullException(nameof(destination));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));

            _stopwatch = new Stopwatch();
            _reportStopwatch = new Stopwatch();
        }

        public async Task EncryptAsync()
        {
            using (var sourceStream = new FileStream(_fileInfo.FullName, FileMode.Open,
                FileAccess.Read, FileShare.Read, FileBufferSize, useAsync: true))
            {
                var salt = GenerateSalt(SaltLength);

                using (var pbkdf2 = new Rfc2898DeriveBytes(_password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA512))
                {
                    _algorithm.Key = pbkdf2.GetBytes(_algorithm.KeySize / 8);
                    _algorithm.GenerateIV();
                }

                using (var destinationStream = new FileStream(_destination, FileMode.Create,
                FileAccess.Write, FileShare.None, FileBufferSize, useAsync: true))
                {
                    WriteHeaderV1Async(destinationStream, new FileHeaderV1
                    {
                        Pbkdf2Iterations = Pbkdf2Iterations,
                        Salt = salt,
                        Iv = _algorithm.IV
                    });

                    using (var encryptor = _algorithm.CreateEncryptor())
                    {
                        _stopwatch.Start();
                        _reportStopwatch.Start();

                        using(var cryptoStream = new CryptoStream(destinationStream, encryptor, CryptoStreamMode.Write))
                        {
                            var buffer = new byte[FileBufferSize];

                            int readBytes;
                            while((readBytes = await sourceStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
                            {
                                await cryptoStream.WriteAsync(buffer, 0, readBytes).ConfigureAwait(false);
                                _processedBytes += readBytes;

                                var bytesPerSecond = _processedBytes / _stopwatch.Elapsed.TotalSeconds;
                                var mibPerSecond = Math.Round(bytesPerSecond / (1024 * 1024), 2);

                                if(_reportStopwatch.ElapsedMilliseconds >= ReportIntervalMs)
                                {
                                    _progress.Report(new EncryptionServiceProgress
                                    {
                                        ProcessedBytes = _processedBytes,
                                        MibPerSecond = mibPerSecond
                                    });

                                    _reportStopwatch.Restart();
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task DecryptAsync()
        {
            throw new NotImplementedException();
        }

        private void WriteHeaderV1Async(Stream stream, FileHeaderV1 header)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (header == null)
                throw new ArgumentNullException(nameof(header));

            using (var writer = new BeBinaryWriter(stream, new UTF8Encoding(false, true), true))
            {
                writer.Write(MagicHeader); // Magic header
                writer.Write(1); // Header version

                writer.Write(header.Pbkdf2Iterations);

                writer.Write(header.Salt.Length);
                writer.Write(header.Salt);

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

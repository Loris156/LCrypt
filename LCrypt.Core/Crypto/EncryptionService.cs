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

        public EncryptionService(SymmetricAlgorithm algorithm,
            FileInfo fileInfo,
            string destination,
            string password,
            IProgress<EncryptionServiceProgress> progress)
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

                // Perform CPU-intensive key derivation on own task
                await Task.Run(() =>
                {
                    using (var pbkdf2 = new Rfc2898DeriveBytes(_password, salt, Pbkdf2Iterations))
                    {
                        _algorithm.Key = pbkdf2.GetBytes(_algorithm.KeySize / 8);
                        _algorithm.GenerateIV();
                    }
                }).ConfigureAwait(false);

                using (var destinationStream = new FileStream(_destination, FileMode.Create,
                FileAccess.Write, FileShare.None, FileBufferSize, useAsync: true))
                {
                    WriteHeaderV1(destinationStream, new FileHeaderV1
                    {
                        Pbkdf2Iterations = Pbkdf2Iterations,
                        Salt = salt,
                        Iv = _algorithm.IV
                    });

                    using (var encryptor = _algorithm.CreateEncryptor())
                    {
                        _stopwatch.Start();
                        _reportStopwatch.Start();

                        using (var cryptoStream = new CryptoStream(destinationStream, encryptor, CryptoStreamMode.Write))
                        {
                            var buffer = new byte[FileBufferSize];

                            int readBytes;
                            while ((readBytes = await sourceStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
                            {
                                await cryptoStream.WriteAsync(buffer, 0, readBytes).ConfigureAwait(false);
                                _processedBytes += readBytes;

                                var bytesPerSecond = _processedBytes / _stopwatch.Elapsed.TotalSeconds;
                                var mibPerSecond = Math.Round(bytesPerSecond / (1024 * 1024), 2);

                                if (_reportStopwatch.ElapsedMilliseconds >= ReportIntervalMs)
                                {
                                    _progress.Report(new EncryptionServiceProgress
                                    {
                                        ProcessedBytes = _processedBytes,
                                        BytesPerSecond = mibPerSecond
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
            using (var sourceStream = new FileStream(_fileInfo.FullName, FileMode.Open,
                FileAccess.Read, FileShare.Read, FileBufferSize, useAsync: true))
            {
                var version = ReadCommonFileHeader(sourceStream);

                int pbkdf2Iterations;
                byte[] salt;

                switch (version)
                {
                    case 1:
                        var header = ReadHeaderV1(sourceStream);
                        pbkdf2Iterations = header.Pbkdf2Iterations;
                        salt = header.Salt;
                        _algorithm.IV = header.Iv;
                        break;
                    default:
                        throw new IOException("unknown file version");
                }

                await Task.Run(() =>
                {
                    using (var pbkdf2 = new Rfc2898DeriveBytes(_password, salt, Pbkdf2Iterations))
                    {
                        _algorithm.Key = pbkdf2.GetBytes(_algorithm.KeySize / 8);
                    }
                }).ConfigureAwait(false);

                using (var destinationStream = new FileStream(_destination, FileMode.Create,
                    FileAccess.Write, FileShare.None, FileBufferSize, useAsync: true))
                {
                    using (var decryptor = _algorithm.CreateDecryptor())
                    {
                        _stopwatch.Start();
                        _reportStopwatch.Start();

                        using (var cryptoStream = new CryptoStream(destinationStream, decryptor, CryptoStreamMode.Write))
                        {
                            var buffer = new byte[FileBufferSize];

                            int readBytes;
                            while ((readBytes = await sourceStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
                            {
                                await cryptoStream.WriteAsync(buffer, 0, readBytes).ConfigureAwait(false);
                                _processedBytes += readBytes;

                                var bytesPerSecond = _processedBytes / _stopwatch.Elapsed.TotalSeconds;
                                var mibPerSecond = Math.Round(bytesPerSecond / (1024 * 1024), 2);

                                if (_reportStopwatch.ElapsedMilliseconds >= ReportIntervalMs)
                                {
                                    _progress.Report(new EncryptionServiceProgress
                                    {
                                        ProcessedBytes = _processedBytes,
                                        BytesPerSecond = mibPerSecond
                                    });

                                    _reportStopwatch.Restart();
                                }
                            }
                        }
                    }
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
                writer.Write(MagicHeader); // Magic header
                writer.Write(1); // Header version

                writer.Write(header.Pbkdf2Iterations);

                writer.Write(header.Salt.Length);
                writer.Write(header.Salt);

                writer.Write(header.Iv.Length);
                writer.Write(header.Iv);
            }
        }

        private int ReadCommonFileHeader(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var reader = new BeBinaryReader(stream, new UTF8Encoding(false, true), leaveOpen: true))
            {
                var magicHeader = reader.ReadBytes(MagicHeader.Length);
                if (!magicHeader.SequenceEqual(MagicHeader))
                    throw new IOException("stream does not contain a LCrypt-encrypted file");

                var version = reader.ReadInt32();
                if (version <= 0)
                    throw new IOException("file version is invalid");

                return version;
            }
        }

        private FileHeaderV1 ReadHeaderV1(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var reader = new BeBinaryReader(stream, new UTF8Encoding(false, true), leaveOpen: true))
            {
                var pbkdf2Iterations = reader.ReadInt32();
                if (pbkdf2Iterations <= 0)
                    throw new IOException("invalid PBKDF2 iterations");

                var saltLength = reader.ReadInt32();
                if (saltLength <= 0)
                    throw new IOException("invalid salt length");

                var salt = reader.ReadBytes(saltLength);

                var ivLength = reader.ReadInt32();
                if (ivLength <= 0)
                    throw new IOException("invalid IV");

                var iv = reader.ReadBytes(ivLength);

                return new FileHeaderV1
                {
                    Pbkdf2Iterations = pbkdf2Iterations,
                    Salt = salt,
                    Iv = iv
                };
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

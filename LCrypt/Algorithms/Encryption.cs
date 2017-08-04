using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Timers;
using LCrypt.Properties;
using MahApps.Metro.Controls.Dialogs;

namespace LCrypt.Algorithms
{
    public class Encryption
    {
        public MainWindow MainWindow { get; set; }

        public SymmetricAlgorithm Algorithm { get; set; }

        public FileInfo Source { get; set; }
        public string Destination { get; set; }
        public string Password { get; set; }

        private readonly Stopwatch _stopwatch;
        private readonly Timer _timer = new Timer(500);

        public string LengthInMiB { get; set; }
        private double _mibPerSec;
        private long _totalBytes;

        public Encryption()
        {
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
            _stopwatch = Stopwatch.StartNew();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MainWindow.TblSpeed.Dispatcher.InvokeAsync(delegate
            {
                MainWindow.TblSpeed.Text = $"{_mibPerSec} MiB/s";
            });

            MainWindow.TblProgress.Dispatcher.InvokeAsync(delegate
            {
                var progressMiB = Math.Round(_totalBytes / 1048576d, 2);
                MainWindow.TblProgress.Text = $"{progressMiB} / {LengthInMiB} MiB";
            });

            MainWindow.PrBFileEncryption.Dispatcher.InvokeAsync(delegate
            {
                MainWindow.PrBFileEncryption.Value = _totalBytes;
            });
        }

        public async Task Encrypt()
        {
            var salt = new byte[8];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            using (var password = new Rfc2898DeriveBytes(Password, salt))
            {
                Algorithm.Key = password.GetBytes(Algorithm.KeySize / 8);
                Algorithm.GenerateIV();

                using (var sourceStream = new FileStream(Source.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
                {
                    using (var destinationStream = new FileStream(Destination, FileMode.Create, FileAccess.Write))
                    {
                        await destinationStream.WriteAsync(salt, 0, 8);
                        await destinationStream.WriteAsync(Algorithm.IV, 0, Algorithm.BlockSize / 8);

                        using (var transform = Algorithm.CreateEncryptor())
                        {
                            using (var cryptoStream =
                                new CryptoStream(destinationStream, transform, CryptoStreamMode.Write))
                            {
                                var buffer = new byte[1048576];
                                while (true)
                                {
                                    var readBytes = await sourceStream.ReadAsync(buffer, 0, 1048576);
                                    if (readBytes > 0)
                                    {
                                        await cryptoStream.WriteAsync(buffer, 0, readBytes);
                                        _totalBytes += readBytes;
                                        var bytesPerSecond = _totalBytes / _stopwatch.Elapsed.TotalSeconds;
                                        _mibPerSec = Math.Round(bytesPerSecond / 1048576d, 2);
                                    }
                                    else
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            _timer.Stop();
        }
    }
}
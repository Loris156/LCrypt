using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Timers;

namespace LCrypt.Algorithms
{
    public class Decryption
    {
        public MainWindow MainWindow { private get; set; }

        public SymmetricAlgorithm Algorithm { private get; set; }

        public FileInfo Source { private get; set; }
        public string Destination { private get; set; }
        public string Password { private get; set; }

        private readonly Stopwatch _stopwatch;
        private readonly Timer _timer = new Timer(500);

        public string LengthInMiB { private get; set; }
        private double _mibPerSec;
        private long _totalBytes;

        private bool _running;
        public Decryption()
        {
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
            _stopwatch = Stopwatch.StartNew();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_running)
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
        }

        public async Task Decrypt()
        {
            _running = true;

            var salt = new byte[8];
            using (var sourceStream = new FileStream(Source.FullName, FileMode.Open, FileAccess.Read))
            {
                await sourceStream.ReadAsync(salt, 0, 8);

                using (var password = new Rfc2898DeriveBytes(Password, salt))
                {
                    Algorithm.Key = password.GetBytes(Algorithm.KeySize / 8);
                }

                var iv = new byte[Algorithm.BlockSize / 8];
                await sourceStream.ReadAsync(iv, 0, Algorithm.BlockSize / 8);
                Algorithm.IV = iv;

                using (var destinationStream = new FileStream(Destination, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    using (var transform = Algorithm.CreateDecryptor())
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
            _timer.Stop();
            _running = false;
        }
    }
}
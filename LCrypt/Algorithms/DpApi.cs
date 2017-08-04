using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using LCrypt.Properties;
using LCrypt.Utility;
using MahApps.Metro.Controls.Dialogs;
using Timer = System.Timers.Timer;

namespace LCrypt.Algorithms
{
    public class DpApi
    {
        public MainWindow MainWindow { get; set; }

        public FileInfo Source { get; set; }
        public string Destination { get; set; }

        private readonly DataProtectionScope _protectionScope;

        public DpApi(MainWindow window, FileInfo source, string destination, string lengthInMiB,
            DataProtectionScope protectionScope)
        {
            MainWindow = window;
            Source = source;
            Destination = destination;
            _protectionScope = protectionScope;
        }

        public async Task Encrypt()
        {
            var entropy = new byte[16];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(entropy);
            }

            var sourceFile = await Util.ReadAllBytesAsync(Source.FullName);
            var encrypted = await Task.Run(() => ProtectedData.Protect(sourceFile, entropy, _protectionScope));

            using (var destinationStream =
                new FileStream(Destination, FileMode.Create, FileAccess.Write, FileShare.None, 4069, useAsync: true))
            {
                await destinationStream.WriteAsync(entropy, 0, 16);
                await destinationStream.WriteAsync(encrypted, 0, encrypted.Length);
            }
        }

        public async Task Decrypt()
        {
            var entropy = new byte[16];
            byte[] encryptedData;
            using (var sourceStream = new FileStream(Source.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                var fileLength = sourceStream.Length;
                if (fileLength > Int32.MaxValue)
                    throw new IOException("File is larger than 2GB.");
                var count = (int)fileLength - 16; //Remove 16 bytes used for entropy

                
                if (await sourceStream.ReadAsync(entropy, 0, 16) != 16)
                    throw new IOException("Could not read entropy.");

                encryptedData = new byte[count];
                while (true)
                {
                    if (await sourceStream.ReadAsync(encryptedData, 0, count) < 1)
                        break;
                }
            }

            var decryptedData = await Task.Run(() => ProtectedData.Unprotect(encryptedData, entropy, _protectionScope));

            using (var destinationStream =
                new FileStream(Destination, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await destinationStream.WriteAsync(decryptedData, 0, decryptedData.Length);
            }
        }
    }
}
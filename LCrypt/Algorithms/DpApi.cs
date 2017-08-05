using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LCrypt.Utility;

namespace LCrypt.Algorithms
{
    public class DpApi
    {
        private FileInfo Source { get; }
        private string Destination { get; }

        private readonly DataProtectionScope _protectionScope;

        public DpApi(FileInfo source, string destination,
            DataProtectionScope protectionScope)
        {
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
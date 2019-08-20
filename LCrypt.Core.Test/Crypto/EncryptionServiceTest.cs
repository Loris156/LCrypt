using LCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LCrypt.Core.Test.Crypto
{
    public class EncryptionServiceTest
    {
        private const string Password = "1234";

        private static readonly byte[] _sourceData = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        private static readonly byte[] _v1Data = new byte[] { 76, 67, 114, 121, 112, 116, 1, 0, 1, 212, 192, 0, 0, 0, 16, 55, 239, 201, 151, 139, 7, 191, 203, 39, 117, 71, 234, 226, 110, 205, 184, 3, 97, 101, 115, 0, 0, 0, 16, 42, 187, 100, 170, 233, 140, 82, 236, 120, 222, 75, 29, 195, 39, 195, 14, 136, 70, 240, 46, 224, 193, 81, 163, 26, 69, 86, 208, 51, 56, 240, 57 };

        [Fact]
        public async Task EncryptAsync()
        {
            using (var sourceStream = new MemoryStream(_sourceData))
            {
                using (var destinationStream = new MemoryStream())
                {
                    using (var encryptionService = new EncryptionService(Algorithm.GetByName("aes"), sourceStream, destinationStream, Password, null))
                    {
                        await encryptionService.EncryptAsync().ConfigureAwait(false);
                    }

                    Debug.Assert(destinationStream.ToArray().Length > 0);
                }
            }
        }

        [Fact]
        public async Task DecryptAsync()
        {
            using (var sourceStream = new MemoryStream(_v1Data))
            {
                using (var destinationStream = new MemoryStream())
                {
                    using (var encryptionService = new DecryptionService(sourceStream, destinationStream, Password, null))
                    {
                        await encryptionService.DecryptAsync().ConfigureAwait(false);
                    }

                    Debug.Assert(destinationStream.ToArray().SequenceEqual(_sourceData));
                }
            }
        }
    }
}

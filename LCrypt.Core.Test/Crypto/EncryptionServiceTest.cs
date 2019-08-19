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

        private static readonly byte[] _v1Data = new byte[] { 76, 67, 114, 121, 112, 116, 0, 0, 0, 1, 0, 1, 212, 192, 0, 0, 0, 16, 96, 70, 124, 214, 203, 65, 189, 67, 174, 227, 143, 217, 210, 141, 197, 123, 0, 0, 0, 16, 35, 88, 115, 229, 5, 239, 45, 208, 94, 243, 58, 90, 220, 45, 138, 61, 143, 207, 106, 233, 26, 49, 90, 57, 23, 12, 218, 245, 11, 38, 241, 206 };

        [Fact]
        public async Task EncryptAsync()
        {
            using(var sourceStream = new MemoryStream(_sourceData))
            {
                using(var destinationStream = new MemoryStream())
                {
                    using (var encryptionService = new EncryptionService(new AesManaged(), sourceStream, destinationStream, Password, null))
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
            using(var sourceStream = new MemoryStream(_v1Data))
            {
                using(var destinationStream = new MemoryStream())
                {
                    using(var encryptionService = new EncryptionService(new AesManaged(), sourceStream, destinationStream, Password, null))
                    {
                        await encryptionService.DecryptAsync().ConfigureAwait(false);
                    }

                    Debug.Assert(destinationStream.ToArray().SequenceEqual(_sourceData));
                }
            }
        }
    }
}

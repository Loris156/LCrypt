using LCrypt.Utility.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LCryptTest.Utility.ExtensionTests
{
    [TestClass]
    public class SymmetricAlgorithmExtensionTest
    {
        private readonly byte[] _key =
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28,
            29, 30, 31
        };

        private readonly byte[] _iv =
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
        };

        [TestMethod]
        public async Task EncryptDecryptString()
        {
            const string input = "LCrypt String Test";
            string output;

            using (var aes = new AesManaged())
            {
                aes.Key = _key;
                aes.IV = _iv;

                var encrypted = await aes.EncryptStringAsync(input);
                output = await aes.DecryptStringAsync(encrypted);
            }

            Assert.AreEqual(input, output);
        }

        [TestMethod]
        public async Task EncryptDecryptStringExceptions()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                await ((SymmetricAlgorithm) null).EncryptStringAsync(""));

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                await ((SymmetricAlgorithm)null).DecryptStringAsync(null));

            using (var aes = new AesManaged())
            {
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                    aes.EncryptStringAsync(null));
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                    aes.DecryptStringAsync(null));
            }           
        }
    }
}

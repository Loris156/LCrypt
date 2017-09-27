using System;
using LCrypt.HashAlgorithms;
using LCrypt.TextEncodings;
using LCrypt.Utility.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.Text;

namespace LCryptTest.Utility.ExtensionTests
{
    [TestClass]
    public class HashAlgorithmExtensionTest
    {
        private const string Input = "LCrypt Unit Test";

        [TestMethod]
        public void ComputeHashOfStringMd5Test()
        {
            const string expected = "6815028c72f1c2c70b89350c9ccdd9b6";

            using (var md5 = new Md5().Create())
            {
                Assert.IsInstanceOfType(md5, typeof(MD5CryptoServiceProvider));
                var hash = md5.ComputeHash(Input, new Utf8().Create());
                Assert.AreEqual(expected, hash.ToHexString());
            }
        }

        [TestMethod]
        public void ComputeHashOfStringCrc32Test()
        {
            const string expected = "461c0cc9";

            using (var crc32 = new Crc32().Create())
            {
                Assert.IsInstanceOfType(crc32, typeof(Crc32Managed));
                var hash = crc32.ComputeHash(Input, new Utf8().Create());
                Assert.AreEqual(expected, hash.ToHexString());
            }
        }

        [TestMethod]
        public void ComputeHashOfStringSha1Test()
        {
            const string expected = "c73d74c90f702355c5d03f4ff4129f2572a287a6";

            using (var sha1 = new Sha1().Create())
            {
                Assert.IsInstanceOfType(sha1, typeof(SHA1Managed));
                var hash = sha1.ComputeHash(Input, new Utf8().Create());
                Assert.AreEqual(expected, hash.ToHexString());
            }
        }

        [TestMethod]
        public void ComputeHashOfStringSha256Test()
        {
            const string expected = "e71d21b0e68bfbe73e5a0db32edb673909b8634396b51b9e93d139307aa95bf1";

            using (var sha256 = new Sha256().Create())
            {
                Assert.IsInstanceOfType(sha256, typeof(SHA256Managed));
                var hash = sha256.ComputeHash(Input, new Utf8().Create());
                Assert.AreEqual(expected, hash.ToHexString());
            }
        }

        [TestMethod]
        public void ComputeHashOfStringSha384Test()
        {
            const string expected =
                "0b2099f21d3514c9d5063ee76a27a82e5775d5d12c20728f17cd818d3723234f01578c1f5b5e1bfeef650b31647517fd";

            using (var sha384 = new Sha384().Create())
            {
                Assert.IsInstanceOfType(sha384, typeof(SHA384Managed));
                var hash = sha384.ComputeHash(Input, new Utf8().Create());
                Assert.AreEqual(expected, hash.ToHexString());
            }
        }

        [TestMethod]
        public void ComputeHashOfStringSha512Test()
        {
            const string expected =
                "37e4970f59a1dc2cd3f3c94f59f27f4f609aeaa715824ca1120a9e7e0038f6613977d1158c401ba6b850658ed524500d7b1e7a1b4e42a51dc4f7149f5913154c";

            using (var sha512 = new Sha512().Create())
            {
                Assert.IsInstanceOfType(sha512, typeof(SHA512Managed));
                var hash = sha512.ComputeHash(Input, new Utf8().Create());
                Assert.AreEqual(expected, hash.ToHexString());
            }
        }

        [TestMethod]
        public void ComputeHashOfStringWhirlpoolTest()
        {
            const string expected =
                "026a68b18b45a31ba72fb6225833542e0b911d16e62b282958de44e0702cc46ba3c2aceab8dfdfa3a80af41736428dd5bb16ae18c6018d0756e8de9fa683a4ce";

            using (var whirlpool = new Whirlpool().Create())
            {
                Assert.IsInstanceOfType(whirlpool, typeof(WhirlpoolManaged));
                var hash = whirlpool.ComputeHash(Input, new Utf8().Create());
                Assert.AreEqual(expected, hash.ToHexString());
            }
        }

        [TestMethod]
        public void ComputeHashOfStringExceptionTest()
        {
            var nullAlgorithm = (HashAlgorithm)null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.ThrowsException<ArgumentNullException>(() => nullAlgorithm.ComputeHash("", Encoding.Unicode));

            var algorithm = new Md5().Create();
            Assert.IsNull(algorithm.ComputeHash(null, Encoding.Unicode));

            // ReSharper disable once AccessToDisposedClosure
            Assert.ThrowsException<ArgumentNullException>(() => algorithm.ComputeHash("", null));

            algorithm.Dispose();
        }
    }
}

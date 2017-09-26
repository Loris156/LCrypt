using System;
using System.Windows.Media;
using LCrypt.HashAlgorithms;
using LCrypt.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LCryptTest.Utility
{
    [TestClass]
    public class UtilTest
    {
        [TestMethod]
        public void GenerateStrongRandomBytesTest()
        {
            Assert.IsTrue(Util.GenerateStrongRandomBytes(8).Length == 8);
            Assert.ThrowsException<ArgumentException>(() => Util.GenerateStrongRandomBytes(-1));
        }

        [TestMethod]
        public void ExtractFileIconTest()
        {
            Assert.IsInstanceOfType(Util.ExtractFileIcon("LCrypt.exe"), typeof(ImageSource));
            Assert.IsNull(Util.ExtractFileIcon("unknownfile.txt"));
        }

        [TestMethod]
        public void GetHashAlgorithmTest()
        {
            Assert.IsInstanceOfType(Util.GetHashAlgorithm("MD5"), typeof(Md5));
            Assert.IsInstanceOfType(Util.GetHashAlgorithm("CRC32"), typeof(Crc32));
            Assert.IsInstanceOfType(Util.GetHashAlgorithm("SHA-1"), typeof(Sha1));
            Assert.IsInstanceOfType(Util.GetHashAlgorithm("SHA-256"), typeof(Sha256));
            Assert.IsInstanceOfType(Util.GetHashAlgorithm("SHA-384"), typeof(Sha384));
            Assert.IsInstanceOfType(Util.GetHashAlgorithm("SHA-512"), typeof(Sha512));
            Assert.IsInstanceOfType(Util.GetHashAlgorithm("Whirlpool"), typeof(Whirlpool));

            Assert.ThrowsException<ArgumentException>(() => Util.GetHashAlgorithm("Hash"));
            Assert.ThrowsException<ArgumentNullException>(() => Util.GetHashAlgorithm(null));
        }
    }
}

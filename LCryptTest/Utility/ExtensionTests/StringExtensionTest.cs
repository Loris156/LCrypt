using System;
using System.Diagnostics;
using System.Linq;
using LCrypt.Utility.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LCryptTest.Utility.ExtensionTests
{
    [TestClass]
    public class StringExtensionTest
    {
        [TestMethod]
        public void IsHexTest()
        {
            Assert.IsFalse(((string)null).IsHex());
            Assert.IsTrue("af6b3c".IsHex());
            Assert.IsTrue("AF6B3C".IsHex());
        }

        [TestMethod]
        public void ToSecureStringTest()
        {
            const string input = "SecureString Test";
            var secureString = input.ToSecureString();
            Assert.IsTrue(secureString.Length == input.Length);
        }

        [TestMethod]
        public void ToSecureStringExceptionTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => ((string)null).ToSecureString());
        }

        [TestMethod]
        public void ToByteArrayTest()
        {
            const string input = "0F0614010A34";
            var expected = new byte[] { 15, 6, 20, 1, 10, 52 };

            var inputBytes = input.ToByteArray();
            Assert.IsTrue(inputBytes.SequenceEqual(expected));
        }

        [TestMethod]
        public void ToByteArrayExceptionTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => ((string)null).ToByteArray());
            Assert.ThrowsException<FormatException>(() => "XZ3P".ToByteArray());
        }
    }
}

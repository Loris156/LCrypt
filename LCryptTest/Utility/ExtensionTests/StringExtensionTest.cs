using System;
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
    }
}

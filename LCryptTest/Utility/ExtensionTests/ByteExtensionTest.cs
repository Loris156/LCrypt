using System;
using LCrypt.Utility.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LCryptTest.Utility.ExtensionTests
{
    [TestClass]
    public class ByteExtensionTest
    {
        [TestMethod]
        public void ToHexStringTest()
        {
            var input = new byte[]
            {
                20,
                50,
                100,
                150,
                200,
                255
            };

            Assert.ThrowsException<ArgumentNullException>(() => ((byte[])null).ToHexString());
            Assert.AreEqual(new byte[0].ToHexString(), string.Empty);
            Assert.AreEqual(input.ToHexString(), "14326496c8ff");

            Assert.AreEqual(input.ToHexString(upperCase: true, hyphens: false), "14326496C8FF");
            Assert.AreEqual(input.ToHexString(upperCase: false, hyphens: true), "14-32-64-96-c8-ff");
            Assert.AreEqual(input.ToHexString(upperCase: true, hyphens: true), "14-32-64-96-C8-FF");
        }
    }
}

using System;
using System.Linq;
using LCrypt.Utility.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security;
using System.Text;

namespace LCryptTest.Utility.ExtensionTests
{
    [TestClass]
    public class SecureStringExtensionTest
    {
        [TestMethod]
        public void EqualityTest()
        {
            var string1 = new SecureString();
            foreach (var c in "Password")
                string1.AppendChar(c);

            var string2 = new SecureString();
            foreach (var c in "Password")
                string2.AppendChar(c);

            Assert.IsTrue(string1.IsEqual(string2));

            var string3 = new SecureString();
            foreach (var c in "InvalidPassword")
                string1.AppendChar(c);

            Assert.IsFalse(string1.IsEqual(string3));

            Assert.ThrowsException<ArgumentNullException>(() => ((SecureString) null).IsEqual(string1));
            Assert.ThrowsException<ArgumentNullException>(() => string1.IsEqual(null));
        }

        [TestMethod]
        public void ToUnsecureStringTest()
        {
            const string expected = "Unsecure String Test";

            var secureString = new SecureString();
            foreach (var c in expected)
                secureString.AppendChar(c);

            var unsecureString = secureString.ToInsecureString();
            Assert.AreEqual(expected, unsecureString);
        }

        [TestMethod]
        public void ToInsecureStringExceptionTest()
        {          
            Assert.ThrowsException<ArgumentNullException>(() => ((SecureString)null).ToInsecureString());

            var secureString = new SecureString();
            Assert.AreEqual(string.Empty, secureString.ToInsecureString());
        }

        [TestMethod]
        public void ToBytesTest()
        {
            const string input = "ToByteArray";
            var secureString = new SecureString();
            var expected = Encoding.Unicode.GetBytes(input);

            foreach (var c in input)
                secureString.AppendChar(c);

            Assert.IsTrue(expected.SequenceEqual(secureString.ToBytes()));
        }

        [TestMethod]
        public void ToBytesExceptionTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => ((SecureString) null).ToBytes());
        }
    }
}

using System;
using System.Security.Cryptography;
using LCrypt.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LCryptTest.PasswordManager
{
    [TestClass]
    public class PasswordStorageTest
    {
        [TestMethod]
        public void PasswordStorageConstructorTest()
        {
            var storage = new PasswordStorage();
            Assert.AreNotEqual(Guid.Empty, storage.Guid);

            Assert.IsNotNull(storage.Entries);
            Assert.IsNotNull(storage.Categories);
        }
    }
}

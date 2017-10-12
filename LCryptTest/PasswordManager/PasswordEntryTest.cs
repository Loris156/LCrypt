using LCrypt.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LCryptTest.PasswordManager
{
    [TestClass]
    public class PasswordEntryTest
    {
        [TestMethod]
        public void CloneTest()
        {
            var entry = new PasswordEntry
            {
                Name = "Old",
                Username = "Old",
                Email = "Old",
                Url = "Old",
                Comment = "Old"
            };

            var clonedEntry = (PasswordEntry) entry.Clone();
            Assert.AreNotSame(entry, clonedEntry);
            Assert.AreEqual(entry.Guid, clonedEntry.Guid);

            clonedEntry.Name = "New";
            clonedEntry.Username = "New";
            clonedEntry.Email = "New";
            clonedEntry.Url = "New";
            clonedEntry.Comment = "New";

            Assert.AreNotEqual(clonedEntry.Name, entry.Name);
            Assert.AreNotEqual(clonedEntry.Username, entry.Username);
            Assert.AreNotEqual(clonedEntry.Email, entry.Email);
            Assert.AreNotEqual(clonedEntry.Url, entry.Url);
            Assert.AreNotEqual(clonedEntry.Comment, entry.Comment);
        }
    }
}

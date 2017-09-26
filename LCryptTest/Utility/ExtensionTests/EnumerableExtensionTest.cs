using System;
using System.Collections.Generic;
using System.Linq;
using LCrypt.Utility.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LCryptTest.Utility.ExtensionTests
{
    [TestClass]
    public class EnumerableExtensionTest
    {
        [TestMethod]
        public void ForEachTest()
        {
            ICollection<int> enumerable = new List<int>(100);
            ICollection<int> copy = new List<int>(100);

            for (var i = 0; i < 100; i++)
                enumerable.Add(0);

            enumerable.ForEach(i => copy.Add(i));

            Assert.IsTrue(copy.Count == 100);
            Assert.IsTrue(copy.All(i => i == 0));

            Assert.ThrowsException<ArgumentNullException>(() => ((IEnumerable<int>)null).ForEach(i => { }));
            Assert.ThrowsException<ArgumentNullException>(() => enumerable.ForEach(null));
        }
    }
}

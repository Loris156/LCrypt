using System;
using System.Globalization;
using LCrypt.Value_Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable PossibleNullReferenceException

namespace LCryptTest.Converter
{
    [TestClass]
    public class BoolInverterTest
    {
        private readonly BoolInverter _converter;

        public BoolInverterTest()
        {
            _converter = new BoolInverter();
            Assert.IsNotNull(_converter);
        }

        [TestMethod]
        public void ConvertTest()
        {
            Assert.IsInstanceOfType(
                (bool) _converter.Convert(false, typeof(bool), null, CultureInfo.InvariantCulture), typeof(bool));
            Assert.IsTrue((bool) _converter.Convert(false, typeof(bool), null, CultureInfo.InvariantCulture));
            Assert.IsFalse((bool) _converter.Convert(true, typeof(bool), null, CultureInfo.InvariantCulture));
            Assert.ThrowsException<ArgumentException>(() => _converter.Convert(new object(), typeof(bool), null,
                CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void ConvertBackTest()
        {          
            Assert.ThrowsException<InvalidOperationException>(() => _converter.ConvertBack(new object(), typeof(bool),
                null, CultureInfo.InvariantCulture));
        }
    }
}

using System;
using System.Security;
using System.Text.RegularExpressions;

namespace LCrypt.Utility.Extensions
{
    public static class StringExtensions
    {
        public static bool IsHex(this string s)
        {
            return s != null && Regex.IsMatch(s, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        public static SecureString ToSecureString(this string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            var secureString = new SecureString();
            s.ForEach(c => secureString.AppendChar(c));
            return secureString;
        }

        public static byte[] ToByteArray(this string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (!s.IsHex())
                throw new FormatException("Input was not a hexadecimal string.");

            var length = s.Length;
            var bytes = new byte[length / 2];
            for (var i = 0; i < length; i += 2)
                bytes[i / 2] = Convert.ToByte(s.Substring(i, 2), 16);
            return bytes;
        }
    }
}

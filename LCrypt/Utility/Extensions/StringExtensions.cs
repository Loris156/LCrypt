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
    }
}

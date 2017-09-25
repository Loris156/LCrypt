using System.Text.RegularExpressions;

namespace LCrypt.Utility.Extensions
{
    public static class StringExtensions
    {
        public static bool IsHex(this string s)
        {
            return s != null && Regex.IsMatch(s, @"\A\b[0-9a-fA-F]+\b\Z");
        }
    }
}

using System;

namespace LCrypt.Utility.Extensions
{
    public static class ByteExtensions
    {
        public static string ToHexString(this byte[] bytes, bool upperCase = false, bool hyphens = false)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length < 1) return string.Empty;

            var hex = BitConverter.ToString(bytes);
            if (!upperCase)
                hex = hex.ToLowerInvariant();
            if (!hyphens)
                hex = hex.Replace("-", "");
            return hex;
        }
    }
}

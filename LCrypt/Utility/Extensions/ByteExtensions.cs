using System;

namespace LCrypt.Utility.Extensions
{
    public static class ByteExtensions
    {
        public static string ToHexString(this byte[] bytes, bool upperCase = false, bool hyphens = false)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            var hex = BitConverter.ToString(bytes);
            if (!upperCase)
                hex = hex.ToLowerInvariant();
            if (!hyphens)
                hex = hex.Replace("-", "");
            return hex;
        }
    }
}

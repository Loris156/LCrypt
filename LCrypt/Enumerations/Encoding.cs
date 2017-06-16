using System;

namespace LCrypt.Enumerations
{
    public enum Encoding
    {
        Ascii,
        Utf7,
        Utf8,
        Utf16,
        Utf16BigEndian,
        Utf32
    }

    public static class HashEncodingConverter
    {
        public static System.Text.Encoding GetEncoding(this Encoding encoding)
        {
            switch (encoding)
            {
                case Encoding.Ascii:
                    return System.Text.Encoding.ASCII;
                case Encoding.Utf7:
                    return System.Text.Encoding.UTF7;
                case Encoding.Utf8:
                    return System.Text.Encoding.UTF8;
                case Encoding.Utf16:
                    return System.Text.Encoding.Unicode;
                case Encoding.Utf16BigEndian:
                    return System.Text.Encoding.BigEndianUnicode;
                case Encoding.Utf32:
                    return System.Text.Encoding.UTF32;
                default:
                    throw new ArgumentOutOfRangeException(nameof(encoding), encoding, null);
            }
        }

        public static byte[] GetBytesFromText(this Encoding encoding, string text)
        {
            switch (encoding)
            {
                case Encoding.Ascii:
                    return System.Text.Encoding.ASCII.GetBytes(text);
                case Encoding.Utf7:
                    return System.Text.Encoding.UTF7.GetBytes(text);
                case Encoding.Utf8:
                    return System.Text.Encoding.UTF8.GetBytes(text);
                case Encoding.Utf16:
                    return System.Text.Encoding.Unicode.GetBytes(text);
                case Encoding.Utf16BigEndian:
                    return System.Text.Encoding.BigEndianUnicode.GetBytes(text);
                case Encoding.Utf32:
                    return System.Text.Encoding.UTF32.GetBytes(text);
                default:
                    throw new ArgumentOutOfRangeException(nameof(encoding), encoding, null);
            }
        }
    }
}
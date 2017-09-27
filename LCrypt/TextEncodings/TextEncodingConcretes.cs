using System.Text;

namespace LCrypt.TextEncodings
{
    public class Ascii : ITextEncoding
    {
        public string DisplayName => "ASCII";

        public Encoding Create()
        {
            return Encoding.ASCII;
        }

        public override string ToString() => DisplayName;
    }

    public class Utf8 : ITextEncoding
    {
        public string DisplayName => "UTF-8";

        public Encoding Create()
        {
            return Encoding.UTF8;
        }

        public override string ToString() => DisplayName;
    }

    public class Utf16 : ITextEncoding
    {
        public string DisplayName => "UTF-16 (Windows Unicode)";

        public Encoding Create()
        {
            return Encoding.Unicode;
        }

        public override string ToString() => DisplayName;
    }

    public class Utf32 : ITextEncoding
    {
        public string DisplayName => "UTF-32";

        public Encoding Create()
        {
            return Encoding.UTF32;
        }

        public override string ToString() => DisplayName;
    }

    public class BigEndianUtf16 : ITextEncoding
    {
        public string DisplayName => "Big Endian UTF-16";

        public Encoding Create()
        {
            return Encoding.BigEndianUnicode;
        }

        public override string ToString() => DisplayName;
    }
}

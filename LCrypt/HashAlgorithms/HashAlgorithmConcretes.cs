using System.Security.Cryptography;

namespace LCrypt.HashAlgorithms
{
    public class Md5 : IHashAlgorithm
    {
        public string DisplayName => "Message-Digest Algorithm 5 (MD5)";

        public HashAlgorithm Create()
        {
            return new MD5CryptoServiceProvider();
        }
    }

    public class Sha1 : IHashAlgorithm
    {
        public string DisplayName => "Secure Hash Algorithm 1 (SHA-1)";

        public HashAlgorithm Create()
        {
            return new SHA1Managed();
        }
    }

    public class Sha256 : IHashAlgorithm
    {
        public string DisplayName => "Secure Hash Algorithm 256 (SHA-256)";

        public HashAlgorithm Create()
        {
            return new SHA256Managed();
        }
    }

    public class Sha384 : IHashAlgorithm
    {
        public string DisplayName => "Secure Hash Algorithm 384 (SHA-384)";

        public HashAlgorithm Create()
        {
            return new SHA384Managed();
        }
    }

    public class Sha512 : IHashAlgorithm
    {
        public string DisplayName => "Secure Hash Algorithm 512 (SHA-512)";

        public HashAlgorithm Create()
        {
            return new SHA512Managed();
        }
    }

    public class Whirlpool : IHashAlgorithm
    {
        public string DisplayName => "Whirlpool";

        public HashAlgorithm Create()
        {
            return new WhirlpoolManaged();
        }
    }

    public class Crc32 : IHashAlgorithm
    {
        public string DisplayName => "Cyclic Redundancy Check 32 (CRC32)";

        public HashAlgorithm Create()
        {
            return new Crc32Managed();
        }
    }
}
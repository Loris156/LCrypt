using System;
using System.Security.Cryptography;

namespace LCrypt.Enumerations
{
    public enum HashAlgorithm
    {
        Md5,
        Sha1,
        Sha256,
        Sha384,
        Sha512
    }

    public static class HashAlgorithmConverter
    {
        public static System.Security.Cryptography.HashAlgorithm GetAlgorithm(this HashAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case HashAlgorithm.Md5:
                    return new MD5CryptoServiceProvider();
                case HashAlgorithm.Sha1:
                    return new SHA1Managed();
                case HashAlgorithm.Sha256:
                    return new SHA256Managed();
                case HashAlgorithm.Sha384:
                    return new SHA384Managed();
                case HashAlgorithm.Sha512:
                    return new SHA512Managed();
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
            }
        }
    }
}
using System;
using System.Security.Cryptography;

namespace LCrypt.Enumerations
{
    public enum Algorithm
    {
        Aes,
        Des,
        Tdea,
        Rc2
    }

    public static class AlgorithmConverter
    {
        public static SymmetricAlgorithm GetAlgorithm(this Algorithm algorithm)
        {
            switch (algorithm)
            {
                case Algorithm.Aes:
                    return new AesManaged();
                case Algorithm.Des:
                    return new DESCryptoServiceProvider();
                case Algorithm.Tdea:
                    return new TripleDESCryptoServiceProvider();
                case Algorithm.Rc2:
                    return new RC2CryptoServiceProvider();
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
            }
        }
    }
}
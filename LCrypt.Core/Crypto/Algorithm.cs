using System;
using System.Security.Cryptography;

namespace LCrypt.Core.Crypto
{
    public class Algorithm
        : IAlgorithm
    {
        public Algorithm(SymmetricAlgorithm algorithm, string name)
        {
            SymmetricAlgorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public SymmetricAlgorithm SymmetricAlgorithm { get; }

        public string Name { get; }

        public static IAlgorithm GetByName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            switch (name)
            {
                case "aes":
                    return new Algorithm(new AesManaged(), "aes");
                case "des":
                    return new Algorithm(new DESCryptoServiceProvider(), "des");
                case "tdes":
                    return new Algorithm(new TripleDESCryptoServiceProvider(), "tdes");
                case "rc2":
                    return new Algorithm(new RC2CryptoServiceProvider(), "rc2");
                default:
                    throw new ArgumentException("unknown algorithm name");
            }
        }

        public void Dispose()
        {
            SymmetricAlgorithm.Dispose();
        }
    }
}

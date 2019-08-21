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

            return name switch
            {
                "aes" => new Algorithm(new AesManaged(), "aes"),
                "des" => new Algorithm(new DESCryptoServiceProvider(), "des"),
                "tdes" => new Algorithm(new TripleDESCryptoServiceProvider(), "tdes"),
                "rc2" => new Algorithm(new RC2CryptoServiceProvider(), "rc2"),
                _ => throw new ArgumentException("unknown algorithm name"),
            };
        }

        public void Dispose()
        {
            SymmetricAlgorithm.Dispose();
        }
    }
}

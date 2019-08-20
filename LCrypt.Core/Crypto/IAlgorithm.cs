using System;
using System.Security.Cryptography;

namespace LCrypt.Core.Crypto
{
    public interface IAlgorithm
        : IDisposable
    {
        SymmetricAlgorithm SymmetricAlgorithm { get; }

        string Name { get; }
    }
}

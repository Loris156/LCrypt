using System.Security.Cryptography;

namespace LCrypt.HashAlgorithms
{
    public interface IHashAlgorithm
    {
        string DisplayName { get; }

        HashAlgorithm Create();
    }
}

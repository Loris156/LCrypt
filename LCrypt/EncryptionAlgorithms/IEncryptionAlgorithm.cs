using System.Security.Cryptography;

namespace LCrypt.EncryptionAlgorithms
{
    public interface IEncryptionAlgorithm
    {
        string DisplayName { get; }

        SymmetricAlgorithm Create();
    }
}

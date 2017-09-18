using System.Security.Cryptography;

namespace LCrypt.EncryptionAlgorithms
{
    public class Aes256 : IEncryptionAlgorithm
    {
        public string DisplayName => "Advanced Encryption Standard (AES-256)";

        public SymmetricAlgorithm Create()
        {
            return new AesManaged();
        }

        public override string ToString() => DisplayName;
    }

    public class Des64 : IEncryptionAlgorithm
    {
        public string DisplayName => "Data Encryption Standard (DES-64)";

        public SymmetricAlgorithm Create()
        {
            return new DESCryptoServiceProvider();
        }

        public override string ToString() => DisplayName;
    }

    public class Tdea192 : IEncryptionAlgorithm
    {
        public string DisplayName => "Triple Data Encryption Standard (3DES/TDEA)";

        public SymmetricAlgorithm Create()
        {
            return new TripleDESCryptoServiceProvider();
        }

        public override string ToString() => DisplayName;
    }

    public class Rc2 : IEncryptionAlgorithm
    {
        public string DisplayName => "Rivest Cipher 2 (RC2/ARC2)";

        public SymmetricAlgorithm Create()
        {
            return new RC2CryptoServiceProvider();
        }

        public override string ToString() => DisplayName;
    }
}

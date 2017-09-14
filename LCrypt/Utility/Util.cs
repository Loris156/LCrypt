using System.Security;

namespace LCrypt.Utility
{
    public static class Util
    {
        public static byte[] DeriveKey(this SecureString secureString, byte[] salt, int iterations,
            int keyLengthInBytes)
        {
            // TODO
            return null;
        }
    }
}

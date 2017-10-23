using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LCrypt.Utility.Extensions
{
    public static class SecureStringExtensions
    {
        public static async Task<byte[]> DeriveKeyAsync(this SecureString secureString, byte[] salt, int iterations,
            int keyLengthInBytes, CancellationToken cancellationToken)
        {
            if (secureString == null)
                throw new ArgumentNullException(nameof(secureString));
            if (salt == null)
                throw new ArgumentNullException(nameof(salt));
            if (salt.Length < 8)
                throw new ArgumentException("The specified salt size is smaller than 8 bytes.", nameof(salt));
            if (iterations < 1)
                throw new ArgumentException("Iteration count cannot be smaller than 1.", nameof(iterations));
            if (keyLengthInBytes < 1)
                throw new ArgumentException("The specified key size is smaller than 1 byte.", nameof(keyLengthInBytes));

            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var ptr = Marshal.SecureStringToBSTR(secureString);
                var length = Marshal.ReadInt32(ptr, -4);
                var passwordByteArray = new byte[length];
                var gcHandle = GCHandle.Alloc(passwordByteArray, GCHandleType.Pinned);

                try
                {
                    for (var index = 0; index < length; ++index)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        passwordByteArray[index] = Marshal.ReadByte(ptr, index);
                    }

                    using (var rfc = new Rfc2898DeriveBytes(passwordByteArray, salt, iterations))
                    {
                        return rfc.GetBytes(keyLengthInBytes);
                    }
                }
                finally
                {
                    Array.Clear(passwordByteArray, 0, length);
                    gcHandle.Free();
                    Marshal.ZeroFreeBSTR(ptr);
                }
            }, cancellationToken);
        }

        public static bool IsEqual(this SecureString secureString, SecureString other)
        {
            if (secureString == null)
                throw new ArgumentNullException(nameof(secureString));
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (secureString.Length != other.Length)
                return false;

            IntPtr bstr1 = IntPtr.Zero, bstr2 = IntPtr.Zero;

            try
            {
                bstr1 = Marshal.SecureStringToBSTR(secureString);
                bstr2 = Marshal.SecureStringToBSTR(other);

                var length1 = Marshal.ReadInt32(bstr1, -4);
                var length2 = Marshal.ReadInt32(bstr2, -4);
                if (length1 == length2)
                    for (var index = 0; index < length1; index++)
                    {
                        var b1 = Marshal.ReadByte(bstr1, index);
                        var b2 = Marshal.ReadByte(bstr2, index);
                        if (b1 != b2) return false;
                    }
                else
                    return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (bstr2 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr2);
                if (bstr1 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr1);
            }
        }

        public static string ToInsecureString(this SecureString secureString)
        {
            if (secureString == null)
                throw new ArgumentNullException(nameof(secureString));
            if (secureString.Length == 0)
                return string.Empty;

            var intPtr = IntPtr.Zero;

            try
            {
                intPtr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(intPtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(intPtr);
            }
        }

        public static byte[] ToBytes(this SecureString secureString)
        {
            if (secureString == null)
                throw new ArgumentNullException(nameof(secureString));

            var unmanagedString = IntPtr.Zero;

            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Encoding.Unicode.GetBytes(Marshal.PtrToStringUni(unmanagedString));
            }
            finally
            {
                if (unmanagedString != IntPtr.Zero)
                    Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}

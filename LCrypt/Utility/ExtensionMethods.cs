using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LCrypt.Utility
{
    public static class ExtensionMethods
    {
        public static ImageSource ToImageSource(this Icon icon)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon));

            using (var bitmap = icon.ToBitmap())
            {
                return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
        }

        public static int GetNextInt32(this RNGCryptoServiceProvider rng, int maxValue)
        {
            if (maxValue < 1)
                throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, "Value must be positive.");

            var buffer = new byte[4];
            int bits, val;

            if ((maxValue & -maxValue) == maxValue)
            {
                rng.GetBytes(buffer);
                bits = BitConverter.ToInt32(buffer, 0);
                return bits & (maxValue - 1);
            }

            do
            {
                rng.GetBytes(buffer);
                bits = BitConverter.ToInt32(buffer, 0) & 0x7FFFFFFF;
                val = bits % maxValue;
            } while (bits - val + (maxValue - 1) < 0);

            return val;
        }

        public static string RemoveTrailingZeros(this Version version)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            var output = version.ToString();
            while (output.EndsWith("0"))
                output = output.Remove(output.Length - 2);
            return output;
        }

        public static byte[] ToArray(this SecureString secureString)
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

        public static string UppercaseFirst(this string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException("Cannot uppercase empty string.", nameof(s));

            var chars = s.ToCharArray();
            chars[0] = char.ToUpper(chars[0]);
            return new string(chars);
        }

        /// <summary>
        /// Encrypts a string to a byte array.
        /// </summary>
        /// <param name="algorithm">Symmetric algorithm for the encryption.</param>
        /// <param name="s">String to encrypt.</param>
        /// <param name="encoding">Byte encoding for encryption. If not specified, UTF-16 is used.</param>
        /// <returns>Encrypted bytes in Task.</returns>
        public static async Task<byte[]> EncryptStringAsync(this SymmetricAlgorithm algorithm, string s,
            Encoding encoding = null)
        {
            if (algorithm == null)
                throw new ArgumentNullException(nameof(algorithm));
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            encoding = encoding ?? Encoding.Unicode;

            using (var ms = new MemoryStream())
            {
                using (var transform = algorithm.CreateEncryptor())
                {
                    using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                    {
                        await cs.WriteAsync(encoding.GetBytes(s), 0, encoding.GetByteCount(s));
                    }
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Encrypts a string to a byte array.
        /// </summary>
        /// <param name="algorithm">Symmetric algorithm for the decryption.</param>
        /// <param name="cipher">Encrypted string bytes to decrypt.</param>
        /// <param name="encoding">Byte encoding for decryption. If not specified, UTF-16 is used.</param>
        /// <returns>Decrypted string in a Task.</returns>
        public static async Task<string> DecryptStringAsync(this SymmetricAlgorithm algorithm, byte[] cipher,
            Encoding encoding = null)
        {
            if (algorithm == null)
                throw new ArgumentNullException(nameof(algorithm));
            if (cipher == null)
                throw new ArgumentNullException(nameof(cipher));

            encoding = encoding ?? Encoding.Unicode;

            using (var ms = new MemoryStream())
            {
                using (var transform = algorithm.CreateDecryptor())
                {
                    using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                    {
                        await cs.WriteAsync(cipher, 0, cipher.Length);
                    }
                }
                return encoding.GetString(ms.ToArray());
            }
        }
    }
}

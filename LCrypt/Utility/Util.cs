using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LCrypt.HashAlgorithms;

namespace LCrypt.Utility
{
    public static class Util
    {
        public static byte[] GenerateStrongRandomBytes(int byteCount)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var array = new byte[byteCount];
                rng.GetBytes(array);
                return array;
            }
        }

        public static ImageSource ExtractFileIcon(string path)
        {
            try
            {
                using (var icon = Icon.ExtractAssociatedIcon(path))
                {
                    return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
            catch (Exception)
            {
                return null; //TODO: Show general icon
            }
        }

        public static IHashAlgorithm GetHashAlgorithm(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            switch (name)
            {
                case "MD5":
                    return new Md5();
                case "CRC32":
                    return new Crc32();
                case "SHA-1":
                    return new Sha1();
                case "SHA-256":
                    return new Sha256();
                case "SHA-384":
                    return new Sha384();
                case "SHA-512":
                    return new Sha512();
                case "Whirlpool":
                    return new Whirlpool();
                default:
                    throw new ArgumentException("Unknown hash algorithm", nameof(name));
            }
        }
    }
}

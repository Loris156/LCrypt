using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
    }
}

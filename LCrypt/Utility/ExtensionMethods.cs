using System;
using System.Drawing;
using System.Security.Cryptography;
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
            if(icon == null)
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
    }
}
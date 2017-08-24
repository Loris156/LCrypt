using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LCrypt.Utility
{
    public static class Util
    {
        public static Icon GetIconByFilename(string path)
        {
            return Icon.ExtractAssociatedIcon(path);
        }

        //Modified code from .NET Reference source
        public static async Task<byte[]> ReadAllBytesAsync(string path)
        {
            byte[] bytes;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                var index = 0;
                var fileLength = fs.Length;
                if (fileLength > Int32.MaxValue)
                    throw new IOException("File is larger than 2GB.");
                var count = (int)fileLength;
                bytes = new byte[count];
                while (count > 0)
                {
                    var n = await fs.ReadAsync(bytes, index, count);
                    if (n == 0)
                        throw new EndOfStreamException("Read beyond 2GB.");
                    index += n;
                    count -= n;
                }
            }
            return bytes;
        }

        /// <summary>
        /// Generates a cryptographically strong sequence of random values.
        /// </summary>
        /// <param name="count">Size of returned byte array.</param>
        /// <returns>A filled byte array with random values.</returns>
        public static byte[] GetStrongRandomBytes(int count)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[count];
                rng.GetBytes(bytes, 0, count);
                return bytes;
            }
        }

        public static void ClearClipboardSafe()
        {
            var thread = new Thread(Clipboard.Clear);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public static void CopyFor(string s, TimeSpan timeSpan)
        {
            Clipboard.SetText(s);
            var timer = new DispatcherTimer {Interval = timeSpan};
            timer.Start();
            timer.Tick += (sender, eventArgs) =>
            {              
                ClearClipboardSafe();
                var dispatcher = (DispatcherTimer)sender;
                dispatcher.Stop();
            };
        }
    }
}
